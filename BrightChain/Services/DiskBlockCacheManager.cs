using BrightChain.Helpers;
using BrightChain.Models.Blocks;
using CSharpTest.Net.Collections;
using Microsoft.Extensions.Logging;

namespace BrightChain.Services
{
    public class DiskBlockCacheManager : DiskCacheManager<BlockHash, Block>
    {
        public static BPlusTree<BlockHash, Block>.OptionsV2 DefaultOptions()
        {
            BPlusTree<BlockHash, Block>.OptionsV2 options = new BPlusTree<BlockHash, Block>.OptionsV2(
                keySerializer: new BlockHashSerializer(),
                valueSerializer: new BlockSerializer());
            return options;
        }

        public DiskBlockCacheManager(ILogger logger) : base(logger: logger, optionsV2: DefaultOptions())
        {
        }
    }
}
