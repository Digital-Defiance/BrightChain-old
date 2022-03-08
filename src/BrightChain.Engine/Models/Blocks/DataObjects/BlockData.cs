using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FASTER.core;
using SimpleBase;

namespace BrightChain.Engine.Models.Blocks.DataObjects;

public abstract class BlockData : IComparable<BlockData>, IEquatable<BlockData>, IEqualityComparer<BlockData>,
    IFasterEqualityComparer<BlockData>
{
    public virtual ReadOnlyMemory<byte> Bytes => throw new NotImplementedException();

    public IEnumerable<byte> SHA256 =>
        System.Security.Cryptography.SHA256.Create().ComputeHash(buffer: this.Bytes.ToArray());

    public uint Crc32 =>
        NeuralFabric.Helpers.Crc32.ComputeChecksum(bytes: this.Bytes.ToArray());

    public ulong Crc64 =>
        NeuralFabric.Helpers.Crc64.ComputeChecksum(bytes: this.Bytes.ToArray());

    public string Base64SHA256 =>
        Base58.Bitcoin.Encode(bytes: new ReadOnlySpan<byte>(array: (byte[])this.SHA256));

    public string Base58Crc64 =>
        Base58.Bitcoin.Encode(bytes: BitConverter.GetBytes(value: this.Crc64));

    public string Base58Data =>
        Base58.Bitcoin.Encode(bytes: this.Bytes.ToArray());

    public int CompareTo(BlockData other)
    {
        return NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(ar1: this.Bytes,
            ar2: other.Bytes);
    }

    public bool Equals(BlockData x, BlockData y)
    {
        return NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(ar1: x.Bytes,
            ar2: y.Bytes) == 0;
    }

    public int GetHashCode([DisallowNull] BlockData obj)
    {
        return (int)this.Crc32;
    }

    public bool Equals(BlockData other)
    {
        return NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(ar1: this.Bytes,
            ar2: other.Bytes) == 0;
    }

    public long GetHashCode64(ref BlockData k)
    {
        return (long)this.Crc64;
    }

    public bool Equals(ref BlockData k1, ref BlockData k2)
    {
        return NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(ar1: k1.Bytes,
            ar2: k2.Bytes) == 0;
    }

    public static bool operator ==(BlockData a, BlockData b)
    {
        return NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(ar1: a.Bytes,
            ar2: b.Bytes) == 0;
    }

    public static bool operator !=(BlockData a, BlockData b)
    {
        return NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(ar1: a.Bytes,
            ar2: b.Bytes) != 0;
    }
}
