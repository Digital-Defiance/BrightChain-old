namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using ProtoBuf;

    /// <summary>
    /// Future construct for source data blocks that are encrypted going into the network, prior to Brightening.
    /// </summary>
    [ProtoContract]
    public class PrivateEncryptedSourceBlock : SourceBlock, IBlock, IComparable<Block>, IComparable<IBlock>
    {
        public PrivateEncryptedSourceBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
            : base(blockParams, data)
        {
            if (!this.StorageContract.PrivateEncrypted)
            {
                throw new BrightChainException("BlockParams.PrivateEncrypted = false");
            }

        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override PrivateEncryptedSourceBlock NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }
    }
}
