using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.Chains;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Contracts;
using BrightChain.Engine.Models.Hashes;
using BrightChain.Engine.Services;
using BrightChain.Engine.Services.CacheManagers.Block;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NeuralFabric.Models.Hashes;

namespace BrightChain.Engine.Tests.Services;

[TestClass]
public class BrightBlockServiceTests
{
    private Mock<IConfiguration> mockConfiguration;

    private Mock<ILoggerFactory> mockLoggerFactory;
    private MockRepository mockRepository;

    [TestInitialize]
    public void TestInitialize()
    {
        this.mockRepository = new MockRepository(defaultBehavior: MockBehavior.Strict);

        this.mockLoggerFactory = this.mockRepository.Create<ILoggerFactory>();
        this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
    }

    private BrightBlockService CreateService()
    {
        return new BrightBlockService(
            logger: this.mockLoggerFactory.Object,
            configuration: this.mockConfiguration.Object);
    }

    [TestMethod]
    public async Task StreamCreatedBrightenedBlocksFromFileAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        var sourceInfo = default(SourceFileInfo);
        BlockParams blockParams = null;
        BlockSize? blockSize = null;

        // Act
        await foreach (var brightenedBlock in service.StreamCreatedBrightenedBlocksFromFileAsync(
                           sourceInfo: sourceInfo,
                           blockParams: blockParams,
                           blockSize: blockSize))
        {
        }

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task MakeCBLChainFromParamsAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        string fileName = null;
        BlockParams blockParams = null;

        // Act
        var result = await service.MakeCBLChainFromParamsAsync(
            fileName: fileName,
            blockParams: blockParams);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task MakeSuperCBLFromCBLChainAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        BlockParams blockParams = null;
        IEnumerable<ConstituentBlockListBlock> chainedCbls = null;
        DataHash sourceId = null;

        // Act
        var result = await service.MakeSuperCBLFromCBLChainAsync(
            blockParams: blockParams,
            chainedCbls: chainedCbls,
            sourceId: sourceId);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task MakeCblOrSuperCblFromFileAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        string fileName = null;
        BlockParams blockParams = null;

        // Act
        var result = await service.MakeCblOrSuperCblFromFileAsync(
            fileName: fileName,
            blockParams: blockParams);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public void GetCBLBlocksFromCacheAsDictionary_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        BrightenedBlockCacheManagerBase blockCacheManager = null;
        ConstituentBlockListBlock block = null;

        // Act
        var result = BrightBlockService.GetCBLBlocksFromCacheAsDictionary(
            blockCacheManager: blockCacheManager,
            block: block);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task RestoreStreamFromCBLAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        ConstituentBlockListBlock constituentBlockListBlock = null;
        Stream? destination = null;

        // Act
        var result = await service.RestoreStreamFromCBLAsync(
            constituentBlockListBlock: constituentBlockListBlock,
            destination: destination);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task RestoreFileFromCBLAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        ConstituentBlockListBlock constituentBlockListBlock = null;

        // Act
        var result = await service.RestoreFileFromCBLAsync(
            constituentBlockListBlock: constituentBlockListBlock);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task FindBlockByIdAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        BlockHash id = null;

        // Act
        var result = await service.FindBlockByIdAsync(
            id: id);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task FindBlocksByIdAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        IAsyncEnumerable<BlockHash> blockIdSource = null;

        // Act
        await foreach (var block in service.FindBlocksByIdAsync(
                           blockIdSource: blockIdSource))
        {
        }

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task FindBlockByIdAsync_StateUnderTest_ExpectedBehavior1()
    {
        // Arrange
        var service = this.CreateService();
        BlockHash id = null;
        var useAsBlock = false;

        // Act
        var result = await service.FindBlockByIdAsync<RestorableBlock>(
            id: id,
            useAsBlock: useAsBlock);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task DropBlockByIdAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        BlockHash id = null;
        RevocationCertificate? ownershipToken = null;

        // Act
        var result = await service.DropBlockByIdAsync(
            id: id,
            ownershipToken: ownershipToken);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task DropBlocksByIdAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        IAsyncEnumerable<BlockHash> idSource = null;
        RevocationCertificate? ownershipToken = null;

        // Act
        await foreach ((var blockHash, var block) in service.DropBlocksByIdAsync(
                           idSource: idSource,
                           ownershipToken: ownershipToken))
        {
        }

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task StoreBlockAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        BrightenedBlock block = null;

        // Act
        var result = await service.StoreBlockAsync(
            block: block);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task StoreBlocksAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        IAsyncEnumerable<BrightenedBlock> blockSource = null;

        // Act
        await foreach ((var block, IEnumerable<BrightChainException> brightChainException) in service.StoreBlocksAsync(
                           blockSource: blockSource))
        {
        }

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task BrightenBlocksAsyncEnumerable_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        IAsyncEnumerable<IdentifiableBlock> identifiableBlocks = null;

        // Act
        await foreach (var brightenedBlock in service.BrightenBlocksAsyncEnumerable(
                           identifiableBlocks: identifiableBlocks))
        {
        }

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task ForgeChainAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        DataHash sourceId = null;
        IAsyncEnumerable<BrightenedBlock> brightenedBlocks = null;

        // Act
        var result = await service.ForgeChainAsync(
            sourceId: sourceId,
            brightenedBlocks: brightenedBlocks);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public void CblToBrightHandle_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        ConstituentBlockListBlock cblBlock = null;
        BrightenedBlock brightenedCbl = null;
        var cblStripe = default(TupleStripe);

        // Act
        var result = service.CblToBrightHandle(
            cblBlock: cblBlock,
            brightenedCbl: brightenedCbl,
            cblStripe: cblStripe);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public void BrightenCbl_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        ConstituentBlockListBlock cblBlock = null;
        var persist = false;
        BrightenedBlock brightenedCbl = null;

        // Act
        var result = service.BrightenCbl(
            cblBlock: cblBlock,
            persist: persist,
            brightenedCbl: out brightenedCbl);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public void FindSourceById_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        DataHash requestedHash = null;

        // Act
        var result = service.FindSourceById(
            requestedHash: requestedHash);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public void BrightHandleToTupleStripe_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        var brightHandle = default(BrightHandle);

        // Act
        var result = service.BrightHandleToTupleStripe(
            brightHandle: brightHandle);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public void BrightHandleToIdentifiableBlock_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();
        var brightHandle = default(BrightHandle);

        // Act
        var result = service.BrightHandleToIdentifiableBlock(
            brightHandle: brightHandle);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public void Dispose_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();

        // Act
        service.Dispose();

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }
}
