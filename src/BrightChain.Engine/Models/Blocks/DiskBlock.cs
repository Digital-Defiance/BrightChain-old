namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Services.CacheManagers;

    /// <summary>
    /// Block that can be contained in a DiskBlockCacheManager / Btree
    /// </summary>
    public class DiskBlock : TransactableBlock, IBlock
    {
        public new DiskBlockCacheManager CacheManager { get; internal set; }

        public DiskBlock(TransactableBlockParams blockParams, ReadOnlyMemory<byte> data)
            : base(
                blockParams: blockParams,
                data: data)
        {
            this.CacheManager.Set(this);
            //var blockpath = this.CacheManager.GetBlockPath(this.Id);
            this.OriginalType = typeof(DiskBlock).AssemblyQualifiedName;
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
