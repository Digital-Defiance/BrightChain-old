namespace BrightChain.Engine.Services.CacheManagers
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Hashes;
    using BrightChain.Engine.Models.Nodes;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Block Cache Manager.
    /// </summary>
    public abstract class BlockCacheManager : IBlockCacheManager
    {
        /// <summary>
        ///     Full to the config file.
        /// </summary>
        protected readonly string configFile;

        private readonly IConfiguration configuration;

        /// <summary>
        ///     Database/directory name for this instance's tree root.
        /// </summary>
        protected readonly string databaseName;

        private readonly ILogger logger;
        private readonly List<BlockSize> supportedReadBlockSizes;
        private readonly List<BlockSize> supportedWriteBlockSizes;
        private readonly List<BrightChainNode> trustedNodes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BlockCacheManager" /> class.
        /// </summary>
        /// <param name="logger">Logging provider</param>
        /// <param name="configuration">Configuration data</param>
        public BlockCacheManager(ILogger logger, IConfiguration configuration, RootBlock rootBlock)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.trustedNodes = new List<BrightChainNode>();
            // TODO: load supported block sizes from configurations
            var section = this.configuration.GetSection("NodeOptions");
            this.RootBlock = rootBlock;
            this.RootBlock.CacheManager = this;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BlockCacheManager" /> class.
        ///     Not implemented to prevent empty logger.
        /// </summary>
        protected BlockCacheManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets a lower classed BlockCacheManager of this object.
        /// </summary>
        public BlockCacheManager AsBlockCacheManager => this;

        public RootBlock RootBlock { get; }

        /// <summary>
        ///     Fired whenever a block is added to the cache
        /// </summary>
        public abstract event ICacheManager<BlockHash, TransactableBlock>.KeyAddedEventHandler KeyAdded;

        /// <summary>
        ///     Fired whenever a block is expired from the cache
        /// </summary>
        public abstract event ICacheManager<BlockHash, TransactableBlock>.KeyExpiredEventHandler KeyExpired;

        /// <summary>
        ///     Fired whenever a block is removed from the collection
        /// </summary>
        public abstract event ICacheManager<BlockHash, TransactableBlock>.KeyRemovedEventHandler KeyRemoved;

        /// <summary>
        ///     Fired whenever a block is requested from the cache but is not present.
        /// </summary>
        public abstract event ICacheManager<BlockHash, TransactableBlock>.CacheMissEventHandler CacheMiss;

        /// <summary>
        ///     Gets a value indicating whether to only accept blocks from trusted nodes.
        /// </summary>
        public bool OnlyAcceptBlocksFromTrustedNodes { get; }

        /// <summary>
        ///     Returns whether the cache manager has the given key and it is not expired
        /// </summary>
        /// <param name="key">key to check the collection for</param>
        /// <returns>boolean with whether key is present</returns>
        public abstract bool Contains(BlockHash key);

        /// <summary>
        ///     Removes a key from the cache and returns a boolean wither whether it was actually present.
        /// </summary>
        /// <param name="key">Key to drop from the collection.</param>
        /// <param name="noCheckContains">Skips the contains check for performance.</param>
        /// <returns>Whether requested key was present and actually dropped.</returns>
        public virtual bool Drop(BlockHash key, bool noCheckContains = true)
        {
            if (!noCheckContains && !this.Contains(key))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Retrieves a block from the cache if it is present.
        /// </summary>
        /// <param name="key">key to retrieve.</param>
        /// <returns>returns requested block or throws.</returns>
        public abstract TransactableBlock Get(BlockHash key);

        /// <summary>
        ///     Adds a key to the cache if it is not already present.
        /// </summary>
        /// <param name="value">block to palce in the cache.</param>
        public virtual void Set(TransactableBlock value)
        {
            if (value is null)
            {
                throw new BrightChainException("Can not store null block");
            }

            if (this.Contains(value.Id))
            {
                throw new BrightChainException("Key already exists");
            }

            if (!value.Validate())
            {
                throw new BrightChainValidationEnumerableException(
                    value.ValidationExceptions,
                    "Can not store invalid block");
            }

            if (value is RootBlock rootBlock)
            {
                throw new BrightChainException(nameof(rootBlock.Id));
            }
        }

        public void SetAll(IEnumerable<TransactableBlock> items)
        {
            foreach (var item in items)
            {
                this.Set(item);
            }
        }

        public async void SetAllAsync(IAsyncEnumerable<TransactableBlock> items)
        {
            await foreach (var item in items)
            {
                this.Set(item);
            }
        }

        /// <summary>
        ///     Add a node that the cache manager should trust.
        /// </summary>
        /// <param name="node">Node submitting the block to the cache.</param>
        public void Trust(BrightChainNode node)
        {
            this.trustedNodes.Add(node);
        }

        /// <summary>
        ///     Returns the maximum number of bytes storable for a given block size.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static long MaximumStorageLength(BlockSize blockSize)
        {
            var iBlockSize = BlockSizeMap.BlockSize(blockSize);
            var hashesPerBlockSquared = BlockSizeMap.HashesPerBlock(blockSize, 2);

            // right now, we can contain 1 SuperCBL with hashesPerSegment blocks, and up to hashesPerSegment blocks there.
            // this means total size is hashes^2*size
            return hashesPerBlockSquared * iBlockSize;
        }
    }
}
