namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using BrightChain.Engine.Models.Entities;

    public class EncryptedBlock : IdentifiableBlock
    {
        public readonly Agent RecipientAgent;

        public EncryptedBlock(BlockParams blockParams, Agent recipientAgent, ReadOnlyMemory<byte> encryptedData)
            : base(blockParams, encryptedData)
        {
            this.RecipientAgent = recipientAgent;
        }
    }
}
