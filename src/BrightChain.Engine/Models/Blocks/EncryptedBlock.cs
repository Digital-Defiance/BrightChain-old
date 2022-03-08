using System;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Entities;

namespace BrightChain.Engine.Models.Blocks;

public class EncryptedBlock : IdentifiableBlock
{
    public readonly Agent RecipientAgent;

    public EncryptedBlock(BlockParams blockParams, Agent recipientAgent, ReadOnlyMemory<byte> encryptedData)
        : base(blockParams: blockParams,
            data: encryptedData)
    {
        this.RecipientAgent = recipientAgent;
    }
}
