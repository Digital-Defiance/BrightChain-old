using BrightChain.Enumerations;
using BrightChain.Interfaces;
using System;
using System.Collections.Generic;

namespace BrightChain.Models.Blocks.Chains
{
    /// <summary>
    /// Represents a virtual map of all the contituent tuple-sets/blocks in a given source/reconstructed file. These cannot themselves be committed to disk
    /// The block datas may not actually be loaded in memory, but the appropriate blocks will be loaded (all non-local will be pulled into the cache first) relative to their access offsets.
    /// source file -> blockchainfilemap -> commit
    /// pull blocks from pool -> blockchainfilemap -> read
    /// </summary>
    public class BlockChainFileMap : ConstituentBlockListBlock
    {
        public IEnumerable<TupleStripe> Tuples { get; }

        public BlockChainFileMap(ICacheManager<BlockHash, TransactableBlock> cacheManager, BlockSize blockSize, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, BlockHash finalDataHash, IEnumerable<Block> constituentBlocks) : base(cacheManager: cacheManager, blockSize: blockSize, requestTime: requestTime, keepUntilAtLeast: keepUntilAtLeast, redundancy: redundancy, finalDataHash: finalDataHash, constituentBlocks: constituentBlocks, allowCommit: false)
        {
        }
    }
}
