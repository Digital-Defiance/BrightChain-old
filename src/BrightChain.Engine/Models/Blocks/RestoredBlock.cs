namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using ProtoBuf;

    [ProtoContract]
    public class RestoredBlock : Block
    {
        public RestoredBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
            : base(blockParams: blockParams, data: data)
        {
        }

        public RestoredBlock(Block sourceBlock)
            : base(
                blockParams: new BlockParams(
                    blockSize: sourceBlock.BlockSize,
                    requestTime: sourceBlock.StorageContract.RequestTime,
                    keepUntilAtLeast: sourceBlock.StorageContract.KeepUntilAtLeast,
                    redundancy: sourceBlock.StorageContract.RedundancyContractType,
                    privateEncrypted: sourceBlock.StorageContract.PrivateEncrypted,
                    originalType: Type.GetType(sourceBlock.OriginalType)),
                data: sourceBlock.Bytes)
        {
        }

        public override void Dispose()
        {

        }

        public override Block NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            return new RestoredBlock(blockParams, data);
        }
    }
}
