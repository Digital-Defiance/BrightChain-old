namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using BrightChain.Engine.Exceptions;

    public class ChainLinqBlockParams : TransactableBlockParams
    {
        public readonly BlockHash Next = null;

        public ChainLinqBlockParams(TransactableBlockParams blockParams, BlockHash next = null)
       : base(
             cacheManager: blockParams.CacheManager,
             allowCommit: blockParams.AllowCommit,
             blockParams: blockParams)
        {
            this.Next = next;
        }

        public ChainLinqBlockParams Merge(ChainLinqBlockParams otherBlockParams)
        {
            if (otherBlockParams.BlockSize != this.BlockSize)
            {
                throw new BrightChainException("BlockSize mismatch");
            }

            return new ChainLinqBlockParams(
                blockParams: this.Merge(otherBlockParams),
                next: this.Next);
        }
    }
}
