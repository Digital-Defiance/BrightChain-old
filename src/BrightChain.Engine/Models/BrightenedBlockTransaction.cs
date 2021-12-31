using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;
using BrightChain.Engine.Services.CacheManagers.Block;

namespace BrightChain.Engine.Models;

public class BrightenedBlockTransaction
{
    public Guid Id => this.TransactionId;

    private readonly Guid TransactionId;
    private readonly BrightenedBlockCacheManagerBase CacheManager;
    private TransactionStatus TransactionState;
    
    /// <summary>
    /// Blocks that are in-memory either pending write to the cache or confirmation of no-rollback required.
    /// </summary>
    private readonly Dictionary<BlockHash, BrightenedBlock> UncommittedBlocksByHash;
    /// <summary>
    /// Hashes of concomitted blocks grouped by transactions status.
    /// </summary>
    private readonly Dictionary<TransactionStatus, List<BlockHash>> UncommittedHashesByStatus;

    public readonly Queue<BlockHash> UncomittedBlockQueue;

    public IEnumerable<BrightenedBlock> UncommittedBlocksByStatus(TransactionStatus transactionStatus) =>
        this.CacheManager.Get(keys: this.UncommittedHashesByStatus[transactionStatus].ToArray());

    public IEnumerable<BrightenedBlock> UncommittedBlocks => this.UncommittedBlocksByHash.Values;

    private int BlockReads = 0;
    private int BlockAdditions = 0;
    private int BlockUpdates = 0;
    private int BlockDrops = 0;
    private int BlockAddDrop = 0;
    private int CommittedBlocks = 0;
    private int RolledBackBlocks = 0;

    public BrightenedBlockTransaction(BrightenedBlockCacheManagerBase cacheManager)
    {
        this.TransactionId = Guid.NewGuid();
        this.CacheManager = cacheManager;
        this.TransactionState = TransactionStatus.Uncommitted;
        this.UncommittedBlocksByHash = new Dictionary<BlockHash, BrightenedBlock>();
        this.UncommittedHashesByStatus = new Dictionary<TransactionStatus, List<BlockHash>>();
        this.UncomittedBlockQueue = new Queue<BlockHash>();
    }

    public BlockHash NextBlockHash
    {
        get
        {
            return this.UncomittedBlockQueue.Dequeue();
        }
    }

    public BrightenedBlock NextBlock
    {
        get
        {
            return this.UncommittedBlocksByHash[this.NextBlockHash];
        }
    }

    public BlockHash PeekNextBlockHash
    {
        get
        {
            return this.UncomittedBlockQueue.Peek();
        }
    }

    public BrightenedBlock PeekNextBlock
    {
        get
        {
            return this.UncommittedBlocksByHash[this.PeekNextBlockHash];
        }
    }

    public void DropTransactionBlock(BlockHash blockHash)
    {
        var currentStatus = this.GetBlockStatus(blockHash: blockHash);

        if (!this.UncomittedBlockQueue.Contains(value: blockHash))
        {
            this.UncomittedBlockQueue.Append(blockHash);
        }

        if (currentStatus.HasValue && currentStatus.Value != TransactionStatus.Uncommitted)
        {
            throw new BrightChainException("Unexpected state");
        }

        this.SetBlockStatus(blockHash: blockHash, newStatus: TransactionStatus.DroppedUncommitted);
    }

    private TransactionStatus? GetBlockStatus(BlockHash blockHash)
    {
        foreach (TransactionStatus status in Enum.GetValues(typeof(TransactionStatus)))
        {
            var hashesByStatusList = this.UncommittedHashesByStatus[status];
            if (hashesByStatusList.Contains(blockHash))
            {
                return status;
            }
        }

        return null;
    }

    private void SetBlockStatus(BlockHash blockHash, TransactionStatus newStatus)
    {
        bool updated = this.UncomittedBlockQueue.Contains(value: blockHash);
        foreach (TransactionStatus status in Enum.GetValues(typeof(TransactionStatus)))
        {
            var hashesByStatusList = this.UncommittedHashesByStatus[newStatus];
            if (status == newStatus)
            {
                if (!hashesByStatusList.Contains(blockHash))
                {
                    hashesByStatusList.Add(blockHash);
                }
            }
            else if (updated)
            {
                if (hashesByStatusList.Contains(blockHash))
                {
                    hashesByStatusList.Remove(blockHash);
                }
            }
        }
    }

    public bool AddUpdateMemoryBlock(BrightenedBlock block)
    {
        bool updated = this.UncomittedBlockQueue.Contains(value: block.Id);

        if (!updated)
        {
            this.UncomittedBlockQueue.Append(block.Id);
        }

        this.UncommittedBlocksByHash[block.Id] = block;
        this.SetBlockStatus(blockHash: block.Id, TransactionStatus.Uncommitted);

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
            throw new BrightChainException("Hash not found");
        }

        return this.UncommittedBlocksByHash[blockHash];
    }

    public bool Commit()
    {
        throw new NotImplementedException();
    }
    
    public bool Rollback()
    {
        throw new NotImplementedException();
    }}
