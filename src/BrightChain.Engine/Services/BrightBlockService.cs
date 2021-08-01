// <copyright file="BrightBlockService.cs" company="BrightChain">
// Copyright (c) BrightChain. All rights reserved.
// </copyright>

namespace BrightChain.Engine.Services
{
#nullable enable
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Core service for BrightChain used by the webservice to retrieve and store blocks.
    /// </summary>
    public class BrightBlockService
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;

        private readonly MemoryDictionaryBlockCacheManager blockMemoryCache;
        private readonly MemoryDictionaryBlockCacheManager randomizerBlockMemoryCache;
        private readonly DiskBlockCacheManager blockDiskCache;
        private readonly BlockBrightener blockBrightener;
        //private readonly BrightChainNode brightChainNodeAuthority;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrightBlockService"/> class.
        /// </summary>
        /// <param name="logger">Instance of the logging provider.</param>
        public BrightBlockService(ILoggerFactory logger, IConfiguration configuration)
        {
            this.logger = logger.CreateLogger(nameof(BrightBlockService));
            if (this.logger is null)
            {
                throw new BrightChainException("CreateLogger failed");
            }

            this.logger.LogInformation(string.Format("<{0}>: logging initialized", nameof(BrightBlockService)));

            var nodeOptions = configuration.GetSection("NodeOptions");
            if (nodeOptions is null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddYamlFile(
                        path: "brightChainSettings.yaml",
                        optional: false,
                        reloadOnChange: true)
                    .AddEnvironmentVariables();

                this.configuration = builder.Build();
                nodeOptions = this.configuration.GetSection("NodeOptions");
            }
            else
            {
                this.configuration = configuration;
            }

            if (nodeOptions is null)
            {
                throw new BrightChainException(string.Format(format: "'NodeOptions' config section must be defined, but is not"));
            }

            var configuredDbName
                = nodeOptions.GetSection("DatabaseName");

            var dbNameConfigured = configuredDbName is not null;
            Guid serviceUnifiedStoreGuid = dbNameConfigured ? Guid.Parse(configuredDbName.Value) : Guid.NewGuid();

            if (!dbNameConfigured)
            {
                global::BrightChain.Engine.Helpers.ConfigurationHelper.AddOrUpdateAppSetting("NodeOptions:DatabaseName", Utilities.HashToFormattedString(serviceUnifiedStoreGuid.ToByteArray()));
            }

            var rootBlock = new RootBlock(
                databaseGuid: serviceUnifiedStoreGuid);

            this.blockMemoryCache = new MemoryDictionaryBlockCacheManager(
                logger: this.logger,
                configuration: this.configuration,
                rootBlock: rootBlock);

            this.blockDiskCache = new DiskBlockCacheManager(
                logger: this.logger,
                configuration: this.configuration,
                rootBlock: rootBlock);

            this.randomizerBlockMemoryCache = new MemoryDictionaryBlockCacheManager(
                logger: this.logger,
                configuration: this.configuration,
                rootBlock: rootBlock);

            this.logger.LogInformation(string.Format("<{0}>: caches initialized", nameof(BrightBlockService)));
            this.blockBrightener = new BlockBrightener(
                pregeneratedRandomizerCache: this.randomizerBlockMemoryCache);
            //this.brightChainNodeAuthority = default(BrightChainNode);
            //this.brightChainNodeAuthority = BrightChainKeyService.LoadPrivateKeyFromBlock(this.blockDiskCache.Get(_));
        }

