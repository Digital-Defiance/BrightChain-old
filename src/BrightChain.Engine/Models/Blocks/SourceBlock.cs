using System;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Extensions;
using BrightChain.Engine.Helpers;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Services;

namespace BrightChain.Engine.Models.Blocks
{
    /// <summary>
    /// User data that must be whitened with the block whitener before being persisted. These blocks must never be stored directly.
    /// </summary>
    public class SourceBlock : Block, IComparable<SourceBlock>, IComparable<Block>, IComparable<IBlock>
    {
        public SourceBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
            : base(
                blockParams: blockParams,
                data: data)
        {
        }

        public override SourceBlock NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            return new SourceBlock(
                blockParams: new BlockParams(
                    blockSize: this.BlockSize,
                    requestTime: this.StorageContract.RequestTime,
                    keepUntilAtLeast: this.StorageContract.KeepUntilAtLeast,
                    redundancy: this.StorageContract.RedundancyContractType,
                    privateEncrypted: blockParams.PrivateEncrypted),
                data: data);
        }

        public int CompareTo(SourceBlock other)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(Data, other.Data);
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
