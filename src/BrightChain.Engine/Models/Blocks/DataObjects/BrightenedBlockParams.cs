namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Hashes;

    public class BrightenedBlockParams : BlockParams
    {
        public ICacheManager<BlockHash, BrightenedBlock> CacheManager;

        public bool AllowCommit { get; }

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

        public BrightenedBlockParams Merge(BrightenedBlockParams otherBlockParams)
        {
            if (otherBlockParams.BlockSize != this.BlockSize)
            {
                throw new BrightChainException("BlockSize mismatch");
            }

            return new BrightenedBlockParams(
                cacheManager: this.CacheManager,
                allowCommit: this.AllowCommit && otherBlockParams.AllowCommit,
                blockParams: this.Merge(otherBlockParams));
        }
    }
}
