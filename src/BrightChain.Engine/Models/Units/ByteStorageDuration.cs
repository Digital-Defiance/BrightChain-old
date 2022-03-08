namespace BrightChain.Engine.Models.Units;

/// <summary>
///     Struct to house the fields for the StorageDurationContract
/// </summary>
public struct ByteStorageDuration
{
    private readonly int ByteCount;
    private readonly ulong DurationSeconds;
    private readonly double TotalCost;

    public ByteStorageDuration(int byteCount, ulong durationSeconds)
    {
        this.ByteCount = byteCount;
        this.DurationSeconds = durationSeconds;
        this.TotalCost = (ulong)byteCount * durationSeconds;
    }
}
