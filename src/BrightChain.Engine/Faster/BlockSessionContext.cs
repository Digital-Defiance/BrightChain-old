namespace BrightChain.Engine.Faster
{
    using System;
    using System.Threading.Tasks;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Faster.Functions;
    using BrightChain.Engine.Faster.Indices;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;
    using Microsoft.Extensions.Logging;

    public class BlockSessionContext : IDisposable
    {
        private readonly ILogger logger;

        public readonly ClientSession<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext, BrightChainBlockHashAdvancedFunctions> BlockDataBlobSession;

        public readonly ClientSession<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext, BrightChainIndicesAdvancedFunctions> SharedCacheSession;

        public BlockSessionContext(
            ILogger logger,
            ClientSession<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext, BrightChainBlockHashAdvancedFunctions> dataSession,
            ClientSession<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext, BrightChainIndicesAdvancedFunctions> cblIndicesSession)
        {
            this.logger = logger;
            this.BlockDataBlobSession = dataSession;
            this.SharedCacheSession = cblIndicesSession;
        }

        public bool Contains(BlockHash blockHash)
        {
            var dataResultTuple = this.BlockDataBlobSession.Read(blockHash);

            return
                dataResultTuple.status == Status.OK;
        }

        public bool Drop(BlockHash blockHash, bool complete = true)
        {
            if (this.BlockDataBlobSession.Delete(blockHash) != Status.OK)
            {
                // TODO: rollback?
                return false;
            }

            // TODO: determine when/where & implement index deletions relevant to the block

            if (complete)
            {
                return this.CompletePending(waitForCommit: false);
            }

            return true;
        }

        private static string BlockMetadataIndexKey(BlockHash blockHash)
            => string.Format("Metadata:{0}", blockHash.ToString());

        public BrightenedBlock Get(BlockHash blockHash)
        {
            var dataResultTuple = this.BlockDataBlobSession.Read(blockHash);

            if (dataResultTuple.status != Status.OK)
            {
                throw new IndexOutOfRangeException(message: blockHash.ToString());
            }

            var result = this.SharedCacheSession.Read(BlockMetadataIndexKey(blockHash));
            if (result.status == Status.NOTFOUND)
            {
                throw new IndexOutOfRangeException(message: blockHash.ToString());
            }
            else if (result.status != Status.OK)
            {
                throw new BrightChainException(
                    message: string.Format("metadata fetch error: {0}", result.status.ToString()));
            }

            if (result.output is BlockMetadataIndexValue blockMetadata)
            {
                var block = blockMetadata.Block;

                block.StoredData = dataResultTuple.output;

                if (!block.Validate())
                {
                    throw new BrightChainValidationEnumerableException(block.ValidationExceptions, "Failed to reload block from store");
                }

                return block;
            }

            throw new BrightChainException("Unexpected index result type for key");

        }

        public void Upsert(BrightenedBlock block, bool completePending = false)
        {
            var resultStatus = this.SharedCacheSession.Upsert(
                key: BlockMetadataIndexKey(block.Id),
                desiredValue: new BlockMetadataIndexValue(block));

            if (resultStatus != Status.OK)
            {
                throw new BrightChainException("Unable to store block");
            }

            resultStatus = this.BlockDataBlobSession.Upsert(block.Id, block.StoredData);
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
                    this.BlockDataBlobSession.WaitForCommitAsync().AsTask(),
                    this.SharedCacheSession.WaitForCommitAsync().AsTask(),
            }).ConfigureAwait(false);
        }

        public bool CompletePending(bool waitForCommit)
        {
            var d = this.BlockDataBlobSession.CompletePending(wait: waitForCommit);
            var c = this.SharedCacheSession.CompletePending(wait: waitForCommit);

            // broken out to prevent short circuit
            return d && c;
        }

        public async Task CompletePendingAsync(bool waitForCommit)
        {
            Task.WaitAll(new Task[]
            {
                this.BlockDataBlobSession.CompletePendingAsync(waitForCommit: waitForCommit).AsTask(),
                this.SharedCacheSession.CompletePendingAsync(waitForCommit: waitForCommit).AsTask(),
            });
        }

        public string SessionID =>
            string.Format("{0}-{1}", this.BlockDataBlobSession.ID, this.SharedCacheSession.ID);

        public void Dispose()
        {
            this.BlockDataBlobSession.Dispose();
            this.SharedCacheSession.Dispose();
        }
    }
}
