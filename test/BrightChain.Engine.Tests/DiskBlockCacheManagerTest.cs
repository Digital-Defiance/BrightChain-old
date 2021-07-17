using System;
using System.Collections.Generic;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrightChain.Engine.Tests
{
    /// <summary>
    /// Serializable testable test block class
    /// </summary>
    public class DiskCacheTestBlock : TransactableBlock
    {
        public static new DiskBlockCacheManager CacheManager;

        public DiskCacheTestBlock(TransactableBlockParams blockParams, ReadOnlyMemory<byte> data)
            : base(
                blockParams: blockParams,
                data: data)
        {
        }

        internal DiskCacheTestBlock()
            : base(
                blockParams: new TransactableBlockParams(
                    cacheManager: DiskCacheTestBlock.CacheManager,
                    allowCommit: true,
                    blockParams: new BlockParams(
                    blockSize: BlockSize.Message,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: RedundancyContractType.HeapAuto,
                    privateEncrypted: false)),
                data: NewRandomData())
        {
        }

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

        public override DiskCacheTestBlock NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            return new DiskCacheTestBlock(
                blockParams: new TransactableBlockParams(
                cacheManager: DiskCacheTestBlock.CacheManager,
                allowCommit: this.AllowCommit,
                blockParams: blockParams),
                data: data);
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Tests disk block cache managers
    /// </summary>
    [TestClass]
    public class DiskBlockCacheManagerTest : TransactableBlockCacheManagerTest<DiskBlockCacheManager>
    {
        [TestInitialize]
        public new void PreTestSetup()
        {
            base.PreTestSetup();
            DiskCacheTestBlock.CacheManager = new DiskBlockCacheManager(logger.Object, configuration.Object);
            cacheManager = DiskCacheTestBlock.CacheManager;
        }

        internal override DiskBlockCacheManager NewCacheManager(ILogger logger, IConfiguration configuration)
        {
            return new DiskBlockCacheManager(
logger: logger, configuration: configuration);
        }

        internal override KeyValuePair<BlockHash, TransactableBlock> NewKeyValue()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var data = new byte[BlockSizeMap.BlockSize(BlockSize.Message)];
            for (int i = 0; i < BlockSizeMap.BlockSize(BlockSize.Message); i++)
            {
                data[i] = (byte)random.Next(0, 255);
            }

            var block = new DiskCacheTestBlock(
                blockParams: new TransactableBlockParams(
                    cacheManager: this.cacheManager,
                    allowCommit: true,
                    blockParams: new BlockParams(
                        blockSize: BlockSize.Message,
                        requestTime: DateTime.Now,
                        keepUntilAtLeast: DateTime.MaxValue,
                        redundancy: Enumerations.RedundancyContractType.LocalNone,
                        privateEncrypted: false)
                    ),
                data: data);

            return new KeyValuePair<BlockHash, TransactableBlock>(block.Id, block);
        }

        internal override DiskCacheTestBlock NewNullData()
        {
            return null;
        }

        /// <summary>
        /// Tries to push a null value into the cache and expects an exception.
        /// </summary>
        [TestMethod]
        public override void ItPutsNullValuesTest()
        {
            // Arrange
            var newData = NewNullData();

            // Act/Expect
            Exceptions.BrightChainException brightChainException = Assert.ThrowsException<BrightChain.Engine.Exceptions.BrightChainException>(() =>
                cacheManager.Set(newData));

            logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(0));
            logger.VerifyNoOtherCalls();
        }
    }
}
