using BrightChain.Enumerations;
using BrightChain.Extensions;
using BrightChain.Models.Blocks;
using BrightChain.Models.Contracts;
using BrightChain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
            this.cacheManager = new MemoryBlockCacheManager(logger: this.logger);
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
            var metaData = block.Metadata;
            var metaDataString = System.Text.Encoding.ASCII.GetString(metaData.ToArray());
            Dictionary<string, object> metaDataDictionary = (Dictionary<string, object>)JsonConvert.DeserializeObject(metaDataString, typeof(Dictionary<string, object>));
            Assert.IsNotNull(metaDataDictionary);
            Assert.IsTrue(metaDataDictionary.ContainsKey("_t"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("_v"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("RedundancyContract"));
            var contractObj = metaDataDictionary["RedundancyContract"] as JObject;
            Assert.AreEqual(3, metaDataDictionary.Count);

            RedundancyContract blockRedundancyContract = new RedundancyContract(
                storageDurationContract: contractObj.GetValue("StorageContract").ToObject<StorageDurationContract>(),
                redundancy: contractObj.GetValue("RedundancyContractType").ToObject<RedundancyContractType>());
            Assert.AreEqual(block.RedundancyContract, blockRedundancyContract);
            Assert.AreEqual(block.StorageContract, blockRedundancyContract.StorageContract);

            var loggerMock = Mock.Get(this.logger);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(3));
            loggerMock.VerifyNoOtherCalls();
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
            var metaData = block.Metadata;

            var block2 = new RandomizerBlock(
                pregeneratedRandomizerCache: this.cacheManager,
                blockSize: BlockSize.Message,
                requestTime: testStart.AddSeconds(5),
                keepUntilAtLeast: testStart.AddDays(1).AddSeconds(5),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: true);
            Assert.IsTrue(block2.TryRestoreMetadataFromBytes(metaData));
            Assert.AreEqual(block.RedundancyContract, block2.RedundancyContract);
            Assert.AreEqual(block.StorageContract, block2.RedundancyContract.StorageContract);
            Assert.AreEqual(block.StorageContract, block2.StorageContract);

            var loggerMock = Mock.Get(this.logger);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(4));
            loggerMock.VerifyNoOtherCalls();
        }
    }
}
