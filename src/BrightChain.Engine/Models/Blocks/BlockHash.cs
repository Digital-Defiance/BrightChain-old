using System;
using System.Security.Cryptography;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Helpers;
using BrightChain.Engine.Interfaces;

namespace BrightChain.Engine.Models.Blocks
{
    /// <summary>
    /// Type box for the sha hashes
    /// </summary>
    public class BlockHash : IBlockHash, IComparable<BlockHash>
    {
        public const int HashSize = 64;

        public ReadOnlyMemory<byte> HashBytes { get; protected set; }
        public BlockSize BlockSize { get; }

        public bool Computed { get; }

        public BlockHash(IBlock block)
        {
            BlockSize = BlockSizeMap.BlockSize(block.Data.Length);
            using (SHA256 mySHA256 = SHA256.Create())
            {
                HashBytes = mySHA256.ComputeHash(block.Data.ToArray());
            }
            Computed = true;
        }

        public BlockHash(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes)
        {
            BlockSize = originalBlockSize;
            HashBytes = providedHashBytes;
            Computed = false;
        }

        internal BlockHash(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes, bool computed = false)
        {
            BlockSize = originalBlockSize;
            HashBytes = providedHashBytes;
            Computed = computed;
        }

        public BlockHash(ReadOnlyMemory<byte> dataBytes)
        {
            BlockSize = BlockSizeMap.BlockSize(dataBytes.Length);
            using (SHA256 mySHA256 = SHA256.Create())
            {
                HashBytes = mySHA256.ComputeHash(dataBytes.ToArray());
            }

            Computed = true;
        }

        public string ToString(string format, IFormatProvider _)
        {
            return HashBytes.ToString().Replace("-", "").ToLower(culture: System.Globalization.CultureInfo.InvariantCulture);
        }

        public new string ToString()
        {
            return BitConverter.ToString(HashBytes.ToArray()).Replace("-", "").ToLower(culture: System.Globalization.CultureInfo.InvariantCulture);
        }

        public static bool operator ==(BlockHash a, BlockHash b)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(a.HashBytes, b.HashBytes) == 0;
        }

        public static bool operator ==(ReadOnlyMemory<byte> b, BlockHash a)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(a.HashBytes, b) == 0;
        }

        public static bool operator !=(ReadOnlyMemory<byte> b, BlockHash a)
        {
            return !(b == a);
        }

        public static bool operator !=(BlockHash a, BlockHash b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockHash ? ReadOnlyMemoryComparer<byte>.Compare(HashBytes, (obj as BlockHash).HashBytes) == 0 : false;
        }

        public override int GetHashCode()
        {
            return HashBytes.GetHashCode();
        }

        public int CompareTo(BlockHash other)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(HashBytes, other.HashBytes);
        }
    }
}
