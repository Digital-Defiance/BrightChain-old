namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Faster.Indices;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;

    public partial class FasterBlockCacheManager
    {
        /// <summary>
        ///     Returns whether the cache manager has the given key and it is not expired.
        /// </summary>
        /// <param name="key">key to check the collection for.</param>
        /// <returns>boolean with whether key is present.</returns>
        public override bool Contains(BlockHash key)
        {
            return this.sessionContext.Contains(key);
        }

        /// <summary>
        ///     Removes a key from the cache and returns a boolean wither whether it was actually present.
        /// </summary>
        /// <param name="key">key to drop from the collection.</param>
        /// <param name="noCheckContains">Skips the contains check for performance.</param>
        /// <returns>whether requested key was present and actually dropped.</returns>
        public override bool Drop(BlockHash key, bool noCheckContains = true)
        {
            bool contains;
            BrightenedBlock block = null;
            try
            {
                block = this.Get(key);
                contains = true;
            }
            catch (Exception _)
            {
                contains = false;
            }

            if (!base.Drop(key, noCheckContains: true))
            {
                return false;
            }

            if (!this.sessionContext.Drop(blockHash: key, complete: true))
            {
                return false;
            }

            this.RemoveExpiration(block);

            return true;
        }

        /// <summary>
        ///     Retrieves a block from the cache if it is present.
        /// </summary>
        /// <param name="key">key to retrieve.</param>
        /// <returns>returns requested block or throws.</returns>
        public override BrightenedBlock Get(BlockHash key)
        {
            return this.sessionContext.Get(key);
        }

        /// <summary>
        ///     Adds a key to the cache if it is not already present.
        /// </summary>
        /// <param name="block">block to palce in the cache.</param>
        public void Set(BrightenedBlock block)
        {
            base.Set(block);
            block.SetCacheManager(this);
            this.sessionContext.Upsert(
                block: block,
                completePending: false);
            this.AddExpiration(block, noCheckContains: true);
        }

        public override void SetAll(IEnumerable<BrightenedBlock> items)
        {
            BrightenedBlock[] blocks = (BrightenedBlock[])items;

            for (int i = 0; i < blocks.Length; i++)
            {
                this.Set(blocks[i]);
            }

            this.sessionContext.CompletePending(waitForCommit: false);
        }

        public async override void SetAllAsync(IAsyncEnumerable<BrightenedBlock> items)
        {
            await foreach (var block in items)
            {
                this.Set(block);
            }

            await this.sessionContext.CompletePendingAsync(waitForCommit: false).ConfigureAwait(false);
        }
    }
}
