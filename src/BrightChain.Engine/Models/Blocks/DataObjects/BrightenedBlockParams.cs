using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Models.Blocks.DataObjects;

public class BrightenedBlockParams : BlockParams
{
    public ICacheManager<BlockHash, BrightenedBlock> CacheManager;

    public BrightenedBlockParams(ICacheManager<BlockHash, BrightenedBlock> cacheManager, bool allowCommit, BlockParams blockParams)
        : base(
            blockSize: blockParams.BlockSize,
            requestTime: blockParams.RequestTime,
            keepUntilAtLeast: blockParams.KeepUntilAtLeast,
            redundancy: blockParams.Redundancy,
            privateEncrypted: blockParams.PrivateEncrypted,
            originalType: blockParams.OriginalType)
    {
        this.CacheManager = cacheManager;
        this.AllowCommit = allowCommit;
    }

    public bool AllowCommit { get; }

    public BrightenedBlockParams Merge(BrightenedBlockParams otherBlockParams)
    {
        if (otherBlockParams.BlockSize != this.BlockSize)
        {
            throw new BrightChainException(message: "BlockSize mismatch");
        }

        return new BrightenedBlockParams(
            cacheManager: this.CacheManager,
            allowCommit: this.AllowCommit && otherBlockParams.AllowCommit,
            blockParams: this.Merge(otherBlockParams: otherBlockParams));
    }
}
