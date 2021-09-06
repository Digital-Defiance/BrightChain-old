namespace BrightChain.Engine.Models.Blocks
{
    using BrightChain.Engine.Models.Entities;
    using BrightChain.Engine.Models.Hashes;

    public class BrightBlockMessage
    {
        private readonly Guid Id;
        private readonly Agent Sender;
        private readonly Agent Recipient;
        private readonly BlockHash BlockHash;
        private readonly DateTime Sent;
        private readonly DateTime? Read;
        private readonly bool Deleted;
        private readonly Guid Thread;
    }
}
