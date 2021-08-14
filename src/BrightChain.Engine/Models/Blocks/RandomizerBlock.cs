namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Services.CacheManagers;
    using ProtoBuf;

    /// <summary>
    /// Input blocks to the whitener service that consist of purely CSPRNG data of the specified block size
    /// </summary>
    [ProtoContract]
    public class RandomizerBlock : TransactableBlock, IComparable<RandomizerBlock>
    {
        public RandomizerBlock(BlockCacheManager destinationCache, BlockSize blockSize, DateTime keepUntilAtLeast, RedundancyContractType redundancyContractType, DateTime? requestTime = null)
            : base(
                 blockParams: new TransactableBlockParams(
                     cacheManager: destinationCache,
                     allowCommit: true,
                     blockParams: new BlockParams(
                        blockSize: blockSize,
                        requestTime: requestTime.GetValueOrDefault(DateTime.Now),
                        keepUntilAtLeast: keepUntilAtLeast,
                        redundancy: redundancyContractType,
                        privateEncrypted: false, // randomizers are never "private encrypted"
                        originalType: typeof(RandomizerBlock))),
                 data: RandomDataHelper.RandomReadOnlyBytes(BlockSizeMap.BlockSize(blockSize)))
        {
            this.CacheManager.Set(this.Id, this);
            this.OriginalType = typeof(RandomizerBlock).AssemblyQualifiedName;
        }

        public RandomizerBlock(TransactableBlockParams blockParams)
            : base(
                blockParams: blockParams,
                data: RandomDataHelper.RandomReadOnlyBytes(BlockSizeMap.BlockSize(blockParams.BlockSize)))
        {
            this.CacheManager.Set(this.Id, this);
            this.OriginalType = typeof(RandomizerBlock).AssemblyQualifiedName;
        }

        /// <summary>
        /// replace incoming data (will be empty byte array to fit conventions) with random data
        /// </summary>
        /// <param name="requestTime"></param>
        /// <param name="keepUntilAtLeast"></param>
        /// <param name="redundancy"></param>
        /// <param name="_"></param>
        /// <param name="allowCommit"></param>
        /// <returns></returns>
        public override RandomizerBlock NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> _)
        {
            return new RandomizerBlock(
                blockParams: new TransactableBlockParams(
                allowCommit: this.AllowCommit,
                cacheManager: this.CacheManager,
                blockParams: blockParams));
        }

        public int CompareTo(RandomizerBlock other)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(this.Data, other.Data);
        }

        public override void Dispose()
        {

        }
    }
}
