using BrightChain.Exceptions;
using BrightChain.Extensions;
using BrightChain.Helpers;
using BrightChain.Interfaces;
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
        private ICacheManager<BlockHash, TransactableBlock> cacheManager;

        public SourceBlock(TransactableBlockArguments blockArguments, ReadOnlyMemory<byte> data) :
            base(
                blockArguments: blockArguments,
                data: data)
        { }

        public override Block NewBlock(BlockArguments blockArguments, ReadOnlyMemory<byte> data)
        {
            if (this.cacheManager is MemoryBlockCacheManager memoryBlockCacheManager)
            {
                return new MemoryBlock(
                    blockArguments: new TransactableBlockArguments(
                        cacheManager: this.CacheManager,
                        blockArguments: new BlockArguments(
                            blockSize: this.BlockSize,
                            requestTime: blockArguments.RequestTime,
                            keepUntilAtLeast: blockArguments.KeepUntilAtLeast,
                            redundancy: blockArguments.Redundancy,
                            allowCommit: blockArguments.AllowCommit,
                            privateEncrypted: blockArguments.PrivateEncrypted)),
                        data: data);
            }
            else if (this.cacheManager is BrightChainBlockCacheManager diskBlockCacheManager)
            {
                return new DiskBlock(
                    blockArguments: new TransactableBlockArguments(
                        cacheManager: diskBlockCacheManager,
                        blockArguments: new BlockArguments(
                            blockSize: this.BlockSize,
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

        public int CompareTo(SourceBlock other) => ReadOnlyMemoryComparer<byte>.Compare(this.Data, other.Data);

        public override void Dispose()
        {
        }

        public new bool Validate() => this.PerformValidation(out _);
    }
}
