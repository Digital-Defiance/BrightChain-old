using System;
using System.Collections.Generic;
using System.IO;
using BrightChain.Engine.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests
{
    /// <summary>
    /// Test harness for cache managers. Inherit, implement, and run. It will excercise your cache.
    /// </summary>
    /// <typeparam name="Tcache"></typeparam>
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="Tvalue"></typeparam>
    [TestClass]
    public abstract class CacheManagerTest<Tcache, Tkey, Tvalue>
        where Tkey : IComparable<Tkey>
        where Tvalue : IComparable<Tvalue>
        where Tcache : ICacheManager<Tkey, Tvalue>
    {
        protected Mock<ILogger<Tcache>> logger;
        protected Mock<IConfiguration> configuration;
        protected Tcache cacheManager;

        /// <summary>
        /// Creates a new value type for test using the standard new(). Override to be more rigorous or for special cases.
        /// </summary>
        /// <returns></returns>
        abstract internal KeyValuePair<Tkey, Tvalue> NewKeyValue();

        /// <summary>
        /// Due to the generics/templates, we are not able to return null at compile time at this class level. The derived classes will have to just have a simple method that returns null.
        /// </summary>
        /// <returns></returns>
        abstract internal Tvalue NewNullData();

        /// <summary>
        /// Method for the derived tests to instantiate the cache manager for the tests with all the right options
        /// </summary>
        /// <returns></returns>
        abstract internal Tcache NewCacheManager(ILogger logger, IConfiguration configuration);

        /// <summary>
        /// Each test will generate a cache key to be set/get/etc
        /// </summary>
        protected KeyValuePair<Tkey, Tvalue> testPair;

        public CacheManagerTest()
        {
        }

        [TestInitialize]
        public void PreTestSetup()
        {
            this.logger = new Mock<ILogger<Tcache>>();
            this.configuration = new Mock<IConfiguration>();


            Mock<IConfigurationSection> mockPathSection = new Mock<IConfigurationSection>();
            mockPathSection.Setup(x => x.Value).Returns(Path.GetTempPath());

            var mockNodeSection = new Mock<IConfigurationSection>();
            mockNodeSection.Setup(x => x.GetSection(It.Is<string>(k => k == "BasePath"))).Returns(mockPathSection.Object);

            this.configuration.Setup(x => x.GetSection(It.Is<string>(k => k == "NodeOptions"))).Returns(mockNodeSection.Object);

            // the cache manager under test
            this.cacheManager = this.NewCacheManager(logger: this.logger.Object, configuration: this.configuration.Object);

            // a key to be used for each test
            this.testPair = this.NewKeyValue();
            Assert.IsFalse(this.cacheManager.Contains(this.testPair.Key));
            // at this point, the tests begin, knowing the key is not already in the cache
        }

        /// <summary>
        /// Create and push a non null object into the cache
        /// </summary>
        [TestMethod]
        public void ItPutsNonNullValuesTest()
        {
            // Arrange
            // generate a new value type
            // pre-setup

            // Act
            this.cacheManager.Set(this.testPair.Value);

            // Assert
            Assert.IsNotNull(this.testPair.Key);
            Assert.IsTrue(this.cacheManager.Contains(this.testPair.Key));
            this.logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            this.logger.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Push a null value into the cache
        /// </summary>
        [TestMethod]
        public virtual void ItPutsNullValuesTest()
        {
            // Arrange
            Tvalue newData = this.NewNullData();

            // Act
            this.cacheManager.Set(newData);

            // Assert
            Assert.IsNull(newData);
            Assert.IsTrue(this.cacheManager.Contains(this.testPair.Key));
            this.logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            this.logger.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Ensure a k/v placed into the cache emerge when a get request occurs
        /// </summary>
        [TestMethod]
        public void ItHitsTheCacheTest()
        {
            // Arrange
            var expectation = this.testPair.Value;
            this.cacheManager.Set(expectation);
            Assert.IsTrue(this.cacheManager.Contains(this.testPair.Key));

            // Act
            Tvalue result = this.cacheManager.Get(this.testPair.Key);

            // Assert
            Assert.IsNotNull(expectation);
            Assert.AreEqual(expectation, result);
            this.logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            this.logger.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Look for an item not already in the cache
        /// </summary>
        [TestMethod]
        public void ItMissesTheCacheTest()
        {
            // Arrange
            // none

            // Assert[/act]
            Assert.ThrowsException<IndexOutOfRangeException>(() =>
            {
                // Act
                Tvalue result = this.cacheManager.Get(this.testPair.Key);
            });
            this.logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            this.logger.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Make sure items are removed from the cache
        /// </summary>
        [TestMethod]
        public void ItDropsCachKeysTest()
        {
            // Arrange
            this.cacheManager.Set(this.testPair.Value);
            // verify that the key tests good before we drop
            Assert.IsTrue(this.cacheManager.Contains(this.testPair.Key));

            // Act
            this.cacheManager.Drop(this.testPair.Key);

            // Assert
            Assert.IsFalse(this.cacheManager.Contains(this.testPair.Key));
            this.logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            this.logger.VerifyNoOtherCalls();
        }

        /// <summary>
        /// ignored test since TTL is not yet implemented
        /// </summary>
        /// <returns></returns>
        [TestMethod, Ignore]
        public void ItExpiresCacheKeysTest()
        {
            var expectation = this.testPair.Value;
            this.cacheManager.Set(expectation);
            Assert.IsTrue(this.cacheManager.Contains(this.testPair.Key));
            // TODO: System.Threading.Thread.Sleep((cacheManager.TTL * 1000) + 1);
            Assert.IsFalse(this.cacheManager.Contains(this.testPair.Key));
            Assert.ThrowsException<KeyNotFoundException>(() =>
            {
                Tvalue result = this.cacheManager.Get(this.testPair.Key);
                Assert.IsNull(result);
            });
            this.logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(4));
            this.logger.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void ItVerifiesCacheDataIntegrityTest()
        {
            // Arrange
            var expectation = this.testPair.Value;
            this.cacheManager.Set(expectation);

            // Act
            Tvalue result = this.cacheManager.Get(this.testPair.Key);

            // Assert
            Assert.IsNotNull(expectation);
            Assert.AreEqual(expectation, result);
        }
    }
}
