using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Helpers;
using BrightChain.Interfaces;
using System;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Block that is able to be stored, rolled back, committed, or prevented from being stored.
    /// TODO: Currently heavily associated with underlying BPlusTree. Abstract
    /// TODO: base off TransactedCompoundFile?
    /// </summary>
    public class TransactableBlock : Block, IDisposable, ITransactable, ITransactableBlock, IComparable<TransactableBlock>, IComparable<ITransactableBlock>
    {
        private bool disposedValue;
        //protected BPlusTree<BlockHash, TransactableBlock> tree;
        public ICacheManager<BlockHash, TransactableBlock> CacheManager { get; internal set; }
        public bool Committed { get; protected set; } = false;
        public bool AllowCommit { get; protected set; } = false;

        public TransactableBlock(ICacheManager<BlockHash, TransactableBlock> cacheManager, BlockSize blockSize, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) :
            base(blockSize: blockSize, requestTime: requestTime, keepUntilAtLeast: keepUntilAtLeast, redundancy: redundancy, data: data)
        {
            this.CacheManager = cacheManager;
            //this.tree = cacheManager is null ? null : this.CacheManager.tree;
            this.disposedValue = false;
        }

        /// <summary>
        /// For test methods
        /// </summary>
        internal TransactableBlock() : base(
            blockSize: BlockSize.Message,
            requestTime: DateTime.Now,
            keepUntilAtLeast: DateTime.MaxValue,
            redundancy: RedundancyContractType.HeapAuto,
            data: new ReadOnlyMemory<byte>() { })
        {

        }

        public void SetCacheManager(ICacheManager<BlockHash, TransactableBlock> cacheManager) => this.CacheManager = cacheManager;

        public static bool operator ==(TransactableBlock a, TransactableBlock b) => ReadOnlyMemoryComparer<byte>.Compare(a.Data, b.Data) == 0;

        public static bool operator !=(TransactableBlock a, TransactableBlock b) => !a.Equals(b);

        public void Commit()
        {
            if (!this.AllowCommit)
            {
                throw new BrightChainException("Block is not allowed to be committed");
            }

            this.Committed = true;
        }

        public void Rollback() => this.Committed = false;

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) => new TransactableBlock(this.CacheManager, this.BlockSize, requestTime, keepUntilAtLeast, redundancy, data, allowCommit);

        public override bool Equals(object obj) => ReadOnlyMemoryComparer<byte>.Compare(this.Data, (obj as TransactableBlock).Data) == 0;

        public override int GetHashCode() => this.Data.GetHashCode();

        public int CompareTo(TransactableBlock other) => ReadOnlyMemoryComparer<byte>.Compare(this.Data, other.Data);

        public int CompareTo(ITransactableBlock other) => ReadOnlyMemoryComparer<byte>.Compare(this.Data, (other as TransactableBlock).Data);

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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TransactableBlock()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
