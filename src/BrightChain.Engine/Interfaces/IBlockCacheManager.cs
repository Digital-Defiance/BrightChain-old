namespace BrightChain.Engine.Interfaces
{
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Nodes;

    /// <summary>
    /// Basic guaranteed members of the cache system. Notably the system is heavily dependent on the BPlusTree caches which have transaction support.
    /// </summary>
    public interface IBlockCacheManager : ICacheManager<BlockHash, TransactableBlock>
    {
        /// <summary>
        /// Gets a value indicating whether to only accept blocks from trusted nodes.
        /// </summary>
        bool OnlyAcceptBlocksFromTrustedNodes { get; }

        /// <summary>
        /// Add a node that the cache manager should trust.
        /// </summary>
        /// <param name="node">Node submitting the block to the cache.</param>
        void Trust(BrightChainNode node);
    }
}
