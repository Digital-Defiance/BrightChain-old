using BrightChain.CSharpTest.Net.Serialization;
using BrightChain.Models.Blocks;
using System;
using System.IO;
namespace BrightChain.Helpers
{
    public class BlockSerializer : ISerializer<Block>
    {
        public Block ReadFrom(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void WriteTo(Block value, Stream stream)
        {
            byte[] buffer = BitConverter.GetBytes(value.Data.Length);
            stream.Write(
                buffer: buffer,
                offset: 0,
                count: buffer.Length);
            stream.Write(
                buffer: value.Data.ToArray(),
                offset: 0,
                count: value.Data.Length);

        }
    }
}
