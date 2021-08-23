namespace BrightChain.Engine.Services.CacheManagers
{
    using System;
    using System.IO;
    using System.Linq;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Disk/Memory hybrid Block Cache Manager based on Microsoft FASTER KV.
    /// </summary>
    public partial class FasterBlockCacheManager : BrightenedBlockCacheManagerBase, IDisposable
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
            var dir = configOption is not null && configOption.Value.Any() ? configOption.Value : Path.Join(Path.GetTempPath(), "brightchain");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                //throw new BrightChainException(string.Format("'BasePath' must exist, but does not: \"{0}\"", dir));
            }

            this.baseDirectory = Path.GetFullPath(dir);

            var configuredDbName
                = nodeOptions.GetSection("DatabaseName");

            if (configuredDbName is null || !configuredDbName.Value.Any())
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

            this.blockMetadataLogDevice = this.CreateLogDevice("metadata-log");
            this.blockMetadataDevice = this.CreateLogDevice("metadata");
            this.blockMetadataKV = this.NewMetdataKV;

            this.blockDataLogDevice = this.CreateLogDevice("data-log");
            this.blockDataDevice = this.CreateLogDevice("data");
            this.blockDataKV = this.NewDataFasterKV;

            this.cblSourceHashesLogDevice = this.CreateLogDevice("cbl-log");
            this.cblSourceHashesDevice = this.CreateLogDevice("cbl");
            this.cblSourceHashesKV = this.NewCblSourceHashesFasterKV;

            this.cblCorrelationIdsLogDevice = this.CreateLogDevice("cbl-corr-log");
            this.cblCorrelationIdsDevice = this.CreateLogDevice("cbl-corr");
            this.cblCorrelationIdsKV = this.NewCblCorrelationIdFasterKV;

            this.SessionContext = this.NewSharedSessionContext();
        }

        /// <summary>
        ///     Full path to the configuration file.
        /// </summary>
        public string ConfigurationFilePath
            => this.ConfigFile;

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
