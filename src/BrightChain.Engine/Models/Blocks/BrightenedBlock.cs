namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using System.Collections.Generic;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Extensions;
    using BrightChain.Engine.Models.Blocks.Chains;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Hashes;
    using BrightChain.Engine.Services;
    using ProtoBuf;

    /// <summary>
    /// User data that must be whitened with the block whitener before being persisted. These blocks must never be stored directly.
    /// *** CBLs are considered user data ***
    /// </summary>
    [ProtoContract]
    public class BrightenedBlock : TransactableBlock, IComparable<BrightenedBlock>, IComparable<Block>
    {
        public BrightenedBlock(TransactableBlockParams blockParams, ReadOnlyMemory<byte> data, IEnumerable<BlockHash> constituentBlockHashes)
            : base(
                blockParams: blockParams,
                data: data)
        {
            this.ConstituentBlocks = constituentBlockHashes;
            this.OriginalType = typeof(BrightenedBlock).AssemblyQualifiedName;
        }

        public override BrightenedBlock NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            throw new BrightChainExceptionImpossible("does this even make sense?");

            return new BrightenedBlock(
                blockParams: this.BlockParams,
                data: data,
                constituentBlockHashes: this.ConstituentBlocks);
        }

        public int CompareTo(BrightenedBlock other)
        {
            return this.StoredData.CompareTo(other.StoredData);
        }

        public override void Dispose()
        {
        }

        public new bool Validate()
        {
            return this.PerformValidation(out _);
        }
    }
}
