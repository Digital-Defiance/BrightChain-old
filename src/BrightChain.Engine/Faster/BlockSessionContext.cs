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
        public readonly ClientSession<BlockHash, BrightenedBlock, BrightenedBlock, BrightenedBlock, CacheContext, SimpleFunctions<BlockHash, BrightenedBlock, CacheContext>> MetadataSession;

        public readonly ClientSession<BlockHash, BlockData, BlockData, BlockData, CacheContext, SimpleFunctions<BlockHash, BlockData, CacheContext>> DataSession;

        public readonly ClientSession<DataHash, BrightHandle, BrightHandle, BrightHandle, CacheContext, SimpleFunctions<DataHash, BrightHandle, CacheContext>> CblSourceHashSession;

        public readonly ClientSession<Guid, DataHash, DataHash, DataHash, CacheContext, SimpleFunctions<Guid, DataHash, CacheContext>> CblCorrelationIdsSession;

        public BlockSessionContext(
            ClientSession<BlockHash, BrightenedBlock, BrightenedBlock, BrightenedBlock, CacheContext, SimpleFunctions<BlockHash, BrightenedBlock, CacheContext>> metadataSession,
            ClientSession<BlockHash, BlockData, BlockData, BlockData, CacheContext, SimpleFunctions<BlockHash, BlockData, CacheContext>> dataSession,
            ClientSession<DataHash, BrightHandle, BrightHandle, BrightHandle, CacheContext, SimpleFunctions<DataHash, BrightHandle, CacheContext>> cblSourceHashSession,
            ClientSession<Guid, DataHash, DataHash, DataHash, CacheContext, SimpleFunctions<Guid, DataHash, CacheContext>> cblCorrelationIdsSession)
        {
            this.MetadataSession = metadataSession;
            this.DataSession = dataSession;
            this.CblSourceHashSession = cblSourceHashSession;
            this.CblCorrelationIdsSession = cblCorrelationIdsSession;
        }

        public bool Drop(BlockHash blockHash, bool complete = true)
        {

            if (!(this.MetadataSession.Delete(blockHash) == Status.OK))
            {
                return false;
            }

            if (!(this.DataSession.Delete(blockHash) == Status.OK))
            {
                // TODO: rollback?
                return false;
            }

            if (complete)
            {
                return this.CompletePending(wait: true);
            }

            return true;
        }

        public BrightenedBlock Get(BlockHash blockHash)
        {
            var metadataResultTuple = this.MetadataSession.Read(blockHash);

            if (metadataResultTuple.status != Status.OK)
            {
                throw new IndexOutOfRangeException(message: blockHash.ToString());
            }

            var block = metadataResultTuple.output;

            var dataResultTuple = this.DataSession.Read(blockHash);

            if (dataResultTuple.status != Status.OK)
            {
                throw new IndexOutOfRangeException(message: blockHash.ToString());
            }

            block.StoredData = dataResultTuple.output;

            if (!block.Validate())
            {
                throw new BrightChainValidationEnumerableException(block.ValidationExceptions, "Failed to reload block from store");
            }

            return block;
        }

        public void Upsert(ref BrightenedBlock block, bool complete = false)
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
            var c = this.CblSourceHashSession.CompletePending(wait: wait);
            var ci = this.CblCorrelationIdsSession.CompletePending(wait: wait);

            // broken out to prevent short circuit
            return m && d && c && ci;
        }

        public async Task CompletePendingAsync(bool wait)
        {
            Task.WaitAll(new Task[]
            {
                this.MetadataSession.CompletePendingAsync(waitForCommit: wait).AsTask(),
                this.DataSession.CompletePendingAsync(waitForCommit: wait).AsTask(),
                this.CblSourceHashSession.CompletePendingAsync(waitForCommit: wait).AsTask(),
                this.CblCorrelationIdsSession.CompletePendingAsync(waitForCommit: wait).AsTask(),
            });
        }

        public string SessionID =>
            string.Format("{0}-{1}-{2}-{3}", this.MetadataSession.ID, this.DataSession.ID, this.CblSourceHashSession.ID, this.CblCorrelationIdsSession.ID);

        public void Dispose()
        {
            this.MetadataSession.Dispose();
            this.DataSession.Dispose();
            this.CblSourceHashSession.Dispose();
            this.CblCorrelationIdsSession.Dispose();
        }
    }
}
