#nullable enable
using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Models.Blocks;
using BrightChain.Models.Blocks.Chains;
using BrightChain.Models.Blocks.DataObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace BrightChain.Services
{
    /// <summary>
    /// Core service for BrightChain used by the webservice to retrieve and store blocks.
    /// </summary>
    public class BrightBlockService
    {
        protected readonly ILogger logger;
        protected readonly IConfiguration configuration;

        protected readonly MemoryBlockCacheManager blockMemoryCache;
        protected readonly MemoryBlockCacheManager randomizerBlockMemoryCache;
        protected readonly BrightChainBlockCacheManager blockDiskCache;
        protected readonly BlockWhitener blockWhitener;

        public BrightBlockService(ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger(nameof(BrightBlockService));
            if (this.logger is null)
            {
                throw new BrightChainException("CreateLogger failed");
            }

            this.logger.LogInformation(String.Format("<{0}>: logging initialized", nameof(BrightBlockService)));
            configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("brightchainSettings.json").Build();

            var services = new ServiceCollection();
            #region API Versioning
            // Add API Versioning to the Project
            services.AddApiVersioning(setupAction: config =>
            {
                // Specify the default API Version as 1.0
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // If the client hasn't specified the API version in the request, use the default API version number 
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            });
            #endregion

            blockMemoryCache = new MemoryBlockCacheManager(logger: this.logger);
            blockDiskCache = new BrightChainBlockCacheManager(logger: this.logger, configuration: configuration);
            randomizerBlockMemoryCache = new MemoryBlockCacheManager(logger: this.logger);
            this.logger.LogInformation(String.Format("<{0}>: caches initialized", nameof(BrightBlockService)));
            blockWhitener = new BlockWhitener(pregeneratedRandomizerCache: randomizerBlockMemoryCache);
        }

        public ConstituentBlockListBlock CreateCblFromFile(string fileName, DateTime keepUntilAtLeast, RedundancyContractType redundancy, bool allowCommit, bool privateEncrypted = false, BlockSize? blockSize = null)
        {
            FileStream inFile = File.OpenRead(fileName);

            if (!blockSize.HasValue)
            {
                // decide best block size if null
                throw new NotImplementedException();
            }

            if (privateEncrypted)
            {
                throw new NotImplementedException();
            }

            var iBlockSize = BlockSizeMap.BlockSize(blockSize.Value);
            int tuplesRequired = (int)Math.Ceiling((double)(inFile.Length / iBlockSize));

            SHA256 hasher = SHA256.Create();
            byte[]? finalHash = null;
            // TODO: figure out how to stream huge files with yield, etc
            // TODO: use block whitener
            TupleStripe[] tupleStripes = new TupleStripe[tuplesRequired];
            List<Block> consumedBlocks = new List<Block>();
            ulong offset = 0;
            for (int i = 0; i < tuplesRequired; i++)
            {
                var finalBlock = i == (tuplesRequired - 1);
                byte[] buffer = new byte[iBlockSize];
                var bytesRead = (ulong)inFile.Read(buffer, 0, iBlockSize);
                offset += bytesRead;

                if ((bytesRead < (ulong)iBlockSize) && !finalBlock)
                {
                    throw new BrightChainException("Unexpected EOF");
                }
                else if ((bytesRead < (ulong)iBlockSize) && finalBlock)
                {
                    buffer = Helpers.RandomDataHelper.DataFiller(new ReadOnlyMemory<byte>(buffer), blockSize.Value).ToArray();
                }

                if (finalBlock)
                {
                    finalHash = hasher.TransformFinalBlock(buffer, 0, iBlockSize);
                }
                else
                {
                    hasher.TransformBlock(buffer, 0, iBlockSize, null, 0);
                }

                var sourceBlock = new SourceBlock(
                    new TransactableBlockParams(
                            cacheManager: blockMemoryCache, // SourceBlock itself cannot be persisted to cache, but resultant blocks from NewBlock via XOR go here
                            blockArguments: new BlockParams(
                                blockSize: blockSize.Value,
                                requestTime: DateTime.Now,
                                keepUntilAtLeast: DateTime.MaxValue,
                                redundancy: RedundancyContractType.HeapAuto,
                                allowCommit: true,
                                privateEncrypted: privateEncrypted)),
                            data: buffer);

                Block whitened = blockWhitener.Whiten(sourceBlock);
                Block[] randomizersUsed = (Block[])whitened.ConstituentBlocks;
                Block[] allBlocks = new Block[BlockWhitener.TupleCount];
                allBlocks[0] = whitened;
                Array.Copy(sourceArray: randomizersUsed, sourceIndex: 0, destinationArray: allBlocks, destinationIndex: 1, length: randomizersUsed.Length);

                tupleStripes[i] = new TupleStripe(
                    tupleCount: BlockWhitener.TupleCount,
                    blocks: allBlocks);

                consumedBlocks.AddRange(allBlocks);
            }

            if (finalHash == null)
            {
                throw new BrightChainException("impossible");
            }

            var cbl = new ConstituentBlockListBlock(
                blockArguments: new ConstituentBlockListBlockParams(
                    blockArguments: new TransactableBlockParams(
                        cacheManager: blockMemoryCache,
                        blockArguments: new BlockParams(
                            blockSize: blockSize.Value,
                            requestTime: DateTime.Now,
                            keepUntilAtLeast: keepUntilAtLeast,
                            redundancy: redundancy,
                            allowCommit: allowCommit,
                            privateEncrypted: privateEncrypted)),
                finalDataHash: new BlockHash(
                    originalBlockSize: blockSize.Value,
                    providedHashBytes:
                    finalHash, true),
                totalLength: (ulong)inFile.Length,
                constituentBlocks: consumedBlocks.ToArray()));

            return cbl;
        }
    }
}
