namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using BrightChain.Engine.Exceptions;

    public class ChainableDataBlockParams : TransactableBlockParams
    {
        public readonly BlockHash Previous = null;
        public readonly BlockHash Next = null;

        public ChainableDataBlockParams(TransactableBlockParams blockParams, BlockHash previous = null, BlockHash next = null)
       : base(
             cacheManager: blockParams.CacheManager,
             allowCommit: blockParams.AllowCommit,
             blockParams: blockParams)
        {
            this.Previous = previous;
            this.Next = next;
        }

        public ChainableDataBlockParams Merge(ChainableDataBlockParams otherBlockParams)
        {
            if (otherBlockParams.BlockSize != this.BlockSize)
            {
                throw new BrightChainException("BlockSize mismatch");
            }

            return new ChainableDataBlockParams(
                blockParams: this.Merge(otherBlockParams),
                previous: this.Previous,
                next: this.Next);
        }
    }
}
