namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Extensions;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using ProtoBuf;

    /// <summary>
    /// A brightened block has been XOR'd with random data and has a more random frequency.
    /// </summary>
    [ProtoContract]
    public class BrightenedBlock : TransactableBlock, IComparable<BrightenedBlock>, IComparable<Block>
    {
        public BrightenedBlock(TransactableBlockParams blockParams, ReadOnlyMemory<byte> data, IEnumerable<BlockHash> constituentBlockHashes)
            : base(
                blockParams: blockParams,
                data: data)
        {
            this.ConstituentBlocks = constituentBlockHashes;
            this.OriginalType = typeof(BrightenedBlock).AssemblyQualifiedName;
        }

        public int CompareTo(BrightenedBlock other)
        {
            return this.StoredData.CompareTo(other.StoredData);
        }

        public override void Dispose()
        {
        }

        public new bool Validate()
        {
            return this.PerformValidation(out _);
        }
    }
}
