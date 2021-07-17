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
        public DiskBlock(TransactableBlockParams blockParams, ReadOnlyMemory<byte> data)
            : base(
                blockParams: blockParams,
                data: data)
        {
            if (!(this.CacheManager is DiskBlockCacheManager))
            {
                throw new BrightChainException(this.CacheManager.GetType().Name);
            }

            this.CacheManager.Set(this);
        }

        public override DiskBlock NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            return new DiskBlock(
                blockParams: new TransactableBlockParams(
                    cacheManager: this.CacheManager,
                    allowCommit: this.AllowCommit,
                    blockParams: blockParams),
                data: data);
        }

        public override void Dispose()
        {
        }
    }
}
