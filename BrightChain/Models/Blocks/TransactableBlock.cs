using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Helpers;
using BrightChain.Interfaces;
using BrightChain.Services;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Interfaces;
using System;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Block that is able to be stored, rolled back, committed, or prevented from being stored.
    /// TODO: Currently heavily associated with underlying BPlusTree. Abstract
    /// TODO: base off TransactedCompoundFile?
    /// </summary>
    public class TransactableBlock : Block, IDisposable, ITransactable, ITransactableBlock
    {
        private bool disposedValue;
        protected BPlusTree<BlockHash, TransactableBlock> tree;
        public BPlusTreeCacheManager<BlockHash, TransactableBlock, BlockHashSerializer, BlockSerializer<TransactableBlock>> CacheManager { get; internal set; }
        public bool Committed { get; protected set; } = false;
        public bool AllowCommit { get; protected set; } = false;

        public TransactableBlock(BPlusTreeCacheManager<BlockHash, TransactableBlock, BlockHashSerializer, BlockSerializer<TransactableBlock>> cacheManager, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) :
            base(requestTime: requestTime, keepUntilAtLeast: keepUntilAtLeast, redundancy: redundancy, data: data)
        {
            this.CacheManager = cacheManager;
            this.tree = cacheManager is null ? null : this.CacheManager.tree;
            this.disposedValue = false;
        }

        /// <summary>
        /// For test methods
        /// </summary>
        public TransactableBlock() : base(
            requestTime: DateTime.Now,
            keepUntilAtLeast: DateTime.MaxValue,
            redundancy: RedundancyContractType.HeapAuto,
            data: new ReadOnlyMemory<byte>() { })
        {

        }

        public void SetCacheManager(BPlusTreeCacheManager<BlockHash, TransactableBlock, BlockHashSerializer, BlockSerializer<TransactableBlock>> cacheManager)
        {
            this.CacheManager = cacheManager;
            this.tree = cacheManager is null ? null : this.CacheManager.tree;
        }

        public bool TreeIsEqual(BPlusTree<BlockHash, TransactableBlock> other) =>
            this.tree is null ? false : this.tree.Equals(other);

        public bool TreeIsSame(BPlusTree<BlockHash, TransactableBlock> other) =>
            this.tree is null ? false : object.ReferenceEquals(this.tree, other);

        public static bool operator ==(TransactableBlock a, TransactableBlock b) =>
            ReadOnlyMemoryComparer<byte>.Compare(a.Data, b.Data) == 0;

        public static bool operator !=(TransactableBlock a, TransactableBlock b) =>
            !(a == b);

        public void Commit()
        {
            if (this.tree is null)
                throw new NullReferenceException(nameof(this.tree));

            if (!this.AllowCommit)
                throw new BrightChainException("Block is not allowed to be committed");

            this.tree.Commit();
            this.Committed = true;
        }

        public void Rollback()
        {
            if (this.tree is null)
                throw new NullReferenceException(nameof(this.tree));

            this.tree.Rollback();
            this.Committed = false;
        }

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit)
        {
            return new TransactableBlock(this.CacheManager, requestTime, keepUntilAtLeast, redundancy, data, allowCommit);
        }

        public override bool Equals(object obj) =>
            this == obj as TransactableBlock;

        public override int GetHashCode() =>
            this.Data.GetHashCode();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                this.Rollback();
                this.Data = null;

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
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
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
