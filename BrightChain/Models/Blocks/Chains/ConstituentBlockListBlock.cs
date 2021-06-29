using BrightChain.Attributes;
using BrightChain.Enumerations;
using BrightChain.Interfaces;
using BrightChain.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightChain.Models.Blocks.Chains
{
    /// <summary>
    /// A block which describes the hashes of all of the blocks needed to reconstitute a resultant block.
    /// TODO: Ensure that the resultant list doesn't exceed a block, split into two lists, make a new top block, etc.
    /// TODO: Ensure that the hash of the source file
    /// TODO: Validate constituent blocks can recompose into that data (break up by tuple size), validate all blocks are same length
    /// </summary>
    public class ConstituentBlockListBlock : SourceBlock, IBlock, IDisposable, IValidatable
    {
        /// <summary>
        /// Hash of the sum bytes of the file when assembled in order
        /// </summary>
        [BrightChainMetadata]
        public BlockHash SourceId { get; }

        /// <summary>
        /// TupleCount at the time
        /// </summary>
        [BrightChainMetadata]
        public int TupleCount { get; } = BlockWhitener.TupleCount;

        public ConstituentBlockListBlock(ICacheManager<BlockHash, TransactableBlock> cacheManager, BlockSize blockSize, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, BlockHash finalDataHash, IEnumerable<Block> constituentBlocks, bool allowCommit)
        : base(destinationCacheManager: cacheManager,
              blockSize: blockSize,
              data: new ReadOnlyMemory<byte>(
                constituentBlocks
                    .SelectMany(b =>
                        b.Id.HashBytes.ToArray())
                    .ToArray()))
        {
            this.AllowCommit = allowCommit;
            this.ConstituentBlocks = new Block[] { };
            this.SourceId = finalDataHash;
            // TODO : if finalDataHash is null, reconstitute and compute- or accept the validation result's hash essentially?
        }

        public IEnumerable<BlockHash> ConstituentBlockHashes =>
            this.ConstituentBlocks
                .Select(b => b.Id)
                    .ToArray();

        public double TotalCost =>
            this.ConstituentBlocks.Sum(b => b.RedundancyContract.Cost);

        public new bool Validate() =>
            // TODO: perform additional validation as described above
            base.Validate();
    }
}
