namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
    using BrightChain.Engine.Exceptions;
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
            catch(Exception _)
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

        public override BrightHandle GetCbl(DataHash sourceHash)
        {
            var result = this.sessionContext.CblSourceHashSession.Read(sourceHash);
            if (result.status == Status.NOTFOUND)
            {
                throw new IndexOutOfRangeException(message: sourceHash.ToString());
            }
            else if (result.status != Status.OK)
            {
                throw new BrightChainException(
                    message: string.Format("cbl handle fetch error: {0}", result.status.ToString()));
            }

            return result.output;
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
                block: ref block,
                completePending: false);
            this.AddExpiration(block, noCheckContains: true);
        }

        public override void SetCbl(BlockHash brightenedCblHash, DataHash identifiableSourceHash, BrightHandle brightHandle)
        {
            // technically the node can allow the CBL to be committed even if the store doesn't have the final block necessary to recreate it
            // this would be allowed in some circumstances TBD.
            // the parameter is provided as a means to check that.
            if (!brightHandle.BrightenedCblHash.Equals(brightenedCblHash))
            {
                throw new BrightChainException(nameof(brightenedCblHash));
            }

            if (!brightHandle.IdentifiableSourceHash.Equals(identifiableSourceHash))
            {
                throw new BrightChainException(nameof(identifiableSourceHash));
            }

            this.sessionContext.CblSourceHashSession.Upsert(ref identifiableSourceHash, ref brightHandle);

            this.sessionContext.CompletePending(waitForCommit: false);
        }

        public override void UpdateCblVersion(ConstituentBlockListBlock newCbl, ConstituentBlockListBlock oldCbl = null)
        {
            base.UpdateCblVersion(newCbl, oldCbl);
            var correlationId = newCbl.CorrelationId;
            newCbl.PreviousVersionHash = oldCbl.SourceId;
            var dataHash = newCbl.SourceId;
            this.sessionContext.CblCorrelationIdsSession.Upsert(ref correlationId, ref dataHash);
        }

        public override BrightHandle GetCbl(Guid correlationID)
        {
            var result = this.sessionContext.CblCorrelationIdsSession.Read(correlationID);
            if (result.status == Status.NOTFOUND)
            {
                throw new IndexOutOfRangeException(message: correlationID.ToString());
            }
            else if (result.status != Status.OK)
            {
                throw new BrightChainException(
                    message: string.Format("cbl correlation fetch error: {0}", result.status.ToString()));
            }

            return this.GetCbl(result.output);
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
