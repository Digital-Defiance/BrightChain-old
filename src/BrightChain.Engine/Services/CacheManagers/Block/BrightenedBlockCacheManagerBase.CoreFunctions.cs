namespace BrightChain.Engine.Services.CacheManagers.Block
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.Chains;
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
        /// <param name="key">key to retrieve.</param>
        /// <returns>returns requested block or throws.</returns>
        public abstract BrightenedBlock Get(BlockHash key);

        public virtual IEnumerable<BrightenedBlock> Get(IEnumerable<BlockHash> keys)
        {
            var blocks = new List<BrightenedBlock>(capacity: keys.Count());
            foreach (var key in keys)
            {
                blocks.Add(this.Get(key));
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

        public abstract BrightHandle GetCbl(DataHash sourceHash);

        /// <summary>
        ///     Adds a key to the cache if it is not already present.
        /// </summary>
        /// <param name="value">block to palce in the cache.</param>
        public virtual void Set(BrightenedBlock value)
        {
            if (value is null)
            {
                throw new BrightChainException("Can not store null block");
            }

            if (this.Contains(value.Id))
            {
                throw new BrightChainException("Key already exists");
            }

            if (!value.Validate())
            {
                throw new BrightChainValidationEnumerableException(
                    value.ValidationExceptions,
                    "Can not store invalid block");
            }
        }

        public abstract void SetCbl(BlockHash cblHash, DataHash dataHash, BrightHandle brightHandle);

        public virtual void UpdateCblVersion(ConstituentBlockListBlock newCbl, ConstituentBlockListBlock oldCbl = null)
        {
            if (oldCbl is not null && oldCbl.CorrelationId != newCbl.CorrelationId)
            {
                throw new BrightChainException(nameof(newCbl.CorrelationId));
            }

            if (oldCbl is not null && newCbl.StorageContract.RequestTime.CompareTo(oldCbl.StorageContract.RequestTime) < 0)
            {
                throw new BrightChainException("New CBL must be newer than old CBL");
            }
        }

        public abstract BrightHandle GetCbl(Guid correlationID);

        public virtual void Set(BlockHash key, BrightenedBlock value)
        {
            if (value.Id != key)
            {
                throw new BrightChainException("Can not store transactable block with different key");
            }

            this.Set(value);
        }

        public virtual void SetAll(IEnumerable<BrightenedBlock> items)
        {
            foreach (var item in items)
            {
                this.Set(item);
            }
        }

        public async virtual void SetAllAsync(IAsyncEnumerable<BrightenedBlock> items)
        {
            await foreach (var item in items)
            {
                this.Set(item);
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
    }
}
