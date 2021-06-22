using BrightChain.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightChain.Tests
{
    [TestClass]
    public abstract class BPlusTreeCacheManagerTest<Tkey, Tvalue> : CacheManagerTest<BPlusTreeCacheManager<Tkey, Tvalue>, Tkey, Tvalue>
            where Tvalue : new()
    {
    }
}
