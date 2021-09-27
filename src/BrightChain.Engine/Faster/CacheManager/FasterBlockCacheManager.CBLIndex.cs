namespace BrightChain.Engine.Faster.CacheManager
{
    using System;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Faster.Indices;
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using FASTER.core;

    public partial class FasterBlockCacheManager
    {
        private static string CblIndexKey(DataHash sourceHash)
        {
            return string.Format("Source:{0}", sourceHash.ToString());
        }

        public override BrightHandle GetCbl(DataHash sourceHash)
        {
            var result = this.sessionContext.SharedCacheSession.Read(CblIndexKey(sourceHash));
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

            this.sessionContext.SharedCacheSession.Upsert(
                key: CblIndexKey(identifiableSourceHash),
                desiredValue: new BrightHandleIndexValue(brightHandle).AsIndex);

            this.sessionContext.CompletePending(waitForCommit: false);
        }

        private static string CorrelationIndexKey(Guid correlationId)
        {
            return string.Format("Correlation:{0}", correlationId.ToString());
        }

        public override void UpdateCblVersion(ConstituentBlockListBlock newCbl, ConstituentBlockListBlock oldCbl = null)
        {
            newCbl.CorrelationId = oldCbl.CorrelationId;
            newCbl.PreviousVersionHash = oldCbl.SourceId;

            base.UpdateCblVersion(newCbl, oldCbl);

            this.sessionContext.SharedCacheSession.Upsert(
                key: CorrelationIndexKey(newCbl.CorrelationId),
                desiredValue: new CBLDataHashIndexValue(newCbl.SourceId).AsIndex);
        }

        public override BrightHandle GetCbl(Guid correlationId)
        {
            var key = CorrelationIndexKey(correlationId);
            var result = this.sessionContext.SharedCacheSession.Read(key);
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
    }
}