        /// <summary>
        /// Creates a descriptor block for a given input file, found on disk.
        /// TODO: Break this up into a block-stream.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="blockParams"></param>
        /// <param name="blockSize"></param>
        /// <returns>Resultant CBL block.</returns>
        public async IAsyncEnumerable<BrightenedBlock> StreamCreatedBrightenedBlocksFromFileAsync(SourceFileInfo sourceInfo, BlockParams blockParams, BlockSize? blockSize = null)
        {
            if (!blockSize.HasValue)
            {
                blockSize = blockParams.BlockSize;
                if (blockSize.Value == BlockSize.Unknown)
                {
                    // decide best block size if null
                    throw new NotImplementedException();
                }
            }

            var iBlockSize = BlockSizeMap.BlockSize(blockSize.Value);
            var maximumStorage = BlockSizeMap.HashesPerBlock(blockSize.Value, 2) * iBlockSize;
            if (sourceInfo.FileInfo.Length > maximumStorage)
            {
                throw new BrightChainException("File exceeds storage for this block size");
            }

            if (blockParams.PrivateEncrypted)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// The outer code already knows the SHA256 of the file in order to build all the CBL params, but serves as a check against that at all points.
            /// </summary>
            using (SHA256 fileHasher = SHA256.Create())
            {
                using (FileStream inFile = File.OpenRead(sourceInfo.FileInfo.FullName))
                {
                    var bytesRemaining = sourceInfo.FileInfo.Length;
                    var blocksRemaining = Math.Max(1, (int)Math.Ceiling((double)(bytesRemaining / iBlockSize)));
                    while (bytesRemaining > 0)
                    {
                        var finalBlock = bytesRemaining <= iBlockSize;
                        var bytesToRead = finalBlock ? (int)bytesRemaining : iBlockSize;
                        byte[] buffer = new byte[bytesToRead];
                        int bytesRead = inFile.Read(buffer, 0, bytesToRead);
                        bytesRemaining -= bytesRead;

                        if (bytesRead < bytesToRead)
                        {
                            throw new BrightChainException("Unexpected EOF");
                        }
                        else if (bytesRead > iBlockSize)
                        {
                            throw new BrightChainExceptionImpossible(nameof(bytesRead));
                        }
                        else if (finalBlock)
                        {
                            if (bytesRemaining != 0)
                            {
                                throw new BrightChainException(nameof(bytesRemaining));
                            }

                            fileHasher.TransformFinalBlock(buffer, 0, bytesToRead); // notably only takes the last bytes of the file not counting filler.
                            // fill in the rest of the block with random data
                            buffer = Helpers.RandomDataHelper.DataFiller(
                                inputData: new ReadOnlyMemory<byte>(buffer),
                                blockSize: blockSize.Value).ToArray();
                        }
                        else
                        {
                            var bytesHashed = fileHasher.TransformBlock(buffer, 0, bytesRead, null, 0);
                            if (bytesHashed != iBlockSize || bytesRead != bytesHashed)
                            {
                                throw new BrightChainException("Unexpected transform mismatch");
                            }
                        }

                        Block[] randomizersUsed;
                        var brightenedBlock = this.blockBrightener.Brighten(
                            block: new SourceBlock(
                                blockParams: blockParams,
                                data: buffer),
                            randomizersUsed: out randomizersUsed);

                        this.blockMemoryCache.Set(new TransactableBlock(
                                cacheManager: this.blockMemoryCache,
                                sourceBlock: brightenedBlock,
                                allowCommit: true));

                        foreach (var block in randomizersUsed)
                        {
                            this.blockMemoryCache.Set(block: new TransactableBlock(
                                cacheManager: this.blockMemoryCache,
                                sourceBlock: block,
                                allowCommit: true));
                        }

                        yield return brightenedBlock;
                    } // end while

                    if (new DataHash(
                        providedHashBytes: fileHasher.Hash,
                        sourceDataLength: sourceInfo.FileInfo.Length,
                        computed: true) != sourceInfo.SourceId)
                    {
                        throw new BrightChainException("Hash mismatch against known hash");
                    }
                } // end using
            } // end using
        }

