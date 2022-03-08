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

namespace BrightChain.Engine.Tests;

using static Utilities;

/// <summary>
///     Exercises the core API service
/// </summary>
[TestClass]
public class BrightChainBlockServiceTest
{
    private IConfiguration _configuration;
    private ILogger _logger;
    private ILoggerFactory _loggerFactory;
    private IServiceCollection _services;

    [TestInitialize]
    public void PreTestSetup()
    {
        var mockConfiguration = new Mock<IConfiguration>();

        this._configuration = mockConfiguration.Object;
        this._services = new Mock<IServiceCollection>().Object;
        this._logger = new Mock<ILogger>().Object;

        var mockPathSection = new Mock<IConfigurationSection>();
        mockPathSection.Setup(expression: x => x.Value).Returns(value: Path.GetTempPath());

        var mockNodeSection = new Mock<IConfigurationSection>();
        mockNodeSection.Setup(expression: x => x.GetSection(It.Is<string>(k => k == "BasePath"))).Returns(value: mockPathSection.Object);

        mockConfiguration.Setup(expression: x => x.GetSection(It.Is<string>(k => k == "NodeOptions")))
            .Returns(value: mockNodeSection.Object);

        var factoryMock = new Mock<ILoggerFactory>();

        factoryMock
            .SetupAllProperties()
            .Setup(expression: f => f.CreateLogger(It.IsAny<string>())).Returns(value: this._logger);

        this._loggerFactory = factoryMock.Object;
    }

    [TestMethod]
    public void ItInitializesTest()
    {
        var loggerMock = Mock.Get(mocked: this._logger);

        var brightChainService = new BrightBlockService(
            logger: this._loggerFactory,
            configuration: this._configuration);

        loggerMock.Verify(expression: l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            times: Times.Exactly(callCount: 2));
        loggerMock.VerifyNoOtherCalls();
    }

    /// <summary>
    ///     TODO: move to BlockHashTests.
    /// </summary>
    [DataTestMethod]
    //[DataRow(BlockSize.Nano)]
    //[DataRow(BlockSize.Micro)]
    [DataRow(data1: BlockSize.Message)]
    [DataRow(data1: BlockSize.Tiny)]
    [DataRow(data1: BlockSize.Small)]
    [DataRow(data1: BlockSize.Medium)]
    [DataRow(data1: BlockSize.Large)]
    public void ItHasCorrectHashSizesTest(BlockSize blockSize)
    {
        var expectedVector = BlockSizeMap.ZeroVectorHash(blockSize: blockSize);
        BlockHash zeroVector;
        GenerateZeroVectorAndVerify(blockSize: blockSize,
            blockHash: out zeroVector);
        Assert.IsNotNull(value: zeroVector);
        Assert.AreEqual(expected: expectedVector.ToString(),
            actual: zeroVector.ToString());
    }

