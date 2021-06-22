using BrightChain.Enumerations;
using System.Collections.Generic;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Map of the block sizes to their (to be determined) costs, which may end up even being calculation functions.
    /// </summary>
    public static class ByteStorageRedundanceDurationCostMap
    {
        public static readonly Dictionary<BlockSize, double> Map = new Dictionary<BlockSize, double> {
            { global::BrightChain.Enumerations.BlockSize.Message,   0 },          // 512B
            { global::BrightChain.Enumerations.BlockSize.Tiny,      0 },         // 1K
            { global::BrightChain.Enumerations.BlockSize.Small,     0},       // 4K
            { global::BrightChain.Enumerations.BlockSize.Medium,    0},    // 1M
            { global::BrightChain.Enumerations.BlockSize.Large,     0 },  // 4M
        };

        /// <summary>
        /// Map a block size back to its cost
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static double BlockSize(BlockSize blockSize)
        {
            if (!ByteStorageRedundanceDurationCostMap.Map.ContainsKey(blockSize))
                throw new KeyNotFoundException(message: nameof(blockSize));

            return ByteStorageRedundanceDurationCostMap.Map[blockSize];
        }

        /// <summary>
        /// Map a block size's cost back to its block size. May be impossible if not a direct map later on.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
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
