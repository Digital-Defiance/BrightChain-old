using BrightChain.CSharpTest.Net.Collections;
using BrightChain.CSharpTest.Net.Interfaces;
using BrightChain.Enumerations;
using BrightChain.Interfaces;
using System;

namespace BrightChain.Models.Blocks
{
    public class TransactableBlock : Block, IDisposable, ITransactable
    {
        public bool Committed { get; private set; }

        private bool disposedValue;
        protected BPlusTree<BlockHash, Block> tree;
        protected ICacheManager<BlockHash, Block> cacheManager;

        public TransactableBlock(BPlusTree<BlockHash, Block> tree, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data) :
            base(requestTime: requestTime, keepUntilAtLeast: keepUntilAtLeast, redundancy: redundancy, data: data)
        {
            this.disposedValue = false;
            this.Committed = false;
        }

        public void Commit()
        {
            this.tree.Commit();
            this.Committed = true;
        }

        public void Rollback() =>
            this.tree.Rollback();

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

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data) =>
            new TransactableBlock(
                tree: this.tree,
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                redundancy: redundancy,
                data: data);
    }
}
