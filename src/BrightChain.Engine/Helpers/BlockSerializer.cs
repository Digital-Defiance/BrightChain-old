using System;
using System.IO;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Extensions;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;

namespace BrightChain.Engine.Helpers
{
    /// <summary>
    /// Serializer class to help BTree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BlockSerializer : IBrightChainSerializer<IBlock>
    {
        public IBlock ReadFrom(Stream stream)
        {
            // part 1a read length
            byte[] lengthBytes = new byte[sizeof(int)];
            stream.Read(
                buffer: lengthBytes,
                offset: 0,
                count: lengthBytes.Length);
            int blockLength = BitConverter.ToInt32(lengthBytes, 0);
            BlockSize blockSize = BlockSizeMap.BlockSize(blockLength);
            // part 1b read data
            byte[] blockData = new byte[blockLength];
            stream.Read(
                buffer: blockData,
                offset: 0,
                count: blockLength);

            // part 2a read metadata length
            lengthBytes = new byte[sizeof(int)];
            stream.Read(
                buffer: lengthBytes,
                offset: 0,
                count: lengthBytes.Length);
            int metaDataLength = BitConverter.ToInt32(lengthBytes, 0);
            // part 2b read metadata
            byte[] metaData = new byte[metaDataLength];
            stream.Read(
                buffer: metaData,
                offset: 0,
                count: metaDataLength);

            // make block
            var restoredBlock = new RestoredBlock(
                new BlockParams(
                    blockSize: BlockSizeMap.BlockSize(blockData.Length),
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: RedundancyContractType.HeapAuto,
                    privateEncrypted: false),
                data: blockData);

            // fill in metadata
            restoredBlock.TryRestoreMetadataFromBytes(metaData);

            return restoredBlock;
        }

        public void WriteTo(IBlock value, Stream stream)
        {
            var block = value as Block;
            if (block is null)
            {
                throw new BrightChainException(nameof(value));
            }

            // part 1: data
            byte[] buffer = BitConverter.GetBytes(block.Data.Length);
            stream.Write(
                buffer: buffer,
                offset: 0,
                count: buffer.Length);
            stream.Write(
                buffer: block.Data.ToArray(),
                offset: 0,
                count: block.Data.Length);
            // part 2: metadata
            ReadOnlyMemory<byte> metaData = block.MetadataBytes();
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
