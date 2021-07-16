using System;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services;

namespace BrightChain.Engine.Models.Blocks
{
    /// <summary>
    /// Block that can be contained in a MemoryBlockCacheManager / Btree
    /// </summary>
    public class MemoryBlock : TransactableBlock, IBlock
    {
        public MemoryBlock(TransactableBlockParams blockParams, ReadOnlyMemory<byte> data) :
            base(
                blockParams: blockParams,
                data: data)
        {
            if (!(CacheManager is MemoryBlockCacheManager))
            {
                throw new BrightChainException(CacheManager.GetType().Name);
            }

            CacheManager.Set(this);
        }

        public override Block NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            return new MemoryBlock(
blockParams: new TransactableBlockParams(CacheManager, blockParams),
data: data);
        }

        public override void Dispose()
        {
        }
    }
}
