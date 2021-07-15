using BrightChain.Enumerations;
using BrightChain.Models.Blocks;
using BrightChain.Models.Blocks.DataObjects;
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
            logger = new Moq.Mock<ILogger>().Object;
            cacheManager = new MemoryBlockCacheManager(logger: logger);
        }

        [TestMethod]
        public void ItCreatesValidRandomDataBlocksTest()
        {
            var block = new RandomizerBlock(
                new TransactableBlockParams(
                cacheManager: cacheManager,
                blockArguments: new BlockParams(
                    blockSize: BlockSize.Message,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.Now.AddDays(1),
                    redundancy: Enumerations.RedundancyContractType.HeapAuto,
                    allowCommit: true,
                    privateEncrypted: false)));

            Assert.IsTrue(block.Validate());
            Assert.IsTrue(cacheManager.Contains(block.Id));

            // TODO verify not all zeros, some level of randomness

            var mockLogger = Mock.Get(logger);
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            mockLogger.VerifyNoOtherCalls();
        }
    }
}
