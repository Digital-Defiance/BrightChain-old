namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Services;

    /// <summary>
    /// Block that is able to be stored, rolled back, committed, or prevented from being stored.
    /// TODO: Currently heavily associated with underlying BPlusTree. Abstract
    /// TODO: base off TransactedCompoundFile?
    /// </summary>
    public class TransactableBlock : Block, IDisposable, ITransactable, ITransactableBlock, IComparable<TransactableBlock>, IComparable<ITransactableBlock>, IEquatable<IBlock>
    {
        public TransactableBlock(BlockCacheManager cacheManager, Block sourceBlock, bool allowCommit)
            : base(
                blockParams: sourceBlock.BlockParams,
                data: sourceBlock.Data)
        {
            this.CacheManager = cacheManager;
            this.AllowCommit = allowCommit;
        }

        public TransactableBlock(TransactableBlockParams blockParams, ReadOnlyMemory<byte> data)
            : base(
                blockParams: blockParams,
                data: data)
        {
            this.CacheManager = blockParams.CacheManager;
            this.AllowCommit = blockParams.AllowCommit;
            this.disposedValue = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactableBlock"/> class.
        /// For test methods.
        /// </summary>
        internal TransactableBlock()
            : base(
                blockParams: new BlockParams(
                    blockSize: BlockSize.Message,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: RedundancyContractType.HeapAuto,
                    privateEncrypted: false,
                    originalType: typeof(TransactableBlock)),
                data: new ReadOnlyMemory<byte>() { })
        {
        }

        /// <summary>
        /// Gets a bool indicating whether the block's data has been loaded from the attached cache, or kept after persisting to cache.
        /// </summary>
        public bool DataInMemory { get; }

        /// <summary>
        /// Boolean indicating whether our data has been disposed.
        /// </summary>
        private bool disposedValue;

        public ICacheManager<BlockHash, TransactableBlock> CacheManager { get; internal set; }

        public bool Committed { get; protected set; } = false;

        public bool AllowCommit { get; protected set; } = false;

        public static bool operator ==(TransactableBlock a, TransactableBlock b)
        {
            return a.BlockSize == b.BlockSize && ReadOnlyMemoryComparer<byte>.Compare(a.Data, b.Data) == 0;
        }

        public static bool operator !=(TransactableBlock a, TransactableBlock b)
        {
            return !a.Equals(b);
        }

        public void SetCacheManager(ICacheManager<BlockHash, TransactableBlock> cacheManager)
        {
            if (this.CacheManager is not null)
            {
                throw new BrightChainException("CacheManager already set");
            }

            this.CacheManager = cacheManager;
        }

        public void Commit()
        {
            if (!this.AllowCommit)
            {
                throw new BrightChainException("Block is not allowed to be committed");
            }

            this.Committed = true;
        }

        public void Rollback()
        {
            this.Committed = false;
        }

        public override TransactableBlock NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            return new TransactableBlock(
                blockParams: new TransactableBlockParams(
                    cacheManager: this.CacheManager,
                    allowCommit: this.AllowCommit,
                    blockParams: this.AsBlock.BlockParams),
                data: data);
        }

        public override TransactableBlockParams BlockParams => new TransactableBlockParams(
                cacheManager: this.CacheManager,
                allowCommit: this.AllowCommit,
                blockParams: new BlockParams(
                    blockSize: this.BlockSize,
                    requestTime: this.StorageContract.RequestTime,
                    keepUntilAtLeast: this.StorageContract.KeepUntilAtLeast,
                    redundancy: this.StorageContract.RedundancyContractType,
                    privateEncrypted: this.StorageContract.PrivateEncrypted,
                    originalType: Type.GetType(this.OriginalType)));

        public override bool Equals(object obj)
        {
            return obj is Block block ? block.BlockSize == this.BlockSize && ReadOnlyMemoryComparer<byte>.Compare(this.Data, block.Data) == 0 : false;
        }

        public override int GetHashCode()
        {
            return this.Data.GetHashCode();
        }

        public int CompareTo(TransactableBlock other)
        {
            return other.BlockSize == this.BlockSize ? ReadOnlyMemoryComparer<byte>.Compare(this.Data, other.Data) : (other.Data.Length > this.Data.Length ? -1 : 1);
        }

        public int CompareTo(ITransactableBlock other)
        {
            return other.BlockSize == this.BlockSize ? ReadOnlyMemoryComparer<byte>.Compare(this.Data, other.Data) : other.Data.Length > this.Data.Length ? -1 : 1;
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TransactableBlock()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Dispose block data and memory contents.
        /// </summary>
        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose block data and memory contents.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                this.Rollback();
                this.Data = null;

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }
    }
}
