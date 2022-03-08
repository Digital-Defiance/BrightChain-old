using System;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Models.Blocks;

public record BlockRating
{
    public readonly Guid AgentId;

    public readonly decimal AgentReputationWeight;

    public readonly BlockHash BlockId;
    public readonly Guid Id;

    public readonly decimal Rating;
}
