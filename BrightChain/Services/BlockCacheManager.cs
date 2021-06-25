using BrightChain.Helpers;
using BrightChain.Interfaces;
using BrightChain.Models.Blocks;
using CSharpTest.Net.Collections;
using Microsoft.Extensions.Logging;

namespace BrightChain.Services
{
    /// <summary>
    /// Block Cache Manager
    /// </summary>
    public class BlockCacheManager : BPlusTreeCacheManager<BlockHash, TransactableBlock, BlockHashSerializer, BlockSerializer<TransactableBlock>>, IBPlusTreeCacheManager<BlockHash, TransactableBlock>
    {
        public BlockCacheManager(ILogger logger, BPlusTree<BlockHash, TransactableBlock> tree) : base(logger, tree) { }

        public BlockCacheManager(BPlusTreeCacheManager<BlockHash, TransactableBlock, BlockHashSerializer, BlockSerializer<TransactableBlock>> other) : base(other) { }

        public BlockCacheManager AsBlockCacheManager { get => this as BlockCacheManager; }

        public void Set(TransactableBlock block) =>
            base.Set(block.Id, block);

        public new void Set(BlockHash key, TransactableBlock value)
        {
            if (value is null)
                throw new BrightChain.Exceptions.BrightChainException("Can not set null value");
            if (key != value.Id)
                throw new BrightChain.Exceptions.BrightChainException("Key does not match supplied block");
            base.Set(value.Id, value);
        }
    }
}
