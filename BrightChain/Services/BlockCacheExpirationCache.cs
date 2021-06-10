using BrightChain.Interfaces;
using System.Collections.Generic;

namespace BrightChain.Services
{
    public class BlockCacheExpirationCache<Tkey, Tvalue>
    {
        private DiskCacheManager<ulong, IEnumerable<Tkey>> expirationCache;
        private ICacheManager<Tkey, Tvalue> sourceCache;

        public BlockCacheExpirationCache(ICacheManager<Tkey, Tvalue> sourceCache)
        {
            /* TODO:
             * for each entry in the cache, create or add to the list of blocks expiring that second
             * whenever new blocks are stored, make sure to update this cache
             * this cache should be written to disk with DiskBlockCache
             */
        }
    }
}
