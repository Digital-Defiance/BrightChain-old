using BrightChain.Services;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Interfaces;
using CSharpTest.Net.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace BrightChain.Tests
{
    /// <summary>
    /// Exercizes the BPlusTree using the CacheManagerTest
    /// TODO: be sure to test the cache events
    /// </summary>
    /// <typeparam name="Tcache"></typeparam>
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="Tvalue"></typeparam>
    /// <typeparam name="TkeySerializer"></typeparam>
    /// <typeparam name="TvalueSerializer"></typeparam>
    [TestClass]
    public abstract class BPlusTreeCacheManagerTest<Tcache, Tkey, Tvalue, TkeySerializer, TvalueSerializer> : CacheManagerTest<Tcache, Tkey, Tvalue>
        where Tcache : BPlusTreeCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer>
        where Tkey : IComparable<Tkey>
        where Tvalue : IComparable<Tvalue>, ITransactable
        where TkeySerializer : ISerializer<Tkey>, new()
        where TvalueSerializer : ISerializer<Tvalue>, new()
    {

        public TransactionLogOptions<Tkey, Tvalue> NewTransactionLogOptions() => new TransactionLogOptions<Tkey, Tvalue>(
fileName: this.cacheManager.TransactionLogPath,
keySerializer: new TkeySerializer(),
valueSerializer: new TvalueSerializer());

        [TestMethod, Ignore]
        public void ItCommitsDataTest()
        {
            var alternateValue = this.NewKeyValue();

            using (var tlog = new TransactionLog<Tkey, Tvalue>(
                this.NewTransactionLogOptions()))
            {
                this.cacheManager.Set(this.testPair.Key, this.testPair.Value);
                Assert.AreEqual(this.testPair.Value, this.cacheManager.Get(this.testPair.Key));
                this.cacheManager.Commit();
                this.cacheManager.Drop(this.testPair.Key);
                this.cacheManager.Set(alternateValue.Key, alternateValue.Value);
                this.cacheManager.Commit();
                this.cacheManager.Rollback();
                Assert.IsFalse(this.cacheManager.Contains(alternateValue.Key));
            }

            this.logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(3));
            this.logger.VerifyNoOtherCalls();
        }

        [TestMethod, Ignore]
        public void ItRollsBackDataTest()
        {
            using (var tlog = new TransactionLog<Tkey, Tvalue>(
                this.NewTransactionLogOptions()))
            {
            }
        }
    }
}
