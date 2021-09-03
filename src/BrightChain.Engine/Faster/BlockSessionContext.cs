namespace BrightChain.Engine.Faster
{
    using System;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Faster.Indices;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;
    using Microsoft.Extensions.Logging;

    public class BlockSessionContext : IDisposable
    {
        private readonly ILogger logger;

        public readonly ClientSession<BlockHash, BrightenedBlock, BrightenedBlock, BrightenedBlock, BrightChainFasterCacheContext, SimpleFunctions<BlockHash, BrightenedBlock, BrightChainFasterCacheContext>> MetadataSession;

        public readonly ClientSession<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext, SimpleFunctions<BlockHash, BlockData, BrightChainFasterCacheContext>> DataSession;

        public readonly ClientSession<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext, SimpleFunctions<string, BrightChainIndexValue, BrightChainFasterCacheContext>> CblIndicesSession;

        public BlockSessionContext(
            ILogger logger,
            ClientSession<BlockHash, BrightenedBlock, BrightenedBlock, BrightenedBlock, BrightChainFasterCacheContext, SimpleFunctions<BlockHash, BrightenedBlock, BrightChainFasterCacheContext>> metadataSession,
            ClientSession<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext, SimpleFunctions<BlockHash, BlockData, BrightChainFasterCacheContext>> dataSession,
            ClientSession<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext, SimpleFunctions<string, BrightChainIndexValue, BrightChainFasterCacheContext>> cblIndicesSession)
        {
            this.logger = logger;
            this.MetadataSession = metadataSession;
            this.DataSession = dataSession;
            this.CblIndicesSession = cblIndicesSession;
        }

        public bool Contains(BlockHash blockHash)
        {
            var metadataResultTuple = this.MetadataSession.Read(blockHash);
            var dataResultTuple = this.DataSession.Read(blockHash);

            return
                metadataResultTuple.status == Status.OK &&
                dataResultTuple.status != Status.OK;
        }

        public bool Drop(BlockHash blockHash, bool complete = true)
        {
            if (this.MetadataSession.Delete(blockHash) != Status.OK)
            {
                return false;
            }

            if (this.DataSession.Delete(blockHash) != Status.OK)
            {
                // TODO: rollback?
                return false;
            }

            if (complete)
            {
                return this.CompletePending(waitForCommit: false);
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

        public void Upsert(ref BrightenedBlock block, bool completePending = false)
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

            if (completePending)
            {
                this.CompletePending(waitForCommit: false);
            }
        }

        public async Task WaitForCommitAsync()
        {
            await Task.WhenAll(new Task[]
            {
                    this.MetadataSession.WaitForCommitAsync().AsTask(),
                    this.DataSession.WaitForCommitAsync().AsTask(),
                    this.CblIndicesSession.WaitForCommitAsync().AsTask(),
            }).ConfigureAwait(false);
        }

        public bool CompletePending(bool waitForCommit)
        {
            var m = this.MetadataSession.CompletePending(wait: waitForCommit);
            var d = this.DataSession.CompletePending(wait: waitForCommit);
            var c = this.CblIndicesSession.CompletePending(wait: waitForCommit);

            // broken out to prevent short circuit
            return m && d && c;
        }

        public async Task CompletePendingAsync(bool waitForCommit)
        {
            Task.WaitAll(new Task[]
            {
                this.MetadataSession.CompletePendingAsync(waitForCommit: waitForCommit).AsTask(),
                this.DataSession.CompletePendingAsync(waitForCommit: waitForCommit).AsTask(),
                this.CblIndicesSession.CompletePendingAsync(waitForCommit: waitForCommit).AsTask(),
            });
        }

        public string SessionID =>
            string.Format("{0}-{1}-{2}-{3}", this.MetadataSession.ID, this.DataSession.ID, this.CblIndicesSession.ID);

        public void Dispose()
        {
            this.MetadataSession.Dispose();
            this.DataSession.Dispose();
            this.CblIndicesSession.Dispose();
        }
    }
}
