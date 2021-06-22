using BrightChain.Models.Blocks;
using CSharpTest.Net.Collections;
using Microsoft.Extensions.Logging;

namespace BrightChain.Services
{
    /// <summary>
    /// Thin wrapper to parallel DiskCacheManager and provide appropriate BTree options
    /// </summary>
    public class MemoryBlockCacheManager : BlockCacheManager
    {
        public MemoryBlockCacheManager(ILogger logger, BPlusTree<BlockHash, TransactableBlock>.OptionsV2 optionsV2 = null) :
            base(
                logger: logger,
                tree: optionsV2 is null ? new BPlusTree<BlockHash, TransactableBlock>() : new BPlusTree<BlockHash, TransactableBlock>(optionsV2: optionsV2))
        { }

        public MemoryBlockCacheManager(BlockCacheManager other) : base(other) { }

        public MemoryBlockCacheManager(ILogger logger, BPlusTree<BlockHash, TransactableBlock> tree) :
            base(logger: logger, tree: tree)
        { }
    }
}
