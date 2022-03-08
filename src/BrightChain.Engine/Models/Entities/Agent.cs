using System;
using System.Collections.Generic;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Keys;

namespace BrightChain.Engine.Models.Entities;

/// <summary>
///     A user in the BrightChain network.
///     Likely to change.
/// </summary>
public class Agent
{
    public Guid Id { get; }

    public BrightChainKey Key { get; }

    public Block[] PublicBlocks { get; }

    public Block[] PrivateBlocks { get; }

    public Dictionary<(string, BrightMailBoxType, BrightMessageType), IEnumerable<BrightMessage>> Mailbox { get; }
}
