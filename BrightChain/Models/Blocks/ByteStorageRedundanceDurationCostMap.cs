using BrightChain.Enumerations;
using System.Collections.Generic;

namespace BrightChain.Models.Blocks
{
    public static class ByteStorageRedundanceDurationCostMap
    {
        public static readonly Dictionary<BlockSize, double> Map = new Dictionary<BlockSize, double> {
            { global::BrightChain.Enumerations.BlockSize.Message,   0 },          // 512B
            { global::BrightChain.Enumerations.BlockSize.Tiny,      0 },         // 1K
            { global::BrightChain.Enumerations.BlockSize.Small,     0},       // 4K
            { global::BrightChain.Enumerations.BlockSize.Medium,    0},    // 1M
            { global::BrightChain.Enumerations.BlockSize.Large,     0 },  // 4M
        };

        public static double BlockSize(BlockSize blockSize)
        {
            if (!ByteStorageRedundanceDurationCostMap.Map.ContainsKey(blockSize))
                throw new KeyNotFoundException(message: nameof(blockSize));

            return ByteStorageRedundanceDurationCostMap.Map[blockSize];
        }

        public static Enumerations.BlockSize BlockSize(double blockSize)
        {
            foreach (KeyValuePair<BlockSize, double> pair in ByteStorageRedundanceDurationCostMap.Map)
            {
                if (pair.Value == blockSize)
                    return pair.Key;
            }

            throw new KeyNotFoundException(message: nameof(blockSize));
        }
    }
}
