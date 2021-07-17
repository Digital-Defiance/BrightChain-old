using System;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests
{
    [TestClass]
    public class BlockValidatorExtensionsTest
    {
        protected ILogger logger { get; set; }

        [TestInitialize]
        public void PreTestSetUp()
        {
            logger = new Mock<ILogger<BlockCacheManager>>().Object;
        }

        [TestMethod]
        public void ItValidatesValidBlocksTest()
        {
            Assert.IsTrue(new RandomDataBlock(
                blockParams: new BlockParams(
                    blockSize: Enumerations.BlockSize.Message,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: Enumerations.RedundancyContractType.HeapAuto,
                    privateEncrypted: false))
                .Validate());

            var loggerMock = Mock.Get(logger);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Exactly(0));
            loggerMock.VerifyNoOtherCalls();
        }

        [TestMethod, Ignore]
        public void ItValidatesUnknownBlockSizeTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod, Ignore]
        public void ItValidatesBlockSizeMatchesDataSizeTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod, Ignore]
        public void ItValidatesBlockHashMatchesBlockHashTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod, Ignore]
        public void ItValidatesStorageContractDataLengthTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod, Ignore]
        public void ItValidatesStorageContractMatchesRedundancyContractTest()
        {
            throw new NotImplementedException();
        }
    }
}
