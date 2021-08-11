﻿namespace BrightChain.Engine.Services.CacheManagers
{
    using System;
    using System.Globalization;
    using System.IO;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Faster;
    using BrightChain.Engine.Faster.Serializers;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Relatively naive Disk Based Block Cache Manager.
    /// </summary>
    public class FasterBlockCacheManager : BlockCacheManager, IDisposable
    {
        /// <summary>
        ///     Directory where the block tree root will be placed.
        /// </summary>
        private readonly string baseDirectory;

        /// <summary>
        ///     Database/directory name for this instance's tree root.
        /// </summary>
        private readonly string databaseName;

        private readonly IDevice logDevice;

        // Whether we enable a read cache
        static readonly bool useReadCache = false;

        /// <summary>
        /// Backing storage device.
        /// </summary>
        private readonly IDevice blocksDevice;

        private readonly FasterKV<BlockHash, IBlock> blocksKV;

        private void Temp()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FasterBlockCacheManager" /> class.
        /// </summary>
        /// <param name="logger">Instance of the logging provider.</param>
        /// <param name="configuration">Instance of the configuration provider.</param>
        /// <param name="databaseName">Database/directory name for the store.</param>
        public FasterBlockCacheManager(ILogger logger, IConfiguration configuration, RootBlock rootBlock)
            : base(logger, configuration, rootBlock)
        {
            this.databaseName = Utilities.HashToFormattedString(this.RootBlock.Guid.ToByteArray());

            var nodeOptions = configuration.GetSection("NodeOptions");
            if (nodeOptions is null)
            {
                throw new BrightChainException("'NodeOptions' config section must be defined, but is not");
            }

            var configOption = nodeOptions.GetSection("BasePath");
            if (configOption is null)
            {
                throw new BrightChainException("'BasePath' config option must be set, but is not");
            }

            var dir = configOption.Value;
            if (dir.Length == 0 || !Directory.Exists(dir))
            {
                throw new BrightChainException(string.Format("'BasePath' must exist, but does not: \"{0}\"", dir));
            }

            this.baseDirectory = Path.GetFullPath(dir);

            var configuredDbName
                = nodeOptions.GetSection("DatabaseName");

            if (configuredDbName is null)
            {
                //ConfigurationHelper.AddOrUpdateAppSetting("NodeOptions:DatabaseName", this.databaseName);
            }
            else
            {
                var expectedGuid = Guid.Parse(configuredDbName.Value);
                if (expectedGuid != this.RootBlock.Guid)
                {
                    throw new BrightChainException("Provided root block does not match configured root block guid");
                }
            }

            this.logDevice = this.OpenDevice("core");
            this.blocksDevice = this.OpenDevice("blocks");

            var logSettings = new LogSettings // log settings (devices, page size, memory size, etc.)
            {
                LogDevice = this.blocksDevice,
                ObjectLogDevice = this.blocksDevice,
                ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
            };

            // Define serializers; otherwise FASTER will use the slower DataContract
            // Needed only for class keys/values
            var serializerSettings = new SerializerSettings<BlockHash, IBlock>
            {
                keySerializer = () => new FasterBlockHashSerializer<FasterBlock>(),
                valueSerializer = () => new FasterBlockSerializer(),
            };

            this.blocksKV = new FasterKV<BlockHash, IBlock>(
                size: 1L << 20, // hash table size (number of 64-byte buckets)
                logSettings: logSettings,
                checkpointSettings: new CheckpointSettings
                {
                    CheckpointDir = this.GetDiskCacheDirectory().FullName,
                },
                serializerSettings: serializerSettings,
                comparer: new EmptyDummyBlock(BlockSize.Message).Id);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FasterBlockCacheManager" /> class.
        ///     Can not build a cache manager with no logger.
        /// </summary>
        private FasterBlockCacheManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Full path to the configuration file.
        /// </summary>
        public string ConfigurationFilePath
            => this.configFile;

        protected DirectoryInfo GetDiskCacheDirectory()
        {
            return Directory.CreateDirectory(
                Path.Combine(
                    this.baseDirectory,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "BrightChain-{0}",
                        this.databaseName)));
        }

        protected string GetDevicePath(string nameSpace, out DirectoryInfo cacheDirectoryInfo)
        {
            cacheDirectoryInfo = this.GetDiskCacheDirectory();

            return Path.Combine(
                    cacheDirectoryInfo.FullName,
                    string.Format(
                        provider: System.Globalization.CultureInfo.InvariantCulture,
                        format: "brightchain-{0}-{1}.log",
                        this.databaseName,
                        nameSpace));
        }

        protected IDevice OpenDevice(string nameSpace)
        {
            var devicePath = this.GetDevicePath(nameSpace, out DirectoryInfo _);

            return Devices.CreateLogDevice(
                logPath: devicePath);
        }

        /// <summary>
        ///     Fired whenever a block is added to the cache
        /// </summary>
        public override event ICacheManager<BlockHash, TransactableBlock>.KeyAddedEventHandler KeyAdded;

        /// <summary>
        ///     Fired whenever a block is expired from the cache
        /// </summary>
        public override event ICacheManager<BlockHash, TransactableBlock>.KeyExpiredEventHandler KeyExpired;

        /// <summary>
        ///     Fired whenever a block is removed from the collection
        /// </summary>
        public override event ICacheManager<BlockHash, TransactableBlock>.KeyRemovedEventHandler KeyRemoved;

        /// <summary>
        ///     Fired whenever a block is requested from the cache but is not present.
        /// </summary>
        public override event ICacheManager<BlockHash, TransactableBlock>.CacheMissEventHandler CacheMiss;

        /// <summary>
        ///     Returns whether the cache manager has the given key and it is not expired.
        /// </summary>
        /// <param name="key">key to check the collection for.</param>
        /// <returns>boolean with whether key is present.</returns>
        public override bool Contains(BlockHash key)
        {
            using var session = this.blocksKV.NewSession(functions: new SimpleFunctions<BlockHash, IBlock, CacheContext>());
            var resultTuple = session.Read(key);
            return resultTuple.status == Status.OK;
        }

        /// <summary>
        ///     Removes a key from the cache and returns a boolean wither whether it was actually present.
        /// </summary>
        /// <param name="key">key to drop from the collection.</param>
        /// <param name="noCheckContains">Skips the contains check for performance.</param>
        /// <returns>whether requested key was present and actually dropped.</returns>
        public override bool Drop(BlockHash key, bool noCheckContains = true)
        {
            if (!base.Drop(key, noCheckContains))
            {
                return false;
            }

            using var session = this.blocksKV.NewSession(functions: new SimpleFunctions<BlockHash, IBlock, CacheContext>());
            var resultStatus = session.Delete(key);
            return resultStatus == Status.OK;
        }

        /// <summary>
        ///     Retrieves a block from the cache if it is present.
        /// </summary>
        /// <param name="key">key to retrieve.</param>
        /// <returns>returns requested block or throws.</returns>
        public override TransactableBlock Get(BlockHash key)
        {
            using var session = this.blocksKV.NewSession(functions: new SimpleFunctions<BlockHash, IBlock, CacheContext>());
            var resultTuple = session.Read(key);

            if (resultTuple.status != Status.OK)
            {
                throw new IndexOutOfRangeException();
            }

            return new RestoredBlock(sourceBlock: resultTuple.output)
                .FactoryConvert(blockCacheManager: this);
        }

        /// <summary>
        ///     Adds a key to the cache if it is not already present.
        /// </summary>
        /// <param name="block">block to palce in the cache.</param>
        public override void Set(TransactableBlock block)
        {
            base.Set(block);
            using var session = this.blocksKV.NewSession(functions: new SimpleFunctions<BlockHash, IBlock, CacheContext>());
            var blockHash = block.Id;
            var iBlock = block.AsIBlock;
            var resultStatus = session.Upsert(ref blockHash, ref iBlock);
            if (resultStatus != Status.OK)
            {
                throw new BrightChainException("Unable to store block");
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}