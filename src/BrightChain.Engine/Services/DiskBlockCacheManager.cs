using System;
using System.Globalization;
using System.IO;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Helpers;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.Chains;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static BrightChain.Engine.Extensions.BlockMetadataExtensions;

namespace BrightChain.Engine.Services
{
    /// <summary>
    /// Relatively naive Disk Based Block Cache Manager.
    /// </summary>
    public class DiskBlockCacheManager : BlockCacheManager
    {
        /// <summary>
        /// Directory where the block tree root will be placed.
        /// </summary>
        private readonly string baseDirectory;

        /// <summary>
        /// Database/directory name for this instance's tree root.
        /// </summary>
        private readonly string databaseName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskBlockCacheManager"/> class.
        /// </summary>
        /// <param name="logger">Instance of the logging provider.</param>
        /// <param name="configuration">Instance of the configuration provider.</param>
        /// <param name="databaseName">Database/directory name for the store.</param>
        public DiskBlockCacheManager(ILogger logger, IConfiguration configuration)
            : base(logger: logger, configuration: configuration)
        {
            var nodeOptions = configuration.GetSection("NodeOptions");
            if (nodeOptions is null)
            {
                throw new BrightChainException(string.Format(format: "'NodeOptions' config section must be defined, but is not"));
            }

            var configOption = nodeOptions.GetSection("BasePath");
            if (configOption is null)
            {
                throw new BrightChainException(string.Format(format: "'BasePath' config option must be set, but is not"));
            }

            var dir = configOption.Value;
            if (dir.Length == 0 || !Directory.Exists(dir))
            {
                throw new BrightChainException(string.Format(format: "'BasePath' must exist, but does not: \"{0}\"", dir));
            }

            this.baseDirectory = Path.GetFullPath(dir);

            var configuredDbName
                = nodeOptions.GetSection("DatabaseName");

            if (configuredDbName is null)
            {
                this.databaseName = Utilities.HashToFormattedString(this.Guid.ToByteArray());
                BrightChain.Engine.Helpers.ConfigurationHelper.AddOrUpdateAppSetting("NodeOptions:DatabaseName", this.databaseName);
            }
            else
            {
                this.Guid = Guid.Parse(configuredDbName.Value);
                this.databaseName = Utilities.HashToFormattedString(this.Guid.ToByteArray());
            }

            foreach (var subDir in new string[] { "blocks", "indices" })
            {
                var info = this.GetDiskCacheSubdirectory(subDir);
                if (!info.Exists)
                {
                    throw new BrightChainException(String.Format("Failed to create blockstore directory '{0}'.", subDir));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskBlockCacheManager"/> class.
        /// Can not build a cache manager with no logger.
        /// </summary>
        private DiskBlockCacheManager()
        {
            throw new NotImplementedException();
        }

        protected DirectoryInfo GetDiskCacheDirectory() =>
            Directory.CreateDirectory(
                path: Path.Combine(
                    path1: this.baseDirectory,
                    path2: string.Format(
                        provider: CultureInfo.InvariantCulture,
                        format: "BrightChain-{0}",
                        arg0: this.databaseName)));

        protected DirectoryInfo GetDiskCacheSubdirectory(string path) =>
            Directory.CreateDirectory(
                path: Path.Combine(
                    path1: this.GetDiskCacheDirectory().FullName,
                    path2: path));

        public DirectoryInfo GetBlocksDirectory() =>
            this.GetDiskCacheSubdirectory("blocks");

        public FileInfo GetBlockPath(BlockHash id)
        {
            var keyString = id.ToString();
            var keySub1 = keyString.Substring(0, 2);
            var keySub2 = keyString.Substring(2, 2);

            var blockSubDir = Path.Combine(
                path1: this.GetBlocksDirectory().FullName,
                path2: keySub1,
                path3: keySub2);

            Directory.CreateDirectory(blockSubDir);

            return new FileInfo(Path.Combine(
                path1: blockSubDir,
                path2: id.ToString()));
        }

        public DirectoryInfo GetIndicesPath() =>
            this.GetDiskCacheSubdirectory("indices");

        public FileInfo GetIndexPath(BlockHash id) =>
            new FileInfo(Path.Combine(
                path1: this.GetIndicesPath().FullName,
                path2: id.ToString()));

        /// <summary>
        /// Full path to the configuration file.
        /// </summary>
        public string ConfigurationFilePath
            => this.configFile;

        /// <summary>
        /// Fired whenever a block is added to the cache
        /// </summary>
        public override event ICacheManager<BlockHash, TransactableBlock>.KeyAddedEventHandler KeyAdded;

        /// <summary>
        /// Fired whenever a block is expired from the cache
        /// </summary>
        public override event ICacheManager<BlockHash, TransactableBlock>.KeyExpiredEventHandler KeyExpired;

        /// <summary>
        /// Fired whenever a block is removed from the collection
        /// </summary>
        public override event ICacheManager<BlockHash, TransactableBlock>.KeyRemovedEventHandler KeyRemoved;

        /// <summary>
        /// Fired whenever a block is requested from the cache but is not present.
        /// </summary>
        public override event ICacheManager<BlockHash, TransactableBlock>.CacheMissEventHandler CacheMiss;

        /// <summary>
        /// Returns whether the cache manager has the given key and it is not expired.
        /// </summary>
        /// <param name="key">key to check the collection for.</param>
        /// <returns>boolean with whether key is present.</returns>
        public override bool Contains(BlockHash key)
        {
            return File.Exists(this.GetBlockPath(key).FullName);
        }

        /// <summary>
        /// Removes a key from the cache and returns a boolean wither whether it was actually present.
        /// </summary>
        /// <param name="key">key to drop from the collection.</param>
        /// <param name="noCheckContains">Skips the contains check for performance.</param>
        /// <returns>whether requested key was present and actually dropped.</returns>
        public override bool Drop(BlockHash key, bool noCheckContains = true)
        {
            var fileInfo = this.GetBlockPath(key);
            if (!noCheckContains && !fileInfo.Exists)
            {
                return false;
            }

            try
            {
                File.Delete(fileInfo.FullName);
                return true;
            }
            catch (Exception _)
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves a block from the cache if it is present.
        /// </summary>
        /// <param name="key">key to retrieve.</param>
        /// <returns>returns requested block or throws.</returns>
        public override TransactableBlock Get(BlockHash key)
        {
            var fileInfo = this.GetBlockPath(key);
            if (!fileInfo.Exists)
            {
                throw new IndexOutOfRangeException(nameof(key));
            }

            var rawBlockData = File.ReadAllBytes(fileInfo.FullName);
            int metadataLength = -1;
            for (int i = 0; i < rawBlockData.Length; i++)
            {
                if (rawBlockData[i] == 0)
                {
                    metadataLength = i;
                    break;
                }
            }

            if (metadataLength == -1)
            {
                throw new BrightChainException("Error reading block metadata");
            }

            // extra char for \0 terminator
            var dataLength = rawBlockData.Length - metadataLength - 1;
            var metadataBytes = new byte[metadataLength];
            var blockBytes = new byte[dataLength];

            Array.Copy(
                sourceArray: rawBlockData,
                sourceIndex: 0,
                destinationArray: metadataBytes,
                destinationIndex: 0,
                length: metadataLength);

            Array.Copy(
                sourceArray: rawBlockData,
                sourceIndex: metadataLength + 1,
                destinationArray: blockBytes,
                destinationIndex: 0,
                length: dataLength);

            var block = new RestoredBlock(
                blockParams: new Models.Blocks.DataObjects.BlockParams(
                    blockSize: Enumerations.BlockSize.Unknown,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MinValue,
                    redundancy: Enumerations.RedundancyContractType.Unknown,
                    privateEncrypted: false),
                data: blockBytes);

            if (!block.TryRestoreMetadataFromBytesAndValidate(metadataBytes))
            {
                throw new BrightChainException("Invalid block metadata, restore failed");
            }

            return new TransactableBlock(
                cacheManager: this,
                sourceBlock: block,
                allowCommit: true);
        }

        /// <summary>
        /// Adds a key to the cache if it is not already present.
        /// </summary>
        /// <param name="block">block to palce in the cache.</param>
        public override void Set(TransactableBlock block)
        {
            if (block is null)
            {
                throw new BrightChain.Engine.Exceptions.BrightChainException("Can not store null block");
            }
            else if (!block.Validate())
            {
                throw new BrightChainValidationEnumerableException(
                    exceptions: block.ValidationExceptions,
                    message: "Can not store invalid block");
            }

            var fileInfo = this.GetBlockPath(block.Id);

            if (fileInfo.Exists)
            {
                throw new BrightChain.Engine.Exceptions.BrightChainException("Key already exists");
            }

            var file = File.OpenWrite(fileInfo.FullName);
            file.Write(new ReadOnlySpan<byte>(block.Metadata.ToArray()));
            file.WriteByte(0);
            file.Write(new ReadOnlySpan<byte>(block.Data.ToArray()));
            file.Close();
        }
    }
}
