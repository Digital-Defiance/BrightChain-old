namespace BrightChain.Engine.Services.CacheManagers
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
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Disk/Memory hybrid Block Cache Manager based on Microsoft FASTER KV.
    /// </summary>
    public class FasterBlockCacheManager : BlockCacheManager, IDisposable
    {
        /// <summary>
        /// hash table size (number of 64-byte buckets).
        /// </summary>
        private const long HashTableBuckets = 1L << 20;

        /// <summary>
        ///     Directory where the block tree root will be placed.
        /// </summary>
        private readonly string baseDirectory;

        /// <summary>
        ///     Database/directory name for this instance's tree root.
        /// </summary>
        private readonly string databaseName;

        // Whether we enable a read cache
        private readonly bool useReadCache = false;

        /// <summary>
        /// Session log backing storage device for block metadata.
        /// </summary>
        private readonly IDevice blockMetadataLogDevice;

        /// <summary>
        /// Backing storage device for block metadata.
        /// </summary>
        private readonly IDevice blockMetadataDevice;

        private readonly FasterKV<BlockHash, TransactableBlock> blockMetadataKV;

        /// <summary>
        /// Session log backing storage device for block data.
        /// </summary>
        private readonly IDevice blockDataLogDevice;

        /// <summary>
        /// Backing storage device for data.
        /// </summary>
        private readonly IDevice blockDataDevice;

        private readonly FasterKV<BlockHash, BlockData> blockDataKV;

        /// <summary>
        /// Session log backing storage device for block data.
        /// </summary>
        private readonly IDevice cblSourceHashesLogDevice;

        /// <summary>
        /// Backing storage device for data.
        /// </summary>
        private readonly IDevice cblSourceHashesDevice;

        private readonly FasterKV<DataHash, BlockHash> cblSourceHashesKV;

        private FasterKV<BlockHash, TransactableBlock> NewMetdataKV
        {
            get
            {
                var blockMetadataLogSettings = new LogSettings // log settings (devices, page size, memory size, etc.)
                {
                    LogDevice = this.blockMetadataLogDevice,
                    ObjectLogDevice = this.blockMetadataDevice,
                    ReadCacheSettings = this.useReadCache ? new ReadCacheSettings() : null,
                };

                // Define serializers; otherwise FASTER will use the slower DataContract
                // Needed only for class keys/values
                var metadataSerializerSettings = new SerializerSettings<BlockHash, TransactableBlock>
                {
                    keySerializer = () => new FasterBlockHashSerializer(),
                    valueSerializer = () => new DataContractObjectSerializer<TransactableBlock>(),
                };

                return new FasterKV<BlockHash, TransactableBlock>(
                    size: HashTableBuckets,
                    logSettings: blockMetadataLogSettings,
                    checkpointSettings: new CheckpointSettings
                    {
                        CheckpointDir = this.GetDiskCacheDirectory().FullName, // TODO: can these be in the same dir?
                    },
                    serializerSettings: metadataSerializerSettings,
                    comparer: BlockSizeMap.ZeroVectorHash(BlockSize.Micro)); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.
            }
        }

        private FasterKV<BlockHash, BlockData> NewDataFasterKV
        {
            get
            {
                var blockDataLogSettings = new LogSettings
                {
                    LogDevice = this.blockDataLogDevice,
                    ObjectLogDevice = this.blockDataDevice,
                    ReadCacheSettings = this.useReadCache ? new ReadCacheSettings() : null,
                };

                var blockDataSerializerSettings = new SerializerSettings<BlockHash, BlockData>
                {
                    keySerializer = () => new FasterBlockHashSerializer(),
                    valueSerializer = () => new DataContractObjectSerializer<BlockData>(),
                };

                return new FasterKV<BlockHash, BlockData>(
                    size: HashTableBuckets,
                    logSettings: blockDataLogSettings,
                    checkpointSettings: new CheckpointSettings
                    {
                        CheckpointDir = this.GetDiskCacheDirectory().FullName,
                    },
                    serializerSettings: blockDataSerializerSettings,
                    comparer: BlockSizeMap.ZeroVectorHash(BlockSize.Micro)); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.
            }
        }

        private FasterKV<DataHash, BlockHash> NewCblSourceHashesFasterKV
        {
            get
            {
                var cblSourceHashesLogSettings = new LogSettings
                {
                    LogDevice = this.blockDataLogDevice,
                    ObjectLogDevice = this.blockDataDevice,
                    ReadCacheSettings = this.useReadCache ? new ReadCacheSettings() : null,
                };

                var cblSourceHashesSerializerSettings = new SerializerSettings<DataHash, BlockHash>
                {
                    keySerializer = () => new FasterDataHashSerializer(),
                    valueSerializer = () => new FasterBlockHashSerializer(),
                };

                return new FasterKV<DataHash, BlockHash>(
                    size: HashTableBuckets,
                    logSettings: cblSourceHashesLogSettings,
                    checkpointSettings: new CheckpointSettings
                    {
                        CheckpointDir = this.GetDiskCacheDirectory().FullName,
                    },
                    serializerSettings: cblSourceHashesSerializerSettings,
                    comparer: BlockSizeMap.ZeroVectorHash(BlockSize.Micro)); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.
            }
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
            if (configOption is null || configOption.Value is null)
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

            if (configuredDbName is null || configuredDbName.Value is null)
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

            var readCache = nodeOptions.GetSection("EnableReadCache");
            this.useReadCache = readCache is null || readCache.Value is null ? false : Convert.ToBoolean(readCache.Value);

            this.blockMetadataLogDevice = this.OpenDevice("metadata-log");
            this.blockMetadataDevice = this.OpenDevice("metadata");
            this.blockMetadataKV = this.NewMetdataKV;

            this.blockDataLogDevice = this.OpenDevice("data-log");
            this.blockDataDevice = this.OpenDevice("data");
            this.blockDataKV = this.NewDataFasterKV;

            this.cblSourceHashesLogDevice = this.OpenDevice("cbl-log");
            this.cblSourceHashesDevice = this.OpenDevice("cbl");
            this.cblSourceHashesKV = this.NewCblSourceHashesFasterKV;
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

        public ClientSession<BlockHash, TransactableBlock, TransactableBlock, TransactableBlock, CacheContext, SimpleFunctions<BlockHash, TransactableBlock, CacheContext>> NewMetadataSession =>
            this.blockMetadataKV.For(functions: new SimpleFunctions<BlockHash, TransactableBlock, CacheContext>())
            .NewSession<SimpleFunctions<BlockHash, TransactableBlock, CacheContext>>();

        public ClientSession<BlockHash, BlockData, BlockData, BlockData, CacheContext, SimpleFunctions<BlockHash, BlockData, CacheContext>> NewDataSession =>
            this.blockDataKV.For(functions: new SimpleFunctions<BlockHash, BlockData, CacheContext>())
            .NewSession<SimpleFunctions<BlockHash, BlockData, CacheContext>>();

        public BlockSessionContext NewSessionContext =>
            new BlockSessionContext(this.NewMetadataSession, this.NewDataSession);

        /// <summary>
        ///     Returns whether the cache manager has the given key and it is not expired.
        /// </summary>
        /// <param name="key">key to check the collection for.</param>
        /// <returns>boolean with whether key is present.</returns>
        public override bool Contains(BlockHash key)
        {
            using var session = this.NewMetadataSession;
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

            using var sessionContext = this.NewSessionContext;
            return sessionContext.Drop(key, true);
        }

        /// <summary>
        ///     Retrieves a block from the cache if it is present.
        /// </summary>
        /// <param name="key">key to retrieve.</param>
        /// <returns>returns requested block or throws.</returns>
        public override TransactableBlock Get(BlockHash key)
        {
            using var sessionContext = this.NewSessionContext;
            return sessionContext.Get(key);
        }

        public void Set(BlockSessionContext sessionContext, TransactableBlock block)
        {
            base.Set(block);
            block.SetCacheManager(this);
            sessionContext.Upsert(ref block);
            //if (block is BrightenedBlock brightenedCBL)
            //{
            //var cblSession = this.cblSourceHashesKV
            //.For(functions: new SimpleFunctions<DataHash, BlockHash, CacheContext>())
            //.NewSession<SimpleFunctions<DataHash, BlockHash, CacheContext>>();

            //cblSession.Upsert()
            //}
        }

        /// <summary>
        ///     Adds a key to the cache if it is not already present.
        /// </summary>
        /// <param name="block">block to palce in the cache.</param>
        public override void Set(TransactableBlock block)
        {
            this.Set(this.NewSessionContext, block);
        }

        public override void SetAll(IEnumerable<TransactableBlock> items)
        {
            using var sessionContext = this.NewSessionContext;
            TransactableBlock[] blocks = (TransactableBlock[])items;

            for (int i = 0; i < blocks.Length; i++)
            {
                this.Set(sessionContext, blocks[i]);
            }

            sessionContext.CompletePending(wait: true);
        }

        public async override void SetAllAsync(IAsyncEnumerable<TransactableBlock> items)
        {
            using var sessionContext = this.NewSessionContext;
            await foreach (var block in items)
            {
                this.Set(sessionContext, block);
            }

            await sessionContext.CompletePendingAsync(wait: true)
                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            this.blockMetadataLogDevice.Dispose();
            this.blockMetadataDevice.Dispose();
            this.blockMetadataKV.Dispose();

            this.blockDataDevice.Dispose();
            this.blockDataLogDevice.Dispose();
            this.blockDataKV.Dispose();

            this.cblSourceHashesDevice.Dispose();
            this.cblSourceHashesLogDevice.Dispose();
            this.cblSourceHashesKV.Dispose();
        }
    }
}
