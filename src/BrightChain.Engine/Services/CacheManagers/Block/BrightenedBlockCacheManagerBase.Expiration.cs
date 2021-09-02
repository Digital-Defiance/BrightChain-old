using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Services.CacheManagers.Block
{
    public partial class BrightenedBlockCacheManagerBase
    {
        public abstract List<BlockHash> GetBlocksExpiringAt(long date);

        public abstract void AddExpiration(BrightenedBlock block, bool noCheckContains = false);

        public abstract void RemoveExpiration(BrightenedBlock block);

        public abstract void ExpireBlocks(long date);

        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// determine/lookup oldest block in cache
        /// expire all seconds between, inclusive, that time and specified time
        /// </remarks>
        /// <param name="date"></param>
        public abstract void ExpireBlocksThrough(long date);
    }
}
