using System;
using System.Collections.Generic;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Faster.CacheManager;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Hashes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests;

/// <summary>
///     Serializable testable test block class
/// </summary>
public class FasterCacheTestBlock : BrightenedBlock
{
    public static new FasterBlockCacheManager CacheManager;

    public FasterCacheTestBlock(BrightenedBlockParams blockParams, ReadOnlyMemory<byte> data)
        : base(
            blockParams: blockParams,
            data: data)
    {
    }

    internal FasterCacheTestBlock()
        : base(
            blockParams: new BrightenedBlockParams(
                cacheManager: CacheManager,
                allowCommit: true,
                blockParams: new BlockParams(
                    blockSize: BlockSize.Message,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: RedundancyContractType.HeapAuto,
                    privateEncrypted: false,
                    originalType: typeof(FasterCacheTestBlock))),
            data: NewRandomData())
    {
    }

    public static ReadOnlyMemory<byte> NewRandomData()
    {
        var random = new Random(Seed: Guid.NewGuid().GetHashCode());
        var data = new byte[BlockSizeMap.BlockSize(blockSize: BlockSize.Message)];
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = (byte)random.Next(minValue: 0,
                maxValue: 255);
        }

        return new ReadOnlyMemory<byte>(array: data);
    }

    public override void Dispose()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///     Tests disk block cache managers
/// </summary>
[TestClass]
public class FasterBlockCacheManagerTest : TransactableBlockCacheManagerTest<FasterBlockCacheManager>
{
    [TestInitialize]
    public new void PreTestSetup()
    {
        base.PreTestSetup();
        var rootBlock = new RootBlock(databaseGuid: Guid.NewGuid(),
            blockSize: BlockSize.Large);
        FasterCacheTestBlock.CacheManager = new FasterBlockCacheManager(
            logger: this.logger.Object,
            configuration: this.configuration.Object,
            rootBlock: rootBlock,
            testingSelfDestruct: true);
        this.cacheManager = FasterCacheTestBlock.CacheManager;
    }

    internal override FasterBlockCacheManager NewCacheManager(ILogger logger, IConfiguration configuration)
    {
        var rootBlock = new RootBlock(databaseGuid: Guid.NewGuid(),
            blockSize: BlockSize.Large);
        return new FasterBlockCacheManager(
            logger: logger,
            configuration: configuration,
            rootBlock: rootBlock,
            testingSelfDestruct: true);
    }

    internal override KeyValuePair<BlockHash, BrightenedBlock> NewKeyValue()
    {
        var random = new Random(Seed: Guid.NewGuid().GetHashCode());
        var data = new byte[BlockSizeMap.BlockSize(blockSize: BlockSize.Message)];
        for (var i = 0; i < BlockSizeMap.BlockSize(blockSize: BlockSize.Message); i++)
        {
            data[i] = (byte)random.Next(minValue: 0,
                maxValue: 255);
        }

        var block = new FasterCacheTestBlock(
            blockParams: new BrightenedBlockParams(
                cacheManager: this.cacheManager,
                allowCommit: true,
                blockParams: new BlockParams(
                    blockSize: BlockSize.Message,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: RedundancyContractType.LocalNone,
                    privateEncrypted: false,
                    originalType: typeof(FasterCacheTestBlock))),
            data: data);

        return new KeyValuePair<BlockHash, BrightenedBlock>(key: block.Id,
            value: block);
    }

    internal override FasterCacheTestBlock NewNullData()
    {
        return null;
    }

    /// <summary>
    ///     Tries to push a null value into the cache and expects an exception.
    /// </summary>
    [TestMethod]
    public override void ItPutsNullValuesTest()
    {
        // Arrange
        var newData = this.NewNullData();

        // Act/Expect
        var brightChainException = Assert.ThrowsException<BrightChainException>(action: () =>
            this.cacheManager.Set(block: newData));

        this.logger.Verify(expression: l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            times: Times.Exactly(callCount: 0));
        this.logger.VerifyNoOtherCalls();
    }
}
