using BrightChain.Models.Blocks;
using BrightChain.Services;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Interfaces;
using System;

namespace BrightChain.Interfaces
{
    /// <summary>
    /// Basic members for a block that is to be transactable (currently tied to BPlus tree)
    /// </summary>
    public interface ITransactableBlock : IBlock, ITransactable, IDisposable
    {
        BPlusTreeCacheManager<BlockHash, TransactableBlock> CacheManager { get; }
        void SetCacheManager(BPlusTreeCacheManager<BlockHash, TransactableBlock> cacheManager);
        bool TreeIsEqual(BPlusTree<BlockHash, TransactableBlock> other);
        bool TreeIsSame(BPlusTree<BlockHash, TransactableBlock> other);
    }
}
