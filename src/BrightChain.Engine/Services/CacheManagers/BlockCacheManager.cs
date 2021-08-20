namespace BrightChain.Engine.Services.CacheManagers
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.Chains;
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
        /// List of nodes we trust.
        /// </summary>
        private readonly List<BrightChainNode> trustedNodes;

        /// <summary>
        /// Gets a string with the full path to the config file.
        /// </summary>
        public string ConfigFile { get; private set; }

        /// <summary>
        /// Gets the IConfiguration for this instance.
        /// </summary>
        public IConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets a string with the Database/directory name for this instance's tree root.
        /// </summary>
        public string DatabaseName { get; private set; }

        /// <summary>
        /// Gets the ILogger for this instance.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// gets a RootBlock with authority for this block cache.
        /// </summary>
        public RootBlock RootBlock { get; private set; }

        /// <summary>
        /// Gets a dictionary of block sizes supported for read by this node.
        /// Done as a dictionary instead of a list for fast search.
        /// </summary>
        public Dictionary<BlockSize, bool> SupportedReadBlockSizes { get; private set; }

        /// <summary>
        /// Gets a dictionary of block sizes supported for write by this node.
        /// Done as a dictionary instead of a list for fast search.
        /// </summary>
        public Dictionary<BlockSize, bool> SupportedWriteBlockSizes { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BlockCacheManager" /> class.
        /// </summary>
        /// <param name="logger">Logging provider.</param>
        /// <param name="configuration">Configuration data.</param>
        /// <param name="rootBlock">Root block definition with authority for the store.</param>
        public BlockCacheManager(ILogger logger, IConfiguration configuration, RootBlock rootBlock)
        {
            this.trustedNodes = new List<BrightChainNode>();
            this.Logger = logger;
            this.Configuration = configuration;
            this.RootBlock = rootBlock;
            this.RootBlock.CacheManager = this;
            this.DatabaseName = Utilities.HashToFormattedString(this.RootBlock.Guid.ToByteArray());

            // TODO: load supported block sizes from configurations, etc.
            var section = this.Configuration.GetSection("NodeOptions");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockCacheManager"/> class.
        /// Blocked parameterless constructor.
        /// </summary>
        private BlockCacheManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets a lower classed BlockCacheManager of this object.
        /// </summary>
        public BlockCacheManager AsBlockCacheManager => this;

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

        public TupleStripe GetFromBlockIDs(BlockHash[] blockHashes)
        {
            int i = 0;
            TransactableBlock[] blocks = new TransactableBlock[blockHashes.Length];
            foreach (var hash in blockHashes)
            {
                blocks[i++] = this.Get(hash);
            }

            return new TupleStripe(BlockBrightenerService.TupleCount, blocks[0].BlockSize, blocks);
        }

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

        public virtual void Set(BlockHash key, TransactableBlock value)
        {
            if (value.Id != key)
            {
                throw new BrightChainException("Can not store transactable block with different key");
            }

            this.Set(value);
        }

        public virtual void SetAll(IEnumerable<TransactableBlock> items)
        {
            foreach (var item in items)
            {
                this.Set(item);
            }
        }

        public async virtual void SetAllAsync(IAsyncEnumerable<TransactableBlock> items)
        {
            await foreach (var item in items)
            {
                this.Set(item);
            }
        }

        public virtual void Set(Block value)
        {
            this.Set(value.MakeTransactable(
                cacheManager: this,
                allowCommit: true));
        }

        public virtual void Set(BlockHash key, Block value)
        {
            this.Set(value.MakeTransactable(
                cacheManager: this,
                allowCommit: true));
        }

        public virtual void SetAll(IEnumerable<Block> items)
        {
            foreach (var item in items)
            {
                this.Set(item.MakeTransactable(
                    cacheManager: this,
                    allowCommit: true));
            }
        }

        public async virtual void SetAllAsync(IAsyncEnumerable<Block> items)
        {
            await foreach (var item in items)
            {
                this.Set(item.MakeTransactable(
                    cacheManager: this,
                    allowCommit: true));
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
