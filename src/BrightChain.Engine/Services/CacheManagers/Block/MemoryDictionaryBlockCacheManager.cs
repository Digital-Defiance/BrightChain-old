using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Hashes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NeuralFabric.Models.Hashes;

namespace BrightChain.Engine.Services.CacheManagers.Block;

/// <summary>
///     Memory based Block Cache Manager.
/// </summary>
public class MemoryDictionaryBlockCacheManager : BrightenedBlockCacheManagerBase
{
    /// <summary>
    ///     Hashtable collection for blocks stored in memory
    /// </summary>
    private readonly Dictionary<BlockHash, BrightenedBlock> blocks = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemoryDictionaryBlockCacheManager" /> class.
    /// </summary>
    /// <param name="logger">Instance of the logging provider.</param>
    /// <param name="configuration">Instance of the configuration provider.</param>
    public MemoryDictionaryBlockCacheManager(ILogger logger, IConfiguration configuration, RootBlock rootBlock)
        : base(logger: logger,
            configuration: configuration,
            rootBlock: rootBlock)
    {
    }

    /// <summary>
    ///     Fired whenever a block is added to the cache
    /// </summary>
    public override event ICacheManager<BlockHash, BrightenedBlock>.KeyAddedEventHandler KeyAdded;

    /// <summary>
    ///     Fired whenever a block is expired from the cache
    /// </summary>
    public override event ICacheManager<BlockHash, BrightenedBlock>.KeyExpiredEventHandler KeyExpired;

    /// <summary>
    ///     Fired whenever a block is removed from the collection
    /// </summary>
    public override event ICacheManager<BlockHash, BrightenedBlock>.KeyRemovedEventHandler KeyRemoved;

    /// <summary>
    ///     Fired whenever a block is requested from the cache but is not present.
    /// </summary>
    public override event ICacheManager<BlockHash, BrightenedBlock>.CacheMissEventHandler CacheMiss;

    /// <summary>
    ///     Returns whether the cache manager has the given key and it is not expired.
    /// </summary>
    /// <param name="key">key to check the collection for.</param>
    /// <returns>boolean with whether key is present.</returns>
    public override bool Contains(BlockHash key)
    {
        return this.blocks.ContainsKey(key: key);
    }

    /// <summary>
    ///     Removes a key from the cache and returns a boolean wither whether it was actually present.
    /// </summary>
    /// <param name="key">key to drop from the collection.</param>
    /// <param name="noCheckContains">Skips the contains check for performance.</param>
    /// <returns>whether requested key was present and actually dropped.</returns>
    public override bool Drop(BlockHash key, bool noCheckContains = true)
    {
        if (!base.Drop(key: key,
                noCheckContains: noCheckContains))
        {
            return false;
        }

        this.blocks.Remove(key: key);
        return true;
    }

    /// <summary>
    ///     Retrieves a block from the cache if it is present
    /// </summary>
    /// <param name="blockHash">key to retrieve</param>
    /// <returns>returns requested block or throws</returns>
    public override BrightenedBlock Get(BlockHash blockHash)
    {
        BrightenedBlock block;
        var found = this.blocks.TryGetValue(key: blockHash,
            value: out block);
        if (!found)
        {
            throw new IndexOutOfRangeException(message: blockHash.ToString());
        }

        if (!block.Validate())
        {
            throw new BrightChainValidationEnumerableException(
                exceptions: block.ValidationExceptions,
                message: "Will not return invalid block. Is store corrupt?");
        }

        return block;
    }

    /// <summary>
    ///     Adds a key to the cache if it is not already present
    /// </summary>
    /// <param name="block">block to palce in the cache</param>
    public override void Set(BrightenedBlock block, bool updateMetadataOnly = false)
    {
        base.Set(value: block,
            updateMetadataOnly: updateMetadataOnly);
        this.blocks[key: block.Id] = block;
    }

    public async Task CopyContentAsync(BrightenedBlockCacheManagerBase destinationCache)
    {
        await foreach (var key in this.KeysAsync())
        {
            destinationCache.Set(value: this.Get(blockHash: key));
        }
    }

    public async IAsyncEnumerable<BlockHash> KeysAsync()
    {
        foreach (var key in this.blocks.Keys)
        {
            yield return key;
        }
    }

    public override BrightHandle GetCbl(DataHash sourceHash)
    {
        throw new NotImplementedException();
    }

    public override void SetCbl(BlockHash cblHash, DataHash dataHash, BrightHandle brightHandle)
    {
        throw new NotImplementedException();
    }

    public override BrightHandle GetCbl(Guid correlationID)
    {
        throw new NotImplementedException();
    }

    public override List<BlockHash> GetBlocksExpiringAt(long date)
    {
        return default;
    }

    public override void AddExpiration(BrightenedBlock block, bool noCheckContains = false)
    {
    }

    public override void RemoveExpiration(BrightenedBlock block)
    {
    }

    public override void ExpireBlocks(long date)
    {
    }

    public override void ExpireBlocksThrough(long date)
    {
    }
}
