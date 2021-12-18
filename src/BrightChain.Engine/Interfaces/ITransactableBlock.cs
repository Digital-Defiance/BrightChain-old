using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Interfaces
{
    /// <summary>
    /// Basic members for a block that is to be transactable (currently tied to BPlus tree)
    /// </summary>
    public interface ITransactableBlock : IBlock, ITransactable, IDisposable
    {
        /// <summary>
        /// Associated cache manager for this block
        /// </summary>
        ICacheManager<BlockHash, BrightenedBlock> CacheManager { get; }
        /// <summary>
        /// Update the cache manager association for the block
        /// </summary>
        /// <param name="cacheManager"></param>
        void SetCacheManager(ICacheManager<BlockHash, BrightenedBlock> cacheManager);
        /// <summary>
        /// Whether this block has been committed to the block store
        /// </summary>
        TransactionStatus State { get; }
    }
}
