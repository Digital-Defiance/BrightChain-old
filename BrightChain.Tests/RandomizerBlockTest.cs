using BrightChain.Enumerations;
using BrightChain.Models.Blocks;
using BrightChain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace BrightChain.Tests
{
    /// <summary>
    /// Verifies random blocks are random, generated correctly and are inserted into the cache
    /// </summary>
    [TestClass]
    public class RandomizerBlockTest
    {
        protected MemoryBlockCacheManager cacheManager;
        protected ILogger logger;

        public RandomizerBlockTest()
        {
        }

        [TestInitialize]
        public void PreTestSetUp()
        {
            this.logger = new Moq.Mock<ILogger>().Object;
            this.cacheManager = new MemoryBlockCacheManager(logger: this.logger);
        }

        [TestMethod]
        public void ItCreatesValidRandomDataBlocksTest()
        {
            var block = new RandomizerBlock(
                pregeneratedRandomizerCache: this.cacheManager,
                blockSize: BlockSize.Message,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: true);

            Assert.IsTrue(block.Validate());
            Assert.IsTrue(this.cacheManager.Contains(block.Id));

            // TODO verify not all zeros, some level of randomness

            var mockLogger = Mock.Get(this.logger);
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(3));
            mockLogger.VerifyNoOtherCalls();
        }
    }
}
