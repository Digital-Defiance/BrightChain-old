using BrightChain.Exceptions;
using BrightChain.Interfaces;
using BrightChain.Models.Blocks.DataObjects;
using BrightChain.Services;
using System;

namespace BrightChain.Models.Blocks
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
            if (!(this.CacheManager is MemoryBlockCacheManager))
            {
                throw new BrightChainException(this.CacheManager.GetType().Name);
            }

            this.CacheManager.Set(this.Id, this);
        }

        public override Block NewBlock(BlockParams blockArguments, ReadOnlyMemory<byte> data) =>
            new MemoryBlock(
                blockArguments: new TransactableBlockParams(this.CacheManager, blockArguments),
            data: data);

        public override void Dispose()
        {
        }
    }
}