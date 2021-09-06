namespace BrightChain.Engine.Models.Blocks
{
    using BrightChain.Engine.Models.Entities;

    public class BrightMessage
    {
        private readonly Guid Id;
        private readonly Agent Sender;
        private readonly Agent Recipient;
        private readonly string Body;
        private readonly DateTime Sent;
        private readonly DateTime? Read;
        private readonly bool Deleted;
        private readonly Guid Thread;
    }
}
