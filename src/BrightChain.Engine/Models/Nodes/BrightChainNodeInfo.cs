namespace BrightChain.Engine.Models.Nodes
{
    using System.Collections.Generic;
    using BrightChain.Engine.Enumerations;

    /// <summary>
    ///     Data Object Model containing node statistics and features.
    /// </summary>
    public record BrightChainNodeInfo(List<NodeFeatures> OfferedFeatures, List<NodeFeatures> ConsumedFeatures,
        List<BlockSize> SupportedReadBlockSizes, List<BlockSize> SupportedWriteBlockSizes, List<object> QuorumAdjustments, ulong LastUptime,
        ulong UnannouncedDisconnections, ulong FlapSeconds, ulong SuccessfulValidations, ulong MissedValidations, ulong RejectedValidations,
        ulong IncorrectValidations)
    {
    }
}
