using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.DataObjects;

namespace BrightChain.Engine.Helpers;

public static class RandomDataHelper
{
    public static byte[] RandomBytes(int length)
    {
        using (var rng = RandomNumberGenerator.Create()) // TODO: guarantee is CSPRNG
        {
            var rnd = new byte[length];
            rng.GetBytes(data: rnd);
            return rnd;
        }
    }

    public static ReadOnlyMemory<byte> RandomReadOnlyBytes(int length)
    {
        return new ReadOnlyMemory<byte>(array: RandomBytes(length: length).ToArray());
    }

    public static ReadOnlyMemory<byte> DataFiller(ReadOnlyMemory<byte> inputData, BlockSize blockSize)
    {
        var iBlockSize = BlockSizeMap.BlockSize(blockSize: blockSize);

        if (inputData.Length > iBlockSize)
        {
            throw new BrightChainException(message: "data length too long");
        }

        if (inputData.Length == iBlockSize)
        {
            return inputData;
        }

        var bytes = new List<byte>(collection: inputData.ToArray());
        bytes.AddRange(collection: RandomBytes(length: iBlockSize - inputData.Length));

        if (bytes.Count != iBlockSize)
        {
            throw new BrightChainException(message: "math error");
        }

        return new ReadOnlyMemory<byte>(array: bytes.ToArray());
    }

    public static FileInfo CreateRandomFile(string filePath, long totalBytes, out byte[] randomFileHash)
    {
        const int writeBufferSize = 1024 * 8;

        var bytesWritten = 0;
        var bytesRemaining = totalBytes;
        using (var sha = SHA256.Create())
        {
            using (var fileStream = File.OpenWrite(path: filePath))
            {
                while (bytesWritten < totalBytes)
                {
                    var finalBlock = bytesRemaining <= writeBufferSize;
                    var lengthToWrite = (int)(finalBlock ? bytesRemaining : writeBufferSize);
                    var data = RandomBytes(length: lengthToWrite);
                    if (lengthToWrite != data.Length)
                    {
                        throw new BrightChainException(message: nameof(data.Length));
                    }

                    fileStream.Write(buffer: data,
                        offset: 0,
                        count: data.Length);
                    bytesWritten += data.Length;
                    bytesRemaining -= data.Length;
                    if (finalBlock)
                    {
                        if (bytesRemaining > 0)
                        {
                            throw new BrightChainException(message: nameof(bytesRemaining));
                        }

                        sha.TransformFinalBlock(inputBuffer: data,
                            inputOffset: 0,
                            inputCount: data.Length);
                        randomFileHash = sha.Hash;
                        fileStream.Flush();
                        fileStream.Close();
                        var fileInfo = new FileInfo(fileName: filePath);
                        if (totalBytes != bytesWritten ||
                            bytesWritten != fileInfo.Length)
                        {
                            throw new BrightChainException(message: nameof(bytesWritten));
                        }

                        return fileInfo;
                    }

                    sha.TransformBlock(inputBuffer: data,
                        inputOffset: 0,
                        inputCount: lengthToWrite,
                        outputBuffer: null,
                        outputOffset: 0);
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
        var requestedLength = lengthFunc(arg: blockSize);
        var fileInfo = CreateRandomFile(filePath: fileName,
            totalBytes: requestedLength,
            randomFileHash: out sourceFileHash);
        var sourceInfo = new SourceFileInfo(fileInfo: fileInfo,
            blockSize: blockSize);
        if (NeuralFabric.Helpers.Utilities.HashToFormattedString(hashBytes: sourceFileHash) !=
            NeuralFabric.Helpers.Utilities.HashToFormattedString(hashBytes: sourceInfo.SourceId.HashBytes.ToArray()))
        {
            throw new BrightChainException(message: nameof(sourceFileHash));
        }

        return sourceInfo;
    }
}
