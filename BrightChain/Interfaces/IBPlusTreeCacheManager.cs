using CSharpTest.Net.Collections;
using System;

namespace BrightChain.Interfaces
{
    public interface IBPlusTreeCacheManager<Tkey, Tvalue> : ICacheManager<Tkey, Tvalue>
        where Tkey : IComparable<Tkey>
        where Tvalue : IComparable<Tvalue>
    {
        bool TreeIsEqual(BPlusTree<Tkey, Tvalue> other);
        bool TreeIsSame(BPlusTree<Tkey, Tvalue> other);
        void Commit();
        void Rollback();
    }
}
