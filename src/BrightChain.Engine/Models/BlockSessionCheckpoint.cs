using System;

namespace BrightChain.Engine.Faster;

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
