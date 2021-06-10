using BrightChain.Enumerations;
using BrightChain.Interfaces;
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
        public new readonly IEnumerable<IBlock> ConstituentBlocks;
        private readonly Block sourceBlock;

        public ConstituentBlockListBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy) : base(requestTime: requestTime, keepUntilAtLeast: keepUntilAtLeast, redundancy: redundancy, data: new byte[] { })
        {
            this.sourceBlock = null;
            this.ConstituentBlocks = new Block[] { };
        }

        public ConstituentBlockListBlock() : base(requestTime: DateTime.Now, keepUntilAtLeast: DateTime.MaxValue, redundancy: RedundancyContractType.LocalNone, data: new byte[] { })
        {
            this.sourceBlock = null;
            this.ConstituentBlocks = new Block[] { };
        }

        public ConstituentBlockListBlock(Block sourceBlock) : base(requestTime: sourceBlock.DurationContract.RequestTime, keepUntilAtLeast: sourceBlock.DurationContract.KeepUntilAtLeast, redundancy: sourceBlock.RedundancyContract.RedundancyContractType, data: sourceBlock.Data)
        {
            this.sourceBlock = sourceBlock;
            this.ConstituentBlocks = sourceBlock.ConstituentBlocks;
        }

        public IEnumerable<BlockHash> ConstituentBlockHashes =>
            ConstituentBlocks
                .Select(b => { return b.Id; })
                    .ToArray();

        public new ReadOnlyMemory<byte> Data =>
            new ReadOnlyMemory<byte>(
                ConstituentBlocks
                    .SelectMany(b =>
                        b.Id.HashBytes.ToArray())
                    .ToArray());



        public override void Dispose()
        {
        }

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data) =>
            !(this.sourceBlock is null) ?
                this.sourceBlock.NewBlock(requestTime: requestTime, keepUntilAtLeast: keepUntilAtLeast, redundancy: redundancy, data: data)
            :
                new ConstituentBlockListBlock();

    }
}
