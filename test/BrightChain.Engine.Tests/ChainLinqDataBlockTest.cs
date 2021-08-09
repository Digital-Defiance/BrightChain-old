namespace BrightChain.Engine.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Services;
    using BrightChain.Engine.Tests.TestModels;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ChainLinqDataBlockTest
    {
        protected Mock<ILogger<BlockCacheManager>> logger;
        protected Mock<IConfiguration> configuration;
        protected Mock<ILoggerFactory> loggerFactory;
        protected BlockCacheManager cacheManager;

        [TestInitialize]
        public new void PreTestSetup()
        {
            this.logger = new Mock<ILogger<BlockCacheManager>>();
            this.configuration = new Mock<IConfiguration>();

            var factoryMock = new Mock<ILoggerFactory>();

            factoryMock
                .SetupAllProperties()
                .Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(this.logger.Object);

            this.loggerFactory = factoryMock;

            Mock<IConfigurationSection> mockPathSection = new Mock<IConfigurationSection>();
            mockPathSection.Setup(x => x.Value).Returns(Path.GetTempPath());

            var mockNodeSection = new Mock<IConfigurationSection>();
            mockNodeSection.Setup(x => x.GetSection(It.Is<string>(k => k == "BasePath"))).Returns(mockPathSection.Object);

            this.configuration.Setup(x => x.GetSection(It.Is<string>(k => k == "NodeOptions"))).Returns(mockNodeSection.Object);

            var rootBlock = new RootBlock(databaseGuid: Guid.NewGuid(), blockSize: BlockSize.Large);
            DiskCacheTestBlock.CacheManager = new DiskBlockCacheManager(
                logger: this.logger.Object,
                configuration: this.configuration.Object,
                rootBlock: rootBlock);
            this.cacheManager = DiskCacheTestBlock.CacheManager;
        }

        public async Task<BrightChain> ForgeChainAsync(BrightBlockService brightBlockService, BlockSize blockSize, int objectCount)
        {
            var requestParams = new Models.Blocks.DataObjects.ChainLinqBlockParams(
                    blockParams: new Models.Blocks.DataObjects.BlockParams(
                        blockSize: blockSize,
                        requestTime: DateTime.Now,
                        keepUntilAtLeast: DateTime.MaxValue,
                        redundancy: RedundancyContractType.HeapAuto,
                        privateEncrypted: false,
                        originalType: typeof(ChainLinqObjectBlock<ChainLinqExampleSerializable>)));

            var datas = ChainLinqExampleSerializable.MakeMultiple(objectCount);
            Assert.AreEqual(objectCount, datas.Count());

            var chainLinq = ChainLinq<ChainLinqExampleSerializable>.ForgeChainLinq(
                blockParams: requestParams,
                objects: datas);
            Assert.AreEqual(objectCount, chainLinq.Count());

            var brightChain = await chainLinq.BrightenAllAsync(
                brightBlockService: brightBlockService);

            return brightChain;
        }

        [DataTestMethod]
        [DataRow(BlockSize.Message, 4)]
        [DataRow(BlockSize.Tiny, 4)]
        [DataRow(BlockSize.Small, 4)]
        [DataRow(BlockSize.Medium, 4)]
        [DataRow(BlockSize.Large, 4)]
        public async Task ItSavesDataCorrectlyTest(BlockSize blockSize, int objectCount)
        {
            var brightBlockService = new BrightBlockService(
                logger: this.loggerFactory.Object,
                configuration: this.configuration.Object);

            var brightChain = await this.ForgeChainAsync(
                brightBlockService: brightBlockService,
                blockSize: blockSize,
                objectCount: objectCount);

            await brightBlockService.PersistMemoryCacheAsync(clearAfter: true);

            Assert.ThrowsException<KeyNotFoundException>(async () =>
            {
                var retrievedChainNull = await brightBlockService.TryFindBlockByIdAsync(brightChain.Id);
                Assert.IsNull(retrievedChainNull);
            });

            brightBlockService.PersistCBL(brightChain);

            // in order to get our original blocks back, first get the chain head
            var retrievedChain = await brightBlockService.TryFindBlockByIdAsync(brightChain.Id);
            Assert.IsNotNull(retrievedChain);

            // how do we validate this?
        }

        [DataTestMethod]
        [DataRow(BlockSize.Message, 4)]
        [DataRow(BlockSize.Tiny, 4)]
        [DataRow(BlockSize.Small, 4)]
        [DataRow(BlockSize.Medium, 4)]
        [DataRow(BlockSize.Large, 4)]
        public async Task ItLoadsDataCorrectlyTest(BlockSize blockSize, int objectCount)
        {
            var brightBlockService = new BrightBlockService(
                logger: this.loggerFactory.Object,
                configuration: this.configuration.Object);

            var brightChain = await this.ForgeChainAsync(
                brightBlockService: brightBlockService,
                blockSize: blockSize,
                objectCount: objectCount);

            await brightBlockService.PersistMemoryCacheAsync(clearAfter: true);

            Assert.ThrowsException<KeyNotFoundException>(async () =>
            {
                var retrievedChainNull = await brightBlockService.TryFindBlockByIdAsync(brightChain.Id);
                Assert.IsNull(retrievedChainNull);
            });

            brightBlockService.PersistCBL(brightChain);

            // in order to get our original blocks back, first get the chain head
            var retrievedChain = await brightBlockService.TryFindBlockByIdAsync(brightChain.Id);
            Assert.IsNotNull(retrievedChain);

            if (retrievedChain is BrightChain retrievedBrightChain)
            {
                Assert.AreEqual(brightChain.Crc32, retrievedBrightChain.Crc32);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
