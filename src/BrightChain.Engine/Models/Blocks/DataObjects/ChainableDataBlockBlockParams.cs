using System.Collections.Generic;

namespace BrightChain.Engine.Models.Blocks.DataObjects
{
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
    }
}
