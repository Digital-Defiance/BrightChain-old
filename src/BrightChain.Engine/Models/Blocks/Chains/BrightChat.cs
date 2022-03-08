using System;
using System.Collections.Generic;
using BrightChain.Engine.Models.Agents;

namespace BrightChain.Engine.Models.Blocks.Chains;

public class BrightChat : ChainLinq<BrightMessage>
{
    public BrightChat(string subject, string body, IEnumerable<BrightChainAgent> recipientAgents)
        : base(blocks: NewChat(subject: subject,
            body: body,
            recipientAgents: recipientAgents))
    {
    }

    public BrightChat(IEnumerable<ChainLinqObjectBlock<BrightMessage>> messages)
        : base(blocks: messages)
    {
    }

    public DateTime DateCreated { get; }

    public IEnumerable<BrightChainAgent> Participants { get; }

    public IEnumerable<BrightMessage> Messages { get; }

    public static IEnumerable<ChainLinqObjectBlock<BrightMessage>> NewChat(
        string subject,
        string body,
        IEnumerable<BrightChainAgent> recipientAgents)
    {
        throw new NotImplementedException();
    }
}
