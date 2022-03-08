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

namespace BrightChain.Engine.Faster;

public class BlockSessionContext : IDisposable
{
    public readonly
        ClientSession<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext, BrightChainBlockHashAdvancedFunctions>
        BlockDataBlobSession;

    private readonly ILogger logger;

    public readonly
        ClientSession<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext,
            BrightChainIndicesAdvancedFunctions> SharedCacheSession;

    public BlockSessionContext(
        ILogger logger,
        ClientSession<BlockHash, BlockData, BlockData, BlockData, BrightChainFasterCacheContext, BrightChainBlockHashAdvancedFunctions>
            dataSession,
        ClientSession<string, BrightChainIndexValue, BrightChainIndexValue, BrightChainIndexValue, BrightChainFasterCacheContext,
            BrightChainIndicesAdvancedFunctions> cblIndicesSession)
    {
        this.logger = logger;
        this.BlockDataBlobSession = dataSession;
        this.SharedCacheSession = cblIndicesSession;
    }

    public string SessionID =>
        string.Format(format: "{0}-{1}",
            arg0: this.BlockDataBlobSession.ID,
            arg1: this.SharedCacheSession.ID);

    public void Dispose()
    {
        this.BlockDataBlobSession.Dispose();
        this.SharedCacheSession.Dispose();
    }

    public bool Contains(BlockHash blockHash)
    {
        var dataResultTuple = this.BlockDataBlobSession.Read(key: blockHash);

        return
            dataResultTuple.status == Status.OK;
    }

    public bool Drop(BlockHash blockHash, bool complete = true)
    {
        if (this.BlockDataBlobSession.Delete(key: blockHash) != Status.OK)
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
    {
        return string.Format(format: "Metadata:{0}",
            arg0: blockHash.ToString());
    }

    public BrightenedBlock Get(BlockHash blockHash)
    {
        var dataResultTuple = this.BlockDataBlobSession.Read(key: blockHash);

        if (dataResultTuple.status != Status.OK)
        {
            throw new IndexOutOfRangeException(message: blockHash.ToString());
        }

        var result = this.SharedCacheSession.Read(key: BlockMetadataIndexKey(blockHash: blockHash));
        if (result.status == Status.NOTFOUND)
        {
            throw new IndexOutOfRangeException(message: blockHash.ToString());
        }

        if (result.status != Status.OK)
        {
            throw new BrightChainException(
                message: string.Format(format: "metadata fetch error: {0}",
                    arg0: result.status.ToString()));
        }

        if (result.output is BlockMetadataIndexValue blockMetadata)
        {
            var block = blockMetadata.Block;

            block.StoredData = dataResultTuple.output;

            if (!block.Validate())
            {
                throw new BrightChainValidationEnumerableException(exceptions: block.ValidationExceptions,
                    message: "Failed to reload block from store");
            }

            return block;
        }

        throw new BrightChainException(message: "Unexpected index result type for key");
    }

    public void Upsert(BrightenedBlock block, bool completePending = false)
    {
        var resultStatus = this.SharedCacheSession.Upsert(
            key: BlockMetadataIndexKey(blockHash: block.Id),
            desiredValue: new BlockMetadataIndexValue(block: block));

        if (resultStatus != Status.OK)
        {
            throw new BrightChainException(message: "Unable to store block");
        }

        resultStatus = this.BlockDataBlobSession.Upsert(key: block.Id,
            desiredValue: block.StoredData);
        if (resultStatus != Status.OK)
        {
            throw new BrightChainException(message: "Unable to store block");
        }

        if (completePending)
        {
            this.CompletePending(waitForCommit: false);
        }
    }

    public async Task WaitForCommitAsync()
    {
        await Task.WhenAll(this.BlockDataBlobSession.WaitForCommitAsync().AsTask(),
            this.SharedCacheSession.WaitForCommitAsync().AsTask()).ConfigureAwait(continueOnCapturedContext: false);
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
        Task.WaitAll(this.BlockDataBlobSession.CompletePendingAsync(waitForCommit: waitForCommit).AsTask(),
            this.SharedCacheSession.CompletePendingAsync(waitForCommit: waitForCommit).AsTask());
    }
}
