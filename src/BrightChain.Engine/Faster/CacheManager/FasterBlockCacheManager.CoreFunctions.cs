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

        private static string CblIndexKey(DataHash sourceHash)
            => string.Format("Source:{0}", sourceHash.ToString());

        public override BrightHandle GetCbl(DataHash sourceHash)
        {
            var result = this.sessionContext.CblIndicesSession.Read(CblIndexKey(sourceHash));
            if (result.status == Status.NOTFOUND)
            {
                throw new IndexOutOfRangeException(message: sourceHash.ToString());
            }
            else if (result.status != Status.OK)
            {
                throw new BrightChainException(
                    message: string.Format("cbl handle fetch error: {0}", result.status.ToString()));
            }

            if (result.output is BrightHandleIndexValue brightHandle)
            {
                return brightHandle.BrightHandle;
            }

            throw new BrightChainException("Unexpected index result type for key");
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

            this.sessionContext.CblIndicesSession.Upsert(
                key: CblIndexKey(identifiableSourceHash),
                desiredValue: new BrightHandleIndexValue(brightHandle).AsIndex);

            this.sessionContext.CompletePending(waitForCommit: false);
        }

        private static string CorrelationIndexKey(Guid correlationId)
            => string.Format("Correlation:{0}", correlationId.ToString());

        public override void UpdateCblVersion(ref ConstituentBlockListBlock newCbl, ConstituentBlockListBlock oldCbl = null)
        {
            newCbl.CorrelationId = oldCbl.CorrelationId;
            newCbl.PreviousVersionHash = oldCbl.SourceId;

            base.UpdateCblVersion(ref newCbl, oldCbl);

            this.sessionContext.CblIndicesSession.Upsert(
                key: CorrelationIndexKey(newCbl.CorrelationId),
                desiredValue: new CBLDataHashIndexValue(newCbl.SourceId).AsIndex);
        }

        public override BrightHandle GetCbl(Guid correlationId)
        {
            var key = CorrelationIndexKey(correlationId);
            var result = this.sessionContext.CblIndicesSession.Read(key);
            if (result.status == Status.NOTFOUND)
            {
                throw new IndexOutOfRangeException(message: correlationId.ToString());
            }
            else if (result.status != Status.OK)
            {
                throw new BrightChainException(
                    message: string.Format("cbl correlation fetch error: {0}", result.status.ToString()));
            }

            if (result.output is BrightHandleIndexValue brightHandle)
            {
                return brightHandle.BrightHandle;
            }

            throw new BrightChainException("Unexpected index result type for key");
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
