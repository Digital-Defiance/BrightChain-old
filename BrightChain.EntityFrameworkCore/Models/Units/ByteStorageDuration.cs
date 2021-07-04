namespace BrightChain.Models.Units
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
            this.ByteCount = byteCount;
            this.DurationSeconds = durationSeconds;
            this.TotalCost = ((ulong)byteCount) * durationSeconds;
        }
    }
}
