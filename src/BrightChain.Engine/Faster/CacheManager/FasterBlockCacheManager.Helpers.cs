namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
    using System.Globalization;
    using System.IO;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Faster.Enumerations;
    using BrightChain.Engine.Faster.Serializers;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;

    public partial class FasterBlockCacheManager
    {
        private readonly Dictionary<CacheStoreType, Func<Dictionary<CacheDeviceType, IDevice>, bool, string, FasterBase>> newKVFuncs =
            new Dictionary<CacheStoreType, Func<Dictionary<CacheDeviceType, IDevice>, bool, string, FasterBase>>()
            {
                {
                    CacheStoreType.PrimaryMetadata,
                    FasterBase (Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache, string cacheDir) =>
                    {
                        var blockMetadataLogSettings = new LogSettings // log settings (devices, page size, memory size, etc.)
                        {
                            LogDevice = storeDevices[CacheDeviceType.Log],
                            ObjectLogDevice = storeDevices[CacheDeviceType.Data],
                            ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
                        };

                        // Define serializers; otherwise FASTER will use the slower DataContract
                        // Needed only for class keys/values
                        var metadataSerializerSettings = new SerializerSettings<BlockHash, BrightenedBlock>
                        {
                            keySerializer = () => new FasterBlockHashSerializer(),
                            valueSerializer = () => new DataContractObjectSerializer<BrightenedBlock>(),
                        };

                        return new FasterKV<BlockHash, BrightenedBlock>(
                            size: HashTableBuckets,
                            logSettings: blockMetadataLogSettings,
                            checkpointSettings: new CheckpointSettings
                            {
                                CheckpointDir = cacheDir, // TODO: can these be in the same dir?
                            },
                            serializerSettings: metadataSerializerSettings,
                            comparer: BlockSizeMap.ZeroVectorHash(BlockSize.Micro)); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.
                    }
                },
                {
                    CacheStoreType.PrimaryData,
                    FasterBase (Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache, string cacheDir) =>
                    {
                        var blockDataLogSettings = new LogSettings
                        {
                            LogDevice = storeDevices[CacheDeviceType.Log],
                            ObjectLogDevice = storeDevices[CacheDeviceType.Data],
                            ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
                        };

                        var blockDataSerializerSettings = new SerializerSettings<BlockHash, BlockData>
                        {
                            keySerializer = () => new FasterBlockHashSerializer(),
                            valueSerializer = () => new DataContractObjectSerializer<BlockData>(),
                        };

                        return new FasterKV<BlockHash, BlockData>(
                            size: HashTableBuckets,
                            logSettings: blockDataLogSettings,
                            checkpointSettings: new CheckpointSettings
                            {
                                CheckpointDir = cacheDir,
                            },
                            serializerSettings: blockDataSerializerSettings,
                            comparer: BlockSizeMap.ZeroVectorHash(BlockSize.Micro)); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.
                    }
                },
                {
                    CacheStoreType.PrimaryExpiration,
                    FasterBase (Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache, string cacheDir) =>
                    {
                        var blockDataLogSettings = new LogSettings
                        {
                            LogDevice = storeDevices[CacheDeviceType.Log],
                            ObjectLogDevice = storeDevices[CacheDeviceType.Data],
                            ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
                        };

                        var blockDataSerializerSettings = new SerializerSettings<long, List<BlockHash>>
                        {
                            keySerializer = () => null,
                            valueSerializer = () => new DataContractObjectSerializer<List<BlockHash>>(),
                        };

                        return new FasterKV<long, List<BlockHash>>(
                            size: HashTableBuckets,
                            logSettings: blockDataLogSettings,
                            checkpointSettings: new CheckpointSettings
                            {
                                CheckpointDir = cacheDir,
                            },
                            serializerSettings: blockDataSerializerSettings,
                            comparer: null);
                    }
                },
                {
                    CacheStoreType.CBL,
                    FasterBase (Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache, string cacheDir) =>
                    {
                        var cblSourceHashesLogSettings = new LogSettings
                        {
                            LogDevice = storeDevices[CacheDeviceType.Log],
                            ObjectLogDevice = storeDevices[CacheDeviceType.Data],
                            ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
                        };

                        var cblSourceHashesSerializerSettings = new SerializerSettings<DataHash, BrightHandle>
                        {
                            keySerializer = () => new FasterDataHashSerializer(),
                            valueSerializer = () => new DataContractObjectSerializer<BrightHandle>(),
                        };

                        return new FasterKV<DataHash, BrightHandle>(
                            size: HashTableBuckets,
                            logSettings: cblSourceHashesLogSettings,
                            checkpointSettings: new CheckpointSettings
                            {
                                CheckpointDir = cacheDir,
                            },
                            serializerSettings: cblSourceHashesSerializerSettings,
                            comparer: new DataHash(dataBytes: new ReadOnlyMemory<byte>())); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.
                    }
                },
                {
                    CacheStoreType.CBLCorrelation,
                    FasterBase (Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache, string cacheDir) =>
                    {
                        var cblSourceHashesLogSettings = new LogSettings
                        {
                            LogDevice = storeDevices[CacheDeviceType.Log],
                            ObjectLogDevice = storeDevices[CacheDeviceType.Data],
                            ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
                        };

                        var cblSourceHashesSerializerSettings = new SerializerSettings<Guid, DataHash>
                        {
                            keySerializer = () => new FasterGuidSerializer(),
                            valueSerializer = () => new FasterDataHashSerializer(),
                        };

                        return new FasterKV<Guid, DataHash>(
                            size: HashTableBuckets,
                            logSettings: cblSourceHashesLogSettings,
                            checkpointSettings: new CheckpointSettings
                            {
                                CheckpointDir = cacheDir,
                            },
                            serializerSettings: cblSourceHashesSerializerSettings,
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

        protected FasterKV<DataHash, BrightHandle> cblSourceHashesKV =>
            (FasterKV<DataHash, BrightHandle>)this.fasterStores[CacheStoreType.CBL];

        protected FasterKV<Guid, DataHash> cblCorrelationIdsKV =>
            (FasterKV<Guid, DataHash>)this.fasterStores[CacheStoreType.CBLCorrelation];

        protected DirectoryInfo GetDiskCacheDirectory()
        {
            return Directory.CreateDirectory(
                Path.Combine(
                    this.baseDirectory,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "BrightChain-{0}",
                        this.DatabaseName)));
        }

        protected string GetDevicePath(string nameSpace, out DirectoryInfo cacheDirectoryInfo)
        {
            cacheDirectoryInfo = this.GetDiskCacheDirectory();

            return Path.Combine(
                cacheDirectoryInfo.FullName,
                string.Format(
                    provider: System.Globalization.CultureInfo.InvariantCulture,
                    format: "brightchain-{0}-{1}.log",
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
