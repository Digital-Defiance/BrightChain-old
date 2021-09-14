namespace BrightChain.Engine.Faster
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Faster.Enumerations;

    public struct BlockSessionCheckpoint
    {
        public readonly bool Success;
        public readonly Dictionary<CacheStoreType, bool> CheckpointResult;
        public readonly Dictionary<CacheStoreType, Guid> CheckpointGuids;

        public BlockSessionCheckpoint(bool success, Dictionary<CacheStoreType, bool> results, Dictionary<CacheStoreType, Guid> guids)
        {
            this.Success = success;
            this.CheckpointResult = results;
            this.CheckpointGuids = guids;
        }
    }
}
