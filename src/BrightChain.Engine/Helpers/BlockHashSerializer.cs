using System;
using System.IO;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks;

namespace BrightChain.Engine.Helpers
{
    /// <summary>
    /// Serializer class to help BTree.
    /// </summary>
    /// <typeparam name="T">Type of the block being serialized.</typeparam>
    public class BlockHashSerializer<T> : IBrightChainSerializer<BlockHash>
    {
        public BlockHash ReadFrom(Stream stream)
        {
            var hashBytes = new byte[BlockHash.HashSize / 8];
            stream.Read(
                buffer: hashBytes,
                offset: 0,
                count: hashBytes.Length);
            return new BlockHash(
                blockType: typeof(T),
                originalBlockSize: Enumerations.BlockSize.Unknown,
                providedHashBytes: hashBytes,
                computed: false);
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
