using BrightChain.Enumerations;
using BrightChain.Models.Blocks;
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
    public class RandomDataBlockTest
    {
        protected ILogger logger;

        public RandomDataBlockTest()
        {
        }

        [TestInitialize]
        public void PreTestSetUp() => this.logger = new Moq.Mock<ILogger>().Object;

        [TestMethod]
        public void ItCreatesValidRandomDataBlocksTest()
        {
            var block = new RandomDataBlock(
                blockSize: BlockSize.Message,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto);

            Assert.IsTrue(block.Validate());

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
