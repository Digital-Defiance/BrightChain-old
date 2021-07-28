namespace BrightChain.Engine.Models.BlockIndexes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.Chains;

    public class CacheManagerCBLBlockIndex : CacheManagerBlockIndex<ConstituentBlockListBlock>
    {
        public CacheManagerCBLBlockIndex()
        {
        }
    }
}
