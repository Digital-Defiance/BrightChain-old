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

        [DataTestMethod]
        [DataRow(BlockSize.Message)]
        [DataRow(BlockSize.Tiny)]
        [DataRow(BlockSize.Small)]
        [DataRow(BlockSize.Medium)]
        [DataRow(BlockSize.Large)]
        public async Task ItSavesDataCorrectlyTest(BlockSize blockSize)
        {
            ChainLinqObjectDataBlock<ChainLinqExampleSerializable>[] blocks = new ChainLinqObjectDataBlock<ChainLinqExampleSerializable>[4];
            for (int i = 0; i < 4; i++)
            {
                var chainableDataBlock = new ChainLinqObjectDataBlock<ChainLinqExampleSerializable>(
                    blockParams: new Models.Blocks.DataObjects.ChainLinqBlockParams(
                        blockParams: new Models.Blocks.DataObjects.TransactableBlockParams(
                            cacheManager: this.cacheManager,
                            allowCommit: true,
                            blockParams: new Models.Blocks.DataObjects.BlockParams(
                                blockSize: blockSize,
                                requestTime: DateTime.Now,
                                keepUntilAtLeast: DateTime.MaxValue,
                                redundancy: RedundancyContractType.HeapAuto,
                                privateEncrypted: false)),
                        next: null),
                    blockObject: new ChainLinqExampleSerializable());
                blocks[i] = chainableDataBlock;
            }

            var chainLinq = new ChainLinq<ChainLinqExampleSerializable>(blocks);

            var brightChainService = new BrightBlockService(
                logger: this.loggerFactory.Object,
                configuration: this.configuration.Object);

            var brightChain = chainLinq.BrightenAll(brightChainService);
            await brightChainService.PersistMemoryCacheAsync(clearAfter: true);

            var firstBlock = (await brightChainService.TryFindBlockByIdAsync(brightChain.First().Id)).AsBlock;
            var typedBlock = firstBlock as ChainLinqObjectDataBlock<ChainLinqExampleSerializable>;
            var block = typedBlock;
            var j = 0;
            while (block is not null)
            {
                var nextBlock = await brightChainService.TryFindBlockByIdAsync(block.Next);
                Assert.IsNotNull(nextBlock);
                typedBlock = nextBlock as ChainLinqObjectDataBlock<ChainLinqExampleSerializable>;
                Assert.AreEqual(blocks[j].BlockObject, block.BlockObject);
                j++;
            }
        }

        [TestMethod, Ignore]
        public void ItChainsCorrectlyTest()
        {
            throw new NotImplementedException();
        }
    }
}
