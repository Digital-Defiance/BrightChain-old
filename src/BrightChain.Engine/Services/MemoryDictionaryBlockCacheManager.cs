namespace BrightChain.Engine.Services
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Memory based Block Cache Manager.
    /// </summary>
    public class MemoryDictionaryBlockCacheManager : BlockCacheManager
    {
        /// <summary>
        /// Hashtable collection for blocks stored in memory
        /// </summary>
        private readonly Dictionary<BlockHash, TransactableBlock> blocks = new Dictionary<BlockHash, TransactableBlock>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryDictionaryBlockCacheManager"/> class.
        /// </summary>
        /// <param name="logger">Instance of the logging provider</param>
        public MemoryDictionaryBlockCacheManager(ILogger logger, IConfiguration configuration)
            : base(logger: logger, configuration: configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryDictionaryBlockCacheManager"/> class.
        /// Can not build a cache manager with no logger.
        /// </summary>
        private MemoryDictionaryBlockCacheManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fired whenever a block is added to the cache
        /// </summary>
        public override event ICacheManager<BlockHash, TransactableBlock>.KeyAddedEventHandler KeyAdded;

        /// <summary>
        /// Fired whenever a block is expired from the cache
        /// </summary>
        public override event ICacheManager<BlockHash, TransactableBlock>.KeyExpiredEventHandler KeyExpired;

        /// <summary>
        /// Fired whenever a block is removed from the collection
        /// </summary>
        public override event ICacheManager<BlockHash, TransactableBlock>.KeyRemovedEventHandler KeyRemoved;

        /// <summary>
        /// Fired whenever a block is requested from the cache but is not present.
        /// </summary>
        public override event ICacheManager<BlockHash, TransactableBlock>.CacheMissEventHandler CacheMiss;

        /// <summary>
        /// Returns whether the cache manager has the given key and it is not expired.
        /// </summary>
        /// <param name="key">key to check the collection for.</param>
        /// <returns>boolean with whether key is present.</returns>
        public override bool Contains(BlockHash key)
        {
            return this.blocks.ContainsKey(key);
        }

        /// <summary>
        /// Removes a key from the cache and returns a boolean wither whether it was actually present.
        /// </summary>
        /// <param name="key">key to drop from the collection.</param>
        /// <param name="noCheckContains">Skips the contains check for performance.</param>
        /// <returns>whether requested key was present and actually dropped.</returns>
        public override bool Drop(BlockHash key, bool noCheckContains = true)
        {
            if (!noCheckContains && !this.Contains(key))
            {
                return false;
            }

            this.blocks.Remove(key);
            return true;
        }

        /// <summary>
        /// Retrieves a block from the cache if it is present
        /// </summary>
        /// <param name="key">key to retrieve</param>
        /// <returns>returns requested block or throws</returns>
        public override TransactableBlock Get(BlockHash key)
        {
            TransactableBlock block;
            bool found = this.blocks.TryGetValue(key, out block);
            if (!found)
            {
                throw new IndexOutOfRangeException(nameof(key));
            }

            return block;
        }

        /// <summary>
        /// Adds a key to the cache if it is not already present
        /// </summary>
        /// <param name="block">block to palce in the cache</param>
        public override void Set(TransactableBlock block)
        {
            if (block is null)
            {
                throw new BrightChain.Engine.Exceptions.BrightChainException("Can not store null block");
            }

            if (Contains(block.Id))
            {
                throw new BrightChain.Engine.Exceptions.BrightChainException("Key already exists");
            }

            this.blocks[block.Id] = block;
        }

        public void CopyContent(BlockCacheManager destinationCache)
        {
            foreach (BlockHash key in this.blocks.Keys)
            {
                destinationCache.Set(this.blocks[key]);
            }
        }

        public IEnumerable<BlockHash> Keys
        {
            get
            {
                BlockHash[] hashArray = new BlockHash[this.blocks.Keys.Count];
                this.blocks.Keys.CopyTo(hashArray, 0);
                return hashArray;
            }
        }
    }
}
