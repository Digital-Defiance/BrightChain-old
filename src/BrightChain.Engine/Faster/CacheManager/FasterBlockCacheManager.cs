namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
    using System.IO;
    using System.Linq;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Faster.Enumerations;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Services.CacheManagers.Block;
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

        private readonly Dictionary<CacheStoreType, Dictionary<CacheDeviceType, IDevice>> fasterDevices;
        private readonly Dictionary<CacheStoreType, FasterBase> fasterStores;

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

            (this.fasterDevices, this.fasterStores) = this.InitFaster();
            this.sessionContext = this.NewSharedSessionContext;
            this.lastAddresses = this.NextSerials();
            this.lastCheckpoint = this.TakeFullCheckpoint();
        }

        /// <summary>
        /// Gets the full path to the configuration file.
        /// </summary>
        public string ConfigurationFilePath
            => this.ConfigFile;

        public void Dispose()
        {
            foreach (var entry in this.fasterDevices)
            {
                (this.fasterStores[entry.Key] as IDisposable).Dispose();
                entry.Value.Values.All(d =>
                {
                    d.Dispose();
                    return true;
                });
            }
        }
    }
}
