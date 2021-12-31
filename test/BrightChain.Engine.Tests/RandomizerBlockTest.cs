namespace BrightChain.Engine.Tests
{
    using System;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Models;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Services.CacheManagers.Block;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

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
                configuration: new BrightChainConfiguration(),
                rootBlock: rootBlock);
        }

        [DataTestMethod]
        [DataRow(BlockSize.Nano)]
        [DataRow(BlockSize.Micro)]
        [DataRow(BlockSize.Message)]
        [DataRow(BlockSize.Tiny)]
        [DataRow(BlockSize.Small)]
        [DataRow(BlockSize.Medium)]
        [DataRow(BlockSize.Large)]
        public void ItCreatesValidRandomDataBlocksTest(BlockSize blockSize)
        {
            var zeroBlock = new ZeroVectorBlock(
                blockParams: new BlockParams(
                blockSize: blockSize,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                privateEncrypted: false,
                originalType: typeof(ZeroVectorBlock)));

            Assert.IsTrue(zeroBlock.Validate());

            var randomBlock = new RandomizerBlock(
                destinationCache: this.cacheManager,
                blockSize: blockSize,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancyContractType: Enumerations.RedundancyContractType.HeapAuto);

            Assert.IsTrue(randomBlock.Validate());
            Assert.IsFalse(this.cacheManager.Contains(randomBlock.Id));

            var zeroBlockEntResult = zeroBlock.EntropyEstimate;
            Assert.AreEqual(0, zeroBlockEntResult.Entropy);

            var randomBlockEntResult = randomBlock.EntropyEstimate;
            Assert.IsTrue(randomBlockEntResult.Entropy > 6.0D);

            var mockLogger = Mock.Get(this.logger);
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Exactly(0));
            mockLogger.VerifyNoOtherCalls();
        }
    }
}
