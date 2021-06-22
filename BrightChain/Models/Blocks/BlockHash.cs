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

        public BlockHash(Block block)
        {
            this.BlockSize = block.BlockSize;
            using (SHA256 mySHA256 = SHA256.Create())
                this.HashBytes = mySHA256.ComputeHash(block.Data.ToArray());
        }

        public BlockHash(BlockSize blockSize, ReadOnlyMemory<byte> hashBytes)
        {
            this.BlockSize = blockSize;
            this.HashBytes = hashBytes;
        }

        public string ToString(string format, IFormatProvider _) =>
            BitConverter.ToString(this.HashBytes.ToArray());
    }
}