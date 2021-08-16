namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Extensions;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using ProtoBuf;

    /// <summary>
    /// User data that must be whitened with the block whitener before being persisted. These blocks must never be stored directly.
    /// *** CBLs are considered user data ***
    /// </summary>
    [ProtoContract]
    public class BrightenedBlock : SourceBlock, IComparable<BrightenedBlock>, IComparable<Block>
    {
        public BrightenedBlock(BlockParams blockParams, ReadOnlyMemory<byte> data, IEnumerable<BlockHash> constituentBlocks)
            : base(
                blockParams: blockParams,
                data: data)
        {
            this.ConstituentBlocks = constituentBlocks;
            this.OriginalType = typeof(BrightenedBlock).AssemblyQualifiedName;
        }

        public override BrightenedBlock NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            return new BrightenedBlock(
                blockParams: this.BlockParams,
                data: data,
                constituentBlocks: this.ConstituentBlocks);
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
