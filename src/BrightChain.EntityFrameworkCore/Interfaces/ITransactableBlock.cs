using BrightChain.Models.Blocks;
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
        ICacheManager<BlockHash, TransactableBlock> CacheManager { get; }
        /// <summary>
        /// Update the cache manager association for the block
        /// </summary>
        /// <param name="cacheManager"></param>
        void SetCacheManager(ICacheManager<BlockHash, TransactableBlock> cacheManager);
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
