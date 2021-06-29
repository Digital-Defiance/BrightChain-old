using BrightChain.Exceptions;
using BrightChain.Interfaces;
using BrightChain.Models.Blocks;
using System;
using System.IO;

namespace BrightChain.Helpers
{
    /// <summary>
    /// Serializer class to help BTree
    /// </summary>
    public class BlockHashSerializer : ISerializer<BlockHash>
    {

        /// <summary>
        /// Generate a hash of an empty array to determine the block hash byte length
        /// </summary>
        public static int getHashLength(out BlockHash tmpHash)
        {
            var messageBytes = new byte[BlockSizeMap.BlockSize(Enumerations.BlockSize.Message)];
            Array.Fill<byte>(messageBytes, 0);
            tmpHash = new BlockHash(messageBytes);
            return tmpHash.HashBytes.Length;
        }

        public BlockHash ReadFrom(Stream stream)
        {
            BlockHash blockHash;
            var hashBytes = new byte[getHashLength(out blockHash)];
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
