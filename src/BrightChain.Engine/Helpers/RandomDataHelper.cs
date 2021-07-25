using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;

namespace BrightChain.Engine.Helpers
{
    public static class RandomDataHelper
    {
        public static byte[] RandomBytes(int length)
        {
            using (var rng = RandomNumberGenerator.Create()) // TODO: guarantee is CSPRNG
            {
                var rnd = new byte[length];
                rng.GetBytes(rnd);
                return rnd;
            }
        }

        public static ReadOnlyMemory<byte> RandomReadOnlyBytes(int length) =>
            new ReadOnlyMemory<byte>(RandomBytes(length: length).ToArray());

        public static ReadOnlyMemory<byte> DataFiller(ReadOnlyMemory<byte> inputData, BlockSize blockSize)
        {
            var iBlockSize = BlockSizeMap.BlockSize(blockSize);

            if (inputData.Length > iBlockSize)
            {
                throw new BrightChainException("data length too long");
            }
            else if (inputData.Length == iBlockSize)
            {
                return inputData;
            }

            var bytes = new List<byte>(inputData.ToArray());
            bytes.AddRange(RandomBytes(iBlockSize - inputData.Length));

            if (bytes.Count != iBlockSize)
            {
                throw new BrightChainException("math error");
            }

            return new ReadOnlyMemory<byte>(bytes.ToArray());
        }

        public static FileInfo CreateRandomFile(string filePath, long totalBytes, out byte[] randomFileHash)
        {
            const int writeBufferSize = 1024 * 8;

            var bytesWritten = 0;
            var bytesRemaining = totalBytes;
            using (SHA256 sha = SHA256.Create())
            {
                using (FileStream fileStream = File.OpenWrite(filePath))
                {
                    while (bytesWritten < totalBytes)
                    {
                        var finalBlock = bytesRemaining <= writeBufferSize;
                        var lengthToWrite = (int)(finalBlock ? bytesRemaining : writeBufferSize);
                        var data = RandomDataHelper.RandomBytes(lengthToWrite);
                        if (lengthToWrite != data.Length)
                        {
                            throw new BrightChainException(nameof(data.Length));
                        }

                        fileStream.Write(data, 0, data.Length);
                        bytesWritten += data.Length;
                        bytesRemaining -= data.Length;
                        if (finalBlock)
                        {
                            if (bytesRemaining > 0)
                            {
                                throw new BrightChainException(nameof(bytesRemaining));
                            }

                            sha.TransformFinalBlock(data, 0, data.Length);
                            randomFileHash = sha.Hash;
                            fileStream.Flush();
                            fileStream.Close();
                            FileInfo fileInfo = new FileInfo(filePath);
                            if ((totalBytes != bytesWritten) ||
                                (bytesWritten != fileInfo.Length))
                            {
                                throw new BrightChainException(nameof(bytesWritten));
                            }

                            return fileInfo;
                        }
                        else
                        {
                            sha.TransformBlock(data, 0, lengthToWrite, null, 0);
                        }
                    }
                }
            }

            randomFileHash = null;
            return null;
        }

        public static SourceFileInfo GenerateRandomFile(BlockSize blockSize, Func<BlockSize, long> lengthFunc)
        {
            var fileName = Path.GetTempFileName();
            byte[] sourceFileHash;
            var requestedLength = lengthFunc(blockSize);
            var fileInfo = CreateRandomFile(fileName, requestedLength, out sourceFileHash);
            var sourceInfo = new SourceFileInfo(fileInfo: fileInfo, blockSize: blockSize);
            if (Utilities.HashToFormattedString(sourceFileHash) != Utilities.HashToFormattedString(sourceInfo.SourceId.HashBytes.ToArray()))
            {
                throw new BrightChainException(nameof(sourceFileHash));
            }

            return sourceInfo;
        }
    }
}
