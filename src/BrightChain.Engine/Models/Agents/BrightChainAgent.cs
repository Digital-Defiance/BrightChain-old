namespace BrightChain.Engine.Models.Agents
{
    using System;
    using BrightChain.Engine.Models.Keys;

    public class BrightChainAgent
    {
        public Guid Id { get; }

        private BrightChainKey AgentKey { get; }
    }
}
