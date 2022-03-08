using System.Collections.Generic;
using BrightChain.Engine.Enumerations;

namespace BrightChain.Engine.Models.Units;

/// <summary>
///     Map of the block sizes to their (to be determined) costs, which may end up even being calculation functions.
/// </summary>
public static class ByteStorageRedundancyDurationCostMap
{
    public static readonly Dictionary<BlockSize, double> SizeMap = new()
    {
        {BlockSize.Micro, 0}, // 256B
        {BlockSize.Message, 0}, // 512B
        {BlockSize.Tiny, 0}, // 1K
        {BlockSize.Small, 0}, // 4K
        {BlockSize.Medium, 0}, // 1M
        {BlockSize.Large, 0}, // 4M
    };

    public static readonly Dictionary<RedundancyContractType, double> RedundancyMap = new()
    {
        {RedundancyContractType.LocalNone, 0},
        {RedundancyContractType.LocalMirror, 0},
        {RedundancyContractType.HeapAuto, 0},
        {RedundancyContractType.HeapLowPriority, 0},
        {RedundancyContractType.HeapHighPriority, 0},
    };

    /// <summary>
    ///     Map a block size back to its cost
    /// </summary>
    /// <param name="blockSize"></param>
    /// <returns></returns>
    public static double Cost(BlockSize blockSize, RedundancyContractType redundancy)
    {
        if (!SizeMap.ContainsKey(key: blockSize))
        {
            throw new KeyNotFoundException(message: nameof(blockSize));
        }

        if (!RedundancyMap.ContainsKey(key: redundancy))
        {
            throw new KeyNotFoundException(message: nameof(redundancy));
        }

        return SizeMap[key: blockSize] * RedundancyMap[key: redundancy];
    }
}
