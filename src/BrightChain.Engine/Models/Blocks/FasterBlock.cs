namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Services.CacheManagers;

    /// <summary>
    /// Block that can be contained in a FasterBlockCacheManager
    /// </summary>
    public class FasterBlock : TransactableBlock, IBlock
    {
        public new FasterBlockCacheManager CacheManager { get; internal set; }

        public FasterBlock(TransactableBlockParams blockParams, ReadOnlyMemory<byte> data)
            : base(
                blockParams: blockParams,
                data: data)
        {
            this.CacheManager.Set(this);
            this.OriginalType = typeof(FasterBlock).AssemblyQualifiedName;
        }

        public override FasterBlock NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            return new FasterBlock(
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
