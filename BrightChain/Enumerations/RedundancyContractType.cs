namespace BrightChain.Enumerations
{
    /// <summary>
    /// Determines the minimum replication effort required/desired for a given block
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
