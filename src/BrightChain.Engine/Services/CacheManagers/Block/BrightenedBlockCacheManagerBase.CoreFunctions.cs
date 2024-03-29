﻿namespace BrightChain.Engine.Services.CacheManagers.Block
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using BrightChain.Engine.Models.Nodes;

    /// <summary>
    ///     Block Cache Manager.
    /// </summary>
    public abstract partial class BrightenedBlockCacheManagerBase : IBrightenedBlockCacheManager
    {
        /// <summary>
        ///     Returns whether the cache manager has the given key and it is not expired
        /// </summary>
        /// <param name="key">key to check the collection for</param>
        /// <returns>boolean with whether key is present</returns>
        public abstract bool Contains(BlockHash key);

        /// <summary>
        ///     Removes a key from the cache and returns a boolean wither whether it was actually present.
        /// </summary>
        /// <param name="key">Key to drop from the collection.</param>
        /// <param name="noCheckContains">Skips the contains check for performance.</param>
        /// <returns>Whether requested key was present and actually dropped.</returns>
        public virtual bool Drop(BlockHash key, bool noCheckContains = true)
        {
            if (!noCheckContains && !this.Contains(key))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Retrieves a block from the cache if it is present.
        /// </summary>
        /// <param name="blockHash">key to retrieve.</param>
        /// <returns>returns requested block or throws.</returns>
        public abstract BrightenedBlock Get(BlockHash blockHash);

        private void AddUpdateMemoryBlock(BrightenedBlock block)
        {
            var memoryHit = this.UncomittedBlocksByHash.ContainsKey(block.Id);
            var oldBlock = memoryHit ? this.UncomittedBlocksByHash[block.Id] : null;
            this.UncomittedBlocksByHash[block.Id] = block;
            this.UncommittedHashesByStatus[block.State].Add(block.Id);

            if (!memoryHit)
            {
                return;
            }

            var memoryHashesByStatusList = this.UncommittedHashesByStatus[oldBlock!.State];
            if (memoryHashesByStatusList.Contains(block.Id))
            {
                memoryHashesByStatusList.Remove(block.Id);
            }
        }

        private BrightenedBlock CacheFetchToMemory(BlockHash blockHash)
        {
            var cacheHit = this.Contains(key: blockHash);
            if (!cacheHit)
            {
                throw new BrightChainException(message: "Cache Miss");
            }

            var cacheBlock = this.Get(blockHash: blockHash);
            this.AddUpdateMemoryBlock(block: cacheBlock);

            return cacheBlock;
        }

        public virtual IEnumerable<BrightenedBlock> Get(IEnumerable<BlockHash> keys)
        {
            var blocks = new List<BrightenedBlock>();
            foreach (var key in keys)
            {
                BrightenedBlock blockData;
                if (this.UncomittedBlocksByHash.ContainsKey(key))
                {
                    blockData = this.UncomittedBlocksByHash[key];
                    this.UncommittedHashesByStatus[blockData.State].Add(key);
                }
                else
                {
                    blockData = this.Get(blockHash: key);
                }

                blocks.Add(blockData);
            }

            return blocks;
        }

        public virtual async IAsyncEnumerable<BrightenedBlock> Get(IAsyncEnumerable<BlockHash> keys)
        {
            await foreach (var key in keys)
            {
                yield return this.Get(key);
            }
        }

        /// <summary>
        ///     Adds a key to the cache if it is not already present.
        /// </summary>
        /// <param name="value">block to palce in the cache.</param>
        /// <param name="updateMetadataOnly">whether to allow duplicate and update the block metadata.</param>
        public virtual void Set(BrightenedBlock value, bool updateMetadataOnly = false)
        {
            if (value is null)
            {
                throw new BrightChainException("Can not store null block");
            }

            if (!value.Validate())
            {
                throw new BrightChainValidationEnumerableException(
                    value.ValidationExceptions,
                    "Can not store invalid block");
            }

            if (this.Contains(value.Id) && !updateMetadataOnly)
            {
                throw new BrightChainException("Key already exists in fasterkv");
            }

            if (this.UncomittedBlocksByHash.ContainsKey(value.Id) && !updateMetadataOnly)
            {
                throw new BrightChainException("Key already exists uncommitted blocks");
            }

            AddUpdateMemoryBlock(block: value);

            // TODO: place into transaction
        }

        public void ExtendStorage(BrightenedBlock block, DateTime keepUntilAtLeast, RedundancyContractType redundancy = RedundancyContractType.Unknown)
        {
            // duplicate block with extended attributes
            var newBlock = new BrightenedBlock(
                blockParams: new BrightenedBlockParams(
                    cacheManager: block.CacheManager,
                    allowCommit: block.AllowCommit,
                    blockParams: new BlockParams(
                        blockSize: block.BlockSize,
                        requestTime: block.StorageContract.RequestTime,
                        keepUntilAtLeast: keepUntilAtLeast,
                        redundancy: redundancy == RedundancyContractType.Unknown ? block.StorageContract.RedundancyContractType : redundancy,
                        privateEncrypted: block.StorageContract.PrivateEncrypted,
                        originalType: block.OriginalType)),
                data: block.Bytes,
                constituentBlockHashes: block.ConstituentBlocks);

            this.RemoveExpiration(block);
            this.AddExpiration(newBlock);
            this.Set(
                value: block,
                updateMetadataOnly: false);
        }

        public virtual void Set(BlockHash key, BrightenedBlock value)
        {
            if (value.Id != key)
            {
                throw new BrightChainException("Can not store transactable block with different key");
            }

            this.Set(
                value: value,
                updateMetadataOnly: false);
        }

        public virtual void SetAll(IEnumerable<BrightenedBlock> items)
        {
            foreach (var item in items)
            {
                this.Set(
                    value: item,
                    updateMetadataOnly: false);
            }
        }

        public async virtual void SetAllAsync(IAsyncEnumerable<BrightenedBlock> items)
        {
            await foreach (var item in items)
            {
                this.Set(
                    value: item,
                    updateMetadataOnly: false);
            }
        }

        /// <summary>
        ///     Add a node that the cache manager should trust.
        /// </summary>
        /// <param name="node">Node submitting the block to the cache.</param>
        public void Trust(BrightChainNode node)
        {
            this.trustedNodes.Add(node);
        }

        /// <summary>
        ///     Returns the maximum number of bytes storable for a given block size.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static long MaximumStorageLength(BlockSize blockSize)
        {
            var iBlockSize = BlockSizeMap.BlockSize(blockSize);
            var hashesPerBlockSquared = BlockSizeMap.HashesPerBlock(blockSize, 2);

            // right now, we can contain 1 SuperCBL with hashesPerSegment blocks, and up to hashesPerSegment blocks there.
            // this means total size is hashes^2*size
            return hashesPerBlockSquared * iBlockSize;
        }

        public IEnumerable<BrightenedBlock> UncommittedBlocksByStatus(TransactionStatus transactionStatus) =>
            this.Get(keys: this.UncommittedHashesByStatus[transactionStatus].ToArray());

        public IEnumerable<BrightenedBlock> UncommittedBlocks => this.UncomittedBlocksByHash.Values;
    }
}
