namespace BrightChain.Engine.Enumerations;

public enum TransactionStatus
{
    /// <summary>
    /// This block should not be written to disk.
    /// </summary>
    DoNotWrite,

    /// <summary>
    /// This block has not been written to disk.
    /// </summary>
    Uncommitted,

    /// <summary>
    /// This block explicitly rolled back. Do not write.
    /// </summary>
    RolledBackDoNotWrite,

    /// <summary>
    /// Another block was rolled back, causing this block to need to be rewritten.
    /// </summary>
    RolledBackRewrite,

    /// <summary>
    /// Written to disk but transaction not confirmed/completed.
    /// </summary>
    WrittenUnconfirmed,

    /// <summary>
    /// Confirmed written to disk and session completed successfully.
    /// </summary>
    Committed,

    /// <summary>
    /// Confirmed removed from disk and session completed successfully. 
    /// </summary>
    DroppedCommitted,
}
