namespace BrightChain.Engine.Enumerations;

public enum TransactionStatus
{
    /// <summary>
    ///     This block should not be written to disk.
    ///     initial state, may be added to Memory dictionary only.
    /// </summary>
    DoNotWrite,

    /// <summary>
    ///     This block has not been written to disk, but should be.
    ///     Memory Dictionary only.
    /// </summary>
    Uncommitted,

    /// <summary>
    ///     This block explicitly rolled back. Do not write.
    ///     Effectively dropped. Removed from fasterKV. Memory dictionary only.
    /// </summary>
    RolledBackDoNotWrite,

    /// <summary>
    ///     Another block was rolled back, causing this block to need to be rewritten.
    ///     Memory dictionary only, pending re-add to FasterKV.
    /// </summary>
    RolledBackRewrite,

    /// <summary>
    ///     Written to disk but transaction not confirmed/completed. FasterKV + MemoryDict.
    /// </summary>
    WrittenUnconfirmed,

    /// <summary>
    ///     Confirmed written to disk and session completed successfully.
    ///     Removed from MemoryDictionary. FasterKV only.
    /// </summary>
    Committed,

    /// <summary>
    ///     Confirmed removed from disk and session completed successfully.
    ///     Removed from memory dictionary and fasterkv.
    ///     Only copies remaining should be variable references to original object.
    /// </summary>
    DroppedCommitted,

    /// <summary>
    /// </summary>
    DroppedUncommitted,
}
