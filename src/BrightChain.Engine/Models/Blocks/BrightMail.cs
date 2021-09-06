namespace BrightChain.Engine.Models.Blocks
{
    using BrightChain.Engine.Models.Entities;

    public class BrightMail : BrightBlockMessage
    {
        private readonly IEnumerable<string> Headers;
        private readonly string Body;
        private readonly IEnumerable<BrightenedBlock> Attachments;
        private readonly bool recipientBcc;

        private bool _disposedValue;
    }
}
