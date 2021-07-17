using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BrightChain.Engine.Models.Agents;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Keys;

namespace BrightChain.Engine.Models.Nodes
{
    /// <summary>
    /// Representation of a bright chain participartory node.
    /// </summary>
    public class BrightChainNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrightChainNode"/> class.
        /// </summary>
        public BrightChainNode()
        {
        }

        /// <summary>
        /// Gets the Id of the Node.
        /// The Id of a Node is tied to its key once the block is accepted.
        /// Duplicate Ids should not be accepted.
        /// This will be used in TrustedNode lists.
        /// </summary>
        public BlockHash Id { get; }
        public ECDiffieHellmanCngPublicKey PublicKey { get; }

        public BrightChainAgent NodeAgent { get; }

        public BrightChainNodeInfo NodeInfo { get; }

    }
}
