﻿namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using global::BrightChain.Engine.Extensions;
    using global::BrightChain.Engine.Interfaces;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;

    /// <summary>
    /// User data that must be whitened with the block whitener before being persisted. These blocks must never be stored directly.
    /// </summary>
    public class IdentifiableBlock
        : Block, IComparable<IdentifiableBlock>, IComparable<Block>, IComparable<IBlock>
    {
        public IdentifiableBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
            : base(
                blockParams: blockParams,
                data: data)
        {
        }

        public int CompareTo(IdentifiableBlock other)
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
