using CSharpTest.Net.Collections;
using Microsoft.Extensions.Logging;

namespace BrightChain.Services
{
    /// <summary>
    /// Thin wrapper to parallel DiskCacheManager and provide appropriate BTree options
    /// </summary>
    public class MemoryCacheManager<Tkey, Tvalue> : BPlusTreeCacheManager<Tkey, Tvalue>
        where Tvalue : new()
    {
        public MemoryCacheManager(ILogger logger, BPlusTree<Tkey, Tvalue>.OptionsV2 optionsV2 = null) :
            base(
                logger: logger,
                tree: optionsV2 is null ? new BPlusTree<Tkey, Tvalue>() : new BPlusTree<Tkey, Tvalue>(optionsV2: optionsV2))
        { }

        public MemoryCacheManager(BPlusTreeCacheManager<Tkey, Tvalue> other) : base(other) { }
    }
}
