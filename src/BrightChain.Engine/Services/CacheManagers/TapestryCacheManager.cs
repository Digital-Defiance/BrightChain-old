using BrightChain.Engine.Faster.CacheManager;
using BrightChain.Engine.Models.Blocks;
using NeuralFabric.Models;

namespace BrightChain.Engine.Services.CacheManagers
{
    using System;
    using System.Globalization;
    using System.IO;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Faster;
    using BrightChain.Engine.Faster.Functions;
    using BrightChain.Engine.Interfaces;
    using FASTER.core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Disk/Memory hybrid Cache Manager based on Microsoft FASTER KV.
    /// </summary>
    public class TapestryCacheManager<Tkey, Tvalue, TkeySerializer, TvalueSerializer>
        : ICacheManager<Tkey, Tvalue>, IDisposable
        where Tkey : IComparable<Tkey>
        where TkeySerializer : BinaryObjectSerializer<Tkey>, new()
        where TvalueSerializer : BinaryObjectSerializer<Tvalue>, new()
    {
        /// <summary>
        ///     Full to the config file.
        /// </summary>
        protected readonly string configFile;

        protected readonly Tapestry _tapestry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TapestryCacheManager{Tkey,Tvalue,TkeySerializer,TvalueSerializer}" /> class.
        /// </summary>
        /// <param name="logger">Instance of the logging provider.</param>
        /// <param name="configuration">Instance of the configuration provider.</param>
        /// <param name="collectionName">Database/directory name for the store.</param>
        public TapestryCacheManager(ILogger logger, IConfiguration configuration, string collectionName)
        {
            this._tapestry = new Tapestry(
                logger: logger,
                configuration: configuration,
                collectionName: collectionName);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FasterBlockCacheManager" /> class.
        ///     Can not build a cache manager with no logger.
        /// </summary>
        private TapestryCacheManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Full path to the configuration file.
        /// </summary>
        public string ConfigurationFilePath
            => this.configFile;

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
            using var session = this.fasterKV.NewSession(functions: new BrightChainAdvancedFunctions<Tkey, Tvalue, Tvalue, Tvalue, BrightChainFasterCacheContext>());
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
            using var session = this.fasterKV.NewSession(functions: new BrightChainAdvancedFunctions<Tkey, Tvalue, Tvalue, Tvalue, BrightChainFasterCacheContext>());
            var resultStatus = session.Delete(key);
            return resultStatus == Status.OK;
        }

        /// <summary>
        ///     Retrieves a block from the cache if it is present.
        /// </summary>
        /// <param name="blockHash">key to retrieve.</param>
        /// <returns>returns requested block or throws.</returns>
        public Tvalue Get(Tkey blockHash)
        {
            using var session = this.fasterKV.NewSession(functions: new BrightChainAdvancedFunctions<Tkey, Tvalue, Tvalue, Tvalue, BrightChainFasterCacheContext>());
            var resultTuple = session.Read(blockHash);

            if (resultTuple.status != Status.OK)
            {
                throw new IndexOutOfRangeException(message: blockHash.ToString());
            }

            return resultTuple.output;
        }

        /// <summary>
        ///     Adds a key to the cache if it is not already present.
        /// </summary>
        /// <param name="block">block to palce in the cache.</param>
        public void Set(Tkey key, Tvalue value)
        {
            var functions = new BrightChainAdvancedFunctions<Tkey, Tvalue, Tvalue, Tvalue, BrightChainFasterCacheContext>();
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
