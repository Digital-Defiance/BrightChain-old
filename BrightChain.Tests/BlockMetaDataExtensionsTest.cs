using BrightChain.Enumerations;
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
            Assert.IsTrue(metaDataDictionary.ContainsKey("DurationContract"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("RedundancyContract"));

            var blockDurationContract = (metaDataDictionary["DurationContract"] as JObject).ToObject<StorageDurationContract>();
            Assert.AreEqual(block.DurationContract, blockDurationContract);

            var blockRedundancyContract = (metaDataDictionary["RedundancyContract"] as JObject).ToObject<RedundancyContract>();
            Assert.AreEqual(block.RedundancyContract, blockRedundancyContract);
        }

        [TestMethod, Ignore]
        public void ItRestoresMetaDataCorrectlyTest()
        {
            throw new NotImplementedException();
        }
    }
}
