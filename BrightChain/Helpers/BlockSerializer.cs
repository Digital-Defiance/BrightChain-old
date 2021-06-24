using BrightChain.Enumerations;
using BrightChain.Extensions;
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
            byte[] lengthBytes = new byte[sizeof(int)];
            stream.Read(
                buffer: lengthBytes,
                offset: 0,
                count: lengthBytes.Length);
            int blockLength = BitConverter.ToInt32(lengthBytes, 0);
            BlockSize blockSize = BlockSizeMap.BlockSize(blockLength);
            byte[] blockData = new byte[blockLength];
            stream.Read(
                buffer: blockData,
                offset: 0,
                count: blockLength);
            return new RestoredBlock(
                requestTime: DateTime.Now,
                keepUntilAtLeast: DateTime.MaxValue,
                redundancy: RedundancyContractType.HeapAuto,
                data: blockData) as T;
        }

        public void WriteTo(T value, Stream stream)
        {
            // part 1: data
            byte[] buffer = BitConverter.GetBytes(value.Data.Length);
            stream.Write(
                buffer: buffer,
                offset: 0,
                count: buffer.Length);
            stream.Write(
                buffer: value.Data.ToArray(),
                offset: 0,
                count: value.Data.Length);
            // part 2: metadata
            ReadOnlyMemory<byte> metaData = value.MetaDataBytes();
            buffer = BitConverter.GetBytes(metaData.Length);
            stream.Write(
                buffer: buffer,
                offset: 0,
                count: buffer.Length);
            stream.Write(
                buffer: metaData.ToArray(),
                offset: 0,
                count: metaData.Length);
        }
    }
}
