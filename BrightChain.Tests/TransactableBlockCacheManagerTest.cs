using BrightChain.Interfaces;
using BrightChain.Models.Blocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightChain.Tests
{
    /// <summary>
    /// Test transactable blocks using the BPlusTreeCacheManagerTest
    /// </summary>
    [TestClass]
    public abstract class TransactableBlockCacheManagerTest<TcacheManager> : CacheManagerTest<TcacheManager, BlockHash, TransactableBlock>
        where TcacheManager : ICacheManager<BlockHash, TransactableBlock>
    {
    }
}
