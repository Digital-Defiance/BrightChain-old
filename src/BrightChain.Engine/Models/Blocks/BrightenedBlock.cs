namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Extensions;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Models.Blocks.DataObjects;

    /// <summary>
    /// User data that must be whitened with the block whitener before being persisted. These blocks must never be stored directly.
    /// *** CBLs are considered user data ***
    /// </summary>
    public class BrightenedBlock : SourceBlock, IComparable<BrightenedBlock>, IComparable<Block>
    {
        public BrightenedBlock(BlockParams blockParams, ReadOnlyMemory<byte> data, IEnumerable<BlockHash> constituentBlocks)
            : base(
                blockParams: blockParams,
                data: data)
        {
            this.ConstituentBlocks = constituentBlocks;
            this.OriginalType = typeof(BrightenedBlock).FullName;
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
            return other.Data.Length == this.Data.Length ? ReadOnlyMemoryComparer<byte>.Compare(this.Data, other.Data) : (other.Data.Length > this.Data.Length ? -1 : 1);
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
