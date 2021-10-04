namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Models.Hashes;

    public record BlockRating
    {
        public readonly Guid Id;

        public readonly BlockHash BlockId;

        public readonly Guid AgentId;

        public readonly decimal AgentReputationWeight;

        public readonly decimal Rating;
    }
}
