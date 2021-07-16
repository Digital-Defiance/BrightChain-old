using System;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services;

namespace BrightChain.Engine.Models.Blocks
{
    /// <summary>
    /// Block that can be contained in a DiskBlockCacheManager / Btree
    /// </summary>
    public class DiskBlock : TransactableBlock, IBlock
    {
        public DiskBlock(TransactableBlockParams blockParams, ReadOnlyMemory<byte> data) :
            base(
                blockParams: blockParams,
                data: data)
        {
            if (!(CacheManager is DiskBlockCacheManager))
            {
                throw new BrightChainException(CacheManager.GetType().Name);
            }

            CacheManager.Set(this);
        }

        public override Block NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            return new DiskBlock(
                blockParams: new TransactableBlockParams(
                    cacheManager: CacheManager,
                    blockParams: blockParams),
                data: data);
        }

        public override void Dispose()
        {
        }
    }
}