        public async Task<IEnumerable<ConstituentBlockListBlock>> MakeCBLChainFromParamsAsync(string fileName, BlockParams blockParams)
        {
            var sourceInfo = new SourceFileInfo(
                fileName: fileName,
                blockSize: blockParams.BlockSize);
            var blocksUsedThisSegment = new List<BlockHash>();
            var sourceBytesRemaining = sourceInfo.FileInfo.Length;
            var totalBytesRemaining = sourceInfo.TotalBlockedBytes;
            var cblsExpected = sourceInfo.CblsExpected;
            var cblsEmitted = new ConstituentBlockListBlock[cblsExpected];
            var cblIdx = 0;
            var blocksRemaining = sourceInfo.TotalBlocksExpected;

            var blocksThisSegment = 0;
            var sourceBytesThisSegment = 0;
            var brightenedBlocksConsumed = 0;

            using (SHA256 segmentHasher = SHA256.Create())
            {
                // last block is always full of random data
                await foreach (BrightenedBlock brightenedBlock in this.StreamCreatedBrightenedBlocksFromFileAsync(sourceInfo: sourceInfo, blockParams: blockParams))
                {
                    sourceBytesThisSegment += (int)(sourceBytesRemaining < sourceInfo.BytesPerBlock ? sourceBytesRemaining : brightenedBlock.Data.Length);
                    blocksUsedThisSegment.Add(brightenedBlock.Id);
                    blocksUsedThisSegment.AddRange(brightenedBlock.ConstituentBlocks);
                    var cblFullAfterThisBlock = ++blocksThisSegment == sourceInfo.HashesPerBlock;
                    var sourceConsumed = totalBytesRemaining <= sourceInfo.BytesPerBlock;
                    blocksRemaining--;
                    brightenedBlocksConsumed++;
                    if (cblFullAfterThisBlock || sourceConsumed || (blocksRemaining == 0))
                    {
                        var sourceBytesThisBlock = sourceBytesRemaining > sourceInfo.BytesPerBlock ? sourceInfo.BytesPerBlock : sourceBytesRemaining;

                        segmentHasher.TransformFinalBlock(brightenedBlock.Data.ToArray(), 0, (int)sourceBytesThisBlock);

                        var cbl = new ConstituentBlockListBlock(
                            blockParams: new ConstituentBlockListBlockParams(
                                blockParams: new TransactableBlockParams(
                                    cacheManager: this.blockMemoryCache,
                                    allowCommit: true,
                                    blockParams: blockParams),
                                sourceId: sourceInfo.SourceId,
                                segmentId: new SegmentHash(
                                    providedHashBytes: segmentHasher.Hash,
                                    sourceDataLength: sourceBytesThisSegment,
                                    computed: true),
                                totalLength: sourceBytesRemaining > sourceInfo.BytesPerCbl ? sourceInfo.BytesPerCbl : sourceBytesRemaining,
                                constituentBlocks: blocksUsedThisSegment.ToArray(),
                                previous: cblIdx > 0 ? cblsEmitted[cblIdx - 1].Id : null,
                                next: null));

                        // update the next pointer of the previous block
                        if (cblIdx > 0)
                        {
                            cblsEmitted[cblIdx].Next = cbl.Id;
                        }

                        cblsEmitted[cblIdx++] = cbl;

                        segmentHasher.Initialize();
                        blocksUsedThisSegment.Clear();
                        blocksThisSegment = 0;
                        sourceBytesThisSegment = 0;
                    }
                    else
                    {
                        segmentHasher.TransformBlock(
                            inputBuffer: brightenedBlock.Data.ToArray(),
                            inputOffset: 0,
                            inputCount: sourceInfo.BytesPerBlock,
                            outputBuffer: null,
                            outputOffset: 0);
                    }
                }
            }

            if (brightenedBlocksConsumed != sourceInfo.TotalBlocksExpected)
            {
                throw new BrightChainException(nameof(brightenedBlocksConsumed));
            }

            return cblsEmitted;
        }

        public async Task<SuperConstituentBlockListBlock> MakeSuperCBLFromCBLChain(BlockParams blockParams, IEnumerable<ConstituentBlockListBlock> chainedCbls)
        {
            var hashBytes = chainedCbls
                    .SelectMany(c => c.Id.HashBytes.ToArray())
                    .ToArray();

            return new SuperConstituentBlockListBlock(
                    blockParams: new ConstituentBlockListBlockParams(
                        blockParams: new TransactableBlockParams(
                            cacheManager: this.blockMemoryCache,
                            allowCommit: true,
                            blockParams: blockParams),
                        sourceId: chainedCbls.First().SourceId,
                        segmentId: new SegmentHash(hashBytes),
                        totalLength: hashBytes.Length,
                        constituentBlocks: chainedCbls.Select(c => c.Id),
                        previous: null,
                        next: null),
                    data: Helpers.RandomDataHelper.DataFiller(
                        inputData: hashBytes,
                        blockSize: blockParams.BlockSize));
        }

        public async Task<ConstituentBlockListBlock> MakeCblOrSuperCblFromFileAsync(string fileName, BlockParams blockParams)
        {
            var firstPass = await this.MakeCBLChainFromParamsAsync(
                fileName: fileName,
                blockParams: blockParams)
                    .ConfigureAwait(false);

            var count = firstPass.Count();
            if (count == 1)
            {
                var loneCbl = firstPass.ElementAt(0);
                return loneCbl;
            }
            else if (count > BlockSizeMap.HashesPerBlock(blockParams.BlockSize))
            {
                throw new NotImplementedException("Super-Super-CBLs not yet implemented");
            }
            else if (count == 0)
            {
                throw new BrightChainException("No blocks returned");
            }

            return await
                this.MakeSuperCBLFromCBLChain(
                    blockParams: blockParams,
                    chainedCbls: firstPass)
                        .ConfigureAwait(false);
        }

        public static Dictionary<BlockHash, Block> GetCBLBlocksFromCacheAsDictionary(BlockCacheManager blockCacheManager, ConstituentBlockListBlock block)
        {
            Dictionary<BlockHash, Block> blocks = new Dictionary<BlockHash, Block>();
            foreach (var blockHash in block.ConstituentBlocks)
            {
                blocks.Add(blockHash, blockCacheManager.Get(blockHash));
            }

            return blocks;
        }

