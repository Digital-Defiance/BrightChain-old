namespace BrightChain.Enumerations
{
    /// <summary>
    /// Determines the minimum replication effort required/desired for a given block
    /// TODO: these were just a thought.
    /// </summary>
    public enum RedundancyContractType
    {
        /// <summary>
        /// Stored on the local node only
        /// </summary>
        LocalNone,
        /// <summary>
        /// Stored locally in at least one cache
        /// </summary>
        LocalMirror,
        /// <summary>
        /// Stored in BrightChain with automatic replication based on consumption
        /// </summary>
        HeapAuto,
        /// <summary>
        /// Stored in BrightChain with automatic replication that is not as guaranteed
        /// </summary>
        HeapLowPriority,
        /// <summary>
        /// Stored in BrightChain with automatic replication at the highest priority
        /// </summary>
        HeapHighPriority
    }
}
