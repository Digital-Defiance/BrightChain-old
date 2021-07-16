using System.Collections.Generic;
using BrightChain.Engine.Enumerations;

namespace BrightChain.Engine.Models.Blocks
{
    /// <summary>
    /// Map of the block size enumeration values to their actual sizes.
    /// </summary>
    public static class BlockSizeMap
    {
        public static readonly Dictionary<BlockSize, int> Map = new Dictionary<BlockSize, int> {
            { global::BrightChain.Engine.Enumerations.BlockSize.Message,   512 },          // 512B
            { global::BrightChain.Engine.Enumerations.BlockSize.Tiny,      1024 },         // 1K
            { global::BrightChain.Engine.Enumerations.BlockSize.Small,     4*1024 },       // 4K
            { global::BrightChain.Engine.Enumerations.BlockSize.Medium,    1024*1024 },    // 1M
            { global::BrightChain.Engine.Enumerations.BlockSize.Large,     4*1024*1024 },  // 4M
        };

        /// <summary>
        /// Map a block size enumeration to its actual size in bytes
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
        /// Map a block size in bytes back to its block size enumeration
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
