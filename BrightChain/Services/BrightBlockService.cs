using BrightChain.Exceptions;
using BrightChain.Interfaces;
using BrightChain.Models.Blocks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace BrightChain.Services
{
    public class BrightBlockService : ILoggingBuilder
    {
        protected ILogger logger;
        protected IConfiguration configuration;

        protected ICacheManager<BlockHash, Block> blockMemoryCache;
        protected ICacheManager<BlockHash, Block> blockDiskCache;

        protected IServiceCollection services;
        public IServiceCollection Services => this.services;

        public BrightBlockService(ILoggerFactory logger, IConfiguration configuration, IServiceCollection services)
        {
            this.logger = logger.CreateLogger(nameof(BrightBlockService));
            if (this.logger is null)
                throw new BrightChainException("CreateLogger failed");
            this.logger.LogInformation(String.Format("<%s>: logging initialized", nameof(BrightBlockService)));
            this.configuration = configuration;
            this.services = services;

            this.blockMemoryCache = new MemoryCacheManager<BlockHash, Block>(logger: this.logger);
            this.blockDiskCache = new DiskBlockCacheManager(logger: this.logger);
            this.logger.LogInformation(String.Format("<%s>: caches initialized", nameof(BrightBlockService)));
        }
    }
}
