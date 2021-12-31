namespace BrightChain.Engine.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Services.CacheManagers;
    using FASTER.core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FasterCacheManagerTest
        : CacheManagerTest<TapestryCacheManager<string, ProtoContractTestObject, BinaryStringSerializer, DataContractObjectSerializer<ProtoContractTestObject>>, string, ProtoContractTestObject>
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

        internal override TapestryCacheManager<string, ProtoContractTestObject, BinaryStringSerializer, DataContractObjectSerializer<ProtoContractTestObject>> NewCacheManager(ILogger logger, IConfiguration configuration)
        {
            return new TapestryCacheManager<string, ProtoContractTestObject, BinaryStringSerializer, DataContractObjectSerializer<ProtoContractTestObject>>(
logger: this.logger.Object,
configuration: this.configuration.Object,
collectionName: Guid.NewGuid().ToString());
        }

        internal override KeyValuePair<string, ProtoContractTestObject> NewKeyValue()
        {
            var testKey = GenerateTestKey();
            var testValue = new ProtoContractTestObject(testKey);
            return new KeyValuePair<string, ProtoContractTestObject>(testKey, testValue);
        }

        internal override ProtoContractTestObject NewNullData()
        {
            return null;
        }

        [TestMethod]
        public void TestSetGetIntegrity()
        {
            // Arrange
            var expectation = this.testPair.Value;
            this.cacheManager.Set(this.testPair.Key, expectation);
            Assert.IsTrue(this.cacheManager.Contains(this.testPair.Key));

            // Act
            object result = this.cacheManager.Get(this.testPair.Key);

            // Assert
            Assert.IsNotNull(expectation);
            Assert.AreEqual(expectation, result);
            Assert.AreSame(expectation, result);
            Assert.AreEqual(this.testPair.Key, expectation.id);

            this.logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            this.logger.VerifyNoOtherCalls();
        }
    }
}
