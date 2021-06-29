using BrightChain.Enumerations;
using BrightChain.Helpers;
using BrightChain.Interfaces;
using System;
using System.Security.Cryptography;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Type box for the sha hashes
    /// </summary>
    public class BlockHash : IBlockHash, IComparable<BlockHash>
    {
        public ReadOnlyMemory<byte> HashBytes { get; protected set; }
        public BlockSize BlockSize { get; }

        public bool Computed { get; }

        public BlockHash(IBlock block)
        {
            this.BlockSize = BlockSizeMap.BlockSize(block.Data.Length);
            using (SHA256 mySHA256 = SHA256.Create())
            {
                this.HashBytes = mySHA256.ComputeHash(block.Data.ToArray());
            }
            this.Computed = true;
        }

        public BlockHash(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes)
        {
            this.BlockSize = originalBlockSize;
            this.HashBytes = providedHashBytes;
            this.Computed = false;
        }

        internal BlockHash(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes, bool computed = false)
        {
            this.BlockSize = originalBlockSize;
            this.HashBytes = providedHashBytes;
            this.Computed = computed;
        }

        public BlockHash(ReadOnlyMemory<byte> dataBytes)
        {
            this.BlockSize = BlockSizeMap.BlockSize(dataBytes.Length);
            using (SHA256 mySHA256 = SHA256.Create())
            {
                this.HashBytes = mySHA256.ComputeHash(dataBytes.ToArray());
            }

            this.Computed = true;
        }

        public string ToString(string format, IFormatProvider _) => this.ToString();

        public new string ToString() => BitConverter.ToString(this.HashBytes.ToArray());

        public static bool operator ==(BlockHash a, BlockHash b) => ReadOnlyMemoryComparer<byte>.Compare(a.HashBytes, b.HashBytes) == 0;

        public static bool operator !=(BlockHash a, BlockHash b) => !a.Equals(b);

        public override bool Equals(object obj) => ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, (obj as BlockHash).HashBytes) == 0;

        public override int GetHashCode() => this.HashBytes.GetHashCode();

        public int CompareTo(BlockHash other) => ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes);
    }
}