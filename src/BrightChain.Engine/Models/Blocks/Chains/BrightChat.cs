namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System;
    using System.Collections.Generic;
    using global::BrightChain.Engine.Enumerations;

    public class BrightChat : ChainLinq<BrightMessage>
    {
        public BrightChat(string subject, string body, IEnumerable<Agents.BrightChainAgent> recipientAgents)
            : base(blocks: NewChat(subject: subject, body: body, recipientAgents: recipientAgents))
        {
        }

        public BrightChat(IEnumerable<ChainLinqObjectBlock<BrightMessage>> messages)
            : base(blocks: messages)
        {
        }

        public static IEnumerable<ChainLinqObjectBlock<BrightMessage>> NewChat(
            string subject,
            string body,
            IEnumerable<Agents.BrightChainAgent> recipientAgents)
        {
            throw new NotImplementedException();
        }

        public DateTime DateCreated { get; }

        public IEnumerable<Agents.BrightChainAgent> Participants { get; }

        public IEnumerable<BrightMessage> Messages { get; }
    }
}
