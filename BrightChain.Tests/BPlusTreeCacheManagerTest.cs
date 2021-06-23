using BrightChain.Interfaces;
using BrightChain.Services;
using CSharpTest.Net.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightChain.Tests
{
    [TestClass]
    public abstract class BPlusTreeCacheManagerTest<Tcache, Tkey, Tvalue> : CacheManagerTest<Tcache, Tkey, Tvalue>
            where Tcache : BPlusTreeCacheManager<Tkey, Tvalue>
            where Tvalue : ITransactable, ITransactableBlock
    {
        [TestMethod, Ignore]
        public void DataCommitTest()
        {
            this.cacheManager.Set(testPair.Key, testPair.Value);
            Assert.IsFalse(testPair.Value.Committed);
            this.cacheManager.Commit();
            Assert.IsTrue(testPair.Value.Committed);
            Assert.IsTrue(this.cacheManager.Contains(testPair.Key));
        }

        [TestMethod, Ignore]
        public void DataRollbackTest()
        {
            this.cacheManager.Set(testPair.Key, testPair.Value);
            Assert.IsFalse(testPair.Value.Committed);
            this.cacheManager.Rollback();
            Assert.IsFalse(this.cacheManager.Contains(testPair.Key));
        }
    }
}
