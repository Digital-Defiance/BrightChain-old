namespace BrightChain.Engine.Models.Units
{
    /// <summary>
    /// Struct to house the fields for the StorageDurationContract
    /// </summary>
    public struct ByteStorageDuration
    {
        readonly int ByteCount;
        readonly ulong DurationSeconds;
        readonly double TotalCost;

        public ByteStorageDuration(int byteCount, ulong durationSeconds)
        {
            ByteCount = byteCount;
            DurationSeconds = durationSeconds;
            TotalCost = ((ulong)byteCount) * durationSeconds;
        }
    }
}
