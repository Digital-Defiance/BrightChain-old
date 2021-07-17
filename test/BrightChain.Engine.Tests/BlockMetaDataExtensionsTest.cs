using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Extensions;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.Chains;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Contracts;
using BrightChain.Engine.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests
{
    /// <summary>
    /// Exercises the block metadata extensions for storing/restoring metadata
    /// </summary>
    [TestClass]
    public class BlockMetaDataExtensionsTest
    {
        protected readonly ILogger logger;
        public BlockMetaDataExtensionsTest()
        {
            logger = new Moq.Mock<ILogger>().Object;
        }

        [TestMethod]
        public void ItExtractsMetaDataCorrectlyTest()
        {
            var block = new EmptyDummyBlock(
                blockParams: new BlockParams(
                blockSize: BlockSize.Message,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
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
            Assert.IsTrue(metaDataDictionary.ContainsKey("RevocationCertificates"));
            Assert.AreEqual(5, metaDataDictionary.Count); // Hash, Signature, RedundancyContract, _t, _v
            var contractObj = (JsonElement)metaDataDictionary["RedundancyContract"];
            var contract = contractObj.ToObject<StorageContract>(BlockMetadataExtensions.NewSerializerOptions());

            Assert.AreEqual(block.StorageContract, contract);

            var loggerMock = Mock.Get(logger);
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
                blockParams: new BlockParams(
                blockSize: BlockSize.Message,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                privateEncrypted: false));

            var block = new ConstituentBlockListBlock(
                            blockParams: new ConstituentBlockListBlockParams(
                                blockParams: new TransactableBlockParams(
                                    cacheManager: new MemoryBlockCacheManager(logger: logger, configuration: new Configuration()),
                                    allowCommit: true,
                                    blockParams: new BlockParams(
                                        blockSize: BlockSize.Message,
                                        requestTime: DateTime.Now,
                                        keepUntilAtLeast: DateTime.Now.AddDays(1),
                                        redundancy: Enumerations.RedundancyContractType.HeapAuto,
                                        privateEncrypted: false)),
                                sourceId: new BlockHash(dummyBlock),
                                segmentHash: new SegmentHash(dummyBlock.Data),
                                totalLength: 0,
                                constituentBlocks: new BlockHash[] { dummyBlock.Id }));

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
            Assert.IsTrue(metaDataDictionary.ContainsKey("RevocationCertificates"));
            var contractObj = (JsonElement)metaDataDictionary["RedundancyContract"];
            var contract = contractObj.ToObject<StorageContract>(BlockMetadataExtensions.NewSerializerOptions());
            Assert.AreEqual(10, metaDataDictionary.Count); // Hash, Signature, RedundancyContract, _t, _v
            Assert.AreEqual(block.StorageContract, contract);
            var sourceIdObj = (JsonElement)metaDataDictionary["SourceId"];
            var sourceId = sourceIdObj.ToObject<BlockHash>(BlockMetadataExtensions.NewSerializerOptions());
            Assert.AreEqual(block.BlockSize, sourceId.BlockSize);
            Assert.AreEqual(
                Helpers.Utilities.HashToFormattedString(Helpers.Utilities.GetZeroVector(sourceId.BlockSize).HashBytes.ToArray()),
                Helpers.Utilities.HashToFormattedString(sourceId.HashBytes.ToArray()));
        }

        [TestMethod]
        public void ItRestoresMetaDataCorrectlyTest()
        {
            var testStart = DateTime.Now;

            var block = new EmptyDummyBlock(
                blockParams: new BlockParams(
                blockSize: BlockSize.Message,
                requestTime: testStart,
                keepUntilAtLeast: testStart.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                privateEncrypted: false));
            Assert.IsTrue(block.Validate());
            var metaData = block.Metadata;

            var block2 = new EmptyDummyBlock(
                blockParams: new BlockParams(
                blockSize: BlockSize.Message,
                requestTime: testStart.AddSeconds(5),
                keepUntilAtLeast: testStart.AddDays(1).AddSeconds(5),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                privateEncrypted: false));
            Assert.IsTrue(block2.TryRestoreMetadataFromBytes(metaData));
            Assert.AreEqual(block.StorageContract, block2.StorageContract);
            Assert.AreEqual(block.Signature, block2.Signature);

            var loggerMock = Mock.Get(logger);
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
                blockParams: new BlockParams(
                blockSize: BlockSize.Message,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(1),
                redundancy: Enumerations.RedundancyContractType.HeapAuto,
                privateEncrypted: false));

            var block = new ConstituentBlockListBlock(
                            blockParams: new ConstituentBlockListBlockParams(
                                blockParams: new TransactableBlockParams(
                                    cacheManager: new MemoryBlockCacheManager(
                                        logger: this.logger,
                                        configuration: new Configuration()),
                                    allowCommit: true,
                                    blockParams: new BlockParams(
                                        blockSize: BlockSize.Message,
                                        requestTime: DateTime.Now,
                                        keepUntilAtLeast: DateTime.Now.AddDays(1),
                                        redundancy: Enumerations.RedundancyContractType.HeapAuto,
                                        privateEncrypted: false)),
                                sourceId: new BlockHash(dummyBlock),
                                segmentHash: new SegmentHash(dummyBlock.Data),
                                totalLength: 0,
                                constituentBlocks: new BlockHash[] { dummyBlock.Id }));

            Assert.IsTrue(block.Validate());
            var metaData = block.Metadata;
            var metaDataString = new string(metaData.ToArray().Select(c => (char)c).ToArray());

            var block2 = new ConstituentBlockListBlock(
                blockParams: new ConstituentBlockListBlockParams(
                    blockParams: new TransactableBlockParams(
                        cacheManager: block.CacheManager,
                        allowCommit: true,
                        blockParams: new BlockParams(
                            blockSize: BlockSize.Message, // match
                            requestTime: DateTime.MinValue, // bad
                            keepUntilAtLeast: DateTime.MinValue, // bad
                            redundancy: RedundancyContractType.LocalNone, // different
                            privateEncrypted: true)), // opposite
                    sourceId: new DataHash(
                        providedHashBytes: dummyBlock.Id.HashBytes,
                        sourceDataLength: dummyBlock.Data.Length,
                        computed: true), // known incorrect hash
                    segmentHash: new SegmentHash(dummyBlock.Data),
                    totalLength: (long)BlockSizeMap.BlockSize(BlockSize.Message),
                    constituentBlocks: new BlockHash[] { dummyBlock.Id }));
            Assert.IsTrue(block2.TryRestoreMetadataFromBytes(metaData));
            Assert.AreEqual(block.StorageContract, block2.StorageContract);
            Assert.AreEqual(block.Signature, block2.Signature);
            Assert.AreEqual(block.SourceId, block2.SourceId);
            Assert.AreEqual(block.TotalLength, block2.TotalLength);
            Assert.AreEqual(block.ConstituentBlocks.Count(), block2.ConstituentBlocks.Count()); // TODO: IMPROVE THIS TEST

            var loggerMock = Mock.Get(logger);
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
