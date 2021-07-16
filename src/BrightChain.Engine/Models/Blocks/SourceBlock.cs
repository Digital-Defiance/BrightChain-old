using System;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Extensions;
using BrightChain.Engine.Helpers;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services;

namespace BrightChain.Engine.Models.Blocks
{
    /// <summary>
    /// User data that must be whitened with the block whitener before being persisted. These blocks must never be stored directly.
    /// *** CBLs are considered user data ***
    /// </summary>
    public class SourceBlock : TransactableBlock, IComparable<SourceBlock>, IComparable<Block>
    {
        private readonly ICacheManager<BlockHash, TransactableBlock> cacheManager;

        public SourceBlock(TransactableBlockParams blockParams, ReadOnlyMemory<byte> data) :
            base(
                blockParams: blockParams,
                data: data)
        {
            cacheManager = blockParams.CacheManager;
        }

        public override Block NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            if (cacheManager is MemoryBlockCacheManager memoryBlockCacheManager)
            {
                return new MemoryBlock(
                    blockParams: new TransactableBlockParams(
                        cacheManager: CacheManager,
                        blockParams: new BlockParams(
                            blockSize: BlockSize,
                            requestTime: blockParams.RequestTime,
                            keepUntilAtLeast: blockParams.KeepUntilAtLeast,
                            redundancy: blockParams.Redundancy,
                            allowCommit: blockParams.AllowCommit,
                            privateEncrypted: blockParams.PrivateEncrypted)),
                        data: data);
            }
            else if (cacheManager is DiskBlockCacheManager diskBlockCacheManager)
            {
                return new DiskBlock(
                    blockParams: new TransactableBlockParams(
                        cacheManager: diskBlockCacheManager,
                        blockParams: new BlockParams(
                            blockSize: BlockSize,
                            requestTime: blockParams.RequestTime,
                            keepUntilAtLeast: blockParams.KeepUntilAtLeast,
                            redundancy: blockParams.Redundancy,
                            allowCommit: blockParams.AllowCommit,
                            privateEncrypted: blockParams.PrivateEncrypted)),
                        data: data);
            }
            else
            {
                throw new BrightChainException("Unexpected destination cache type");
            }
        }

        public int CompareTo(SourceBlock other)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(Data, other.Data);
        }

        public override void Dispose()
        {
        }

        public new bool Validate()
        {
            return this.PerformValidation(out _);
        }
    }
}
