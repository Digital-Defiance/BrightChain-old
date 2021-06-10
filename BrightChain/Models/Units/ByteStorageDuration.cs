namespace BrightChain.Models.Units
{
    public struct ByteStorageDuration
    {
        readonly ulong ByteCount;
        readonly ulong DurationSeconds;
        readonly double TotalCost;

        public ByteStorageDuration(ulong byteCount, ulong durationSeconds)
        {
            this.ByteCount = byteCount;
            this.DurationSeconds = durationSeconds;
            this.TotalCost = byteCount * durationSeconds;
        }
    }
}
