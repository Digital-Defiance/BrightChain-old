using CSharpTest.Net.Serialization;
using BrightChain.Models.Blocks;
using System;
using System.IO;

namespace BrightChain.Helpers
{
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
