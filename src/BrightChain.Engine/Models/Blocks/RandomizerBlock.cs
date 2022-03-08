using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Helpers;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services.CacheManagers.Block;
using ProtoBuf;

namespace BrightChain.Engine.Models.Blocks;

/// <summary>
///     Input blocks to the whitener service that consist of purely CSPRNG data of the specified block size
/// </summary>
[ProtoContract]
public class RandomizerBlock : BrightenedBlock, IComparable<RandomizerBlock>
{
    public RandomizerBlock(BrightenedBlockCacheManagerBase destinationCache, BlockSize blockSize, DateTime keepUntilAtLeast,
        RedundancyContractType redundancyContractType, DateTime? requestTime = null)
        : base(
            blockParams: new BrightenedBlockParams(
                cacheManager: destinationCache,
                allowCommit: true,
                blockParams: new BlockParams(
                    blockSize: blockSize,
                    requestTime: requestTime.GetValueOrDefault(defaultValue: DateTime.Now),
                    keepUntilAtLeast: keepUntilAtLeast,
                    redundancy: redundancyContractType,
                    privateEncrypted: false, // randomizers are never "private encrypted"
                    originalType: typeof(RandomizerBlock))),
            data: RandomDataHelper.RandomReadOnlyBytes(length: BlockSizeMap.BlockSize(blockSize: blockSize)))
    {
        this.OriginalAssemblyTypeString = typeof(RandomizerBlock).AssemblyQualifiedName;
    }

    public RandomizerBlock(BrightenedBlockParams blockParams)
        : base(
            blockParams: blockParams,
            data: RandomDataHelper.RandomReadOnlyBytes(length: BlockSizeMap.BlockSize(blockSize: blockParams.BlockSize)))
    {
        this.OriginalAssemblyTypeString = typeof(RandomizerBlock).AssemblyQualifiedName;
    }

    public int CompareTo(RandomizerBlock other)
    {
        return this.StoredData.CompareTo(other: other.StoredData);
    }

    public override void Dispose()
    {
    }
}
