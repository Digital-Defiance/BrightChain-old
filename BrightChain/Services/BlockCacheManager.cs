﻿using BrightChain.Helpers;
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
    }
}