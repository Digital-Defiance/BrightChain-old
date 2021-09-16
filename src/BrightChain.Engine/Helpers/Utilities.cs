namespace BrightChain.Engine.Helpers
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Hashes;
    using SimpleBase;

    public static class Utilities
    {
        public static Version GetAssemblyVersionForType(Type assemblyType = null) =>
                System.Reflection.Assembly.GetAssembly(
                    type: assemblyType is null ? typeof(Services.BrightBlockService) : assemblyType).GetName().Version;

        public static async IAsyncEnumerable<byte> ReadOnlyMemoryToAsyncEnumerable(ReadOnlyMemory<byte> source)
        {
            foreach (var b in source.ToArray())
            {
                yield return b;
            }
        }

        public static async IAsyncEnumerable<byte> ParallelReadOnlyMemoryXORToAsyncEnumerable(ReadOnlyMemory<byte> sourceA, ReadOnlyMemory<byte> sourceB)
        {
            if (sourceA.Length != sourceB.Length)
            {
                throw new BrightChainException(nameof(sourceB.Length));
            }

            var aArray = sourceA.ToArray();
            var bArray = sourceB.ToArray();
            for (int i = 0; i < aArray.Length; i++)
            {
                yield return (byte)(aArray[i] ^ bArray[i]);
            }
        }

        public static ReadOnlyMemory<byte> ReadOnlyMemoryXOR(ReadOnlyMemory<byte> sourceA, ReadOnlyMemory<byte> sourceB)
        {
            if (sourceA.Length != sourceB.Length)
            {
                throw new BrightChainException(nameof(sourceB.Length));
            }

            var aArray = sourceA.ToArray();
            var bArray = sourceB.ToArray();
            var cArray = new byte[aArray.Length];
            for (int i = 0; i < aArray.Length; i++)
            {
                cArray[i] = (byte)(aArray[i] ^ bArray[i]);
            }

            return new ReadOnlyMemory<byte>(cArray);
        }

        public static string HashToFormattedString(byte[] hashBytes)
        {
            return BitConverter.ToString(hashBytes)
                .Replace("-", string.Empty)
                .ToLower(culture: System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Generate a hash of an empty array to determine the block hash byte length
        /// Used during testing.
        /// </summary>
        /// <param name="blockSize">Block size to generate zero vector for.</param>
        /// <param name="blockHash">Hash of the zero vector for the block.</param>
        public static void GenerateZeroVectorAndVerify(BlockSize blockSize, out BlockHash blockHash)
        {
            var blockBytes = new byte[BlockSizeMap.BlockSize(blockSize)];
            Array.Fill<byte>(blockBytes, 0);
            blockHash = new BlockHash(blockType: typeof(Block), dataBytes: blockBytes);
            if (blockHash.HashBytes.Length != (BlockHash.HashSize / 8))
            {
                throw new BrightChainException("BlockHash size mismatch.");
            }
        }

        public static string BytesToBase58(ReadOnlyMemory<byte> data)
        {
            return SimpleBase.Base58.Bitcoin.Encode(data.ToArray());
        }
    }
}
