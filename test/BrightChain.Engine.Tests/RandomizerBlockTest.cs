using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services.CacheManagers.Block;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests;

/// <summary>
///     Verifies random blocks are random, generated correctly and are inserted into the cache
/// </summary>
[TestClass]
public class RandomizerBlockTest
{
    private MemoryDictionaryBlockCacheManager cacheManager;
    private ILogger logger;

    [TestInitialize]
    public void PreTestSetUp()
    {
        this.logger = new Mock<ILogger>().Object;
        var rootBlock = new RootBlock(databaseGuid: Guid.NewGuid(),
            blockSize: BlockSize.Large);
        this.cacheManager = new MemoryDictionaryBlockCacheManager(
            logger: this.logger,
            configuration: new BrightChainConfiguration(),
            rootBlock: rootBlock);
    }

    [DataTestMethod]
    [DataRow(data1: BlockSize.Nano)]
    [DataRow(data1: BlockSize.Micro)]
    [DataRow(data1: BlockSize.Message)]
    [DataRow(data1: BlockSize.Tiny)]
    [DataRow(data1: BlockSize.Small)]
    [DataRow(data1: BlockSize.Medium)]
    [DataRow(data1: BlockSize.Large)]
    public void ItCreatesValidRandomDataBlocksTest(BlockSize blockSize)
    {
        var zeroBlock = new ZeroVectorBlock(
            blockParams: new BlockParams(
                blockSize: blockSize,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.Now.AddDays(value: 1),
                redundancy: RedundancyContractType.HeapAuto,
                privateEncrypted: false,
                originalType: typeof(ZeroVectorBlock)));

        Assert.IsTrue(condition: zeroBlock.Validate());

        var randomBlock = new RandomizerBlock(
            destinationCache: this.cacheManager,
            blockSize: blockSize,
            keepUntilAtLeast: DateTime.Now.AddDays(value: 1),
            redundancyContractType: RedundancyContractType.HeapAuto);

        Assert.IsTrue(condition: randomBlock.Validate());
        Assert.IsFalse(condition: this.cacheManager.Contains(key: randomBlock.Id));

        var zeroBlockEntResult = zeroBlock.EntropyEstimate;
        Assert.AreEqual(expected: 0,
            actual: zeroBlockEntResult.Entropy);

        var randomBlockEntResult = randomBlock.EntropyEstimate;
        Assert.IsTrue(condition: randomBlockEntResult.Entropy > 6.0D);

        var mockLogger = Mock.Get(mocked: this.logger);
        mockLogger.Verify(
            expression: l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            times: Times.Exactly(callCount: 0));
        mockLogger.VerifyNoOtherCalls();
    }
}
