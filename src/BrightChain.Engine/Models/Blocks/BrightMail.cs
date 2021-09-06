namespace BrightChain.Engine.Models.Blocks
{
    using BrightChain.Engine.Models.Entities;
    using BrightChain.Engine.Models.Hashes;

    public class BrightMail : BrightBlockMessage
    {
        private readonly IEnumerable<string> Headers;
        private readonly IEnumerable<BlockHash> Attachments;
        private readonly bool recipientBcc;

        private bool _disposedValue;
    }
}
