﻿using System.Collections.Generic;
using BrightChain.Engine.Enumerations;

namespace BrightChain.Engine.Models.Units
{
    /// <summary>
    /// Map of the block sizes to their (to be determined) costs, which may end up even being calculation functions.
    /// </summary>
    public static class ByteStorageRedundancyDurationCostMap
    {
        public static readonly Dictionary<BlockSize, double> SizeMap = new Dictionary<BlockSize, double> {
            { global::BrightChain.Engine.Enumerations.BlockSize.Micro,     0 }, // 256B
            { global::BrightChain.Engine.Enumerations.BlockSize.Message,   0 }, // 512B
            { global::BrightChain.Engine.Enumerations.BlockSize.Tiny,      0 }, // 1K
            { global::BrightChain.Engine.Enumerations.BlockSize.Small,     0 }, // 4K
            { global::BrightChain.Engine.Enumerations.BlockSize.Medium,    0 }, // 1M
            { global::BrightChain.Engine.Enumerations.BlockSize.Large,     0 }, // 4M
        };

        public static readonly Dictionary<RedundancyContractType, double> RedundancyMap = new Dictionary<RedundancyContractType, double> {
            {RedundancyContractType.LocalNone, 0 },
            {RedundancyContractType.LocalMirror, 0 },
            {RedundancyContractType.HeapAuto, 0 },
            {RedundancyContractType.HeapLowPriority, 0 },
            {RedundancyContractType.HeapHighPriority, 0 },
        };

        /// <summary>
        /// Map a block size back to its cost
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static double Cost(BlockSize blockSize, RedundancyContractType redundancy)
        {
            if (!ByteStorageRedundancyDurationCostMap.SizeMap.ContainsKey(blockSize))
            {
                throw new KeyNotFoundException(message: nameof(blockSize));
            }

            if (!ByteStorageRedundancyDurationCostMap.RedundancyMap.ContainsKey(redundancy))
            {
                throw new KeyNotFoundException(message: nameof(redundancy));
            }

            return ByteStorageRedundancyDurationCostMap.SizeMap[blockSize] * ByteStorageRedundancyDurationCostMap.RedundancyMap[redundancy];
        }
    }
}
