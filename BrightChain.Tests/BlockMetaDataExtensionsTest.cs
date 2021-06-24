using BrightChain.Enumerations;
using BrightChain.Models.Blocks;
using BrightChain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BrightChain.Tests
{
    /// <summary>
    /// Exercises the block metadata extensions for storing/restoring metadata
    /// </summary>
    [TestClass]
    public class BlockMetaDataExtensionsTest
    {
        protected readonly MemoryBlockCacheManager cacheManager;
        protected readonly ILogger logger;
        public BlockMetaDataExtensionsTest()
        {
            this.logger = new Moq.Mock<ILogger>().Object;
            this.cacheManager = new MemoryBlockCacheManager(
                logger: this.logger,
                optionsV2: null);
        }
        [TestMethod, Ignore]
        public void ItExtractsMetaDataCorrectlyTest()
        {
            var block = new RandomizerBlock(
                pregeneratedRandomizerCache: this.cacheManager,
                blockSize: BlockSize.Message,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: true);
        }

        [TestMethod, Ignore]
        public void ItRestoresMetaDataCorrectlyTest()
        {
            throw new NotImplementedException();
        }
    }
}
