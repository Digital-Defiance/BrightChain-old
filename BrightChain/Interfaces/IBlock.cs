using BrightChain.Attributes;
using BrightChain.Models.Blocks;
using BrightChain.Models.Contracts;
using System;

namespace BrightChain.Interfaces
{
    public interface IBlock : IDisposable
    {
        BlockHash Id { get; }
        Block XOR(Block other);
        Block XOR(Block[] others);
        StorageDurationContract DurationContract { get; }
        RedundancyContract RedundancyContract { get; }
        [BrightChainDataIgnore]
        ReadOnlyMemory<byte> MetaData { get; }
        ReadOnlyMemory<byte> Data { get; }
        bool Committed { get; }
        bool AllowCommit { get; }
    }
}