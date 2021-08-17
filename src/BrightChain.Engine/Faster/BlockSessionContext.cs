namespace BrightChain.Engine.Faster
{
    using System;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;

    public struct BlockSessionContext : IDisposable
    {
        public readonly ClientSession<BlockHash, TransactableBlock, TransactableBlock, TransactableBlock, CacheContext, SimpleFunctions<BlockHash, TransactableBlock, CacheContext>> MetadataSession;

        public readonly ClientSession<BlockHash, BlockData, BlockData, BlockData, CacheContext, SimpleFunctions<BlockHash, BlockData, CacheContext>> DataSession;

        public BlockSessionContext(
            ClientSession<BlockHash, TransactableBlock, TransactableBlock, TransactableBlock, CacheContext, SimpleFunctions<BlockHash, TransactableBlock, CacheContext>> metadataSession,
            ClientSession<BlockHash, BlockData, BlockData, BlockData, CacheContext, SimpleFunctions<BlockHash, BlockData, CacheContext>> dataSession)
        {
            this.MetadataSession = metadataSession;
            this.DataSession = dataSession;
        }

        public void Upsert(ref TransactableBlock block, bool complete = false)
        {
            var blockHash = block.Id;
            var resultStatus = this.MetadataSession.Upsert(ref blockHash, ref block);
            if (resultStatus != Status.OK)
            {
                throw new BrightChainException("Unable to store block");
            }

            var blockData = block.StoredData;
            resultStatus = this.DataSession.Upsert(ref blockHash, ref blockData);
            if (resultStatus != Status.OK)
            {
                throw new BrightChainException("Unable to store block");
            }

            if (complete)
            {
                this.CompletePending(wait: true);
            }
        }

        public bool CompletePending(bool wait)
        {
            var m = this.MetadataSession.CompletePending(wait: wait);
            var d = this.DataSession.CompletePending(wait: wait);

            // broken out to prevent short circuit
            return m && d;
        }

        public async Task CompletePendingAsync(bool wait)
        {
            await this.MetadataSession.CompletePendingAsync(waitForCommit: wait).ConfigureAwait(false);
            await this.DataSession.CompletePendingAsync(waitForCommit: wait).ConfigureAwait(false);
        }

        public string SessionID =>
            string.Format("{0}-{1}", this.MetadataSession.ID, this.DataSession.ID);

        public void Dispose()
        {
            this.MetadataSession.Dispose();
            this.DataSession.Dispose();
        }
    }
}
