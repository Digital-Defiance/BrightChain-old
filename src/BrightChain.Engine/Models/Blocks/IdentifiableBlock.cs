using System;
using BrightChain.Engine.Extensions;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks.DataObjects;

namespace BrightChain.Engine.Models.Blocks;

/// <summary>
///     User data that must be whitened with the block whitener before being persisted. These blocks must never be stored directly.
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
        return this.StoredData.CompareTo(other: other.StoredData);
    }

    public override void Dispose()
    {
    }

    public new bool Validate()
    {
        return this.PerformValidation(validationExceptions: out _);
    }
}
