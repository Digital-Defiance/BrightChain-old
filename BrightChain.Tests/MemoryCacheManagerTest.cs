using BrightChain.Interfaces;
using BrightChain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightChain.Tests
{
    public class MemoryCacheTestObject : object
    {
        public string id;

        public MemoryCacheTestObject(string id)
        {
            this.id = id;
        }

        public MemoryCacheTestObject()
        {
            this.id = MemoryCacheManagerTest.GenerateTestKey();
        }
    }

    [TestClass]
    public class MemoryCacheManagerTest : CacheManagerTest<MemoryCacheManager<string, MemoryCacheTestObject>, string, MemoryCacheTestObject>
    {
        private static int TestKeyLength { get; } = 11;
        public static string GenerateTestKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random(Guid.NewGuid().GetHashCode());
            var randomString = new string(Enumerable.Repeat(chars, TestKeyLength)
                                                    .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }

        internal override ICacheManager<string, MemoryCacheTestObject> NewCacheManager(ILogger logger) => new MemoryCacheManager<string, MemoryCacheTestObject>(logger: logger);

        internal override KeyValuePair<string, MemoryCacheTestObject> NewKeyValue()
        {
            var testKey = GenerateTestKey();
            var testValue = new MemoryCacheTestObject(testKey);
            return new KeyValuePair<string, MemoryCacheTestObject>(testKey, testValue);
        }

        internal override MemoryCacheTestObject NewNullData() => null;

        [TestMethod]
        public void TestSetGetIntegrity()
        {
            // Arrange
            var expectation = testPair.Value;
            cacheManager.Set(testPair.Key, expectation);
            Assert.IsTrue(cacheManager.Contains(testPair.Key));

            // Act
            object result = cacheManager.Get(testPair.Key);

            // Assert
            Assert.IsNotNull(expectation);
            Assert.AreEqual(expectation, result);
            Assert.AreSame(expectation, result);
            Assert.AreEqual(testPair.Key, expectation.id);
        }
    }
}
