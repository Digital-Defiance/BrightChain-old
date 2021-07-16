using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BrightChain.Engine.Services
{
    public class BrightChainBlockCacheManager : BlockCacheManager
    {
        private readonly IConfiguration configuration;
        public BrightChainBlockCacheManager(ILogger logger, IConfiguration configuration) : base(logger: logger)
        {
            this.configuration = configuration;
        }
    }
}
