using System;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BrightChain.Engine.Services
{
    /// <summary>
    /// Block Cache Manager
    /// </summary>
    public abstract class BlockCacheManager : ICacheManager<BlockHash, TransactableBlock>
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockCacheManager"/> class.
        /// </summary>
        /// <param name="logger">Logging provider</param>
        /// <param name="configuration">Configuration data</param>
        public BlockCacheManager(ILogger logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockCacheManager"/> class.
        /// Not implemented to prevent empty logger.
        /// </summary>
        protected BlockCacheManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fired whenever a block is added to the cache
        /// </summary>
        public abstract event ICacheManager<BlockHash, TransactableBlock>.KeyAddedEventHandler KeyAdded;

        /// <summary>
        /// Fired whenever a block is expired from the cache
        /// </summary>
        public abstract event ICacheManager<BlockHash, TransactableBlock>.KeyExpiredEventHandler KeyExpired;

        /// <summary>
        /// Fired whenever a block is removed from the collection
        /// </summary>
        public abstract event ICacheManager<BlockHash, TransactableBlock>.KeyRemovedEventHandler KeyRemoved;

        /// <summary>
        /// Fired whenever a block is requested from the cache but is not present.
        /// </summary>
        public abstract event ICacheManager<BlockHash, TransactableBlock>.CacheMissEventHandler CacheMiss;

        /// <summary>
        /// Gets a lower classed BlockCacheManager of this object.
        /// </summary>
        public BlockCacheManager AsBlockCacheManager => this;

        /// <summary>
        /// Returns whether the cache manager has the given key and it is not expired
        /// </summary>
        /// <param name="key">key to check the collection for</param>
        /// <returns>boolean with whether key is present</returns>
        public abstract bool Contains(BlockHash key);

        /// <summary>
        /// Removes a key from the cache and returns a boolean wither whether it was actually present.
        /// </summary>
        /// <param name="key">Key to drop from the collection.</param>
        /// <param name="noCheckContains">Skips the contains check for performance.</param>
        /// <returns>Whether requested key was present and actually dropped.</returns>
        public abstract bool Drop(BlockHash key, bool noCheckContains = true);

        /// <summary>
        /// Retrieves a block from the cache if it is present.
        /// </summary>
        /// <param name="key">key to retrieve.</param>
        /// <returns>returns requested block or throws.</returns>
        public abstract TransactableBlock Get(BlockHash key);

        /// <summary>
        /// Adds a key to the cache if it is not already present.
        /// </summary>
        /// <param name="value">block to palce in the cache.</param>
        public abstract void Set(TransactableBlock value);
    }
}
