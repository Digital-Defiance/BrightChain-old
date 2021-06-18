using BrightChain.CSharpTest.Net.Collections;
using BrightChain.CSharpTest.Net.IO;
using Microsoft.Extensions.Logging;

namespace BrightChain.Services
{
    public class DiskCacheManager<Tkey, Tvalue> : BPlusTreeCacheManager<Tkey, Tvalue>
    {
        private TempFile backingFile = new TempFile();

        public BPlusTree<Tkey, Tvalue>.OptionsV2 DefaultOptions(BPlusTree<Tkey, Tvalue>.OptionsV2 options)
        {
            options.CalcBTreeOrder(16, 24);
            options.CreateFile = CreatePolicy.Always;
            options.FileName = backingFile.TempPath;
            return options;
        }

        public DiskCacheManager(ILogger logger, BPlusTree<Tkey, Tvalue>.OptionsV2 optionsV2) : base(logger: logger, tree: new BPlusTree<Tkey, Tvalue>(optionsV2)) { }
    }
}
