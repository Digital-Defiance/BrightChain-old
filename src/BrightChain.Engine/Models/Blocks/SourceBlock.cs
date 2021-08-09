namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using global::BrightChain.Engine.Extensions;
    using global::BrightChain.Engine.Helpers;
    using global::BrightChain.Engine.Interfaces;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;

    /// <summary>
    /// User data that must be whitened with the block whitener before being persisted. These blocks must never be stored directly.
    /// </summary>
    public class SourceBlock
        : Block, IComparable<SourceBlock>, IComparable<Block>, IComparable<IBlock>
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
                    privateEncrypted: this.StorageContract.PrivateEncrypted,
                    originalType: Type.GetType(this.OriginalType)),
                data: data);
        }

        public int CompareTo(SourceBlock other)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(this.Data, other.Data);
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
