namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
    using System.Globalization;
    using System.IO;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Faster.Enumerations;
    using BrightChain.Engine.Faster.Indices;
    using BrightChain.Engine.Faster.Serializers;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;

    public partial class FasterBlockCacheManager
    {
        private static LogSettings NewLogSettings(Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache) =>
            new LogSettings
            {
                LogDevice = storeDevices[CacheDeviceType.Log],
                ObjectLogDevice = storeDevices[CacheDeviceType.Data],
                ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
            };

        private static CheckpointSettings NewCheckpointSettings(string cacheDir) =>
            new CheckpointSettings
            {
                CheckpointDir = cacheDir,
            };

        private readonly Dictionary<CacheStoreType, Func<Dictionary<CacheDeviceType, IDevice>, bool, string, FasterBase>> newKVFuncs =
            new Dictionary<CacheStoreType, Func<Dictionary<CacheDeviceType, IDevice>, bool, string, FasterBase>>()
            {
                {
                    CacheStoreType.PrimaryMetadata,
                    FasterBase (Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache, string cacheDir) =>
                    {
                        // Define serializers; otherwise FASTER will use the slower DataContract
                        // Needed only for class keys/values
                        var metadataSerializerSettings = new SerializerSettings<BlockHash, BrightenedBlock>
                        {
                            keySerializer = () => new FasterBlockHashSerializer(),
                            valueSerializer = () => new DataContractObjectSerializer<BrightenedBlock>(),
                        };

                        return new FasterKV<BlockHash, BrightenedBlock>(
                            size: HashTableBuckets,
                            logSettings: NewLogSettings(storeDevices, useReadCache),
                            checkpointSettings: NewCheckpointSettings(cacheDir),
                            serializerSettings: metadataSerializerSettings,
                            comparer: BlockSizeMap.ZeroVectorHash(BlockSize.Micro)); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.
                    }
                },
                {
                    CacheStoreType.PrimaryData,
                    FasterBase (Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache, string cacheDir) =>
                    {
                        var blockDataSerializerSettings = new SerializerSettings<BlockHash, BlockData>
                        {
                            keySerializer = () => new FasterBlockHashSerializer(),
                            valueSerializer = () => new DataContractObjectSerializer<BlockData>(),
                        };

                        return new FasterKV<BlockHash, BlockData>(
                            size: HashTableBuckets,
                            logSettings: NewLogSettings(storeDevices, useReadCache),
                            checkpointSettings: NewCheckpointSettings(cacheDir),
                            serializerSettings: blockDataSerializerSettings,
                            comparer: BlockSizeMap.ZeroVectorHash(BlockSize.Micro)); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.
                    }
                },
                {
                    CacheStoreType.PrimaryExpiration,
                    FasterBase (Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache, string cacheDir) =>
                    {
                        var blockDataSerializerSettings = new SerializerSettings<long, List<BlockHash>>
                        {
                            keySerializer = () => null,
                            valueSerializer = () => new DataContractObjectSerializer<List<BlockHash>>(),
                        };

                        return new FasterKV<long, List<BlockHash>>(
                            size: HashTableBuckets,
                            logSettings: NewLogSettings(storeDevices, useReadCache),
                            checkpointSettings: NewCheckpointSettings(cacheDir),
                            serializerSettings: blockDataSerializerSettings,
                            comparer: null);
                    }
                },
                {
                    CacheStoreType.CBLIndices,
                    FasterBase (Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache, string cacheDir) =>
                    {
                        return new FasterKV<string, BrightChainIndexValue>(
                            size: HashTableBuckets,
                            logSettings: NewLogSettings(storeDevices, useReadCache),
                            checkpointSettings: NewCheckpointSettings(cacheDir),
                            serializerSettings: null,
                            comparer: null);
                    }
                },
            };

        protected FasterKV<BlockHash, BrightenedBlock> primaryMetadataKV =>
            (FasterKV<BlockHash, BrightenedBlock>)this.fasterStores[CacheStoreType.PrimaryMetadata];

        protected FasterKV<BlockHash, BlockData> primaryDataKV =>
            (FasterKV<BlockHash, BlockData>)this.fasterStores[CacheStoreType.PrimaryData];

        protected FasterKV<long, List<BlockHash>> primaryExpirationKV =>
            (FasterKV<long, List<BlockHash>>)this.fasterStores[CacheStoreType.PrimaryExpiration];

        /// <summary>
        /// Map of correlation GUIDs to latest CBL source hash associated with a correlation ID.
        /// </summary>
        protected FasterKV<string, BrightChainIndexValue> cblIndicesKV =>
            (FasterKV<string, BrightChainIndexValue>)this.fasterStores[CacheStoreType.CBLIndices];

        protected DirectoryInfo GetDiskCacheDirectory()
        {
            return Directory.CreateDirectory(
                Path.Combine(
                    path1: this.baseDirectory.FullName,
                    path2: "BrightChain",
                    path3: this.DatabaseName));
        }

        protected string GetDevicePath(string nameSpace, out DirectoryInfo cacheDirectoryInfo)
        {
            cacheDirectoryInfo = this.GetDiskCacheDirectory();

            return Path.Combine(
                cacheDirectoryInfo.FullName,
                string.Format(
                    provider: System.Globalization.CultureInfo.InvariantCulture,
                    format: "{0}-{1}.log",
                    this.DatabaseName,
                    nameSpace));
        }

        protected IDevice CreateLogDevice(string nameSpace)
        {
            var devicePath = this.GetDevicePath(nameSpace, out DirectoryInfo _);

            return Devices.CreateLogDevice(
                logPath: devicePath);
        }

        private
            (Dictionary<CacheStoreType, Dictionary<CacheDeviceType, IDevice>> DevicesByStoreType,
            Dictionary<CacheStoreType, FasterBase> StoresByStoreType)
            InitFaster()
        {
            var cacheDir = this.GetDiskCacheDirectory().FullName;
            var kvs = new Dictionary<CacheStoreType, FasterBase>();
            var devices = new Dictionary<CacheStoreType, Dictionary<CacheDeviceType, IDevice>>();
            foreach (CacheStoreType storeType in Enum.GetValues(enumType: typeof(CacheStoreType)))
            {
                var logDevicesByType = new Dictionary<CacheDeviceType, IDevice>();
                foreach (CacheDeviceType deviceType in Enum.GetValues(enumType: typeof(CacheDeviceType)))
                {
                    var device = this.CreateLogDevice(string.Format("{0}-{1}", storeType.ToString(), deviceType.ToString()));
                    logDevicesByType.Add(deviceType, device);
                }

                devices.Add(storeType, logDevicesByType);
                var newKv = this.newKVFuncs[storeType](logDevicesByType, this.useReadCache, cacheDir);
                kvs.Add(storeType, newKv);
            }

            return (devices, kvs);
        }
    }
}
