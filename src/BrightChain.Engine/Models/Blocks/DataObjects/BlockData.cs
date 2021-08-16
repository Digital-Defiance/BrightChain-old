namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using FASTER.core;

    public struct BlockData : IComparable<BlockData>, IEquatable<BlockData>, IEqualityComparer<BlockData>, IFasterEqualityComparer<BlockData>
    {
        public readonly ReadOnlyMemory<byte> Bytes;

        public BlockData(ReadOnlyMemory<byte> data)
        {
            this.Bytes = data;
        }

        public uint Crc32 =>
            Helpers.Crc32.ComputeNewChecksum(this.Bytes.ToArray());

        public ulong Crc64 =>
            DamienG.Security.Cryptography.Crc64Iso.Compute(this.Bytes.ToArray());

        public static bool operator ==(BlockData a, BlockData b)
        {
            return Helpers.ReadOnlyMemoryComparer<byte>.Compare(a.Bytes, b.Bytes) == 0;
        }

        public static bool operator !=(BlockData a, BlockData b)
        {
            return Helpers.ReadOnlyMemoryComparer<byte>.Compare(a.Bytes, b.Bytes) != 0;
        }

        public int CompareTo(BlockData other)
        {
            return Helpers.ReadOnlyMemoryComparer<byte>.Compare(this.Bytes, other.Bytes);
        }

        public bool Equals(BlockData other)
        {
            return Helpers.ReadOnlyMemoryComparer<byte>.Compare(this.Bytes, other.Bytes) == 0;
        }

        public bool Equals(BlockData x, BlockData y)
        {
            return Helpers.ReadOnlyMemoryComparer<byte>.Compare(x.Bytes, y.Bytes) == 0;
        }

        public int GetHashCode([DisallowNull] BlockData obj)
        {
            return (int)this.Crc32;
        }

        public long GetHashCode64(ref BlockData k)
        {
            return (long)this.Crc64;
        }

        public bool Equals(ref BlockData k1, ref BlockData k2)
        {
            return Helpers.ReadOnlyMemoryComparer<byte>.Compare(k1.Bytes, k2.Bytes) == 0;
        }
    }
}
