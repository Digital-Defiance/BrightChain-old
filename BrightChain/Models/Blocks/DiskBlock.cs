using BrightChain.Enumerations;
using BrightChain.Interfaces;
using BrightChain.Services;
using System;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Block associated with a disk based bplus tree cache
    /// </summary>
    public class DiskBlock : TransactableBlock, IBlock
    {
        public DiskBlock(ICacheManager<BlockHash, TransactableBlock> cacheManager, BlockSize blockSize, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) :
            base(
                cacheManager: cacheManager,
                blockSize: blockSize,
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                redundancy: redundancy,
                data: data,
                allowCommit: allowCommit)
        {
            if (!(cacheManager is DiskBlockCacheManager))
            {
                throw new InvalidCastException();
            }

            this.CacheManager.Set(this.Id, this);
        }

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) => new DiskBlock(
                cacheManager: this.CacheManager,
                blockSize: this.BlockSize,
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                redundancy: redundancy,
                data: data,
                allowCommit: allowCommit);

        public override void Dispose()
        {

        }
    }
}