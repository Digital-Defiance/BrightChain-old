namespace BrightChain.Engine.Faster
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Faster.Enumerations;

    public struct BlockSessionCheckpoint
    {
        public readonly bool Success;
        public readonly bool CheckpointResult;
        public readonly Guid CheckpointGuids;

        public BlockSessionCheckpoint(bool success, bool result, Guid guid)
        {
            this.Success = success;
            this.CheckpointResult = result;
            this.CheckpointGuids = guid;
        }
    }
}
