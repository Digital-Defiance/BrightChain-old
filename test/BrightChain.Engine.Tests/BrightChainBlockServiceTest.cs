namespace BrightChain.Engine.Tests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using BrightChain.Engine.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using static BrightChain.Engine.Helpers.Utilities;

    /// <summary>
    /// Exercises the core API service
    /// </summary>
    [TestClass]
    public class BrightChainBlockServiceTest
    {
        private ILoggerFactory _loggerFactory;
        private IConfiguration _configuration;
        private IServiceCollection _services;
        private ILogger _logger;

        [TestInitialize]
        public void PreTestSetup()
        {
            var mockConfiguration = new Mock<IConfiguration>();

            this._configuration = mockConfiguration.Object;
            this._services = new Mock<IServiceCollection>().Object;
            this._logger = new Mock<ILogger>().Object;

            Mock<IConfigurationSection> mockPathSection = new Mock<IConfigurationSection>();
            mockPathSection.Setup(x => x.Value).Returns(Path.GetTempPath());

            var mockNodeSection = new Mock<IConfigurationSection>();
            mockNodeSection.Setup(x => x.GetSection(It.Is<string>(k => k == "BasePath"))).Returns(mockPathSection.Object);

            mockConfiguration.Setup(x => x.GetSection(It.Is<string>(k => k == "NodeOptions"))).Returns(mockNodeSection.Object);

            var factoryMock = new Mock<ILoggerFactory>();

            factoryMock
                .SetupAllProperties()
                .Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(this._logger);

            this._loggerFactory = factoryMock.Object;
        }

        [TestMethod]
        public void ItInitializesTest()
        {
            var loggerMock = Mock.Get(this._logger);

            var brightChainService = new BrightBlockService(
                logger: this._loggerFactory,
                configuration: this._configuration);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(2));
            loggerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// TODO: move to BlockHashTests.
        /// </summary>
        [DataTestMethod]
        [DataRow(BlockSize.Message)]
        [DataRow(BlockSize.Tiny)]
        [DataRow(BlockSize.Small)]
        [DataRow(BlockSize.Medium)]
        [DataRow(BlockSize.Large)]
        public void ItHasCorrectHashSizesTest(BlockSize blockSize)
        {
            var expectedVector = GetZeroVector(blockSize);
            BlockHash zeroVector;
            GenerateZeroVectorAndVerify(blockSize, out zeroVector);
            Assert.IsNotNull(zeroVector);
            Assert.AreEqual(expectedVector.ToString(), zeroVector.ToString());
        }

        [DataTestMethod]
        [DataRow(BlockSize.Message)]
        [DataRow(BlockSize.Tiny)]
        [DataRow(BlockSize.Small)]
        [DataRow(BlockSize.Medium)]
        [DataRow(BlockSize.Large)]

        public async Task ItBrightensBlocksAndCreatesCblsTest(BlockSize blockSize)
        {
            var loggerMock = Mock.Get(this._logger);

            var brightChainService = new BrightBlockService(
                logger: this._loggerFactory,
                configuration: this._configuration);

            var sourceInfo = RandomDataHelper.GenerateRandomFile(
                blockSize: blockSize,
                lengthFunc: (BlockSize blockSize) =>
                    (BlockSizeMap.BlockSize(blockSize) * 2) + 7); // don't land on even block mark for data testing

            ConstituentBlockListBlock cblBlock = await brightChainService.MakeCblOrSuperCblFromFileAsync(
                fileName: sourceInfo.FileInfo.FullName,
                blockParams: new BlockParams(
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: Enumerations.RedundancyContractType.HeapAuto,
                    privateEncrypted: false,
                    blockSize: blockSize,
                    originalType: typeof(ConstituentBlockListBlock)));

            if (cblBlock is SuperConstituentBlockListBlock)
            {
                foreach (var blockHash in cblBlock.ConstituentBlocks)
                {
                    var cbl = (ConstituentBlockListBlock)await brightChainService
                        .FindBlockByIdAsync(blockHash)
                            .ConfigureAwait(false);

                    Assert.IsTrue(cbl.Validate());
                    Assert.AreEqual(sourceInfo.FileInfo.Length, cbl.TotalLength);
                    Assert.AreEqual(
                        HashToFormattedString(sourceInfo.SourceId.HashBytes.ToArray()),
                        HashToFormattedString(cbl.SourceId.HashBytes.ToArray()));

                    var cblMap = cbl.GenerateBlockMap();
                    Assert.IsTrue(cblMap is BlockChainFileMap);
                }
            }
            else
            {
                Assert.IsTrue(cblBlock.Validate());
                Assert.AreEqual(sourceInfo.FileInfo.Length, cblBlock.TotalLength);
                Assert.AreEqual(
                    HashToFormattedString(sourceInfo.SourceId.HashBytes.ToArray()),
                    HashToFormattedString(cblBlock.SourceId.HashBytes.ToArray()));

                var cblMap = cblBlock.GenerateBlockMap();
                Assert.IsTrue(cblMap is BlockChainFileMap);
            }

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(2));
            loggerMock.VerifyNoOtherCalls();

            await brightChainService.PersistMemoryCacheAsync(true);
        }

        [DataTestMethod]
        [DataRow(BlockSize.Message)]
        [DataRow(BlockSize.Tiny)]
        [DataRow(BlockSize.Small)]
        [DataRow(BlockSize.Medium)]
        [DataRow(BlockSize.Large)]
        public async Task ItReadsCBLsBackToDisk(BlockSize blockSize)
        {
            var loggerMock = Mock.Get(this._logger);

            var brightChainService = new BrightBlockService(
                logger: this._loggerFactory,
                configuration: this._configuration);

            var sourceInfo = RandomDataHelper.GenerateRandomFile(
                blockSize: blockSize,
                lengthFunc: (BlockSize blockSize) =>
                    (BlockSizeMap.BlockSize(blockSize) * 2) + 7); // don't land on even block mark for data testing

            ConstituentBlockListBlock cblBlock = await brightChainService.MakeCblOrSuperCblFromFileAsync(
                fileName: sourceInfo.FileInfo.FullName,
                blockParams: new BlockParams(
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: Enumerations.RedundancyContractType.HeapAuto,
                    privateEncrypted: false,
                    blockSize: blockSize,
                    originalType: typeof(ConstituentBlockListBlock)));

            var restoredFile = await brightChainService.RestoreFileFromCBLAsync(cblBlock);

            Assert.AreEqual(
                HashToFormattedString(sourceInfo.SourceId.HashBytes.ToArray()),
                HashToFormattedString(restoredFile.SourceId.HashBytes.ToArray()));

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(2));
            loggerMock.VerifyNoOtherCalls();

            await brightChainService.PersistMemoryCacheAsync(true);
        }
    }
}