        /// <summary>
        /// Given a CBL Block, produce a temporary file on disk containing the reconstitued bytes.
        /// </summary>
        /// <param name="constituentBlockListBlock">The CBL Block containing the list/order of blocks needed to rebuild the file.</param>
        /// <returns>Returns a Restored file object containing the path and hash for the resulting file as written, for verification.</returns>
        public async Task<SourceFileInfo> RestoreFileFromCBLAsync(ConstituentBlockListBlock constituentBlockListBlock)
        {
            if (constituentBlockListBlock.TotalLength > long.MaxValue)
            {
                throw new NotImplementedException();
            }

            if (constituentBlockListBlock is SuperConstituentBlockListBlock superConstituentBlockListBlock)
            {
                throw new NotImplementedException();
            }

            long bytesWritten = 0;
            var iBlockSize = BlockSizeMap.BlockSize(constituentBlockListBlock.BlockSize);
            var blockMap = constituentBlockListBlock.GenerateBlockMap();
            string tempFilename = Path.GetTempFileName();
            using (var stream = File.OpenWrite(tempFilename))
            {
                using (SHA256 sha = SHA256.Create())
                {
                    StreamWriter streamWriter = new StreamWriter(stream);
                    var cacheManager = this.blockMemoryCache.AsBlockCacheManager;
                    await foreach (Block block in blockMap.ConsolidateTuplesToChainAsync(cacheManager))
                    {
                        var bytesLeft = constituentBlockListBlock.TotalLength - bytesWritten;
                        var lastBlock = bytesLeft <= iBlockSize;
                        var length = lastBlock ? bytesLeft : iBlockSize;
                        if (lastBlock)
                        {
                            sha.TransformFinalBlock(block.Data.ToArray(), 0, (int)length);
                        }
                        else
                        {
                            sha.TransformBlock(block.Data.ToArray(), 0, (int)length, null, 0);
                        }

                        await stream.WriteAsync(buffer: block.Data.ToArray(), offset: 0, count: (int)length)
                            .ConfigureAwait(false);
                        bytesWritten += length;
                    }

                    var finalHash = new BlockHash(
                        blockType: typeof(ConstituentBlockListBlock),
                        originalBlockSize: constituentBlockListBlock.BlockSize,
                        providedHashBytes: sha.Hash,
                        computed: true);
                }

                stream.Flush();
                stream.Close();
            }

            if (bytesWritten == 0)
            {
                throw new BrightChainException(nameof(bytesWritten));
            }

            var restoredSourceInfo = new SourceFileInfo(
                fileName: tempFilename,
                blockSize: constituentBlockListBlock.BlockSize);

            if (restoredSourceInfo.FileInfo.Length != constituentBlockListBlock.TotalLength)
            {
                throw new BrightChainException(nameof(restoredSourceInfo.FileInfo.Length));
            }

            if (!restoredSourceInfo.SourceId.Equals(constituentBlockListBlock.SourceId))
            {
                throw new BrightChainException(nameof(restoredSourceInfo.SourceId));
            }

            return restoredSourceInfo;
        }

        public async Task<Block> TryFindBlockByIdAsync(BlockHash id)
        {
            if (this.blockMemoryCache.Contains(id))
            {
                return this.blockMemoryCache.Get(id);
            }

            if (this.blockDiskCache.Contains(id))
            {
                return this.blockDiskCache.Get(id);
            }

            // TODO: Dapr
            throw new KeyNotFoundException(id.ToString());
        }

        public async IAsyncEnumerable<Block> TryFindBlocksByIdAsync(IAsyncEnumerable<BlockHash> blockIdSource)
        {
            await foreach (var id in blockIdSource)
            {
                var block = await this.TryFindBlockByIdAsync(id)
                    .ConfigureAwait(false);

                if (block is Block && block.Validate())
                {
                    yield return block;
                }
            }
        }

        public async Task<T> TryFindBlockByIdAsync<T>(BlockHash id, bool useAsBlock)
            where T : class
        {
            var firstBlock = await this.TryFindBlockByIdAsync(id)
                .ConfigureAwait(false);
            var temp = firstBlock.OriginalType;
            var block = useAsBlock ? firstBlock.AsBlock as T : firstBlock as T;
            if (block is null)
            {
                throw new BrightChainException("Unable to cast from unrelated type");
            }

            return block;
        }

        protected async Task ClearAllBlocks()
        {
            await foreach (var blockHash in this.blockMemoryCache.KeysAsync())
            {
                this.blockMemoryCache.Drop(blockHash);
            }
        }

