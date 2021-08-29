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
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using BrightChain.Engine.Models.Nodes;
    using BrightChain.Engine.Services.CacheManagers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Core service for BrightChain used by the webservice to retrieve and store blocks.
    /// TODO: Eventually needs to contain blockhash indices for things like expiration (grouped by ulong expiration second),
    /// lists by block type, etc.
    /// </summary>
    public class BrightBlockService : IDisposable
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;

        private readonly MemoryDictionaryBlockCacheManager randomizerBlockMemoryCache;
        private readonly FasterBlockCacheManager blockFasterCache;
        private readonly BlockBrightenerService blockBrightener;
        private readonly BrightChainNode brightChainNodeAuthority;

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

            this.configuration = configuration;

            var nodeOptions = configuration.GetSection("NodeOptions");
            if (nodeOptions is null || !nodeOptions.Exists())
            {
                this.configuration = ConfigurationHelper.LoadConfiguration();
                nodeOptions = this.configuration.GetSection("NodeOptions");
            }

            if (nodeOptions is null || !nodeOptions.Exists())
            {
                throw new BrightChainException(string.Format(format: "'NodeOptions' config section must be defined, but is not"));
            }

            var configuredDbName
                = nodeOptions.GetSection("DatabaseName");

            var dbNameConfigured = configuredDbName is not null && configuredDbName.Value.Any();
            Guid serviceUnifiedStoreGuid = dbNameConfigured ? Guid.Parse(configuredDbName.Value) : Guid.NewGuid();

            if (!dbNameConfigured)
            {
                //global::BrightChain.Engine.Helpers.ConfigurationHelper.AddOrUpdateAppSetting("NodeOptions:DatabaseName", Utilities.HashToFormattedString(serviceUnifiedStoreGuid.ToByteArray()));
            }

            var rootBlock = new RootBlock(
                databaseGuid: serviceUnifiedStoreGuid);

            this.blockFasterCache = new FasterBlockCacheManager(
                logger: this.logger,
                configuration: this.configuration,
                rootBlock: rootBlock);

            this.randomizerBlockMemoryCache = new MemoryDictionaryBlockCacheManager(
                logger: this.logger,
                configuration: this.configuration,
                rootBlock: rootBlock);

            this.logger.LogInformation(string.Format("<{0}>: caches initialized", nameof(BrightBlockService)));
            this.blockBrightener = new BlockBrightenerService(
                resultCache: this.blockFasterCache);
            this.brightChainNodeAuthority = new BrightChainNode(this.configuration);
        }

        public RootBlock RootBlock => this.blockFasterCache.RootBlock;

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

                        var brightenedBlock = this.blockBrightener.Brighten(
                            identifiableBlock: new IdentifiableBlock(
                                blockParams: blockParams,
                                data: buffer),
                            randomizersUsed: out _,
                            brightenedStripe: out _);

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

        /// <summary>
        /// TODO: refactor out the core into a streaming CBL maker with a file stream wrapper. Then we can have a functionm that just takes the data.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="blockParams"></param>
        /// <returns></returns>
        public async Task<IEnumerable<BrightChain>> MakeCBLChainFromParamsAsync(string fileName, BlockParams blockParams)
        {
            var sourceInfo = new SourceFileInfo(
                fileName: fileName,
                blockSize: blockParams.BlockSize);
            var blockHashesUsedThisSegment = new List<BlockHash>();
            var brightenedBlocksThisSegment = new List<BrightenedBlock>();
            var sourceBytesRemaining = sourceInfo.FileInfo.Length;
            var totalBytesRemaining = sourceInfo.TotalBlockedBytes;
            var cblsExpected = sourceInfo.CblsExpected;
            var cblsEmitted = new BrightChain[cblsExpected];
            var cblIdx = 0;
            var blocksRemaining = sourceInfo.TotalBlocksExpected;

            var blockCountThisSegment = 0;
            var sourceByteCountThisSegment = 0;
            var brightenedBlocksConsumed = 0;

            using (SHA256 segmentHasher = SHA256.Create())
            {
                // last block is always full of random data
                await foreach (BrightenedBlock brightenedBlock in this.StreamCreatedBrightenedBlocksFromFileAsync(sourceInfo: sourceInfo, blockParams: blockParams))
                {
                    brightenedBlocksThisSegment.Add(brightenedBlock);
                    this.blockFasterCache.Set(brightenedBlock);
                    sourceByteCountThisSegment += (int)(sourceBytesRemaining < sourceInfo.BytesPerBlock ? sourceBytesRemaining : brightenedBlock.Bytes.Length);
                    blockHashesUsedThisSegment.Add(brightenedBlock.Id);
                    blockHashesUsedThisSegment.AddRange(brightenedBlock.ConstituentBlocks);
                    var cblFullAfterThisBlock = ++blockCountThisSegment == sourceInfo.HashesPerBlock;
                    var sourceConsumed = totalBytesRemaining <= sourceInfo.BytesPerBlock;
                    blocksRemaining--;
                    brightenedBlocksConsumed++;
                    if (cblFullAfterThisBlock || sourceConsumed || (blocksRemaining == 0))
                    {
                        var sourceBytesThisBlock = sourceBytesRemaining > sourceInfo.BytesPerBlock ? sourceInfo.BytesPerBlock : sourceBytesRemaining;

                        segmentHasher.TransformFinalBlock(brightenedBlock.Bytes.ToArray(), 0, (int)sourceBytesThisBlock);

                        var cblParams = new ConstituentBlockListBlockParams(
                                blockParams: new BrightenedBlockParams(
                                    cacheManager: this.blockFasterCache,
                                    allowCommit: true,
                                    blockParams: blockParams),
                                sourceId: sourceInfo.SourceId,
                                segmentId: new SegmentHash(
                                    providedHashBytes: segmentHasher.Hash,
                                    sourceDataLength: sourceByteCountThisSegment,
                                    computed: true),
                                totalLength: sourceBytesRemaining > sourceInfo.BytesPerCbl ? sourceInfo.BytesPerCbl : sourceBytesRemaining,
                                constituentBlockHashes: blockHashesUsedThisSegment.ToArray(),
                                previous: cblIdx > 0 ? cblsEmitted[cblIdx - 1].Id : null,
                                next: null);

                        var cbl = new BrightChain(
                            blockParams: cblParams,
                            brightenedBlocks: brightenedBlocksThisSegment);

                        // update the next pointer of the previous block
                        if (cblIdx > 0)
                        {
                            cblsEmitted[cblIdx].Next = cbl.Id;
                        }

                        cblsEmitted[cblIdx++] = cbl;

                        segmentHasher.Initialize();
                        blockHashesUsedThisSegment.Clear();
                        brightenedBlocksThisSegment.Clear();
                        blockCountThisSegment = 0;
                        sourceByteCountThisSegment = 0;
                    }
                    else
                    {
                        segmentHasher.TransformBlock(
                            inputBuffer: brightenedBlock.Bytes.ToArray(),
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

        public async Task<BrightChain> MakeSuperCBLFromCBLChainAsync(BlockParams blockParams, IEnumerable<ConstituentBlockListBlock> chainedCbls, DataHash sourceId)
        {
            var hashBytes = chainedCbls
                    .SelectMany(c => c.Id.HashBytes.ToArray())
                    .ToArray();

            var constituentHashes = chainedCbls.Select(c => c.Id);

            var sCbl = new SuperConstituentBlockListBlock(
                    blockParams: new ConstituentBlockListBlockParams(
                        blockParams: new BrightenedBlockParams(
                            cacheManager: this.blockFasterCache,
                            allowCommit: true,
                            blockParams: blockParams),
                        sourceId: sourceId,
                        segmentId: new SegmentHash(hashBytes),
                        totalLength: hashBytes.Length,
                        constituentBlockHashes: constituentHashes,
                        previous: null,
                        next: null));

            return new BrightChain(
                blockParams: sCbl.BlockParams,
                sourceCache: this.blockFasterCache);
        }

        public async Task<BrightChain> MakeCblOrSuperCblFromFileAsync(string fileName, BlockParams blockParams)
        {
            var firstPass = await this.MakeCBLChainFromParamsAsync(
                fileName: fileName,
                blockParams: blockParams)
                    .ConfigureAwait(false);

            var count = firstPass.Count();

            if (count == 0)
            {
                throw new BrightChainException("No blocks returned");
            }

            var loneCbl = firstPass.ElementAt(0);

            if (count == 1)
            {
                return loneCbl;
            }
            else if (count > BlockSizeMap.HashesPerBlock(blockParams.BlockSize))
            {
                throw new NotImplementedException("Uber-CBLs not yet implemented");
            }

            // TODO: figure out where/when to commit the firstPass blocks

            return await
                this.MakeSuperCBLFromCBLChainAsync(
                    blockParams: blockParams,
                    chainedCbls: firstPass,
                    sourceId: loneCbl.SourceId)
                        .ConfigureAwait(false);
        }

        public static Dictionary<BlockHash, Block> GetCBLBlocksFromCacheAsDictionary(BrightenedBlockCacheManagerBase blockCacheManager, ConstituentBlockListBlock block)
        {
            Dictionary<BlockHash, Block> blocks = new Dictionary<BlockHash, Block>();
            foreach (var blockHash in block.ConstituentBlocks)
            {
                blocks.Add(blockHash, blockCacheManager.Get(blockHash));
            }

            return blocks;
        }

        public async Task<Stream> RestoreStreamFromCBLAsync(ConstituentBlockListBlock constituentBlockListBlock, Stream? destination = null)
        {
            if (destination is null)
            {
                destination = new MemoryStream();
            }

            if (constituentBlockListBlock.TotalLength > long.MaxValue)
            {
                throw new NotImplementedException();
            }

            if (constituentBlockListBlock is SuperConstituentBlockListBlock superConstituentBlockListBlock)
            {
                throw new NotImplementedException();
            }

            long bytesWritten = 0;
            using (SHA256 sha = SHA256.Create())
            {
                var iBlockSize = BlockSizeMap.BlockSize(constituentBlockListBlock.BlockSize);
                StreamWriter streamWriter = new StreamWriter(destination);
                var cacheManager = this.blockFasterCache.AsBlockCacheManager;
                var blockMap = constituentBlockListBlock.CreateBrightMap();
                await foreach (Block block in blockMap.ConsolidateTuplesToChainAsync(cacheManager))
                {
                    var bytesLeft = constituentBlockListBlock.TotalLength - bytesWritten;
                    var lastBlock = bytesLeft <= iBlockSize;
                    var length = lastBlock ? bytesLeft : iBlockSize;
                    if (lastBlock)
                    {
                        sha.TransformFinalBlock(block.Bytes.ToArray(), 0, (int)length);
                    }
                    else
                    {
                        sha.TransformBlock(block.Bytes.ToArray(), 0, (int)length, null, 0);
                    }

                    await destination.WriteAsync(buffer: block.Bytes.ToArray(), offset: 0, count: (int)length)
                        .ConfigureAwait(false);
                    bytesWritten += length;
                }

                if (bytesWritten == 0)
                {
                    throw new BrightChainException(nameof(bytesWritten));
                }

                var finalHash = new DataHash(
                    providedHashBytes: sha.Hash,
                    sourceDataLength: bytesWritten,
                    computed: true);

                if (!finalHash.Equals(constituentBlockListBlock.SourceId))
                {
                    throw new BrightChainException(nameof(finalHash));
                }

                await destination
                    .FlushAsync()
                    .ConfigureAwait(false);

                destination.Position = 0;

                return destination;
            }
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

            string tempFilename = Path.GetTempFileName();

            using (Stream destination = File.OpenWrite(tempFilename))
            {
                using (await this
                   .RestoreStreamFromCBLAsync(constituentBlockListBlock, destination)
                   .ConfigureAwait(false))
                {
                    destination.Close();
                    destination.Dispose();
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
        }

        public async Task<BrightenedBlock> FindBlockByIdAsync(BlockHash id)
        {
            if (this.blockFasterCache.Contains(id))
            {
                return this.blockFasterCache.Get(id);
            }

            // TODO: look to other nodes
            throw new KeyNotFoundException(id.ToString());
        }

        public async IAsyncEnumerable<Block> FindBlocksByIdAsync(IAsyncEnumerable<BlockHash> blockIdSource)
        {
            await foreach (var id in blockIdSource)
            {
                var block = await this.FindBlockByIdAsync(id)
                    .ConfigureAwait(false);

                if (block is Block && block.Validate())
                {
                    yield return block;
                }
            }
        }

        public async Task<T> FindBlockByIdAsync<T>(BlockHash id, bool useAsBlock)
            where T : class
        {
            var retrievedBlock = await this.FindBlockByIdAsync(id)
                .ConfigureAwait(false);
            var block = useAsBlock ? retrievedBlock.AsBlock as T : retrievedBlock as T;
            if (block is null)
            {
                throw new BrightChainException("Unable to cast from unrelated type");
            }

            return block;
        }

        public async Task<Block> DropBlockByIdAsync(BlockHash id, object? ownershipToken = null)
        {
            var block = await this.FindBlockByIdAsync(id: id)
                .ConfigureAwait(false);

            // verify pernmission/ownership/date, etc
            // can't drop unless expired or invalid
            if (block is Block && true)
            {
                throw new BrightChainException("Permission denied!");
            }

            if (this.blockFasterCache.Contains(id))
            {
                this.blockFasterCache.Drop(id);
            }

            // TODO: broadcast
            return block;
        }

        public async IAsyncEnumerable<(BlockHash, Block?)> DropBlocksByIdAsync(IAsyncEnumerable<BlockHash> idSource, object ownershipToken = null)
        {
            await foreach (var id in idSource)
            {
                var dropped = await this.DropBlockByIdAsync(id, ownershipToken)
                    .ConfigureAwait(false);

                yield return (id, dropped);
            }
        }

        public async Task<Block> StoreBlockAsync(BrightenedBlock block)
        {
            if (!block.Validate())
            {
                throw new BrightChainValidationEnumerableException(
                    exceptions: block.ValidationExceptions,
                    message: "Can not store invalid block");
            }

            this.blockFasterCache.Set(block);

            return block;
        }

        public async IAsyncEnumerable<(Block, IEnumerable<BrightChainValidationException>)> StoreBlocksAsync(IAsyncEnumerable<BrightenedBlock> blockSource)
        {
            await foreach (var block in blockSource)
            {
                (Block, IEnumerable<BrightChainValidationException>) response;
                try
                {
                    var storedBlock = await this.StoreBlockAsync(block)
                    .ConfigureAwait(false);

                    response = (block, new BrightChainValidationException[] { });
                }
                catch (BrightChainValidationEnumerableException brightChainValidation)
                {
                    response = (block, brightChainValidation.Exceptions);
                }

                yield return response;
            }
        }

        public async IAsyncEnumerable<BrightenedBlock> BrightenBlocksAsyncEnumerable(IAsyncEnumerable<IdentifiableBlock> identifiableBlocks)
        {
            await foreach (var identifiableBlock in identifiableBlocks)
            {
                var brightenedBlock = this.blockBrightener.Brighten(
                    identifiableBlock: identifiableBlock,
                    randomizersUsed: out _,
                    brightenedStripe: out _);

                brightenedBlock.MakeTransactable(
                    cacheManager: this.blockFasterCache,
                    allowCommit: true);

                yield return brightenedBlock;
            }
        }

        /// <summary>
        /// Given a collection of brightened blocks either as part of a file or ChainLinq, assemble them into a CBL.
        /// </summary>
        /// <param name="brightenedBlocks"></param>
        /// <returns></returns>
        public async Task<BrightChain> ForgeChainAsync(DataHash sourceId, IAsyncEnumerable<BrightenedBlock> brightenedBlocks)
        {
            var hashes = new List<BlockHash>();
            var awaitedBlocks = new List<BrightenedBlock>();

            await foreach (var block in brightenedBlocks)
            {
                hashes.Add(block.Id);
                awaitedBlocks.Add(block);
            }

            var segmentBytes = hashes.SelectMany(h => h.HashBytes.ToArray()).ToArray();

            var firstBlock = awaitedBlocks.First();

            return new BrightChain(
                blockParams: new ConstituentBlockListBlockParams(
                    blockParams: new BrightenedBlockParams(
                        cacheManager: this.blockFasterCache,
                        allowCommit: true,
                        blockParams: firstBlock.BlockParams),
                    sourceId: sourceId,
                    segmentId: new SegmentHash(
                        dataBytes: new ReadOnlyMemory<byte>(segmentBytes)),
                    totalLength: BlockSizeMap.BlockSize(firstBlock.BlockSize) * awaitedBlocks.Count,
                    constituentBlockHashes: hashes,
                    previous: null,
                    next: null),
                brightenedBlocks: awaitedBlocks);
        }

        public BrightHandle CblToBrightHandle(ConstituentBlockListBlock cblBlock, BrightenedBlock brightenedCbl, TupleStripe cblStripe)
        {
            return new BrightHandle(
                    blockSize: brightenedCbl.BlockSize,
                    blockHashes: cblStripe.Blocks
                        .Select(b => b.Id)
                        .ToArray(),
                    originalType: cblStripe.OriginalType,
                    brightenedCblHash: brightenedCbl.Id,
                    identifiableSourceHash: cblBlock.SourceId);
        }

        public BrightHandle BrightenCbl(ConstituentBlockListBlock cblBlock, bool persist, out BrightenedBlock brightenedCbl)
        {
            // TODO: update indices
            // TODO: CBLs may be a server option to disable
            // CBL should itself be brightened before entering the cache!
            brightenedCbl = this.blockBrightener.Brighten(
                identifiableBlock: cblBlock,
                randomizersUsed: out _,
                brightenedStripe: out TupleStripe cblStripe);

            var handle = new BrightHandle(
                    blockSize: cblBlock.BlockSize,
                    blockHashes: cblStripe.Blocks
                        .Select(b => b.Id)
                        .ToArray(),
                    originalType: cblStripe.OriginalType,
                    brightenedCblHash: brightenedCbl.Id,
                    identifiableSourceHash: cblBlock.SourceId);

            if (persist)
            {
                this.blockFasterCache.Set(brightenedCbl);
                this.blockFasterCache.SetCbl(
                    brightenedCblHash: brightenedCbl.Id,
                    identifiableSourceHash: cblBlock.SourceId,
                    brightHandle: handle);
            }

            return handle;
        }

        public BrightHandle FindSourceById(DataHash requestedHash)
        {
            return this.blockFasterCache.GetCbl(requestedHash);
        }

        public TupleStripe BrightHandleToTupleStripe(BrightHandle brightHandle)
        {
            return new TupleStripe(
                tupleCountMatch: brightHandle.BlockHashByteArrays.Count(),
                blockSizeMatch: brightHandle.BlockSize,
                originalType: brightHandle.OriginalType,
                brightenedBlocks: this.blockFasterCache.Get(brightHandle.BlockHashes));
        }

        public IdentifiableBlock BrightHandleToIdentifiableBlock(BrightHandle brightHandle)
        {
            return this.BrightHandleToTupleStripe(brightHandle)
                .Consolidate();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
