using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Helpers;
using BrightChain.Services;
using System;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// User data that must be whitened with the block whitener before being persisted. These blocks must never be stored directly.
    /// </summary>
    public class SourceBlock : Block
    {
        private BPlusTreeCacheManager<BlockHash, TransactableBlock, BlockHashSerializer, BlockSerializer<TransactableBlock>> cacheManager;

        public SourceBlock(BPlusTreeCacheManager<BlockHash, TransactableBlock, BlockHashSerializer, BlockSerializer<TransactableBlock>> destinationCacheManager, ReadOnlyMemory<byte> data) :
            base(requestTime: DateTime.Now, keepUntilAtLeast: DateTime.MinValue, redundancy: RedundancyContractType.LocalNone, data: data)
        {
            this.cacheManager = destinationCacheManager;
        }

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool _)
        {
            if (this.cacheManager is MemoryBlockCacheManager memoryBlockCacheManager)
                return new MemoryBlock(
                    cacheManager: memoryBlockCacheManager,
                    requestTime: requestTime,
                    keepUntilAtLeast: keepUntilAtLeast,
                    redundancy: redundancy,
                    data: data,
                    allowCommit: false);
            else if (this.cacheManager is DiskBlockCacheManager diskBlockCacheManager)
                return new DiskBlock(
                    cacheManager: diskBlockCacheManager,
                    requestTime: requestTime,
                    keepUntilAtLeast: keepUntilAtLeast,
                    redundancy: redundancy,
                    data: data,
                    allowCommit: false);
            else
                throw new BrightChainException("Unexpected destination cache type");
        }

        public override void Dispose()
        {
        }
    }
}
