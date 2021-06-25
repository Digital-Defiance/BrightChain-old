using BrightChain.Enumerations;
using BrightChain.Extensions;
using BrightChain.Models.Blocks;
using BrightChain.Models.Contracts;
using BrightChain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

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

        [TestMethod]
        public void ItExtractsMetaDataCorrectlyTest()
        {
            var block = new RandomizerBlock(
                pregeneratedRandomizerCache: this.cacheManager,
                blockSize: BlockSize.Message,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: true);
            Assert.IsTrue(block.Validate());
            var metaData = block.MetaData;
            var metaDataString = System.Text.Encoding.ASCII.GetString(metaData.ToArray());
            var metaDataObject = JsonConvert.DeserializeObject(metaDataString, typeof(Dictionary<string, object>));
            var metaDataDictionary = metaDataObject as Dictionary<string, object>;
            Assert.IsTrue(metaDataDictionary.ContainsKey("_t"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("_v"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("RedundancyContract"));

            var blockRedundancyContract = (metaDataDictionary["RedundancyContract"] as JObject).ToObject<RedundancyContract>();
            Assert.AreEqual(block.RedundancyContract, blockRedundancyContract);
            Assert.AreEqual(block.StorageContract, blockRedundancyContract.StorageContract);
        }

        [TestMethod]
        public void ItRestoresMetaDataCorrectlyTest()
        {
            var testStart = DateTime.Now;

            var block = new RandomizerBlock(
                pregeneratedRandomizerCache: this.cacheManager,
                blockSize: BlockSize.Message,
                requestTime: testStart,
                keepUntilAtLeast: testStart.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: true);
            Assert.IsTrue(block.Validate());
            var metaData = block.MetaData;

            var block2 = new RandomizerBlock(
                pregeneratedRandomizerCache: this.cacheManager,
                blockSize: BlockSize.Message,
                requestTime: testStart.AddSeconds(5),
                keepUntilAtLeast: testStart.AddDays(1).AddSeconds(5),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: true);
            block2.RestoreMetaDataFromBytes(metaData);
            Assert.AreEqual(block.StorageContract, block2.StorageContract);
            Assert.AreEqual(block.RedundancyContract, block2.RedundancyContract);
            Assert.AreEqual(block.StorageContract, block2.RedundancyContract.StorageContract);
        }
    }
}
