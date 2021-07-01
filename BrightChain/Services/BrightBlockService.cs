#nullable enable
using BrightChain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;


namespace BrightChain.Services
{
    /// <summary>
    /// Core service for BrightChain used by the webservice to retrieve and store blocks.
    /// </summary>
    public class BrightBlockService
    {
        protected ILogger logger;
        protected IConfiguration configuration;

        protected MemoryBlockCacheManager blockMemoryCache;
        protected BrightChainBlockCacheManager blockDiskCache;


        public BrightBlockService(ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger(nameof(BrightBlockService));
            if (this.logger is null)
            {
                throw new BrightChainException("CreateLogger failed");
            }

            this.logger.LogInformation(String.Format("<{0}>: logging initialized", nameof(BrightBlockService)));
            this.configuration = new ConfigurationBuilder()
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

            this.blockMemoryCache = new MemoryBlockCacheManager(logger: this.logger);
            this.blockDiskCache = new BrightChainBlockCacheManager(logger: this.logger, configuration: this.configuration);
            this.logger.LogInformation(String.Format("<{0}>: caches initialized", nameof(BrightBlockService)));

        }
    }
}
