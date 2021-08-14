﻿namespace BrightChain.Engine.Interfaces
{
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Hashes;
    using BrightChain.Engine.Models.Nodes;

    /// <summary>
    /// Basic guaranteed members of the cache system.
    /// </summary>
    public interface IBlockCacheManager : ICacheManager<BlockHash, TransactableBlock>
    {
        /// <summary>
        /// Adds a key to the cache if it is not already present
        /// </summary>
        /// <param name="key">key to palce in the cache</param>
        void Set(TransactableBlock value);

        /// <summary>
        /// Add a node that the cache manager should trust.
        /// </summary>
        /// <param name="node">Node submitting the block to the cache.</param>
        void Trust(BrightChainNode node);
    }
}
