namespace BrightChain.Engine.Tests
{
    using System;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Services.CacheManagers;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BlockValidatorExtensionsTest
    {
        protected ILogger logger { get; set; }

        [TestInitialize]
        public void PreTestSetUp()
        {
            this.logger = new Mock<ILogger<BrightenedBlockCacheManagerBase>>().Object;
        }

        [DataTestMethod]
        [DataRow(BlockSize.Nano)]
        [DataRow(BlockSize.Micro)]
        [DataRow(BlockSize.Message)]
        [DataRow(BlockSize.Tiny)]
        [DataRow(BlockSize.Small)]
        [DataRow(BlockSize.Medium)]
        [DataRow(BlockSize.Large)]
        public void ItValidatesValidBlocksTest(BlockSize blockSize)
        {
            Assert.IsTrue(new ZeroVectorBlock(
                blockParams: new BlockParams(
                    blockSize: blockSize,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: Enumerations.RedundancyContractType.HeapAuto,
                    privateEncrypted: false,
                    originalType: typeof(ZeroVectorBlock)))
                .Validate());

            var loggerMock = Mock.Get(this.logger);
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
