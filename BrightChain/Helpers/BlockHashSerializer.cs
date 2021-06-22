using BrightChain.Models.Blocks;
using CSharpTest.Net.Serialization;
using System;
using System.IO;

namespace BrightChain.Helpers
{
    /// <summary>
    /// Serializer class to help BTree
    /// </summary>
    public class BlockHashSerializer : ISerializer<BlockHash>
    {
        public BlockHash ReadFrom(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void WriteTo(BlockHash value, Stream stream)
        {
            stream.Write(
                buffer: value.HashBytes.ToArray(),
                offset: 0,
                count: value.HashBytes.Length);
        }
    }
}
