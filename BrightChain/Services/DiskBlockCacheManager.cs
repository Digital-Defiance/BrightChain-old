using BrightChain.Models.Blocks;
using CSharpTest.Net.Collections;
using CSharpTest.Net.IO;
using Microsoft.Extensions.Logging;

namespace BrightChain.Services
{
    /// <summary>
    /// Disk backed version of BPlus tree
    /// </summary>
    public class DiskBlockCacheManager : BlockCacheManager
    {
        private TempFile backingFile = new TempFile();

        public DiskBlockCacheManager(BlockCacheManager other) : base(other) { }

        public DiskBlockCacheManager(ILogger logger, BPlusTree<BlockHash, TransactableBlock>.OptionsV2 optionsV2) :
            base(logger: logger, tree: new BPlusTree<BlockHash, TransactableBlock>(optionsV2))
        { }

        public DiskBlockCacheManager(ILogger logger, BPlusTree<BlockHash, TransactableBlock> tree) :
            base(logger: logger, tree: tree)
        { }

        public BPlusTree<BlockHash, TransactableBlock>.OptionsV2 DefaultOptions(BPlusTree<BlockHash, TransactableBlock>.OptionsV2 options)
        {
            options.CalcBTreeOrder(16, 24);
            options.CreateFile = CreatePolicy.Always;
            options.FileName = backingFile.TempPath;
            return options;
        }
    }
}
