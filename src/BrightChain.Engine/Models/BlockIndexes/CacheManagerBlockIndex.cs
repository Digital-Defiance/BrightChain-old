namespace BrightChain.Engine.Models.BlockIndexes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Services;

    public class CacheManagerBlockIndex<Tblock>
        where Tblock : Blocks.Block
    {
        public CacheManagerBlockIndex()
        {
        }
    }
}
