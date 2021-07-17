using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests
{
    /// <summary>
    /// Verifies random blocks are random, generated correctly and are inserted into the cache
    /// </summary>
    [TestClass]
    public class RandomizerBlockTest
    {
        private MemoryBlockCacheManager cacheManager;
        private ILogger logger;

        public RandomizerBlockTest()
        {
        }

        [TestInitialize]
        public void PreTestSetUp()
        {
            logger = new Moq.Mock<ILogger>().Object;
            cacheManager = new MemoryBlockCacheManager(logger: logger, configuration: new Configuration());
        }

        [TestMethod]
        public void ItCreatesValidRandomDataBlocksTest()
        {
            var block = new RandomizerBlock(
                new TransactableBlockParams(
                    cacheManager: cacheManager,
                    allowCommit: true,
                    blockParams: new BlockParams(
                        blockSize: BlockSize.Message,
                        requestTime: DateTime.Now,
                        keepUntilAtLeast: DateTime.Now.AddDays(1),
                        redundancy: Enumerations.RedundancyContractType.HeapAuto,
                        privateEncrypted: false)));

            Assert.IsTrue(block.Validate());
            Assert.IsTrue(this.cacheManager.Contains(block.Id));

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
