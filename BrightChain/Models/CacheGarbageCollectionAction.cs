using BrightChain.CSharpTest.Net.Collections;

namespace BrightChain.Models
{
    public static class CacheGarbageCollectionAction<Tkey, Tvalue>
    {
        public static void GarbageCollect(BPlusTree<Tkey, Tvalue> tree)
        {
            // get (keep and store elsewhere?) list of keys expiring this second in the given tree (and which haven't been required longer by renewed/extended contracts)
            // remove expired entries
        }
    }
}
