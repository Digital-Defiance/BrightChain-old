using System.Collections.Generic;

namespace BrightChain.Models.Blocks.DataObjects
{
    public class ConstituentBlockListBlockParams : TransactableBlockParams
    {
        public BlockHash FinalDataHash;
        public ulong TotalLength;
        public IEnumerable<Block> ConstituentBlocks;

        public ConstituentBlockListBlockParams(TransactableBlockParams blockArguments, BlockHash finalDataHash, ulong totalLength, IEnumerable<Block> constituentBlocks)
       : base(
             cacheManager: blockArguments.CacheManager,
             blockArguments: blockArguments)
        {
            this.FinalDataHash = finalDataHash;
            this.TotalLength = totalLength;
            this.ConstituentBlocks = constituentBlocks;
        }
    }
}
