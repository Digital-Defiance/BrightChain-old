using BrightChain.Models.Blocks;
using CSharpTest.Net.Collections;
using Microsoft.Extensions.Logging;

namespace BrightChain.Services
{
    public class MemoryBlockCacheManager : MemoryCacheManager<BlockHash, Block>
    {
        public MemoryBlockCacheManager(ILogger logger, BPlusTree<BlockHash, Block>.OptionsV2 optionsV2) : base(logger: logger, optionsV2: optionsV2)
        {
        }
    }
}
