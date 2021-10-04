namespace BrightChain.Engine.Tests.Services
{
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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    [TestClass]
    public class BrightBlockServiceTests
    {
        private MockRepository mockRepository;

        private Mock<ILoggerFactory> mockLoggerFactory;
        private Mock<IConfiguration> mockConfiguration;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLoggerFactory = this.mockRepository.Create<ILoggerFactory>();
            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
        }

        private BrightBlockService CreateService()
        {
            return new BrightBlockService(
                this.mockLoggerFactory.Object,
                this.mockConfiguration.Object);
        }

        [TestMethod]
        public async Task StreamCreatedBrightenedBlocksFromFileAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            SourceFileInfo sourceInfo = default(global::BrightChain.Engine.Models.Blocks.DataObjects.SourceFileInfo);
            BlockParams blockParams = null;
            BlockSize? blockSize = null;

            // Act
            await foreach (var brightenedBlock in service.StreamCreatedBrightenedBlocksFromFileAsync(
                sourceInfo,
                blockParams,
                blockSize))
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
                fileName,
                blockParams);

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
                blockParams,
                chainedCbls,
                sourceId);

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
                fileName,
                blockParams);

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
                blockCacheManager,
                block);

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
                constituentBlockListBlock,
                destination);

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
                constituentBlockListBlock);

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
                id);

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
                blockIdSource))
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
            bool useAsBlock = false;

            // Act
            var result = await service.FindBlockByIdAsync<RestorableBlock>(
                id,
                useAsBlock);

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
                id,
                ownershipToken);

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
            await foreach ((BlockHash blockHash, Block block) in service.DropBlocksByIdAsync(
                idSource,
                ownershipToken))
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
                block);

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
            await foreach ((Block block, IEnumerable<BrightChainException> brightChainException) in service.StoreBlocksAsync(
                blockSource))
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
                identifiableBlocks))
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
                sourceId,
                brightenedBlocks);

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
            TupleStripe cblStripe = default(global::BrightChain.Engine.Models.Blocks.Chains.TupleStripe);

            // Act
            var result = service.CblToBrightHandle(
                cblBlock,
                brightenedCbl,
                cblStripe);

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
            bool persist = false;
            BrightenedBlock brightenedCbl = null;

            // Act
            var result = service.BrightenCbl(
                cblBlock,
                persist,
                out brightenedCbl);

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
                requestedHash);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void BrightHandleToTupleStripe_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            BrightHandle brightHandle = default(global::BrightChain.Engine.Models.Blocks.DataObjects.BrightHandle);

            // Act
            var result = service.BrightHandleToTupleStripe(
                brightHandle);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void BrightHandleToIdentifiableBlock_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            BrightHandle brightHandle = default(global::BrightChain.Engine.Models.Blocks.DataObjects.BrightHandle);

            // Act
            var result = service.BrightHandleToIdentifiableBlock(
                brightHandle);

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
}
