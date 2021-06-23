using BrightChain.Helpers;
using BrightChain.Models.Blocks;
using BrightChain.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightChain.Tests
{
    [TestClass]
    public abstract class TransactableBlockCacheManagerTest : BPlusTreeCacheManagerTest<BlockCacheManager, BlockHash, TransactableBlock, BlockHashSerializer, BlockSerializer<TransactableBlock>>
    {
    }
}
