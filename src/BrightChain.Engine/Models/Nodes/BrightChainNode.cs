﻿namespace BrightChain.Engine.Models.Nodes
{
    using System.Security.Cryptography;
    using BrightChain.Engine.Models.Agents;
    using BrightChain.Engine.Models.Hashes;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Representation of a bright chain participartory node.
    /// </summary>
    public class BrightChainNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrightChainNode"/> class.
        /// </summary>
        public BrightChainNode(IConfiguration configuration)
        {
        }

        /// <summary>
        /// Gets the Id of the Node.
        /// The Id of a Node is tied to its key once the block is accepted.
        /// Duplicate Ids should not be accepted.
        /// This will be used in TrustedNode lists.
        /// </summary>
        public BlockHash Id { get; }

        /// <summary>
        /// Gets the node agent's public key. Shortcut.
        /// </summary>
        public ECDiffieHellmanCngPublicKey PublicKey =>
            this.NodeAgent.PublicKey;

        /// <summary>
        /// Entity with keys to perform actions on behalf of the node.
        /// </summary>
        public BrightChainAgent NodeAgent { get; }

        public BrightChainNodeInfo NodeInfo { get; }
    }
}
