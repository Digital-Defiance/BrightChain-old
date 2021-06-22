namespace BrightChain.Enumerations
{
    /// <summary>
    /// Determines the minimum replication effort required/desired for a given block
    /// TODO: these were just a thought.
    /// </summary>
    public enum RedundancyContractType
    {
        LocalNone,
        LocalMirror,
        HeapAuto,
        HeapLowPriority,
        HeapHighPriority
    }
}
