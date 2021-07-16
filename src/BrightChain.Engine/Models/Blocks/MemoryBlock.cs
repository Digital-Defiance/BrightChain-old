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
        public MemoryBlock(TransactableBlockParams blockArguments, ReadOnlyMemory<byte> data) :
            base(
                blockArguments: blockArguments,
                data: data)
        {
            if (!(CacheManager is MemoryBlockCacheManager))
            {
                throw new BrightChainException(CacheManager.GetType().Name);
            }

            CacheManager.Set(Id, this);
        }

        public override Block NewBlock(BlockParams blockArguments, ReadOnlyMemory<byte> data)
        {
            return new MemoryBlock(
blockArguments: new TransactableBlockParams(CacheManager, blockArguments),
data: data);
        }

        public override void Dispose()
        {
        }
    }
}
