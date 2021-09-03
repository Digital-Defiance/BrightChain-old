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
        public abstract BrightHandle GetCbl(DataHash sourceHash);

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
    }
}
