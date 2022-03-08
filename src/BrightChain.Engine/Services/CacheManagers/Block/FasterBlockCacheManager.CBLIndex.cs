using System;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Faster.Indices;
using BrightChain.Engine.Models.Blocks.Chains;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Hashes;
using FASTER.core;
using NeuralFabric.Models.Hashes;

namespace BrightChain.Engine.Faster.CacheManager;

public partial class FasterBlockCacheManager
{
    private static string CblIndexKey(DataHash sourceHash)
    {
        return string.Format(format: "Source:{0}",
            arg0: sourceHash.ToString());
    }

    public override BrightHandle GetCbl(DataHash sourceHash)
    {
        using var sessionContext = this.NewFasterSessionContext;
        {
            var result = sessionContext.SharedCacheSession.Read(key: CblIndexKey(sourceHash: sourceHash));
            if (result.status == Status.NOTFOUND)
            {
                throw new IndexOutOfRangeException(message: sourceHash.ToString());
            }

            if (result.status != Status.OK)
            {
                throw new BrightChainException(
                    message: string.Format(format: "cbl handle fetch error: {0}",
                        arg0: result.status.ToString()));
            }

            if (result.output is BrightHandleIndexValue brightHandle)
            {
                return brightHandle.BrightHandle;
            }

            throw new BrightChainException(message: "Unexpected index result type for key");
        }
    }

    public override void SetCbl(BlockHash brightenedCblHash, DataHash identifiableSourceHash, BrightHandle brightHandle)
    {
        // technically the node can allow the CBL to be committed even if the store doesn't have the final block necessary to recreate it
        // this would be allowed in some circumstances TBD.
        // the parameter is provided as a means to check that.
        if (!brightHandle.BrightenedCblHash.Equals(other: brightenedCblHash))
        {
            throw new BrightChainException(message: nameof(brightenedCblHash));
        }

        if (!brightHandle.IdentifiableSourceHash.Equals(other: identifiableSourceHash))
        {
            throw new BrightChainException(message: nameof(identifiableSourceHash));
        }

        using var sessionContext = this.NewFasterSessionContext;
        {
            sessionContext.SharedCacheSession.Upsert(
                key: CblIndexKey(sourceHash: identifiableSourceHash),
                desiredValue: new BrightHandleIndexValue(brightHandle: brightHandle).AsIndex);

            sessionContext.CompletePending(waitForCommit: false);
        }
    }

    private static string CorrelationIndexKey(Guid correlationId)
    {
        return string.Format(format: "Correlation:{0}",
            arg0: correlationId.ToString());
    }

    public override void UpdateCblVersion(ConstituentBlockListBlock newCbl, ConstituentBlockListBlock oldCbl = null)
    {
        newCbl.CorrelationId = oldCbl.CorrelationId;
        newCbl.PreviousVersionHash = oldCbl.SourceId;

        base.UpdateCblVersion(newCbl: newCbl,
            oldCbl: oldCbl);
        using var sessionContext = this.NewFasterSessionContext;
        {
            sessionContext.SharedCacheSession.Upsert(
                key: CorrelationIndexKey(correlationId: newCbl.CorrelationId),
                desiredValue: new CBLDataHashIndexValue(dataHash: newCbl.SourceId).AsIndex);
        }
    }

    public override BrightHandle GetCbl(Guid correlationId)
    {
        using var sessionContext = this.NewFasterSessionContext;
        {
            var key = CorrelationIndexKey(correlationId: correlationId);
            var result = sessionContext.SharedCacheSession.Read(key: key);
            if (result.status == Status.NOTFOUND)
            {
                throw new IndexOutOfRangeException(message: correlationId.ToString());
            }

            if (result.status != Status.OK)
            {
                throw new BrightChainException(
                    message: string.Format(format: "cbl correlation fetch error: {0}",
                        arg0: result.status.ToString()));
            }

            if (result.output is BrightHandleIndexValue brightHandle)
            {
                return brightHandle.BrightHandle;
            }

            throw new BrightChainException(message: "Unexpected index result type for key");
        }
    }
}
