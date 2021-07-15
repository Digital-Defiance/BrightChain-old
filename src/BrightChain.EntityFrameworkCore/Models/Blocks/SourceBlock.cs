using BrightChain.Exceptions;
using BrightChain.Extensions;
using BrightChain.Helpers;
using BrightChain.Interfaces;
using BrightChain.Models.Blocks.DataObjects;
using BrightChain.Services;
using System;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// User data that must be whitened with the block whitener before being persisted. These blocks must never be stored directly.
    /// *** CBLs are considered user data ***
    /// </summary>
    public class SourceBlock : TransactableBlock, IComparable<SourceBlock>, IComparable<Block>
    {
        private readonly ICacheManager<BlockHash, TransactableBlock> cacheManager;

        public SourceBlock(TransactableBlockParams blockArguments, ReadOnlyMemory<byte> data) :
            base(
                blockArguments: blockArguments,
                data: data)
        {
            cacheManager = blockArguments.CacheManager;
        }

        public override Block NewBlock(BlockParams blockArguments, ReadOnlyMemory<byte> data)
        {
            if (cacheManager is MemoryBlockCacheManager memoryBlockCacheManager)
            {
                return new MemoryBlock(
                    blockArguments: new TransactableBlockParams(
                        cacheManager: CacheManager,
                        blockArguments: new BlockParams(
                            blockSize: BlockSize,
                            requestTime: blockArguments.RequestTime,
                            keepUntilAtLeast: blockArguments.KeepUntilAtLeast,
                            redundancy: blockArguments.Redundancy,
                            allowCommit: blockArguments.AllowCommit,
                            privateEncrypted: blockArguments.PrivateEncrypted)),
                        data: data);
            }
            else if (cacheManager is BrightChainBlockCacheManager diskBlockCacheManager)
            {
                return new DiskBlock(
                    blockArguments: new TransactableBlockParams(
                        cacheManager: diskBlockCacheManager,
                        blockArguments: new BlockParams(
                            blockSize: BlockSize,
                            requestTime: blockArguments.RequestTime,
                            keepUntilAtLeast: blockArguments.KeepUntilAtLeast,
                            redundancy: blockArguments.Redundancy,
                            allowCommit: blockArguments.AllowCommit,
                            privateEncrypted: blockArguments.PrivateEncrypted)),
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
