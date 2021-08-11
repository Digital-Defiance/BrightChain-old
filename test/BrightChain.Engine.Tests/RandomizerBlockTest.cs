using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services.CacheManagers;
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
        private MemoryDictionaryBlockCacheManager cacheManager;
        private ILogger logger;

        public RandomizerBlockTest()
        {
        }

        [TestInitialize]
        public void PreTestSetUp()
        {
            this.logger = new Moq.Mock<ILogger>().Object;
            var rootBlock = new RootBlock(databaseGuid: Guid.NewGuid(), blockSize: BlockSize.Large);
            this.cacheManager = new MemoryDictionaryBlockCacheManager(
                logger: this.logger,
                configuration: new Configuration(),
                rootBlock: rootBlock);
        }

        [DataTestMethod]
        [DataRow(BlockSize.Message)]
        [DataRow(BlockSize.Tiny)]
        [DataRow(BlockSize.Small)]
        [DataRow(BlockSize.Medium)]
        [DataRow(BlockSize.Large)]
        public void ItCreatesValidRandomDataBlocksTest(BlockSize blockSize)
        {
            var block = new RandomizerBlock(
                new TransactableBlockParams(
                    cacheManager: this.cacheManager,
                    allowCommit: true,
                    blockParams: new BlockParams(
                        blockSize: blockSize,
                        requestTime: DateTime.Now,
                        keepUntilAtLeast: DateTime.Now.AddDays(1),
                        redundancy: Enumerations.RedundancyContractType.HeapAuto,
                        privateEncrypted: false,
                        originalType: typeof(RandomizerBlock))));

            Assert.IsTrue(block.Validate());
            Assert.IsTrue(this.cacheManager.Contains(block.Id));

            // TODO verify not all zeros, some level of randomness

            var mockLogger = Mock.Get(this.logger);
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
