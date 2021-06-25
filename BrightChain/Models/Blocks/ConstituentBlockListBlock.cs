using BrightChain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// A block which describes the hashes of all of the blocks needed to reconstitute a resultant block.
    /// TODO: Ensure that the resultant list doesn't exceed a block, split into two lists, make a new top block, etc.
    /// </summary>
    public class ConstituentBlockListBlock : Block
    {
        public new readonly IEnumerable<Block> ConstituentBlocks;
        private readonly Block sourceBlock;

        public ConstituentBlockListBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy) :
            base(requestTime: requestTime, keepUntilAtLeast: keepUntilAtLeast, redundancy: redundancy, data: new byte[] { })
        {
            this.sourceBlock = null;
            this.ConstituentBlocks = new Block[] { };
        }

        public ConstituentBlockListBlock(Block sourceBlock) :
            base(
                requestTime: sourceBlock.StorageContract.RequestTime,
                keepUntilAtLeast: sourceBlock.StorageContract.KeepUntilAtLeast,
                redundancy: sourceBlock.RedundancyContract.RedundancyContractType,
                data: sourceBlock.Data)
        {
            this.sourceBlock = sourceBlock;
            this.ConstituentBlocks = sourceBlock.ConstituentBlocks;
        }

        public IEnumerable<BlockHash> ConstituentBlockHashes =>
            ConstituentBlocks
                .Select(b => b.Id)
                    .ToArray();

        public new ReadOnlyMemory<byte> Data =>
            new ReadOnlyMemory<byte>(
                ConstituentBlocks
                    .SelectMany(b =>
                        b.Id.HashBytes.ToArray())
                    .ToArray());

        public double TotalCost =>
            ConstituentBlocks.Sum(b => b.RedundancyContract.Cost);

        public override void Dispose()
        {
        }

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) =>
            (this.sourceBlock is null) ?
                this.sourceBlock.NewBlock(requestTime: requestTime, keepUntilAtLeast: keepUntilAtLeast, redundancy: redundancy, data: data, allowCommit: allowCommit)
            :
                throw new NullReferenceException(nameof(this.sourceBlock));

    }
}
