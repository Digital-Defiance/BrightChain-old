using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Interfaces;
using NeuralFabric.Models.Hashes;
using ProtoBuf;

namespace BrightChain.Engine.Models.Blocks;

/// <summary>
///     Type box for the sha hashes.
/// </summary>
[ProtoContract]
public class BlockSignature : DataSignature, IDataSignature, IComparable<BlockSignature>
{
    public BlockSignature(IBlock block)
        : base(dataBytes: block.StoredData.Bytes)
    {
    }

    public BlockSignature(ReadOnlyMemory<byte> dataBytes)
        : base(dataBytes: dataBytes)
    {
    }

    public BlockSignature(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes)
        : base(providedHashBytes: providedHashBytes,
            computed: false)
    {
        if (providedHashBytes.Length != BlockSizeMap.BlockSize(blockSize: originalBlockSize))
        {
            throw new BrightChainException(message: "hash size mismatch");
        }
    }

    internal BlockSignature(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes, bool computed = false)
        : base(providedHashBytes: providedHashBytes,
            computed: computed)
    {
        if (providedHashBytes.Length != BlockSizeMap.BlockSize(blockSize: originalBlockSize))
        {
            throw new BrightChainException(message: "hash size mismatch");
        }
    }

    public int CompareTo(BlockSignature other)
    {
        throw new NotImplementedException();
    }
}
