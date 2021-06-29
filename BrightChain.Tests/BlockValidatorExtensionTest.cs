using BrightChain.Models.Blocks;
using BrightChain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace BrightChain.Tests
{
    [TestClass]
    public class BlockValidatorExtensionTest
    {
        protected BlockCacheManager CacheManager { get; set; }
        protected ILogger logger { get; set; }

        [TestInitialize]
        public void PreTestSetUp()
        {
            this.logger = new Mock<ILogger<BlockCacheManager>>().Object;
            this.CacheManager = new MemoryBlockCacheManager(
                logger: this.logger);
        }

        [TestMethod]
        public void ItValidatesValidBlocksTest()
        {
            Assert.IsTrue((new RandomizerBlock(
                pregeneratedRandomizerCache: this.CacheManager,
                blockSize: Enumerations.BlockSize.Message,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.MaxValue,
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: false)).Validate());

            var loggerMock = Mock.Get(this.logger);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(3));
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
