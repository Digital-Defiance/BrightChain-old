using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Helpers;
using BrightChain.Interfaces;
using BrightChain.Services;
using System;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// User data that must be whitened with the block whitener before being persisted. These blocks must never be stored directly.
    /// *** CBLs are considered user data ***
    /// </summary>
    public class SourceBlock : TransactableBlock, IComparable<SourceBlock>, IComparable<Block>
    {
        private ICacheManager<BlockHash, TransactableBlock> cacheManager;

        public SourceBlock(ICacheManager<BlockHash, TransactableBlock> destinationCacheManager, BlockSize blockSize, ReadOnlyMemory<byte> data) :
            base(cacheManager: destinationCacheManager, blockSize: blockSize, requestTime: DateTime.Now, keepUntilAtLeast: DateTime.MinValue, redundancy: RedundancyContractType.LocalNone, data: data, allowCommit: false) => this.cacheManager = destinationCacheManager;

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool _)
        {
            if (this.cacheManager is MemoryBlockCacheManager memoryBlockCacheManager)
            {
                return new MemoryBlock(
                    cacheManager: memoryBlockCacheManager,
                    blockSize: this.BlockSize,
                    requestTime: requestTime,
                    keepUntilAtLeast: keepUntilAtLeast,
                    redundancy: redundancy,
                    data: data,
                    allowCommit: false);
            }
            else if (this.cacheManager is DiskBlockCacheManager diskBlockCacheManager)
            {
                return new DiskBlock(
                    cacheManager: diskBlockCacheManager,
                    blockSize: this.BlockSize,
                    requestTime: requestTime,
                    keepUntilAtLeast: keepUntilAtLeast,
                    redundancy: redundancy,
                    data: data,
                    allowCommit: false);
            }
            else
            {
                throw new BrightChainException("Unexpected destination cache type");
            }
        }

        public int CompareTo(SourceBlock other) => ReadOnlyMemoryComparer<byte>.Compare(this.Data, other.Data);

        public override void Dispose()
        {
        }
    }
}
