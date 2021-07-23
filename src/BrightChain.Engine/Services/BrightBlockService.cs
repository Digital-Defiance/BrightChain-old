#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.Chains;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrightChain.Engine.Services
{
    /// <summary>
    /// Core service for BrightChain used by the webservice to retrieve and store blocks.
    /// </summary>
    public class BrightBlockService
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;

        private readonly MemoryBlockCacheManager blockMemoryCache;
        private readonly MemoryBlockCacheManager randomizerBlockMemoryCache;
        private readonly DiskBlockCacheManager blockDiskCache;
        private readonly BlockBrightener blockBrightener;
        //private readonly BrightChainNode brightChainNodeAuthority;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrightBlockService"/> class.
        /// </summary>
        /// <param name="logger">Instance of the logging provider.</param>
        public BrightBlockService(ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger(nameof(BrightBlockService));
            if (this.logger is null)
            {
                throw new BrightChainException("CreateLogger failed");
            }

            this.logger.LogInformation(string.Format("<{0}>: logging initialized", nameof(BrightBlockService)));
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddYamlFile(
                    path: "brightChainSettings.yaml",
                    optional: false,
                    reloadOnChange: true)
                .AddEnvironmentVariables();

            this.configuration = builder.Build();

            this.blockMemoryCache = new MemoryBlockCacheManager(
                logger: this.logger,
                configuration: this.configuration);
            this.blockDiskCache = new DiskBlockCacheManager(
                logger: this.logger,
                configuration: this.configuration);
            this.randomizerBlockMemoryCache = new MemoryBlockCacheManager(
                logger: this.logger,
                configuration: this.configuration);
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
        public async IAsyncEnumerable<BrightenedBlock> StreamCreatedBrightenedBlocksFromFileAsync(FileInfo fileInfo, DataHash knownSourceHash, BlockParams blockParams, BlockSize? blockSize = null)
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
            if (fileInfo.Length > maximumStorage)
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
                using (FileStream inFile = File.OpenRead(fileInfo.FullName))
                {
                    var bytesRemaining = fileInfo.Length;
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
                        sourceDataLength: fileInfo.Length,
                        computed: true) != knownSourceHash)
                    {
                        throw new BrightChainException("Hash mismatch against known hash");
                    }
                } // end using
            } // end using
        }

        /// <summary>
        /// Computes a SourceId BlockHash for the given file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="blockType"></param>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static DataHash CreateFileDataHash(FileInfo fileInfo)
        {
            using (Stream stream = File.OpenRead(fileInfo.FullName))
            {
                var dataHash = CreateStreamDataHash(stream);
                if (dataHash.SourceDataLength != fileInfo.Length)
                {
                    throw new BrightChainException(nameof(dataHash.SourceDataLength));
                }

                return dataHash;
            }
        }

        public static DataHash CreateStreamDataHash(Stream stream)
        {
            using (var sha = SHA256.Create())
            {
                var streamStart = stream.Position;
                sha.ComputeHash(stream);
                var streamLength = stream.Position - streamStart;
                return new DataHash(
                    providedHashBytes: sha.Hash,
                    sourceDataLength: streamLength,
                    computed: true);
            }
        }

        public async Task<IEnumerable<ConstituentBlockListBlock>> MakeCBLChainFromParamsAsync(string fileName, BlockParams blockParams)
        {
            var blocksUsedThisSegment = new List<BlockHash>();
            var fileInfo = new FileInfo(fileName);
            var sourceHash = CreateFileDataHash(fileInfo: fileInfo);
            var iBlockSize = BlockSizeMap.BlockSize(blockParams.BlockSize);
            var totalBytes = fileInfo.Length;
            var bytesRemaining = totalBytes;
            var brightenedBlocksExpected = (int)(totalBytes / iBlockSize)+((totalBytes % iBlockSize) > 0 ? 1 : 0);
            var hashesPerSegment = BlockSizeMap.HashesPerBlock(blockParams.BlockSize);
            var segmentsExpected = (int)Math.Ceiling((decimal)(brightenedBlocksExpected / hashesPerSegment));
            var cblsEmitted = new ConstituentBlockListBlock[segmentsExpected];
            var cblIdx = 0;

            var bytesPerCBL = hashesPerSegment * iBlockSize;

            var blocksThisSegment = 0;
            var bytesThisSegment = 0;
            var brightenedBlocksConsumed = 0;

            using (SHA256 segmentHasher = SHA256.Create())
            {
                await foreach (BrightenedBlock brightenedBlock in this.StreamCreatedBrightenedBlocksFromFileAsync(fileInfo: fileInfo, knownSourceHash: sourceHash, blockParams: blockParams))
                {
                    brightenedBlocksConsumed++;
                    var lastBlockBytes = bytesRemaining == iBlockSize;
                    bytesRemaining -= iBlockSize;
                    bytesThisSegment += iBlockSize;
                    blocksUsedThisSegment.Add(brightenedBlock.Id);
                    blocksUsedThisSegment.AddRange(brightenedBlock.ConstituentBlocks);
                    var cblFullAfterThisBlock = ++blocksThisSegment == hashesPerSegment;

                    if (cblFullAfterThisBlock || lastBlockBytes)
                    {
                        segmentHasher.TransformFinalBlock(brightenedBlock.Data.ToArray(), 0, iBlockSize);

                        // we have room for one more block, it must either be another CBL or the final block
                        var lastCBL = brightenedBlocksConsumed == brightenedBlocksExpected;

                        // return cbl
                        var cbl = new ConstituentBlockListBlock(blockParams: new ConstituentBlockListBlockParams(
                                blockParams: new TransactableBlockParams(
                                    cacheManager: this.blockMemoryCache,
                                    allowCommit: true,
                                    blockParams: blockParams),
                                sourceId: sourceHash,
                                segmentHash: new SegmentHash(
                                    providedHashBytes: segmentHasher.Hash,
                                    sourceDataLength: bytesThisSegment,
                                    computed: true),
                                totalLength: bytesRemaining > bytesPerCBL ? bytesPerCBL : bytesRemaining,
                                constituentBlocks: blocksUsedThisSegment,
                                previous: cblIdx > 0 ? cblsEmitted[cblIdx - 1].Id : null,
                                next: null));
                        if (cblIdx > 0)
                        {
                            cblsEmitted[cblIdx].Next = cbl.Id;
                        }

                        cblsEmitted[cblIdx++] = cbl;

                        segmentHasher.Initialize();
                        blocksUsedThisSegment.Clear();
                        blocksThisSegment = 0;
                        bytesThisSegment = 0;
                    }
                    else
                    {
                        segmentHasher.TransformBlock(brightenedBlock.Data.ToArray(), 0, iBlockSize, null, 0);
                    }
                }
            }

            if (brightenedBlocksConsumed != brightenedBlocksExpected)
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
                        segmentHash: new SegmentHash(hashBytes),
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
                return firstPass.ElementAt(0);
            }
            else if (count > BlockSizeMap.HashesPerBlock(blockParams.BlockSize))
            {
                throw new NotImplementedException("Super-Super-CBLs not yet implemented");
            }
            else if (count == 0)
            {
                throw new BrightChainException("No blocks returned");
            }

            return await this
                .MakeSuperCBLFromCBLChain(
                    blockParams: blockParams,
                    chainedCbls: firstPass)
                        .ConfigureAwait(false);
        }

        public Dictionary<BlockHash, Block> GetCBLBlocks(ConstituentBlockListBlock block)
        {
            Dictionary<BlockHash, Block> blocks = new Dictionary<BlockHash, Block>();
            foreach (var blockHash in block.ConstituentBlocks)
            {
                blocks.Add(blockHash, this.blockMemoryCache.Get(blockHash));
            }

            return blocks;
        }

        /// <summary>
        /// Given a CBL Block, produce a temporary file on disk containing the reconstitued bytes.
        /// </summary>
        /// <param name="constituentBlockListBlock">The CBL Block containing the list/order of blocks needed to rebuild the file.</param>
        /// <returns>Returns a Restored file object containing the path and hash for the resulting file as written, for verification.</returns>
        public async Task<RestoredFile> RestoreFileFromCBLAsync(ConstituentBlockListBlock constituentBlockListBlock)
        {
            if (constituentBlockListBlock.TotalLength > long.MaxValue)
            {
                throw new NotImplementedException();
            }

            var iBlockSize = BlockSizeMap.BlockSize(constituentBlockListBlock.BlockSize);
            var restoredFile = default(RestoredFile);
            var blockMap = constituentBlockListBlock.GenerateBlockMap();
            restoredFile.Path = Path.GetTempFileName();
            using (var stream = File.OpenWrite(restoredFile.Path))
            {
                using (SHA256 sha = SHA256.Create())
                {
                    long bytesWritten = 0;
                    StreamWriter streamWriter = new StreamWriter(stream);
                    await foreach (Block block in blockMap.ConsolidateTuplesToChainAsyc())
                    {
                        var bytesLeft = constituentBlockListBlock.TotalLength - bytesWritten;
                        var lastBlock = bytesLeft < iBlockSize;
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
                    }

                    restoredFile.SourceId = new BlockHash(
                        blockType: typeof(ConstituentBlockListBlock),
                        originalBlockSize: constituentBlockListBlock.BlockSize,
                        providedHashBytes: sha.Hash,
                        computed: true);
                }

                stream.Flush();
                stream.Close();
                FileInfo fileInfo = new FileInfo(restoredFile.Path);

                if (fileInfo.Length != constituentBlockListBlock.TotalLength)
                {
                    throw new BrightChainException(nameof(fileInfo.Length));
                }
            }

            return restoredFile;
        }
    }
}
