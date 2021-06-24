using BrightChain.Helpers;
using BrightChain.Models.Blocks;
using BrightChain.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightChain.Tests
{
    /// <summary>
    /// Test transactable blocks using the BPlusTreeCacheManagerTest
    /// </summary>
    [TestClass]
    public abstract class TransactableBlockCacheManagerTest : BPlusTreeCacheManagerTest<BlockCacheManager, BlockHash, TransactableBlock, BlockHashSerializer, BlockSerializer<TransactableBlock>>
    {
    }
}
