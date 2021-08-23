namespace BrightChain.Engine.Services.CacheManagers
{
    using System;
    using System.Globalization;
    using System.IO;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Faster.Serializers;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;

    public partial class FasterBlockCacheManager
    {
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

        private FasterKV<BlockHash, BrightenedBlock> NewMetdataKV
        {
            get
            {
                var blockMetadataLogSettings = new LogSettings // log settings (devices, page size, memory size, etc.)
                {
                    LogDevice = this.blockMetadataLogDevice,
                    ObjectLogDevice = this.blockMetadataDevice,
                    ReadCacheSettings = this.useReadCache ? new ReadCacheSettings() : null,
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
                        CheckpointDir = this.GetDiskCacheDirectory().FullName, // TODO: can these be in the same dir?
                    },
                    serializerSettings: metadataSerializerSettings,
                    comparer: BlockSizeMap.ZeroVectorHash(BlockSize.Micro)); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.
            }
        }

        private FasterKV<BlockHash, BlockData> NewDataFasterKV
        {
            get
            {
                var blockDataLogSettings = new LogSettings
                {
                    LogDevice = this.blockDataLogDevice,
                    ObjectLogDevice = this.blockDataDevice,
                    ReadCacheSettings = this.useReadCache ? new ReadCacheSettings() : null,
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
                        CheckpointDir = this.GetDiskCacheDirectory().FullName,
                    },
                    serializerSettings: blockDataSerializerSettings,
                    comparer: BlockSizeMap.ZeroVectorHash(BlockSize.Micro)); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.
            }
        }

        private FasterKV<DataHash, BrightHandle> NewCblSourceHashesFasterKV
        {
            get
            {
                var cblSourceHashesLogSettings = new LogSettings
                {
                    LogDevice = this.blockDataLogDevice,
                    ObjectLogDevice = this.blockDataDevice,
                    ReadCacheSettings = this.useReadCache ? new ReadCacheSettings() : null,
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
                        CheckpointDir = this.GetDiskCacheDirectory().FullName,
                    },
                    serializerSettings: cblSourceHashesSerializerSettings,
                    comparer: new DataHash(dataBytes: new ReadOnlyMemory<byte>())); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.
            }
        }

        private FasterKV<Guid, DataHash> NewCblCorrelationIdFasterKV
        {
            get
            {
                var cblSourceHashesLogSettings = new LogSettings
                {
                    LogDevice = this.blockDataLogDevice,
                    ObjectLogDevice = this.blockDataDevice,
                    ReadCacheSettings = this.useReadCache ? new ReadCacheSettings() : null,
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
                        CheckpointDir = this.GetDiskCacheDirectory().FullName,
                    },
                    serializerSettings: cblSourceHashesSerializerSettings,
                    comparer: null);
            }
        }

    }
}
