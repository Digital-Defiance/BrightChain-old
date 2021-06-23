using CSharpTest.Net.Collections;

namespace BrightChain.Interfaces
{
    public interface IBPlusTreeCacheManager<Tkey, Tvalue> : ICacheManager<Tkey, Tvalue>
    {
        bool TreeIsEqual(BPlusTree<Tkey, Tvalue> other);
        bool TreeIsSame(BPlusTree<Tkey, Tvalue> other);
        void Commit();
        void Rollback();
    }
}
