using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Faster.CacheManager;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.Chains;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services;
using BrightChain.Engine.Services.CacheManagers.Block;
using BrightChain.Engine.Tests.TestModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests;

[TestClass]
public class ChainLinqDataBlockTest
{
    protected BrightenedBlockCacheManagerBase cacheManager;
    protected Mock<IConfiguration> configuration;
    protected Mock<ILogger<BrightenedBlockCacheManagerBase>> logger;
    protected Mock<ILoggerFactory> loggerFactory;

    [TestInitialize]
    public void PreTestSetup()
    {
        this.logger = new Mock<ILogger<BrightenedBlockCacheManagerBase>>();
        this.configuration = new Mock<IConfiguration>();

        var factoryMock = new Mock<ILoggerFactory>();

        factoryMock
            .SetupAllProperties()
            .Setup(expression: f => f.CreateLogger(It.IsAny<string>())).Returns(value: this.logger.Object);

        this.loggerFactory = factoryMock;

        var mockPathSection = new Mock<IConfigurationSection>();
        mockPathSection.Setup(expression: x => x.Value).Returns(value: Path.GetTempPath());

        var mockNodeSection = new Mock<IConfigurationSection>();
        mockNodeSection.Setup(expression: x => x.GetSection(It.Is<string>(k => k == "BasePath"))).Returns(value: mockPathSection.Object);

        this.configuration.Setup(expression: x => x.GetSection(It.Is<string>(k => k == "NodeOptions")))
            .Returns(value: mockNodeSection.Object);

        var rootBlock = new RootBlock(databaseGuid: Guid.NewGuid(),
            blockSize: BlockSize.Large);
        this.cacheManager = new FasterBlockCacheManager(
            logger: this.logger.Object,
            configuration: this.configuration.Object,
            rootBlock: rootBlock,
            testingSelfDestruct: true);
    }

    public async Task<Models.Blocks.Chains.BrightChain> ForgeChainAsync(BrightBlockService brightBlockService, BlockSize blockSize,
        int objectCount)
    {
        var requestParams = new BlockParams(
            blockSize: blockSize,
            requestTime: DateTime.Now,
            keepUntilAtLeast: DateTime.MaxValue,
            redundancy: RedundancyContractType.HeapAuto,
            privateEncrypted: false,
            originalType: typeof(ChainLinqObjectBlock<ChainLinqExampleSerializable>));

        var datas = ChainLinqExampleSerializable.MakeMultiple(count: objectCount);
        Assert.AreEqual(expected: objectCount,
            actual: datas.Count());

        var chainLinq = ChainLinq<ChainLinqExampleSerializable>.ForgeChainLinq(
            blockParams: requestParams,
            objects: datas);
        Assert.AreEqual(expected: objectCount,
            actual: chainLinq.Count());

        var brightChain = await chainLinq.BrightenAllAsync(
            brightBlockService: brightBlockService);

        return brightChain;
    }

    [DataTestMethod]
    //[DataRow(BlockSize.Nano, 2)]
    //[DataRow(BlockSize.Micro, 2)]
    [DataRow(data1: BlockSize.Message,
        4)]
    [DataRow(data1: BlockSize.Tiny,
        4)]
    [DataRow(data1: BlockSize.Small,
        4)]
    [DataRow(data1: BlockSize.Medium,
        4)]
    [DataRow(data1: BlockSize.Large,
        4)]
    public async Task ItSavesDataCorrectlyTest(BlockSize blockSize, int objectCount)
    {
        var brightBlockService = new BrightBlockService(
            logger: this.loggerFactory.Object,
            configuration: this.configuration.Object);

        var brightChain = await this.ForgeChainAsync(
            brightBlockService: brightBlockService,
            blockSize: blockSize,
            objectCount: objectCount);

        Assert.AreEqual(expected: brightChain.ConstituentBlocks.Count(),
            actual: brightChain.Count());

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(action: async () =>
        {
            var retrievedChainNull = await brightBlockService.FindBlockByIdAsync(id: brightChain.Id);
            Assert.IsNull(value: retrievedChainNull);
        });

        var brightHandle = brightBlockService.BrightenCbl(cblBlock: brightChain,
            persist: true,
            brightenedCbl: out var brightenedCbl);

        // in order to get our original blocks back, first get the chain head
        var retrievedChain = await brightBlockService.FindBlockByIdAsync(id: brightHandle.BrightenedCblHash);
        Assert.IsNotNull(value: retrievedChain);

        var retrievedHandle = brightBlockService.FindSourceById(requestedHash: brightChain.SourceId);
        Assert.IsNotNull(value: retrievedHandle);
    }

    [DataTestMethod]
    //[DataRow(BlockSize.Nano, 2)]
    //[DataRow(BlockSize.Micro, 2)]
    [DataRow(data1: BlockSize.Message,
        4)]
    [DataRow(data1: BlockSize.Tiny,
        4)]
    [DataRow(data1: BlockSize.Small,
        4)]
    [DataRow(data1: BlockSize.Medium,
        4)]
    [DataRow(data1: BlockSize.Large,
        4)]
    public async Task ItLoadsDataCorrectlyTest(BlockSize blockSize, int objectCount)
    {
        var brightBlockService = new BrightBlockService(
            logger: this.loggerFactory.Object,
            configuration: this.configuration.Object);

        var brightChain = await this.ForgeChainAsync(
            brightBlockService: brightBlockService,
            blockSize: blockSize,
            objectCount: objectCount);

        Assert.AreEqual(expected: brightChain.ConstituentBlocks.Count(),
            actual: brightChain.Count());

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(action: async () =>
        {
            var retrievedChainNull = await brightBlockService.FindBlockByIdAsync(id: brightChain.Id);
            Assert.IsNull(value: retrievedChainNull);
        });

        var brightHandle = brightBlockService.BrightenCbl(cblBlock: brightChain,
            persist: true,
            brightenedCbl: out var brightenedCbl);

        // in order to get our original blocks back, first get the chain head
        var retrievedChain = await brightBlockService.FindBlockByIdAsync(id: brightenedCbl.Id);
        Assert.IsNotNull(value: retrievedChain);

        Assert.AreEqual(expected: brightenedCbl.Crc32,
            actual: retrievedChain.Crc32);
    }
}
