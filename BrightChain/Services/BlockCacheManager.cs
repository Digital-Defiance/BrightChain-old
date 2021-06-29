using BrightChain.Interfaces;
using BrightChain.Models.Blocks;
using Microsoft.Extensions.Logging;

namespace BrightChain.Services
{
    /// <summary>
    /// Block Cache Manager
    /// </summary>
    public abstract class BlockCacheManager : ICacheManager<BlockHash, TransactableBlock>
    {
        public BlockCacheManager(ILogger logger) { }

        public BlockCacheManager AsBlockCacheManager => this;

        public event ICacheManager<BlockHash, TransactableBlock>.KeyAddedEventHandler KeyAdded;
        public event ICacheManager<BlockHash, TransactableBlock>.KeyRemovedEventHandler KeyRemoved;
        public event ICacheManager<BlockHash, TransactableBlock>.CacheMissEventHandler CacheMiss;

        public bool Contains(BlockHash key) => throw new System.NotImplementedException();
        public bool Drop(BlockHash key, bool noCheckContains = false) => throw new System.NotImplementedException();
        public TransactableBlock Get(BlockHash key) => throw new System.NotImplementedException();
        public void Set(BlockHash key, TransactableBlock value) => throw new System.NotImplementedException();
    }
}
