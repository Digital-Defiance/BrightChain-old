using System;
using System.Collections.Generic;
using BrightChain.Engine.Enumerations;

namespace BrightChain.Engine.Models.Blocks
{
    /// <summary>
    /// Map of the block size enumeration values to their actual sizes.
    /// </summary>
    public static class BlockSizeMap
    {
        public const int MessageSize = 512;           // 512B
        public const int TinySize = 1024;             // 1K
        public const int SmallSize = 4 * 1024;        // 4K
        public const int MediumSize = 1024 * 1024;    // 1M
        public const int LargeSize = 4 * 1024 * 1024; // 4M

        public static readonly Dictionary<BlockSize, int> Map = new Dictionary<BlockSize, int>
        {
            { Enumerations.BlockSize.Unknown,   -1 },
            { Enumerations.BlockSize.Message,   MessageSize },
            { Enumerations.BlockSize.Tiny,      TinySize },
            { Enumerations.BlockSize.Small,     SmallSize },
            { Enumerations.BlockSize.Medium,    MediumSize },
            { Enumerations.BlockSize.Large,     LargeSize },
        };

        public static readonly Dictionary<BlockSize, int> HashesPerBlockMap = new Dictionary<BlockSize, int>
        {
            { Enumerations.BlockSize.Unknown,   -1 },
            { Enumerations.BlockSize.Message,   (int)(BlockHash.HashSizeBytes / MessageSize) },
            { Enumerations.BlockSize.Tiny,      (int)(BlockHash.HashSizeBytes / TinySize) },
            { Enumerations.BlockSize.Small,     (int)(BlockHash.HashSizeBytes / SmallSize) },
            { Enumerations.BlockSize.Medium,    (int)(BlockHash.HashSizeBytes / MediumSize) },
            { Enumerations.BlockSize.Large,     (int)(BlockHash.HashSizeBytes / LargeSize) },
        };

        /// <summary>
        /// Map a block size enumeration to its actual size in bytes.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static int BlockSize(BlockSize blockSize)
        {
            if (!BlockSizeMap.Map.ContainsKey(blockSize))
            {
                throw new KeyNotFoundException(message: nameof(blockSize));
            }

            return BlockSizeMap.Map[blockSize];
        }

        /// <summary>
        /// Map a block size enumeration to the number of hashes it can contain.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static int HashesPerSegment(BlockSize blockSize)
        {
            if (!BlockSizeMap.HashesPerBlockMap.ContainsKey(blockSize))
            {
                throw new KeyNotFoundException(message: nameof(blockSize));
            }

            return BlockSizeMap.HashesPerBlockMap[blockSize];
        }

        /// <summary>
        /// Map a block size in bytes back to its block size enumeration.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static Enumerations.BlockSize BlockSize(int blockSize)
        {
            foreach (KeyValuePair<BlockSize, int> pair in BlockSizeMap.Map)
            {
                if (pair.Value == blockSize)
                {
                    return pair.Key;
                }
            }

            throw new KeyNotFoundException(message: nameof(blockSize));
        }
    }
}
