using Microsoft.Extensions.Logging;

namespace BrightChain.Services
{
    /// <summary>
    /// Thin wrapper to parallel DiskCacheManager and provide appropriate BTree options
    /// </summary>
    public class MemoryBlockCacheManager : BlockCacheManager
    {
        public MemoryBlockCacheManager(ILogger logger) :
            base(logger: logger)
        { }
    }
}
