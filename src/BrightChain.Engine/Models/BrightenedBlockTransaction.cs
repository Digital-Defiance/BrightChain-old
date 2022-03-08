using System;
using System.Collections.Generic;
using System.Linq;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;
using BrightChain.Engine.Services.CacheManagers.Block;

namespace BrightChain.Engine.Models;

public class BrightenedBlockTransaction
{
    private readonly BrightenedBlockCacheManagerBase CacheManager;

    public readonly Queue<BlockHash> UncomittedBlockQueue;

    /// <summary>
    ///     Blocks that are in-memory either pending write to the cache or confirmation of no-rollback required.
    /// </summary>
    private readonly Dictionary<BlockHash, BrightenedBlock> UncommittedBlocksByHash;

    /// <summary>
    ///     Hashes of concomitted blocks grouped by transactions status.
    /// </summary>
    private readonly Dictionary<TransactionStatus, List<BlockHash>> UncommittedHashesByStatus;

    private int BlockAddDrop = 0;
    private int BlockAdditions = 0;
    private int BlockDrops = 0;

    private int BlockReads = 0;
    private int BlockUpdates = 0;
    private int CommittedBlocks = 0;
    private int RolledBackBlocks = 0;
    private TransactionStatus TransactionState;

    public BrightenedBlockTransaction(BrightenedBlockCacheManagerBase cacheManager)
    {
        this.Id = Guid.NewGuid();
        this.CacheManager = cacheManager;
        this.TransactionState = TransactionStatus.Uncommitted;
        this.UncommittedBlocksByHash = new Dictionary<BlockHash, BrightenedBlock>();
        this.UncommittedHashesByStatus = new Dictionary<TransactionStatus, List<BlockHash>>();
        this.UncomittedBlockQueue = new Queue<BlockHash>();
    }

    public Guid Id { get; }

    public IEnumerable<BrightenedBlock> UncommittedBlocks => this.UncommittedBlocksByHash.Values;

    public BlockHash NextBlockHash => this.UncomittedBlockQueue.Dequeue();

    public BrightenedBlock NextBlock => this.UncommittedBlocksByHash[key: this.NextBlockHash];

    public BlockHash PeekNextBlockHash => this.UncomittedBlockQueue.Peek();

    public BrightenedBlock PeekNextBlock => this.UncommittedBlocksByHash[key: this.PeekNextBlockHash];

    public IEnumerable<BrightenedBlock> UncommittedBlocksByStatus(TransactionStatus transactionStatus)
    {
        return this.CacheManager.Get(keys: this.UncommittedHashesByStatus[key: transactionStatus].ToArray());
    }

    public void DropTransactionBlock(BlockHash blockHash)
    {
        var currentStatus = this.GetBlockStatus(blockHash: blockHash);

        if (!this.UncomittedBlockQueue.Contains(value: blockHash))
        {
            this.UncomittedBlockQueue.Append(element: blockHash);
        }

        if (currentStatus.HasValue && currentStatus.Value != TransactionStatus.Uncommitted)
        {
            throw new BrightChainException(message: "Unexpected state");
        }

        this.SetBlockStatus(blockHash: blockHash,
            newStatus: TransactionStatus.DroppedUncommitted);
    }

    private TransactionStatus? GetBlockStatus(BlockHash blockHash)
    {
        foreach (TransactionStatus status in Enum.GetValues(enumType: typeof(TransactionStatus)))
        {
            var hashesByStatusList = this.UncommittedHashesByStatus[key: status];
            if (hashesByStatusList.Contains(item: blockHash))
            {
                return status;
            }
        }

        return null;
    }

    private void SetBlockStatus(BlockHash blockHash, TransactionStatus newStatus)
    {
        var updated = this.UncomittedBlockQueue.Contains(value: blockHash);
        foreach (TransactionStatus status in Enum.GetValues(enumType: typeof(TransactionStatus)))
        {
            var hashesByStatusList = this.UncommittedHashesByStatus[key: newStatus];
            if (status == newStatus)
            {
                if (!hashesByStatusList.Contains(item: blockHash))
                {
                    hashesByStatusList.Add(item: blockHash);
                }
            }
            else if (updated)
            {
                if (hashesByStatusList.Contains(item: blockHash))
                {
                    hashesByStatusList.Remove(item: blockHash);
                }
            }
        }
    }

    public bool AddUpdateMemoryBlock(BrightenedBlock block)
    {
        var updated = this.UncomittedBlockQueue.Contains(value: block.Id);

        if (!updated)
        {
            this.UncomittedBlockQueue.Append(element: block.Id);
        }

        this.UncommittedBlocksByHash[key: block.Id] = block;
        this.SetBlockStatus(blockHash: block.Id,
            newStatus: TransactionStatus.Uncommitted);

        return updated;
    }

    public BrightenedBlock CacheFetchToMemory(BlockHash blockHash)
    {
        var cacheHit = this.CacheManager.Contains(key: blockHash);
        if (!cacheHit)
        {
            throw new BrightChainException(message: "Cache Miss");
        }

        var cacheBlock = this.CacheManager.Get(blockHash: blockHash);
        this.AddUpdateMemoryBlock(block: cacheBlock);

        return cacheBlock;
    }

    public BrightenedBlock TransactionBlock(BlockHash blockHash)
    {
        if (!this.UncommittedBlocksByHash.ContainsKey(key: blockHash))
        {
            throw new BrightChainException(message: "Hash not found");
        }

        return this.UncommittedBlocksByHash[key: blockHash];
    }

    public bool Commit()
    {
        throw new NotImplementedException();
    }

    public bool Rollback()
    {
        throw new NotImplementedException();
    }
}
