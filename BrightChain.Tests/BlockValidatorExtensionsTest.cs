using BrightChain.Models.Blocks;
using BrightChain.Models.Blocks.DataObjects;
using BrightChain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace BrightChain.Tests
{
    [TestClass]
    public class BlockValidatorExtensionsTest
    {
        protected ILogger logger { get; set; }

        [TestInitialize]
        public void PreTestSetUp() => this.logger = new Mock<ILogger<BlockCacheManager>>().Object;

        [TestMethod]
        public void ItValidatesValidBlocksTest()
        {
            Assert.IsTrue((new RandomDataBlock(
                blockArguments: new BlockParams(
                blockSize: Enumerations.BlockSize.Message,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.MaxValue,
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: true,
                privateEncrypted: false))).Validate());

            var loggerMock = Mock.Get(this.logger);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            loggerMock.VerifyNoOtherCalls();
        }

        [TestMethod, Ignore]
        public void ItValidatesUnknownBlockSizeTest() =>
            throw new NotImplementedException();

        [TestMethod, Ignore]
        public void ItValidatesBlockSizeMatchesDataSizeTest() =>
            throw new NotImplementedException();

        [TestMethod, Ignore]
        public void ItValidatesBlockHashMatchesDataHashTest() =>
            throw new NotImplementedException();

        [TestMethod, Ignore]
        public void ItValidatesStorageContractDataLengthTest() =>
            throw new NotImplementedException();

        [TestMethod, Ignore]
        public void ItValidatesStorageContractMatchesRedundancyContractTest() =>
            throw new NotImplementedException();
    }
}
