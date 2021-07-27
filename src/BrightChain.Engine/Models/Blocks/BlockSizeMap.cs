using System;
using System.Collections.Generic;
using BrightChain.Engine.Enumerations;

namespace BrightChain.Engine.Models.Blocks
{
    /// <summary>
    ///     Map of the block size enumeration values to their actual sizes.
    /// </summary>
    public static class BlockSizeMap
    {
        public const int MessageSize = 512; // 512B
        public const int TinySize = 1024; // 1K
        public const int SmallSize = 4 * 1024; // 4K
        public const int MediumSize = 1024 * 1024; // 1M
        public const int LargeSize = 4 * 1024 * 1024; // 4M

        public static readonly Dictionary<BlockSize, int> Map = new()
        {
            { Enumerations.BlockSize.Unknown, -1 },
            { Enumerations.BlockSize.Message, MessageSize },
            { Enumerations.BlockSize.Tiny, TinySize },
            { Enumerations.BlockSize.Small, SmallSize },
            { Enumerations.BlockSize.Medium, MediumSize },
            { Enumerations.BlockSize.Large, LargeSize }
        };

        public static readonly Dictionary<BlockSize, int> HashesPerBlockMap = new()
        {
            { Enumerations.BlockSize.Unknown, -1 },
            { Enumerations.BlockSize.Message, MessageSize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Tiny, TinySize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Small, SmallSize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Medium, MediumSize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Large, LargeSize / DataHash.HashSizeBytes }
        };

        /// <summary>
        ///     Map a block size enumeration to its actual size in bytes.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static int BlockSize(BlockSize blockSize)
        {
            if (!Map.ContainsKey(blockSize))
            {
                throw new KeyNotFoundException(nameof(blockSize));
            }

            return Map[blockSize];
        }

        public static bool LengthIsValid(int length)
        {
            foreach (var pair in Map)
            {
                if (pair.Value == length)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Map a block size enumeration to the number of hashes it can contain.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static long HashesPerBlock(BlockSize blockSize, int exponent = 1)
        {
            if (!HashesPerBlockMap.ContainsKey(blockSize))
            {
                throw new KeyNotFoundException(nameof(blockSize));
            }

            var value = HashesPerBlockMap[blockSize];
            if (exponent <= 1)
            {
                return value;
            }

            return (long)Math.Pow(value, exponent);
        }

        /// <summary>
        ///     Map a block size in bytes back to its block size enumeration.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static BlockSize BlockSize(int blockSize)
        {
            foreach (var pair in Map)
            {
                if (pair.Value == blockSize)
                {
                    return pair.Key;
                }
            }

            throw new KeyNotFoundException(nameof(blockSize));
        }
    }
}
