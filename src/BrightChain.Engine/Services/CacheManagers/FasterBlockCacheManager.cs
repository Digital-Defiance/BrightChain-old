﻿namespace BrightChain.Engine.Services.CacheManagers
{
    using System;
    using System.Globalization;
    using System.IO;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Faster;
    using BrightChain.Engine.Faster.Serializers;
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
    public class FasterBlockCacheManager : BrightenedBlockCacheManagerBase, IDisposable
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
        /// Whether we enable a read cache.
        /// Updated from config.
        /// </summary>
        private readonly bool useReadCache = false;

        /// <summary>
        /// Session log storage device for block metadata.
        /// </summary>
        private readonly IDevice blockMetadataLogDevice;

        /// <summary>
        /// Storage device for block metadata.
        /// </summary>
        private readonly IDevice blockMetadataDevice;

        /// <summary>
        /// FasterKV instance for block metadata.
        /// </summary>
        private readonly FasterKV<BlockHash, BrightenedBlock> blockMetadataKV;

        /// <summary>
        /// Session log storage device for block data.
        /// </summary>
        private readonly IDevice blockDataLogDevice;

        /// <summary>
        /// Storage device for block data.
        /// </summary>
        private readonly IDevice blockDataDevice;

        /// <summary>
        /// FasterKV instance store for block data.
        /// </summary>
        private readonly FasterKV<BlockHash, BlockData> blockDataKV;

        /// <summary>
        /// Session log storage device for Source File Id -> CBL map.
        /// </summary>
        private readonly IDevice cblSourceHashesLogDevice;

        /// <summary>
        /// Storage device for Source File Id -> CBL map.
        /// </summary>
        private readonly IDevice cblSourceHashesDevice;

        /// <summary>
        /// FasterKV instance store for Source File Id -> CBL map.
        /// </summary>
        private readonly FasterKV<DataHash, BrightHandle> cblSourceHashesKV;

        /// <summary>
        /// Session log storage device for CBL correlation Guid -> CBL id.
        /// </summary>
        private readonly IDevice cblCorrelationIdsLogDevice;

        /// <summary>
        /// Storage device for CBL correlation Guid -> CBL id.
        /// </summary>
        private readonly IDevice cblCorrelationIdsDevice;

        /// <summary>
        /// FasterKV instance store for CBL correlation Guid -> CBL id.
        /// </summary>
        private readonly FasterKV<Guid, DataHash> cblCorrelationIdsKV;

        private FasterKV<BlockHash, BrightenedBlock> NewMetdataKV
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
                var metadataSerializerSettings = new SerializerSettings<BlockHash, BrightenedBlock>
                {
                    keySerializer = () => new FasterBlockHashSerializer(),
                    valueSerializer = () => new DataContractObjectSerializer<BrightenedBlock>(),
                };

                return new FasterKV<BlockHash, BrightenedBlock>(
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

        private FasterKV<DataHash, BrightHandle> NewCblSourceHashesFasterKV
        {
            get
            {
                var cblSourceHashesLogSettings = new LogSettings
                {
                    LogDevice = this.blockDataLogDevice,
                    ObjectLogDevice = this.blockDataDevice,
                    ReadCacheSettings = this.useReadCache ? new ReadCacheSettings() : null,
                };

                var cblSourceHashesSerializerSettings = new SerializerSettings<DataHash, BrightHandle>
                {
                    keySerializer = () => new FasterDataHashSerializer(),
                    valueSerializer = () => new DataContractObjectSerializer<BrightHandle>(),
                };

                return new FasterKV<DataHash, BrightHandle>(
                    size: HashTableBuckets,
                    logSettings: cblSourceHashesLogSettings,
                    checkpointSettings: new CheckpointSettings
                    {
                        CheckpointDir = this.GetDiskCacheDirectory().FullName,
                    },
                    serializerSettings: cblSourceHashesSerializerSettings,
                    comparer: new DataHash(dataBytes: new ReadOnlyMemory<byte>())); // gets an arbitrary BlockHash object which has the IFasterEqualityComparer on the class.
            }
        }

        private FasterKV<Guid, DataHash> NewCblCorrelationIdFasterKV
        {
            get
            {
                var cblSourceHashesLogSettings = new LogSettings
                {
                    LogDevice = this.blockDataLogDevice,
                    ObjectLogDevice = this.blockDataDevice,
                    ReadCacheSettings = this.useReadCache ? new ReadCacheSettings() : null,
                };

                var cblSourceHashesSerializerSettings = new SerializerSettings<Guid, DataHash>
                {
                    keySerializer = () => new FasterGuidSerializer(),
                    valueSerializer = () => new FasterDataHashSerializer(),
                };

                return new FasterKV<Guid, DataHash>(
                    size: HashTableBuckets,
                    logSettings: cblSourceHashesLogSettings,
                    checkpointSettings: new CheckpointSettings
                    {
                        CheckpointDir = this.GetDiskCacheDirectory().FullName,
                    },
                    serializerSettings: cblSourceHashesSerializerSettings,
                    comparer: null);
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

            this.cblCorrelationIdsLogDevice = this.OpenDevice("cbl-corr-log");
            this.cblCorrelationIdsDevice = this.OpenDevice("cbl-corr");
            this.cblCorrelationIdsKV = this.NewCblCorrelationIdFasterKV;
        }

        /// <summary>
        ///     Full path to the configuration file.
        /// </summary>
        public string ConfigurationFilePath
            => this.ConfigFile;

        protected DirectoryInfo GetDiskCacheDirectory()
        {
            return Directory.CreateDirectory(
                Path.Combine(
                    this.baseDirectory,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "BrightChain-{0}",
                        this.DatabaseName)));
        }

        protected string GetDevicePath(string nameSpace, out DirectoryInfo cacheDirectoryInfo)
        {
            cacheDirectoryInfo = this.GetDiskCacheDirectory();

            return Path.Combine(
                cacheDirectoryInfo.FullName,
                string.Format(
                    provider: System.Globalization.CultureInfo.InvariantCulture,
                    format: "brightchain-{0}-{1}.log",
                    this.DatabaseName,
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

        public BlockSessionContext NewSessionContext => new BlockSessionContext(
                metadataSession: this.NewMetadataSession,
                dataSession: this.NewDataSession,
                cblSourceHashSession: this.NewCblSourceHashSession,
                cblCorrelationIdsSession: this.NewCblCorrelationIdSession);

        private ClientSession<BlockHash, BrightenedBlock, BrightenedBlock, BrightenedBlock, CacheContext, SimpleFunctions<BlockHash, BrightenedBlock, CacheContext>> NewMetadataSession =>
            this.blockMetadataKV.For(functions: new SimpleFunctions<BlockHash, BrightenedBlock, CacheContext>())
            .NewSession<SimpleFunctions<BlockHash, BrightenedBlock, CacheContext>>();

        private ClientSession<BlockHash, BlockData, BlockData, BlockData, CacheContext, SimpleFunctions<BlockHash, BlockData, CacheContext>> NewDataSession =>
            this.blockDataKV.For(functions: new SimpleFunctions<BlockHash, BlockData, CacheContext>())
            .NewSession<SimpleFunctions<BlockHash, BlockData, CacheContext>>();

        private ClientSession<DataHash, BrightHandle, BrightHandle, BrightHandle, CacheContext, SimpleFunctions<DataHash, BrightHandle, CacheContext>> NewCblSourceHashSession =>
            this.cblSourceHashesKV.For(functions: new SimpleFunctions<DataHash, BrightHandle, CacheContext>())
            .NewSession<SimpleFunctions<DataHash, BrightHandle, CacheContext>>();

        private ClientSession<Guid, DataHash, DataHash, DataHash, CacheContext, SimpleFunctions<Guid, DataHash, CacheContext>> NewCblCorrelationIdSession =>
            this.cblCorrelationIdsKV.For(functions: new SimpleFunctions<Guid, DataHash, CacheContext>())
            .NewSession<SimpleFunctions<Guid, DataHash, CacheContext>>();

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
        public override BrightenedBlock Get(BlockHash key)
        {
            using var sessionContext = this.NewSessionContext;
            return sessionContext.Get(key);
        }

        public override BrightHandle GetCbl(DataHash sourceHash)
        {
            using var sessionContext = this.NewSessionContext;
            var result = sessionContext.CblSourceHashSession.Read(sourceHash);
            if (result.status != Status.OK)
            {
                throw new IndexOutOfRangeException(sourceHash.ToString());
            }

            return result.output;
        }

        public void Set(BlockSessionContext sessionContext, BrightenedBlock block)
        {
            base.Set(block);
            block.SetCacheManager(this);
            sessionContext.Upsert(ref block);
        }

        /// <summary>
        ///     Adds a key to the cache if it is not already present.
        /// </summary>
        /// <param name="block">block to palce in the cache.</param>
        public override void Set(BrightenedBlock block)
        {
            using var context = this.NewSessionContext;
            this.Set(context, block);
            context.CompletePending(wait: true);
        }

        public override void SetCbl(BlockHash brightenedCblHash, DataHash identifiableSourceHash, BrightHandle brightHandle)
        {
            // technically the node can allow the CBL to be committed even if the store doesn't have the final block necessary to recreate it
            // this would be allowed in some circumstances TBD.
            // the parameter is provided as a means to check that.
            if (!brightHandle.BrightenedCblHash.Equals(brightenedCblHash))
            {
                throw new BrightChainException(nameof(brightenedCblHash));
            }

            if (!brightHandle.IdentifiableSourceHash.Equals(identifiableSourceHash))
            {
                throw new BrightChainException(nameof(identifiableSourceHash));
            }

            using var context = this.NewSessionContext;
            context.CblSourceHashSession.Upsert(ref identifiableSourceHash, ref brightHandle);

            /*
            base.UpdateCblVersion(newCbl, oldCbl);
            var correlationId = newCbl.CorrelationId;
            var dataHash = newCbl.SourceId;
            context.CblCorrelationIdsSession.Upsert(ref correlationId, ref dataHash);
            */

            context.CompletePending(wait: true);
        }

        public override void UpdateCblVersion(ConstituentBlockListBlock newCbl, ConstituentBlockListBlock oldCbl = null)
        {
            base.UpdateCblVersion(newCbl, oldCbl);

            using var context = this.NewSessionContext;
            var correlationId = newCbl.CorrelationId;
            var dataHash = newCbl.SourceId;
            context.CblCorrelationIdsSession.Upsert(ref correlationId, ref dataHash);
            context.CompletePending(wait: true);
        }

        public override BrightHandle GetCbl(Guid correlationID)
        {
            using var context = this.NewSessionContext;
            var result = context.CblCorrelationIdsSession.Read(correlationID);
            if (result.status != Status.OK)
            {
                throw new IndexOutOfRangeException(nameof(correlationID));
            }

            return this.GetCbl(result.output);
        }

        public override void SetAll(IEnumerable<BrightenedBlock> items)
        {
            using var sessionContext = this.NewSessionContext;
            BrightenedBlock[] blocks = (BrightenedBlock[])items;

            for (int i = 0; i < blocks.Length; i++)
            {
                this.Set(sessionContext, blocks[i]);
            }

            sessionContext.CompletePending(wait: true);
        }

        public async override void SetAllAsync(IAsyncEnumerable<BrightenedBlock> items)
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
