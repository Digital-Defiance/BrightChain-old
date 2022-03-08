using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Helpers;

public static class Utilities
{
    public static ReadOnlyMemory<byte> ReadOnlyMemoryXOR(ReadOnlyMemory<byte> sourceA, ReadOnlyMemory<byte> sourceB)
    {
        if (sourceA.Length != sourceB.Length)
        {
            throw new Exception(message: nameof(sourceB.Length));
        }

        var aArray = sourceA.ToArray();
        var bArray = sourceB.ToArray();
        var cArray = new byte[aArray.Length];
        for (var i = 0; i < aArray.Length; i++)
        {
            cArray[i] = (byte)(aArray[i] ^ bArray[i]);
        }

        return new ReadOnlyMemory<byte>(array: cArray);
    }

    /// <summary>
    ///     Generate a hash of an empty array to determine the block hash byte length
    ///     Used during testing.
    /// </summary>
    /// <param name="blockSize">Block size to generate zero vector for.</param>
    /// <param name="blockHash">Hash of the zero vector for the block.</param>
    public static void GenerateZeroVectorAndVerify(BlockSize blockSize, out BlockHash blockHash)
    {
        var blockBytes = new byte[BlockSizeMap.BlockSize(blockSize: blockSize)];
        Array.Fill<byte>(array: blockBytes,
            value: 0);
        blockHash = new BlockHash(blockType: typeof(Block),
            dataBytes: blockBytes);
        if (blockHash.HashBytes.Length != BlockHash.HashSize / 8)
        {
            throw new BrightChainException(message: "BlockHash size mismatch.");
        }
    }
}