        public async Task PersistMemoryCacheAsync(bool clearAfter)
        {
            await this.blockMemoryCache.CopyContentAsync(this.blockDiskCache)
                .ConfigureAwait(false);

            if (clearAfter)
            {
                await this.ClearAllBlocks()
                    .ConfigureAwait(false);
            }
        }

        public async Task<Block> TryDropBlockAsync(BlockHash id, object? ownershipToken = null)
        {
            var block = await this.TryFindBlockByIdAsync(id: id)
                .ConfigureAwait(false);

            // verify pernmission/ownership/date, etc
            // can't drop unless expired or invalid
            if (block is Block && true)
            {
                throw new BrightChainException("Permission denied!");
            }

            if (this.blockDiskCache.Contains(id))
            {
                this.blockDiskCache.Drop(id);
            }

            if (this.blockMemoryCache.Contains(id))
            {
                this.blockMemoryCache.Drop(id);
            }

            // TODO: dapr broadcast
            return block;
        }

        public async IAsyncEnumerable<Tuple<BlockHash, Block?>> TryDropBlocksAsync(IAsyncEnumerable<BlockHash> idSource, object ownershipToken = null)
        {
            await foreach (var id in idSource)
            {
                var dropped = await this.TryDropBlockAsync(id, ownershipToken)
                    .ConfigureAwait(false);

                yield return new Tuple<BlockHash, Block?>(id, dropped);
            }
        }

        public async Task<Block> TryStoreBlockAsync(Block block)
        {
            if (!block.Validate())
            {
                throw new BrightChainValidationEnumerableException(
                    exceptions: block.ValidationExceptions,
                    message: "Can not store invalid block");
            }

            this.blockMemoryCache.Set(new TransactableBlock(
                cacheManager: this.blockMemoryCache,
                sourceBlock: block,
                allowCommit: true));

            this.blockDiskCache.Set(new TransactableBlock(
                cacheManager: this.blockDiskCache,
                sourceBlock: block,
                allowCommit: true));

            // TODO dapr
            return block;
        }

        public async IAsyncEnumerable<Tuple<Block, IEnumerable<BrightChainValidationException>>> TryStoreBlocksAsync(IAsyncEnumerable<Block> blockSource)
        {
            await foreach (var block in blockSource)
            {
                Tuple<Block, IEnumerable<BrightChainValidationException>> response;
                try
                {
                    var storedBlock = await this.TryStoreBlockAsync(block)
                    .ConfigureAwait(false);

                    response = new Tuple<Block, IEnumerable<BrightChainValidationException>>(block, new BrightChainValidationException[] { });
                }
                catch (BrightChainValidationEnumerableException brightChainValidation)
                {
                    response = new Tuple<Block, IEnumerable<BrightChainValidationException>>(block, brightChainValidation.Exceptions);
                }

                yield return response;
            }
        }

        public BrightChain BrightenBlocks(IEnumerable<SourceBlock> sourceBlocks)
        {
            BrightenedBlock[] brightenedBlocks = new BrightenedBlock[sourceBlocks.Count()];
            long i = 0;
            foreach (var sourceBlock in sourceBlocks)
            {
                Block[] randomizersUsed;

                var brightenedBlock = this.blockBrightener.Brighten(
                    block: sourceBlock,
                    randomizersUsed: out randomizersUsed);

                this.blockMemoryCache.Set(new TransactableBlock(
                        cacheManager: this.blockMemoryCache,
                        sourceBlock: brightenedBlock,
                        allowCommit: true));

                foreach (var randomizer in randomizersUsed)
                {
                    this.blockMemoryCache.Set(block: new TransactableBlock(
                        cacheManager: this.blockMemoryCache,
                        sourceBlock: randomizer,
                        allowCommit: true));
                }

                brightenedBlocks[i++] = brightenedBlock;
            }

            var segmentBytes = brightenedBlocks.SelectMany(b => b.Id.HashBytes.ToArray()).ToArray();

            return new BrightChain(
                blockParams: new ConstituentBlockListBlockParams(
                    blockParams: new TransactableBlockParams(
                        cacheManager: this.blockMemoryCache,
                        allowCommit: true,
                        blockParams: brightenedBlocks[0].BlockParams),
                    sourceId: brightenedBlocks[0].Id,
                    segmentId: new SegmentHash(
                        dataBytes: new ReadOnlyMemory<byte>(segmentBytes)),
                    totalLength: BlockSizeMap.BlockSize(brightenedBlocks[0].BlockSize) * brightenedBlocks.Length,
                    constituentBlocks: brightenedBlocks.Select(b => b.Id),
                    previous: null,
                    next: null),
                sourceBlocks: brightenedBlocks);
        }
    }
}
