using BrightChain.Enumerations;
using BrightChain.Helpers;
using BrightChain.Models.Blocks;
using BrightChain.Services;
using CSharpTest.Net.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace BrightChain.Tests
{
    /// <summary>
    /// Serializable testable test block class
    /// </summary>
    public class DiskCacheTestBlock : TransactableBlock
    {
        public new static DiskBlockCacheManager CacheManager;

        public DiskCacheTestBlock(DiskBlockCacheManager cacheManager, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) :
            base(
                cacheManager: cacheManager,
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                redundancy: redundancy,
                data: data,
                allowCommit: allowCommit)
        {

        }

        public DiskCacheTestBlock() :
            base(
                cacheManager: CacheManager,
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.MaxValue,
                redundancy: RedundancyContractType.LocalNone,
                data: NewRandomData(),
                allowCommit: true)
        { }

        public static ReadOnlyMemory<byte> NewRandomData()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var data = new byte[BlockSizeMap.BlockSize(BlockSize.Message)];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)random.Next(0, 255);
            }

            return new ReadOnlyMemory<byte>(data);
        }

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) => new DiskCacheTestBlock(
cacheManager: CacheManager,
requestTime: requestTime,
keepUntilAtLeast: keepUntilAtLeast,
redundancy: redundancy,
data: data,
allowCommit: allowCommit);

        public override void Dispose() => throw new NotImplementedException();
    }

    /// <summary>
    /// Tests disk block cache managers
    /// </summary>
    [TestClass]
    public class DiskBlockCacheManagerTest : TransactableBlockCacheManagerTest
    {
        public DiskBlockCacheManagerTest()
        {
            this.logger = new Mock<ILogger<BlockCacheManager>>();
            DiskCacheTestBlock.CacheManager = new DiskBlockCacheManager(
                                                    new BlockCacheManager(
                                                        this.NewCacheManager(this.logger.Object)));
        }

        public static BPlusTree<BlockHash, TransactableBlock>.OptionsV2 DefaultOptions() => new BPlusTree<BlockHash, TransactableBlock>.OptionsV2(
keySerializer: new BlockHashSerializer(),
valueSerializer: new BlockSerializer<TransactableBlock>());

        internal override DiskBlockCacheManager NewCacheManager(ILogger logger) => new DiskBlockCacheManager(
logger: logger,
optionsV2: DefaultOptions());

        internal override KeyValuePair<BlockHash, TransactableBlock> NewKeyValue()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var data = new byte[BlockSizeMap.BlockSize(BlockSize.Message)];
            for (int i = 0; i < BlockSizeMap.BlockSize(BlockSize.Message); i++)
            {
                data[i] = (byte)random.Next(0, 255);
            }

            var block = new DiskCacheTestBlock(
                new DiskBlockCacheManager(
                    new BlockCacheManager(
                        this.cacheManager)),
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.MaxValue,
                redundancy: Enumerations.RedundancyContractType.LocalNone,
                data: data,
                allowCommit: true);
            return new KeyValuePair<BlockHash, TransactableBlock>(block.Id, block);
        }

        internal override DiskCacheTestBlock NewNullData() => null;

        /// <summary>
        /// Push a null value into the cache
        /// </summary>
        [TestMethod]
        public override void ItPutsNullValuesTest()
        {
            // Arrange
            var newData = this.NewNullData();

            // Act/Expect
            Exceptions.BrightChainException brightChainException = Assert.ThrowsException<BrightChain.Exceptions.BrightChainException>(() =>
                this.cacheManager.Set(this.testPair.Key, newData));

            this.logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(2));
            this.logger.VerifyNoOtherCalls();
        }
    }
}
