using BrightChain.Enumerations;
using BrightChain.Exceptions;
using BrightChain.Services;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Interfaces;
using System;

namespace BrightChain.Models.Blocks
{
    public class TransactableBlock : Block, IDisposable, ITransactable
    {
        private bool disposedValue;
        protected BPlusTree<BlockHash, TransactableBlock> tree;
        public BPlusTreeCacheManager<BlockHash, TransactableBlock> cacheManager { get; internal set; }

        public TransactableBlock(BPlusTreeCacheManager<BlockHash, TransactableBlock> cacheManager, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) :
            base(requestTime: requestTime, keepUntilAtLeast: keepUntilAtLeast, redundancy: redundancy, data: data)
        {
            this.cacheManager = cacheManager;
            this.tree = cacheManager is null ? null : this.cacheManager.tree;
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

        public void SetCacheManager(BPlusTreeCacheManager<BlockHash, TransactableBlock> cacheManager)
        {
            this.cacheManager = cacheManager;
            this.tree = cacheManager is null ? null : this.cacheManager.tree;
        }

        public bool TreeIsEqual(BPlusTree<BlockHash, TransactableBlock> other) =>
            this.tree is null ? false : this.tree.Equals(other);

        public bool TreeIsSame(BPlusTree<BlockHash, TransactableBlock> other) =>
            this.tree is null ? false : object.ReferenceEquals(this.tree, other);

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
            return new TransactableBlock(this.cacheManager, requestTime, keepUntilAtLeast, redundancy, data, allowCommit);
        }

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
