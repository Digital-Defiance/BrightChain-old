using BrightChain.EntityFrameworkCore.Interfaces;
using BrightChain.Exceptions;
using BrightChain.Models.Blocks;
using System;
using System.IO;

namespace BrightChain.Helpers
{
    /// <summary>
    /// Serializer class to help BTree
    /// </summary>
    public class BlockHashSerializer : IBrightChainSerializer<BlockHash>
    {

        /// <summary>
        /// Generate a hash of an empty array to determine the block hash byte length
        /// Used during testing
        /// </summary>
        internal static void verifyHashLength(out BlockHash tmpHash)
        {
            var messageBytes = new byte[BlockSizeMap.BlockSize(Enumerations.BlockSize.Message)];
            Array.Fill<byte>(messageBytes, 0);
            tmpHash = new BlockHash(messageBytes);
            if (tmpHash.HashBytes.Length != BlockHash.HashSize)
            {
                throw new BrightChainException("hash size mismatch");
            }
        }

        public BlockHash ReadFrom(Stream stream)
        {
            var hashBytes = new byte[BlockHash.HashSize];
            stream.Read(
                buffer: hashBytes,
                offset: 0,
                count: hashBytes.Length);
            return new BlockHash(
                originalBlockSize: Enumerations.BlockSize.Unknown,
                providedHashBytes: hashBytes);
        }

        public void WriteTo(BlockHash value, Stream stream)
        {
            if (value.HashBytes.Length == 0)
            {
                throw new BrightChainException("HashBytes is empty");
            }

            stream.Write(
                buffer: value.HashBytes.ToArray(),
                offset: 0,
                count: value.HashBytes.Length);
        }
    }
}
