using BrightChain.Enumerations;
using BrightChain.Models.Blocks;
using BrightChain.Services;
using Microsoft.Extensions.Configuration;
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
        public new static BrightChainBlockCacheManager CacheManager;

        public DiskCacheTestBlock(TransactableBlockArguments blockArguments, ReadOnlyMemory<byte> data) :
            base(
                blockArguments: blockArguments,
                data: data)
        {

        }

        internal DiskCacheTestBlock() :
            base(
                blockArguments: new TransactableBlockArguments(
                    cacheManager: DiskCacheTestBlock.CacheManager,
                    blockArguments: new BlockArguments(
                    blockSize: BlockSize.Message,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: RedundancyContractType.HeapAuto,
                    allowCommit: true,
                    privateEncrypted: false)),
                data: NewRandomData())
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

        public override Block NewBlock(BlockArguments blockArguments, ReadOnlyMemory<byte> data) => new DiskCacheTestBlock(
            blockArguments: new TransactableBlockArguments(
                cacheManager: DiskCacheTestBlock.CacheManager,
                blockArguments: blockArguments),
            data: data);

        public override void Dispose() => throw new NotImplementedException();
    }

    /// <summary>
    /// Tests disk block cache managers
    /// </summary>
    [TestClass]
    public class DiskBlockCacheManagerTest : TransactableBlockCacheManagerTest<BrightChainBlockCacheManager>
    {
        [TestInitialize]
        public new void PreTestSetup()
        {
            base.PreTestSetup();
            DiskCacheTestBlock.CacheManager = new BrightChainBlockCacheManager(this.logger.Object, this.configuration.Object);
            this.cacheManager = DiskCacheTestBlock.CacheManager;
        }

        internal override BrightChainBlockCacheManager NewCacheManager(ILogger logger, IConfiguration configuration) => new BrightChainBlockCacheManager(
logger: logger, configuration: configuration);

        internal override KeyValuePair<BlockHash, TransactableBlock> NewKeyValue()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var data = new byte[BlockSizeMap.BlockSize(BlockSize.Message)];
            for (int i = 0; i < BlockSizeMap.BlockSize(BlockSize.Message); i++)
            {
                data[i] = (byte)random.Next(0, 255);
            }

            var block = new DiskCacheTestBlock(
                blockArguments: new TransactableBlockArguments(
                    cacheManager: this.cacheManager,
                    blockArguments: new BlockArguments(
                        blockSize: BlockSize.Message,
                        requestTime: DateTime.Now,
                        keepUntilAtLeast: DateTime.MaxValue,
                        redundancy: Enumerations.RedundancyContractType.LocalNone,
                        allowCommit: true,
                        privateEncrypted: false)
                    ),
                data: data);

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
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            this.logger.VerifyNoOtherCalls();
        }
    }
}
