namespace BrightChain.Engine.Services.CacheManagers
{
    using System;
    using System.Globalization;
    using System.IO;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Faster;
    using BrightChain.Engine.Interfaces;
    using FASTER.core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Disk/Memory hybrid Cache Manager based on Microsoft FASTER KV.
    /// </summary>
    public class FasterCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer>
        : ICacheManager<Tkey, Tvalue>, IDisposable
        where Tkey : IComparable<Tkey>
        where TkeySerializer : BinaryObjectSerializer<Tkey>, new()
        where TvalueSerializer : BinaryObjectSerializer<Tvalue>, new()
    {
        /// <summary>
        ///     Full to the config file.
        /// </summary>
        protected readonly string configFile;

        protected readonly string databaseName;

        /// <summary>
        ///     Directory where the block tree root will be placed.
        /// </summary>
        private readonly DirectoryInfo baseDirectory;

        private readonly IDevice logDevice;

        // Whether we enable a read cache
        static readonly bool useReadCache = false;

        /// <summary>
        /// Backing storage device.
        /// </summary>
        private readonly IDevice fasterDevice;

        private readonly FasterKV<Tkey, Tvalue> fasterKV;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FasterCacheManager" /> class.
        /// </summary>
        /// <param name="logger">Instance of the logging provider.</param>
        /// <param name="configuration">Instance of the configuration provider.</param>
        /// <param name="databaseName">Database/directory name for the store.</param>
        public FasterCacheManager(ILogger logger, IConfiguration configuration, string databaseName)
        {
            this.databaseName = databaseName;

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

            this.baseDirectory = new DirectoryInfo(dir);

            this.logDevice = this.OpenDevice(string.Format("{0}-log", typeof(Tkey).Name));
            this.fasterDevice = this.OpenDevice(string.Format("{0}-data", typeof(Tkey).Name));

            var logSettings = new LogSettings // log settings (devices, page size, memory size, etc.)
            {
                LogDevice = this.fasterDevice,
                ObjectLogDevice = this.fasterDevice,
                ReadCacheSettings = useReadCache ? new ReadCacheSettings() : null,
            };

            // Define serializers; otherwise FASTER will use the slower DataContract
            // Needed only for class keys/values
            var serializerSettings = new SerializerSettings<Tkey, Tvalue>
            {
                keySerializer = () => new TkeySerializer(),
                valueSerializer = () => new TvalueSerializer(),
            };

            this.fasterKV = new FasterKV<Tkey, Tvalue>(
                size: 1L << 20, // hash table size (number of 64-byte buckets)
                logSettings: logSettings,
                checkpointSettings: new CheckpointSettings
                {
                    CheckpointDir = this.GetDiskCacheDirectory().FullName,
                },
                serializerSettings: serializerSettings,
                comparer: null);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FasterBlockCacheManager" /> class.
        ///     Can not build a cache manager with no logger.
        /// </summary>
        private FasterCacheManager()
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
                    this.baseDirectory.FullName,
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
        public event ICacheManager<Tkey, Tvalue>.KeyAddedEventHandler KeyAdded;

        /// <summary>
        ///     Fired whenever a block is expired from the cache
        /// </summary>
        public event ICacheManager<Tkey, Tvalue>.KeyExpiredEventHandler KeyExpired;

        /// <summary>
        ///     Fired whenever a block is removed from the collection
        /// </summary>
        public event ICacheManager<Tkey, Tvalue>.KeyRemovedEventHandler KeyRemoved;

        /// <summary>
        ///     Fired whenever a block is requested from the cache but is not present.
        /// </summary>
        public event ICacheManager<Tkey, Tvalue>.CacheMissEventHandler CacheMiss;

        /// <summary>
        ///     Returns whether the cache manager has the given key and it is not expired.
        /// </summary>
        /// <param name="key">key to check the collection for.</param>
        /// <returns>boolean with whether key is present.</returns>
        public bool Contains(Tkey key)
        {
            using var session = this.fasterKV.NewSession(functions: new SimpleFunctions<Tkey, Tvalue, BrightChainFasterCacheContext>());
            var resultTuple = session.Read(key);
            return resultTuple.status == Status.OK;
        }

        /// <summary>
        ///     Removes a key from the cache and returns a boolean wither whether it was actually present.
        /// </summary>
        /// <param name="key">key to drop from the collection.</param>
        /// <param name="noCheckContains">Skips the contains check for performance.</param>
        /// <returns>whether requested key was present and actually dropped.</returns>
        public bool Drop(Tkey key, bool noCheckContains = true)
        {
            using var session = this.fasterKV.NewSession(functions: new SimpleFunctions<Tkey, Tvalue, BrightChainFasterCacheContext>());
            var resultStatus = session.Delete(key);
            return resultStatus == Status.OK;
        }

        /// <summary>
        ///     Retrieves a block from the cache if it is present.
        /// </summary>
        /// <param name="key">key to retrieve.</param>
        /// <returns>returns requested block or throws.</returns>
        public Tvalue Get(Tkey key)
        {
            using var session = this.fasterKV.NewSession(functions: new SimpleFunctions<Tkey, Tvalue, BrightChainFasterCacheContext>());
            var resultTuple = session.Read(key);

            if (resultTuple.status != Status.OK)
            {
                throw new IndexOutOfRangeException(message: key.ToString());
            }

            return resultTuple.output;
        }

        /// <summary>
        ///     Adds a key to the cache if it is not already present.
        /// </summary>
        /// <param name="block">block to palce in the cache.</param>
        public void Set(Tkey key, Tvalue value)
        {
            var functions = new SimpleFunctions<Tkey, Tvalue, BrightChainFasterCacheContext>();
            using var session = this.fasterKV.NewSession(functions: functions);
            var resultStatus = session.Upsert(
                key: key,
                desiredValue: value);
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
