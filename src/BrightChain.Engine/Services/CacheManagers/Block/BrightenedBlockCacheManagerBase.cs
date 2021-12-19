using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Services.CacheManagers.Block
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Nodes;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Block Cache Manager.
    /// </summary>
    public abstract partial class BrightenedBlockCacheManagerBase : IBrightenedBlockCacheManager
    {
        /// <summary>
        /// Whether the device and store files will delete on shutdown for testing.
        /// </summary>
        protected readonly bool testingSelfDestruct = false;

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
        ///     Gets a lower classed BlockCacheManager of this object.
        /// </summary>
        public BrightenedBlockCacheManagerBase AsBlockCacheManager => this;

        /// <summary>
        /// Blocks that are in-memory either pending write to the cache or confirmation of no-rollback required.
        /// </summary>
        public readonly Dictionary<BlockHash, BrightenedBlock> UncomittedBlocksByHash;
        /// <summary>
        /// Hashes of concomitted blocks grouped by transactions status.
        /// </summary>
        public readonly Dictionary<TransactionStatus, List<BlockHash>> UncommittedHashesByStatus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BrightenedBlockCacheManagerBase" /> class.
        /// </summary>
        /// <param name="logger">Logging provider.</param>
        /// <param name="configuration">Configuration data.</param>
        /// <param name="rootBlock">Root block definition with authority for the store.</param>
        /// <param name="testingSelfDestruct">Whether to delete store and device files on shutdown.</param>
        public BrightenedBlockCacheManagerBase(ILogger logger, IConfiguration configuration, RootBlock rootBlock, bool testingSelfDestruct = false)
        {
            this.trustedNodes = new List<BrightChainNode>();
            this.Logger = logger;
            this.Configuration = configuration;
            this.RootBlock = rootBlock;
            this.RootBlock.CacheManager = this;
            this.DatabaseName = Utilities.HashToFormattedString(this.RootBlock.Guid.ToByteArray());
            this.testingSelfDestruct = testingSelfDestruct;
            this.UncomittedBlocksByHash = new Dictionary<BlockHash, BrightenedBlock>();
            this.UncommittedHashesByStatus = new Dictionary<TransactionStatus, List<BlockHash>>();

            // TODO: load supported block sizes from configurations, etc.
            var section = this.Configuration.GetSection("NodeOptions");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrightenedBlockCacheManagerBase"/> class.
        /// Blocked parameterless constructor.
        /// </summary>
        private BrightenedBlockCacheManagerBase()
        {
            throw new NotImplementedException();
        }
    }
}
