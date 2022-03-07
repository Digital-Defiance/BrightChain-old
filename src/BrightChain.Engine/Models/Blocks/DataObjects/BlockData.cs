namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using FASTER.core;

    public abstract class BlockData : IComparable<BlockData>, IEquatable<BlockData>, IEqualityComparer<BlockData>, IFasterEqualityComparer<BlockData>
    {
        public virtual ReadOnlyMemory<byte> Bytes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public BlockData()
        {
        }

        public IEnumerable<byte> SHA256 =>
            System.Security.Cryptography.SHA256.Create().ComputeHash(this.Bytes.ToArray());

        public uint Crc32 =>
            NeuralFabric.Helpers.Crc32.ComputeChecksum(this.Bytes.ToArray());

        public ulong Crc64 =>
            NeuralFabric.Helpers.Crc64Iso.ComputeChecksum(this.Bytes.ToArray());

        public string Base64SHA256 =>
            SimpleBase.Base58.Bitcoin.Encode(new ReadOnlySpan<byte>((byte[])this.SHA256));

        public static bool operator ==(BlockData a, BlockData b)
        {
            return NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(a.Bytes, b.Bytes) == 0;
        }

        public string Base58Crc64 =>
            SimpleBase.Base58.Bitcoin.Encode(BitConverter.GetBytes(this.Crc64));

        public static bool operator !=(BlockData a, BlockData b)
        {
            return NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(a.Bytes, b.Bytes) != 0;
        }

        public string Base58Data =>
            SimpleBase.Base58.Bitcoin.Encode(this.Bytes.ToArray());

        public int CompareTo(BlockData other)
        {
            return NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(this.Bytes, other.Bytes);
        }

        public bool Equals(BlockData other)
        {
            return NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(this.Bytes, other.Bytes) == 0;
        }

        public bool Equals(BlockData x, BlockData y)
        {
            return NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(x.Bytes, y.Bytes) == 0;
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
            return NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(k1.Bytes, k2.Bytes) == 0;
        }
    }
}
