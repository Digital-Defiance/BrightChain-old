using BrightChain.Enumerations;
using BrightChain.Extensions;
using BrightChain.Models.Blocks;
using BrightChain.Models.Blocks.Chains;
using BrightChain.Models.Blocks.DataObjects;
using BrightChain.Models.Contracts;
using BrightChain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BrightChain.Tests
{
    /// <summary>
    /// Exercises the block metadata extensions for storing/restoring metadata
    /// </summary>
    [TestClass]
    public class BlockMetaDataExtensionsTest
    {
        protected readonly ILogger logger;
        public BlockMetaDataExtensionsTest() => this.logger = new Moq.Mock<ILogger>().Object;

        [TestMethod]
        public void ItExtractsMetaDataCorrectlyTest()
        {
            var block = new EmptyDummyBlock(
                blockArguments: new BlockParams(
                blockSize: BlockSize.Message,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: true,
                privateEncrypted: false));
            Assert.IsTrue(block.Validate());
            var metaData = block.Metadata;
            var metaDataString = new string(metaData.ToArray().Select(c => (char)c).ToArray());
            Dictionary<string, object> metaDataDictionary = (Dictionary<string, object>)JsonSerializer.Deserialize(metaDataString, typeof(Dictionary<string, object>));
            Assert.IsNotNull(metaDataDictionary);
            Assert.IsTrue(metaDataDictionary.ContainsKey("_t"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("_v"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("RedundancyContract"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("Signature"));
            Assert.AreEqual(4, metaDataDictionary.Count); // Hash, Signature, RedundancyContract, _t, _v
            var contractObj = (JsonElement)metaDataDictionary["RedundancyContract"];
            var contract = contractObj.ToObject<RedundancyContract>(BlockMetadataExtensions.NewSerializerOptions());

            Assert.AreEqual(block.RedundancyContract, contract);
            Assert.AreEqual(block.StorageContract, contract.StorageContract);

            var loggerMock = Mock.Get(this.logger);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            loggerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void ItExtractsCBLMetadataCorrectlyTest()
        {
            var dummyBlock = new EmptyDummyBlock(
                blockArguments: new BlockParams(
                blockSize: BlockSize.Message,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: true,
                privateEncrypted: false));

            var block = new ConstituentBlockListBlock(
                            blockArguments: new ConstituentBlockListBlockParams(
                                blockArguments: new TransactableBlockParams(
                                    cacheManager: new MemoryBlockCacheManager(this.logger),
                                    blockArguments: new BlockParams(
                                        blockSize: BlockSize.Message,
                                        requestTime: DateTime.Now,
                                        keepUntilAtLeast: DateTime.Now.AddDays(1),
                                        redundancy: Enumerations.RedundancyContractType.HeapAuto,
                                        allowCommit: true,
                                        privateEncrypted: false)),
                                finalDataHash: new BlockHash(dummyBlock),
                                totalLength: 0,
                                constituentBlocks: new Block[] { dummyBlock }));

            Assert.IsTrue(block.Validate());
            var metaData = block.Metadata;
            var metaDataString = new string(metaData.ToArray().Select(c => (char)c).ToArray());
            Dictionary<string, object> metaDataDictionary = (Dictionary<string, object>)JsonSerializer.Deserialize(metaDataString, typeof(Dictionary<string, object>), BlockMetadataExtensions.NewSerializerOptions());
            Assert.IsNotNull(metaDataDictionary);
            Assert.IsTrue(metaDataDictionary.ContainsKey("_t"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("_v"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("RedundancyContract"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("Signature"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("PrivateEncrypted"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("TotalLength"));
            Assert.IsTrue(metaDataDictionary.ContainsKey("SourceId"));
            var contractObj = (JsonElement)metaDataDictionary["RedundancyContract"];
            var contract = contractObj.ToObject<RedundancyContract>(BlockMetadataExtensions.NewSerializerOptions());
            Assert.AreEqual(9, metaDataDictionary.Count); // Hash, Signature, RedundancyContract, _t, _v
            Assert.AreEqual(block.RedundancyContract, contract);
            Assert.AreEqual(block.StorageContract, contract.StorageContract);
            var sourceIdObj = (JsonElement)metaDataDictionary["SourceId"];
            var sourceId = sourceIdObj.ToObject<BlockHash>(BlockMetadataExtensions.NewSerializerOptions());
            Assert.AreEqual(block.BlockSize, sourceId.BlockSize);
            Assert.AreEqual("076a27c79e5ace2a3d47f9dd2e83e4ff6ea8872b3c2218f66c92b89b55f36560", sourceId.ToString()); // all-zero vector
        }

        [TestMethod]
        public void ItRestoresMetaDataCorrectlyTest()
        {
            var testStart = DateTime.Now;

            var block = new EmptyDummyBlock(
                blockArguments: new BlockParams(
                blockSize: BlockSize.Message,
                requestTime: testStart,
                keepUntilAtLeast: testStart.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: true,
                privateEncrypted: false));
            Assert.IsTrue(block.Validate());
            var metaData = block.Metadata;

            var block2 = new EmptyDummyBlock(
                blockArguments: new BlockParams(
                blockSize: BlockSize.Message,
                requestTime: testStart.AddSeconds(5),
                keepUntilAtLeast: testStart.AddDays(1).AddSeconds(5),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: true,
                privateEncrypted: false));
            Assert.IsTrue(block2.TryRestoreMetadataFromBytes(metaData));
            Assert.AreEqual(block.RedundancyContract, block2.RedundancyContract);
            Assert.AreEqual(block.StorageContract, block2.RedundancyContract.StorageContract);
            Assert.AreEqual(block.StorageContract, block2.StorageContract);
            Assert.AreEqual(block.Signature, block2.Signature);

            var loggerMock = Mock.Get(this.logger);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            loggerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void ItRestoresCBLMetaDataCorrectlyTest()
        {
            var testStart = DateTime.Now;

            var dummyBlock = new EmptyDummyBlock(
                blockArguments: new BlockParams(
                blockSize: BlockSize.Message,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                allowCommit: true,
                privateEncrypted: false));

            var block = new ConstituentBlockListBlock(
                            blockArguments: new ConstituentBlockListBlockParams(
                                blockArguments: new TransactableBlockParams(
                                    cacheManager: new MemoryBlockCacheManager(this.logger),
                                    blockArguments: new BlockParams(
                                        blockSize: BlockSize.Message,
                                        requestTime: DateTime.Now,
                                        keepUntilAtLeast: DateTime.Now.AddDays(1),
                                        redundancy: Enumerations.RedundancyContractType.HeapAuto,
                                        allowCommit: true,
                                        privateEncrypted: false)),
                                finalDataHash: new BlockHash(dummyBlock),
                                totalLength: 0,
                                constituentBlocks: new Block[] { dummyBlock }));

            Assert.IsTrue(block.Validate());
            var metaData = block.Metadata;
            var metaDataString = new string(metaData.ToArray().Select(c => (char)c).ToArray());

            var block2 = new ConstituentBlockListBlock(new ConstituentBlockListBlockParams(new TransactableBlockParams(
                cacheManager: block.CacheManager,
                blockArguments: new BlockParams(
                    blockSize: BlockSize.Message, // match
                    requestTime: DateTime.MinValue, // bad
                    keepUntilAtLeast: DateTime.MinValue, // bad
                    redundancy: RedundancyContractType.LocalNone, // different
                    allowCommit: true, // irrelevant
                    privateEncrypted: true)), // opposite
                    finalDataHash: new BlockHash(originalBlockSize: BlockSize.Message, providedHashBytes: dummyBlock.Id.HashBytes), // known incorrect hash
                    totalLength: (ulong)BlockSizeMap.BlockSize(BlockSize.Message),
                    constituentBlocks: new Block[] { dummyBlock }));
            Assert.IsTrue(block2.TryRestoreMetadataFromBytes(metaData));
            Assert.AreEqual(block.RedundancyContract, block2.RedundancyContract);
            Assert.AreEqual(block.StorageContract, block2.RedundancyContract.StorageContract);
            Assert.AreEqual(block.StorageContract, block2.StorageContract);
            Assert.AreEqual(block.Signature, block2.Signature);
            Assert.AreEqual(block.SourceId, block2.SourceId);
            Assert.AreEqual(block.TotalLength, block2.TotalLength);
            Assert.AreEqual(block.ConstituentBlocks.Count(), block2.ConstituentBlocks.Count()); // TODO: IMPROVE THIS TEST

            var loggerMock = Mock.Get(this.logger);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            loggerMock.VerifyNoOtherCalls();
        }
    }
}
