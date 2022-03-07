namespace BrightChain.Engine.Models.Nodes
{
    using System.Collections.Generic;
    using BrightChain.Engine.Enumerations;

    /// <summary>
    /// Data Object Model containing node statistics and features.
    /// </summary>
    public struct BrightChainNodeInfo
    {
        /// <summary>
        /// List of features this node offers to the BrightChain network.
        /// </summary>
        public readonly List<NodeFeatures> OfferedFeatures;

        /// <summary>
        /// List of features this node consumes from the BrightChain network.
        /// Anything not declared at construction cannot be consumed. It will be be broadcasted out about your node.
        /// </summary>
        public readonly List<NodeFeatures> ConsumedFeatures;

        /// <summary>
        /// List of block sizes this node supports for reading. The list may include no longer supported write sizes.
        /// </summary>
        public readonly List<BlockSize> SupportedReadBlockSizes;

        /// <summary>
        /// List of block sizes this node supports for writing.
        /// </summary>
        public readonly List<BlockSize> SupportedWriteBlockSizes;

        /// <summary>
        /// Reserved concept property to indicate either positive or negative adjustments to the public info/statistics about this node.
        /// All entries must be signed and match quorum in order to participate.
        /// </summary>
        public readonly List<object> QuorumAdjustments;

        /// <summary>
        /// Last time the node came online.
        /// </summary>
        public readonly ulong LastUptime;

        /// <summary>
        /// Numbers of unannounced disconnections without indicating stored block fate.
        /// </summary>
        public readonly ulong UnannouncedDisconnections;

        /// <summary>
        /// Number of seconds total downtime during unnannounced disconnection events.
        /// Avg flap duration = flapSeconds / unannouncedDisconnections.
        /// </summary>
        public readonly ulong FlapSeconds = 0;

        /// <summary>
        /// Number of corroborated validations performed for the network.
        /// </summary>
        public readonly ulong SuccessfulValidations;

        /// <summary>
        /// Number of validation requests from the network that were not accepted/performed/compelted.
        /// </summary>
        public readonly ulong MissedValidations;

        /// <summary>
        /// Number of validation requests from the network that were rejected and not performed.
        /// </summary>
        public readonly ulong RejectedValidations;

        /// <summary>
        /// Number of validation requests from the network that were accepted and yielded a contested result. In the event of a disagreement between the initial two, a third node will vote majority.
        /// </summary>
        public readonly ulong IncorrectValidations;
    }
}
