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
        /// <summary>
        /// Associated cache manager for this block
        /// </summary>
        BPlusTreeCacheManager<BlockHash, TransactableBlock> CacheManager { get; }
        /// <summary>
        /// Update the cache manager association for the block
        /// </summary>
        /// <param name="cacheManager"></param>
        void SetCacheManager(BPlusTreeCacheManager<BlockHash, TransactableBlock> cacheManager);
        /// <summary>
        /// Whether this tree node is on a tree equal to the other cache's tree
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool TreeIsEqual(BPlusTree<BlockHash, TransactableBlock> other);
        /// <summary>
        /// Whether this tree node is on the exact same tree as the other node
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool TreeIsSame(BPlusTree<BlockHash, TransactableBlock> other);
        /// <summary>
        /// Whether this block has been committed to the block store
        /// </summary>
        bool Committed { get; }
        /// <summary>
        /// Whether this block should be allowed to be committed to the block store
        /// </summary>
        bool AllowCommit { get; }
    }
}
