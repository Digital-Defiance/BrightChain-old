namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Enumerations;

    /// <summary>
    ///     Map of the block size enumeration values to their actual sizes.
    /// </summary>
    public static class BlockSizeMap
    {
        /// <summary>
        /// Smallest block size. Best for small payloads like messages. 512 bytes.
        /// </summary>
        public const int MessageSize = 512;

        /// <summary>
        /// Tiny block size. Best for small payloads larger than basic messages. 1024 bytes. 1K.
        /// </summary>
        public const int TinySize = 1024;

        /// <summary>
        /// Small block size. Best for small files. 4024 bytes. 4K.
        /// </summary>
        public const int SmallSize = 4 * 1024;

        /// <summary>
        /// Medium block size. Best for small to moderately sized data. 1,048,576 bytes. 1M.
        /// </summary>
        public const int MediumSize = 1024 * 1024;

        /// <summary>
        /// Large block size. Best for large data. 4,194.304 bytes. 4M.
        /// </summary>
        public const int LargeSize = 4 * 1024 * 1024;

        public static readonly Dictionary<BlockSize, int> Map = new()
        {
            { Enumerations.BlockSize.Unknown, -1 },
            { Enumerations.BlockSize.Message, MessageSize },
            { Enumerations.BlockSize.Tiny, TinySize },
            { Enumerations.BlockSize.Small, SmallSize },
            { Enumerations.BlockSize.Medium, MediumSize },
            { Enumerations.BlockSize.Large, LargeSize },
        };

        public static readonly Dictionary<BlockSize, int> HashesPerBlockMap = new()
        {
            { Enumerations.BlockSize.Unknown, -1 },
            { Enumerations.BlockSize.Message, MessageSize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Tiny, TinySize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Small, SmallSize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Medium, MediumSize / DataHash.HashSizeBytes },
            { Enumerations.BlockSize.Large, LargeSize / DataHash.HashSizeBytes },
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
