using System.Collections.Generic;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;
using BrightChain.Engine.Models.Nodes;

namespace BrightChain.Engine.Interfaces;

/// <summary>
///     Basic guaranteed members of the cache system.
/// </summary>
public interface IBrightenedBlockCacheManager : ICacheManager<BlockHash, BrightenedBlock>
{
    /// <summary>
    ///     Adds a key to the cache if it is not already present
    /// </summary>
    /// <param name="key">key to palce in the cache.</param>
    /// <param name="updateMetadataOnly">whether to allow duplicate to update metadata.</param>
    void Set(BrightenedBlock value, bool updateMetadataOnly = false);

    /// <summary>
    ///     Adds all keys to the cache if not already present
    /// </summary>
    /// <param name="key">key to palce in the cache</param>
    void SetAll(IEnumerable<BrightenedBlock> value);

    /// <summary>
    ///     Adds all keys to the cache if not already present
    /// </summary>
    /// <param name="key">key to palce in the cache</param>
    void SetAllAsync(IAsyncEnumerable<BrightenedBlock> value);

    /// <summary>
    ///     Add a node that the cache manager should trust.
    /// </summary>
    /// <param name="node">Node submitting the block to the cache.</param>
    void Trust(BrightChainNode node);
}
