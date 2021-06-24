using BrightChain.Services;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Interfaces;
using CSharpTest.Net.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        public TransactionLogOptions<Tkey, Tvalue> NewTransactionLogOptions() =>
            new TransactionLogOptions<Tkey, Tvalue>(
                    fileName: this.cacheManager.TransactionLogPath,
                    keySerializer: new TkeySerializer(),
                    valueSerializer: new TvalueSerializer());

        [TestMethod, Ignore]
        public void ItCommitsDataTest()
        {
            var alternateValue = NewKeyValue();

            using (var tlog = new TransactionLog<Tkey, Tvalue>(
                this.NewTransactionLogOptions()))
            {
            }
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
