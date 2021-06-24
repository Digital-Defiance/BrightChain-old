using CSharpTest.Net.Serialization;
using System;

namespace BrightChain.Services
{
    /// <summary>
    /// Cache system focused on grouping expiring blocks into cache keys by second and containing a list of expiring block hashes
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="Tvalue"></typeparam>
    public class BlockCacheExpirationCache<Tkey, Tvalue, TkeySerializer, TvalueSerializer>
        where Tkey : IComparable<Tkey>
        where Tvalue : IComparable<Tvalue>
        where TkeySerializer : ISerializer<Tkey>, new()
        where TvalueSerializer : ISerializer<Tvalue>, new()
    {
        private BlockCacheManager expirationCache;
        private BPlusTreeCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer> sourceCache;

        public BlockCacheExpirationCache(BPlusTreeCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer> sourceCache)
        {
            /* TODO:
             * for each entry in the cache, create or add to the list of blocks expiring that second
             * whenever new blocks are stored, make sure to update this cache
             * this cache should be written to disk with DiskBlockCache
             */
        }
    }
}
