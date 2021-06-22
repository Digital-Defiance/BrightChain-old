using BrightChain.Services;
using CSharpTest.Net.Collections;

namespace BrightChain.Interfaces
{
    public interface IBPlusTreeCacheManager<Tkey, Tvalue> : ICacheManager<Tkey, Tvalue>
        where Tvalue : new()
    {
        BPlusTreeCacheManager<Tkey, Tvalue> CacheManager { get; internal set; }
        void SetCacheManager(BPlusTreeCacheManager<Tkey, Tvalue> cacheManager);
        bool TreeIsEqual(BPlusTree<Tkey, Tvalue> other);
        bool TreeIsSame(BPlusTree<Tkey, Tvalue> other);
        void Commit();
        void Rollback();
    }
}
