using BrightChain.Enumerations;
using BrightChain.Interfaces;
using System;
using System.Security.Cryptography;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Type box for the sha hashes
    /// </summary>
    public class BlockHash : IBlockHash
    {
        public ReadOnlyMemory<byte> HashBytes { get; protected set; }
        public BlockSize BlockSize { get; }

        public bool Computed { get; }

        public BlockHash(Block block)
        {
            this.BlockSize = block.BlockSize;
            using (SHA256 mySHA256 = SHA256.Create())
                this.HashBytes = mySHA256.ComputeHash(block.Data.ToArray());
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
                this.HashBytes = mySHA256.ComputeHash(dataBytes.ToArray());
            this.Computed = true;
        }

        public string ToString(string format, IFormatProvider _) =>
            BitConverter.ToString(this.HashBytes.ToArray());

        public static bool operator ==(BlockHash a, BlockHash b) =>
            a.HashBytes.Equals(b.HashBytes);

        public static bool operator !=(BlockHash a, BlockHash b) =>
            !a.HashBytes.Equals(b.HashBytes);

    }
}