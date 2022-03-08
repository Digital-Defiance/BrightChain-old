using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Services.CacheManagers.Block;

/// <summary>
///     Block Cache Manager.
/// </summary>
public abstract partial class BrightenedBlockCacheManagerBase : IBrightenedBlockCacheManager
{
    /// <summary>
    ///     Fired whenever a block is added to the cache
    /// </summary>
    public abstract event ICacheManager<BlockHash, BrightenedBlock>.KeyAddedEventHandler KeyAdded;

    /// <summary>
    ///     Fired whenever a block is expired from the cache
    /// </summary>
    public abstract event ICacheManager<BlockHash, BrightenedBlock>.KeyExpiredEventHandler KeyExpired;

    /// <summary>
    ///     Fired whenever a block is removed from the collection
    /// </summary>
    public abstract event ICacheManager<BlockHash, BrightenedBlock>.KeyRemovedEventHandler KeyRemoved;

    /// <summary>
    ///     Fired whenever a block is requested from the cache but is not present.
    /// </summary>
    public abstract event ICacheManager<BlockHash, BrightenedBlock>.CacheMissEventHandler CacheMiss;
}
