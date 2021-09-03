namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
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

        /// <summary>
        /// Map of correlation GUIDs to latest CBL source hash associated with a correlation ID.
        /// </summary>
        protected FasterKV<string, BrightChainIndexValue> cblIndicesKV =>
            (FasterKV<string, BrightChainIndexValue>)this.fasterStores[CacheStoreType.CBLIndices];
    }
}