    [DataTestMethod]
    //[DataRow(BlockSize.Nano)]
    //[DataRow(BlockSize.Micro)]
    [DataRow(data1: BlockSize.Message)]
    [DataRow(data1: BlockSize.Tiny)]
    [DataRow(data1: BlockSize.Small)]
    [DataRow(data1: BlockSize.Medium)]
    [DataRow(data1: BlockSize.Large)]
    public async Task ItBrightensBlocksAndCreatesCblsTest(BlockSize blockSize)
    {
        var loggerMock = Mock.Get(mocked: this._logger);

        var brightChainService = new BrightBlockService(
            logger: this._loggerFactory,
            configuration: this._configuration);

        var sourceInfo = RandomDataHelper.GenerateRandomFile(
            blockSize: blockSize,
            lengthFunc: blockSize =>
                (BlockSizeMap.BlockSize(blockSize: blockSize) * 2) + 7); // don't land on even block mark for data testing

        var brightenedCbl = await brightChainService.MakeCblOrSuperCblFromFileAsync(
            fileName: sourceInfo.FileInfo.FullName,
            blockParams: new BlockParams(
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.MaxValue,
                redundancy: RedundancyContractType.HeapAuto,
                privateEncrypted: false,
                blockSize: blockSize,
                originalType: typeof(ConstituentBlockListBlock)));

        if (brightenedCbl is SuperConstituentBlockListBlock)
        {
            throw new NotImplementedException();
            foreach (var blockHash in brightenedCbl.ConstituentBlocks)
            {
                var fetchedBlock = await brightChainService
                    .FindBlockByIdAsync(id: blockHash)
                    .ConfigureAwait(continueOnCapturedContext: false);
                var cbl = (SuperConstituentBlockListBlock)fetchedBlock.AsBlock;
                Assert.IsTrue(condition: cbl.Validate());
                Assert.AreEqual(expected: sourceInfo.FileInfo.Length,
                    actual: cbl.TotalLength);
                Assert.AreEqual(
                    expected: NeuralFabric.Helpers.Utilities.HashToFormattedString(hashBytes: sourceInfo.SourceId.HashBytes.ToArray()),
                    actual: NeuralFabric.Helpers.Utilities.HashToFormattedString(hashBytes: cbl.SourceId.HashBytes.ToArray()));

                var cblMap = cbl.CreateBrightMap();
                Assert.IsTrue(condition: cblMap is BrightMap);
            }
        }

        {
            Assert.IsTrue(condition: brightenedCbl.Validate());
            Assert.AreEqual(expected: sourceInfo.FileInfo.Length,
                actual: brightenedCbl.TotalLength);
            Assert.AreEqual(
                expected: NeuralFabric.Helpers.Utilities.HashToFormattedString(hashBytes: sourceInfo.SourceId.HashBytes.ToArray()),
                actual: NeuralFabric.Helpers.Utilities.HashToFormattedString(hashBytes: brightenedCbl.SourceId.HashBytes.ToArray()));

            var cblMap = brightenedCbl.CreateBrightMap();
            Assert.IsTrue(condition: cblMap is BrightMap);
        }

        loggerMock.Verify(expression: l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            times: Times.Exactly(callCount: 2));
        loggerMock.VerifyNoOtherCalls();
    }

    [DataTestMethod]
    //[DataRow(BlockSize.Nano)]
    //[DataRow(BlockSize.Micro)]
    [DataRow(data1: BlockSize.Message)]
    [DataRow(data1: BlockSize.Tiny)]
    [DataRow(data1: BlockSize.Small)]
    [DataRow(data1: BlockSize.Medium)]
    [DataRow(data1: BlockSize.Large)]
    public async Task ItReadsCBLsBackToDisk(BlockSize blockSize)
    {
        var loggerMock = Mock.Get(mocked: this._logger);

        var brightChainService = new BrightBlockService(
            logger: this._loggerFactory,
            configuration: this._configuration);

        var sourceInfo = RandomDataHelper.GenerateRandomFile(
            blockSize: blockSize,
            lengthFunc: blockSize =>
                (BlockSizeMap.BlockSize(blockSize: blockSize) * 2) + 7); // don't land on even block mark for data testing

        ConstituentBlockListBlock cblBlock = await brightChainService.MakeCblOrSuperCblFromFileAsync(
            fileName: sourceInfo.FileInfo.FullName,
            blockParams: new BlockParams(
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.MaxValue,
                redundancy: RedundancyContractType.HeapAuto,
                privateEncrypted: false,
                blockSize: blockSize,
                originalType: typeof(ConstituentBlockListBlock)));

        var restoredFile = await brightChainService.RestoreFileFromCBLAsync(constituentBlockListBlock: cblBlock);

        Assert.AreEqual(
            expected: NeuralFabric.Helpers.Utilities.HashToFormattedString(hashBytes: sourceInfo.SourceId.HashBytes.ToArray()),
            actual: NeuralFabric.Helpers.Utilities.HashToFormattedString(hashBytes: restoredFile.SourceId.HashBytes.ToArray()));

        loggerMock.Verify(expression: l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            times: Times.Exactly(callCount: 2));
        loggerMock.VerifyNoOtherCalls();
    }
}
