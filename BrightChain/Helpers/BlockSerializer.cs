using BrightChain.Models.Blocks;
using CSharpTest.Net.Serialization;
using System;
using System.IO;
namespace BrightChain.Helpers
{
    /// <summary>
    /// Serializer class to help BTree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BlockSerializer<T> : ISerializer<T> where T : Block
    {
        public T ReadFrom(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void WriteTo(T value, Stream stream)
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
