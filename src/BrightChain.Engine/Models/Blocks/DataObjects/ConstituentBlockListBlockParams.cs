using System.Collections.Generic;

namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    public class ConstituentBlockListBlockParams : TransactableBlockParams
    {
        public BlockHash FinalDataHash;
        public ulong TotalLength;
        public IEnumerable<Block> ConstituentBlocks;

        public ConstituentBlockListBlockParams(TransactableBlockParams blockParams, BlockHash finalDataHash, ulong totalLength, IEnumerable<Block> constituentBlocks)
       : base(
             cacheManager: blockParams.CacheManager,
             blockParams: blockParams)
        {
            FinalDataHash = finalDataHash;
            TotalLength = totalLength;
            ConstituentBlocks = constituentBlocks;
        }
    }
}
