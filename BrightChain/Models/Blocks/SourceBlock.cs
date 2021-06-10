using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Interfaces;
using BrightChain.Services;
using System;

namespace BrightChain.Models.Blocks
{
    public class SourceBlock : Block
    {
        private ICacheManager<BlockHash, Block> cacheManager;

        public SourceBlock(ICacheManager<BlockHash, Block> destinationCacheManager, ReadOnlyMemory<byte> data) :
            base(requestTime: DateTime.Now, keepUntilAtLeast: DateTime.MinValue, redundancy: RedundancyContractType.LocalNone, data: data)
        {
            this.cacheManager = destinationCacheManager;
        }

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data)
        {
            if (this.cacheManager is MemoryBlockCacheManager memoryBlockCacheManager)
                return new MemoryBlock(
                    cacheManager: memoryBlockCacheManager,
                    requestTime: requestTime,
                    keepUntilAtLeast: keepUntilAtLeast,
                    redundancy: redundancy,
                    data: data);
            else if (this.cacheManager is DiskBlockCacheManager diskBlockCacheManager)
                return new DiskBlock(
                    cacheManager: diskBlockCacheManager,
                    requestTime: requestTime,
                    keepUntilAtLeast: keepUntilAtLeast,
                    redundancy: redundancy,
                    data: data);
            else
                throw new BrightChainException("Unexpected destination cache type");
        }

        public override void Dispose()
        {
        }
    }
}
