using System.Collections.Generic;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Models.Blocks;

/// <summary>
///     TODO: This needs a total rethink/redo. Decide whether/where to store IEnumerable<(RecipientType, BrightChainAgent)>
/// </summary>
public record BrightMail : BrightMessage
{
    private readonly IEnumerable<BlockHash> Attachments;
    private readonly IEnumerable<string> Headers;
    private readonly bool recipientBcc;
}
