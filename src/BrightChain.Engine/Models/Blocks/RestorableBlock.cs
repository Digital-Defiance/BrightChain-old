namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using ProtoBuf;

    [ProtoContract]
    public class RestorableBlock : Block
    {
        public RestorableBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
            : base(blockParams: blockParams, data: data)
        {
        }

        public RestorableBlock(Block block)
            : base(
                blockParams: new BlockParams(
                    blockSize: block.BlockSize,
                    requestTime: block.StorageContract.RequestTime,
                    keepUntilAtLeast: block.StorageContract.KeepUntilAtLeast,
                    redundancy: block.StorageContract.RedundancyContractType,
                    privateEncrypted: block.StorageContract.PrivateEncrypted,
                    originalType: Type.GetType(block.OriginalType)),
                data: block.Bytes)
        {
        }

        public override void Dispose()
        {

        }

        public override Block NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            return new RestorableBlock(blockParams, data);
        }
    }
}
