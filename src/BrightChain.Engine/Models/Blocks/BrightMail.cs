namespace BrightChain.Engine.Models.Blocks
{
    using System.Collections.Generic;
    using BrightChain.Engine.Models.Hashes;

    /// <summary>
    /// TODO: This needs a total rethink/redo. Decide whether/where to store IEnumerable<(RecipientType, BrightChainAgent)>
    /// </summary>
    public record BrightMail : BrightMessage
    {
        private readonly IEnumerable<string> Headers;
        private readonly IEnumerable<BlockHash> Attachments;
        private readonly bool recipientBcc;
    }
}
