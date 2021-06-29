using Microsoft.Extensions.Logging;

namespace BrightChain.Services
{
    /// <summary>
    /// Disk backed version of BPlus tree
    /// </summary>
    public class DiskBlockCacheManager : BlockCacheManager
    {
        public DiskBlockCacheManager(ILogger logger) :
            base(logger: logger)
        { }
    }
}
