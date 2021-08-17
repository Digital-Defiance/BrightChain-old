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

        private readonly IDevice logDevice;

        // Whether we enable a read cache
        static readonly bool useReadCache = false;

        /// <summary>
        /// Backing storage device.
        /// </summary>
        private readonly IDevice blockMetadataDevice;
        private readonly IDevice blockDataDevice;

        private readonly FasterKV<BlockHash, TransactableBlock> blockMetadataKV;
        private readonly FasterKV<BlockHash, BlockData> blockDataKV;

        private FasterKV<BlockHash, TransactableBlock> NewMetdataKV
        {
            get
            {
                var blockMetadataLogSettings = new LogSettings // log settings (devices, page size, memory size, etc.)
                {
                    LogDevice = this.logDevice,
                    ObjectLogDevice = this.blockMetadataDevice,
                    ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
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
                    LogDevice = this.logDevice,
                    ObjectLogDevice = this.blockDataDevice,
                    ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
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
            this.blockMetadataDevice = this.OpenDevice("metadata");
            this.blockDataDevice = this.OpenDevice("data");

            this.blockMetadataKV = this.NewMetdataKV;
            this.blockDataKV = this.NewDataFasterKV;
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
            if (!(sessionContext.MetadataSession.Delete(key) == Status.OK))
            {
                return false;
            }

            if (!(sessionContext.DataSession.Delete(key) == Status.OK))
            {
                return false;
            }

            sessionContext.CompletePending(wait: true);
            return true;
        }

        /// <summary>
        ///     Retrieves a block from the cache if it is present.
        /// </summary>
        /// <param name="key">key to retrieve.</param>
        /// <returns>returns requested block or throws.</returns>
        public override TransactableBlock Get(BlockHash key)
        {
            using var sessionContext = this.NewSessionContext;
            var metadataResultTuple = sessionContext.MetadataSession.Read(key);

            if (metadataResultTuple.status != Status.OK)
            {
                throw new IndexOutOfRangeException(message: key.ToString());
            }

            var block = metadataResultTuple.output;

            var dataResultTuple = sessionContext.DataSession.Read(key);

            if (dataResultTuple.status != Status.OK)
            {
                throw new IndexOutOfRangeException(message: key.ToString());
            }

            block.StoredData = dataResultTuple.output;

            if (!block.Validate())
            {
                throw new BrightChainValidationEnumerableException(block.ValidationExceptions, "Failed to reload block from store");
            }

            sessionContext.CompletePending(wait: true);

            return block;
        }

        /// <summary>
        ///     Adds a key to the cache if it is not already present.
        /// </summary>
        /// <param name="block">block to palce in the cache.</param>
        public override void Set(TransactableBlock block)
        {
            base.Set(block);
            block.SetCacheManager(this);
            using var sessionContext = this.NewSessionContext;
            var blockHash = block.Id;
            var resultStatus = sessionContext.MetadataSession.Upsert(ref blockHash, ref block);
            if (resultStatus != Status.OK)
            {
                throw new BrightChainException("Unable to store block");
            }

            var blockData = block.StoredData;
            resultStatus = sessionContext.DataSession.Upsert(ref blockHash, ref blockData);
            if (resultStatus != Status.OK)
            {
                throw new BrightChainException("Unable to store block");
            }

            sessionContext.CompletePending(wait: true);
        }

        public override void SetAll(IEnumerable<TransactableBlock> items)
        {
            using var sessionContext = this.NewSessionContext;
            TransactableBlock[] blocks = (TransactableBlock[])items;

            for (int i = 0; i < blocks.Length; i++)
            {
                base.Set(blocks[i]);
                blocks[i].SetCacheManager(this);
                var blockHash = blocks[i].Id;
                var resultStatus = sessionContext.MetadataSession.Upsert(ref blockHash, ref blocks[i]);
                if (resultStatus != Status.OK)
                {
                    throw new BrightChainException("Unable to store block");
                }

                var blockData = blocks[i].StoredData;
                resultStatus = sessionContext.DataSession.Upsert(ref blockHash, ref blockData);
                if (resultStatus != Status.OK)
                {
                    throw new BrightChainException("Unable to store block");
                }
            }

            sessionContext.CompletePending(wait: true);
        }

        public async override void SetAllAsync(IAsyncEnumerable<TransactableBlock> items)
        {
            using var sessionContext = this.NewSessionContext;
            await foreach (var block in items)
            {
                TransactableBlock t = block;
                base.Set(t);
                t.SetCacheManager(this);
                var blockHash = t.Id;
                var resultStatus = sessionContext.MetadataSession.Upsert(ref blockHash, ref t);
                if (resultStatus != Status.OK)
                {
                    throw new BrightChainException("Unable to store block");
                }

                var blockData = t.StoredData;
                resultStatus = sessionContext.DataSession.Upsert(ref blockHash, ref blockData);
                if (resultStatus != Status.OK)
                {
                    throw new BrightChainException("Unable to store block");
                }
            }

            await sessionContext.CompletePendingAsync(wait: true)
                .ConfigureAwait(false);
        }

        public void Dispose()
        {
        }
    }
}
