using BrightChain.Enumerations;
using BrightChain.Interfaces;
using BrightChain.Services;
using System;

namespace BrightChain.Models.Blocks
{
    public class DiskBlock : TransactableBlock, IBlock
    {
        public DiskBlock(DiskBlockCacheManager cacheManager, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data) :
            base(
                tree: cacheManager.tree,
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                redundancy: redundancy,
                data: data)
        {
            this.cacheManager = cacheManager;
            this.cacheManager.Set(this.Id, this);
        }

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data)
        {
            return new DiskBlock(
                cacheManager: (DiskBlockCacheManager)this.cacheManager,
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                redundancy: redundancy,
                data: data);
        }

        public override void Dispose()
        {

        }
    }
}