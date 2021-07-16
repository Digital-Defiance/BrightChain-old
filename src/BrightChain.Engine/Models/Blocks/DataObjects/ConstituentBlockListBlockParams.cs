using System.Collections.Generic;

namespace BrightChain.Engine.Models.Blocks.DataObjects
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
            FinalDataHash = finalDataHash;
            TotalLength = totalLength;
            ConstituentBlocks = constituentBlocks;
        }
    }
}
