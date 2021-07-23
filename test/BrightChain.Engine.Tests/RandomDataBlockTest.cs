using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests
{
    /// <summary>
    /// Verifies random blocks are random, generated correctly and are inserted into the cache
    /// </summary>
    [TestClass]
    public class RandomDataBlockTest
    {
        protected ILogger logger;

        public RandomDataBlockTest()
        {
        }

        [TestInitialize]
        public void PreTestSetUp()
        {
            logger = new Moq.Mock<ILogger>().Object;
        }

        [DataTestMethod]
        [DataRow(BlockSize.Message)]
        [DataRow(BlockSize.Tiny)]
        [DataRow(BlockSize.Small)]
        [DataRow(BlockSize.Medium)]
        [DataRow(BlockSize.Large)]
        public void ItCreatesValidRandomDataBlocksTest(BlockSize blockSize)
        {
            var block = new RandomDataBlock(
                blockParams: new BlockParams(
                blockSize: blockSize,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                privateEncrypted: false));

            Assert.IsTrue(block.Validate());

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
