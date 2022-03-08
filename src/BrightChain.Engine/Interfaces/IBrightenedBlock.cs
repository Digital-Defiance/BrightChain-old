using System;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Interfaces;

/// <summary>
///     Basic members for a block that is to be transactable.
/// </summary>
public interface IBrightenedBlock : ITransactableBlock, ITransactable, IDisposable
{
    /// <summary>
    ///     Associated cache manager for this block
    /// </summary>
    ICacheManager<BlockHash, BrightenedBlock> CacheManager { get; }

    /// <summary>
    ///     Whether this block has been committed to the block store
    /// </summary>
    bool Committed { get; }

    /// <summary>
    ///     Whether this block should be allowed to be committed to the block store
    /// </summary>
    bool AllowCommit { get; }

    /// <summary>
    ///     Update the cache manager association for the block
    /// </summary>
    /// <param name="cacheManager"></param>
    void SetCacheManager(ICacheManager<BlockHash, BrightenedBlock> cacheManager);
}
