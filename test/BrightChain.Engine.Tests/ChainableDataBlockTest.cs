namespace BrightChain.Engine.Tests
{
    using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ChainableDataBlockTest
    {
        protected Mock<ILogger<BlockCacheManager>> logger;
        protected Mock<IConfiguration> configuration;
        protected BlockCacheManager cacheManager;

        [TestInitialize]
        public new void PreTestSetup()
        {
            this.logger = new Mock<ILogger<BlockCacheManager>>();
            this.configuration = new Mock<IConfiguration>();

            var rootBlock = new RootBlock(databaseGuid: Guid.NewGuid(), blockSize: BlockSize.Large);
            MemoryDictionaryCacheTestBlock.CacheManager = new MemoryDictionaryBlockCacheManager(
                logger: this.logger.Object,
                configuration: this.configuration.Object,
                rootBlock: rootBlock);
            this.cacheManager = MemoryDictionaryCacheTestBlock.CacheManager;
        }

        [DataTestMethod]
        [DataRow(BlockSize.Message)]
        [DataRow(BlockSize.Tiny)]
        [DataRow(BlockSize.Small)]
        [DataRow(BlockSize.Medium)]
        [DataRow(BlockSize.Large)]
        public void ItSavesDataCorrectlyTest(BlockSize blockSize)
        {
            var chainableDataBlock = new ChainableDataBlock(
                blockParams: new Models.Blocks.DataObjects.ChainableDataBlockParams(
                    blockParams: new Models.Blocks.DataObjects.TransactableBlockParams(
                        cacheManager: this.cacheManager,
                        allowCommit: true,
                        blockParams: new Models.Blocks.DataObjects.BlockParams(
                            blockSize: blockSize,
                            requestTime: DateTime.Now,
                            keepUntilAtLeast: DateTime.MaxValue,
                            redundancy: RedundancyContractType.HeapAuto,
                            privateEncrypted: false)),
                    previous: null,
                    next: null),
                blockData: new System.Collections.Generic.Dictionary<string, object>()
                {
                    { "TestData", "TestValue" },
                });
            // WIP
        }

        [TestMethod, Ignore]
        public void ItChainsCorrectlyTest()
        {
            throw new NotImplementedException();
        }
    }
}
