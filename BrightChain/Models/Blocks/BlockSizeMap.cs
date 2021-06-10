using BrightChain.Enumerations;
using System.Collections.Generic;

namespace BrightChain.Models.Blocks
{
    public static class BlockSizeMap
    {
        public static readonly Dictionary<BlockSize, int> Map = new Dictionary<BlockSize, int> {
            { global::BrightChain.Enumerations.BlockSize.Message,   512 },          // 512B
            { global::BrightChain.Enumerations.BlockSize.Tiny,      1024 },         // 1K
            { global::BrightChain.Enumerations.BlockSize.Small,     4*1024 },       // 4K
            { global::BrightChain.Enumerations.BlockSize.Medium,    1024*1024 },    // 1M
            { global::BrightChain.Enumerations.BlockSize.Large,     4*1024*1024 },  // 4M
        };

        public static int BlockSize(BlockSize blockSize)
        {
            if (!BlockSizeMap.Map.ContainsKey(blockSize))
                throw new KeyNotFoundException(message: nameof(blockSize));

            return BlockSizeMap.Map[blockSize];
        }

        public static Enumerations.BlockSize BlockSize(int blockSize)
        {
            foreach (KeyValuePair<BlockSize, int> pair in BlockSizeMap.Map)
            {
                if (pair.Value == blockSize)
                    return pair.Key;
            }

            throw new KeyNotFoundException(message: nameof(blockSize));
        }
    }
}
