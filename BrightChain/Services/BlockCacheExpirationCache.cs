namespace BrightChain.Services
{
    public class BlockCacheExpirationCache<Tkey, Tvalue>
    {
        private BlockCacheManager expirationCache;
        private BPlusTreeCacheManager<Tkey, Tvalue> sourceCache;

        public BlockCacheExpirationCache(BPlusTreeCacheManager<Tkey, Tvalue> sourceCache)
        {
            /* TODO:
             * for each entry in the cache, create or add to the list of blocks expiring that second
             * whenever new blocks are stored, make sure to update this cache
             * this cache should be written to disk with DiskBlockCache
             */
        }
    }
}
