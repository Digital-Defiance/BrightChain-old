using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightChain.Engine.Models.Keys;

namespace BrightChain.Engine.Models.Agents
{
    public class BrightChainAgent
    {
        public object Id { get; }

        private BrightChainKey AgentKey { get; }
    }
}
