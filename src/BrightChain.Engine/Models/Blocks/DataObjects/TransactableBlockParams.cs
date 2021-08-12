namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Hashes;

    [Serializable]
    public class TransactableBlockParams : BlockParams
    {
        public ICacheManager<BlockHash, TransactableBlock> CacheManager;

        public bool AllowCommit { get; }

        public TransactableBlockParams(ICacheManager<BlockHash, TransactableBlock> cacheManager, bool allowCommit, BlockParams blockParams)
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

        public TransactableBlockParams Merge(TransactableBlockParams otherBlockParams)
        {
            if (otherBlockParams.BlockSize != this.BlockSize)
            {
                throw new BrightChainException("BlockSize mismatch");
            }

            return new TransactableBlockParams(
                cacheManager: this.CacheManager,
                allowCommit: this.AllowCommit && otherBlockParams.AllowCommit,
                blockParams: this.Merge(otherBlockParams));
        }
    }
}
