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

        public BrightChain MakeChain(BrightBlockService brightBlockService, BlockSize blockSize, int objectCount, out IEnumerable<ChainLinqExampleSerializable> datas)
        {
            var requestParams = new Models.Blocks.DataObjects.ChainLinqBlockParams(
                    blockParams: new Models.Blocks.DataObjects.BlockParams(
                        blockSize: blockSize,
                        requestTime: DateTime.Now,
                        keepUntilAtLeast: DateTime.MaxValue,
                        redundancy: RedundancyContractType.HeapAuto,
                        privateEncrypted: false,
                        originalType: typeof(ChainLinqObjectBlock<ChainLinqExampleSerializable>)));

            datas = ChainLinqExampleSerializable.MakeMultiple(objectCount);
            Assert.AreEqual(objectCount, datas.Count());

            var brightChain = ChainLinq<ChainLinqExampleSerializable>.MakeChain(
                brightBlockService: brightBlockService,
                blockParams: requestParams,
                blockObjects: datas);
            Assert.AreEqual(objectCount, brightChain.Count());

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

            IEnumerable<ChainLinqExampleSerializable> datas;
            var brightChain = this.MakeChain(
                brightBlockService: brightBlockService,
                blockSize: blockSize,
                objectCount: objectCount,
                out datas);

            var chainType = brightChain.BlockParams.OriginalType;
            Assert.AreEqual(chainType, typeof(ChainLinqObjectBlock<ChainLinqExampleSerializable>));

            await brightBlockService.PersistMemoryCacheAsync(clearAfter: true);

            var id = brightChain.First().Id;

            // V1
            var firstBlock = (await brightBlockService.TryFindBlockByIdAsync(id)).AsBlock;
            var typedBlock = firstBlock as ChainLinqObjectBlock<ChainLinqExampleSerializable>;
            Assert.IsNotNull(typedBlock);
            var block = typedBlock;

            // V2
            var block2 = await brightBlockService
                .TryFindBlockByIdAsync<ChainLinqObjectBlock<ChainLinqExampleSerializable>>(id, true);
            Assert.IsNotNull(block2);

            var j = 0;
            while (block is not null)
            {
                var next = block.Next;
                if (next is null)
                {
                    break;
                }

                block = await brightBlockService
                    .TryFindBlockByIdAsync<ChainLinqObjectBlock<ChainLinqExampleSerializable>>(next, false);
                Assert.IsNotNull(block);
                Assert.AreEqual(datas.ElementAt(j++), block.BlockObject);
            }
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

            IEnumerable<ChainLinqExampleSerializable> datas;
            var brightChain = this.MakeChain(
                brightBlockService: brightBlockService,
                blockSize: blockSize,
                objectCount: objectCount,
                out datas);

            await brightBlockService.PersistMemoryCacheAsync(clearAfter: true);
        }
    }
}
