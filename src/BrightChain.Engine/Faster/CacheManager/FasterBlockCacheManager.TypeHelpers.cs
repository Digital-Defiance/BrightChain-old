﻿namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
    using System.Collections.Generic;
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
                    CacheStoreType.BlockData,
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
                    CacheStoreType.Indices,
                    FasterBase (Dictionary<CacheDeviceType, IDevice> storeDevices, bool useReadCache, string cacheDir) =>
                    {
                        var cblIndexSerializerSettings = new SerializerSettings<string, BrightChainIndexValue>
                        {
                            keySerializer = () => null,
                            valueSerializer = () => new FasterBrightChainIndexValueSerializer(),
                        };

                        return new FasterKV<string, BrightChainIndexValue>(
                            size: HashTableBuckets,
                            logSettings: NewLogSettings(storeDevices, useReadCache),
                            checkpointSettings: NewCheckpointSettings(cacheDir),
                            serializerSettings: cblIndexSerializerSettings,
                            comparer: null);
                    }
                },
            };

        protected FasterKV<BlockHash, BlockData> primaryDataKV =>
            (FasterKV<BlockHash, BlockData>)this.fasterStores[CacheStoreType.BlockData];

        /// <summary>
        /// Map of correlation GUIDs to latest CBL source hash associated with a correlation ID.
        /// </summary>
        protected FasterKV<string, BrightChainIndexValue> cblIndicesKV =>
            (FasterKV<string, BrightChainIndexValue>)this.fasterStores[CacheStoreType.Indices];
    }
}
