using BrightChain.Interfaces;
using BrightChain.Models.Blocks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace BrightChain.Services
{
    /// <summary>
    /// Block Cache Manager
    /// </summary>
    public abstract class BlockCacheManager : ICacheManager<BlockHash, TransactableBlock>
    {
        public BlockCacheManager(ILogger logger) { }

        public BlockCacheManager AsBlockCacheManager => this;

        private Dictionary<BlockHash, TransactableBlock> blocks { get; } = new Dictionary<BlockHash, TransactableBlock>();

        public BlockCacheManager()
        {
        }

        public event ICacheManager<BlockHash, TransactableBlock>.KeyAddedEventHandler KeyAdded;
        public event ICacheManager<BlockHash, TransactableBlock>.KeyRemovedEventHandler KeyRemoved;
        public event ICacheManager<BlockHash, TransactableBlock>.CacheMissEventHandler CacheMiss;

        public bool Contains(BlockHash key)
        {
            return blocks.ContainsKey(key);
        }

        public bool Drop(BlockHash key, bool noCheckContains = false)
        {
            if (!noCheckContains && !Contains(key))
            {
                return false;
            }

            blocks.Remove(key);
            return true;
        }
        public TransactableBlock Get(BlockHash key)
        {
            TransactableBlock block;
            bool found = blocks.TryGetValue(key, out block);
            if (!found)
            {
                throw new IndexOutOfRangeException(nameof(key));
            }

            return block;
        }
        public void Set(BlockHash key, TransactableBlock value)
        {
            if (Contains(key))
            {
                throw new BrightChain.Exceptions.BrightChainException("Key already exists");
            }

            if (value is null)
            {
                throw new BrightChain.Exceptions.BrightChainException("Can not store null block");
            }

            blocks[key] = value;
        }
    }
}
