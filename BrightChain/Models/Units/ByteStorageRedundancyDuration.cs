using BrightChain.Enumerations;
namespace BrightChain.Models.Units
{
    /// <summary>
    /// Struct to house the fields for the RedundancyContract. Per block.
    /// </summary>
    public struct ByteStorageRedundancyDuration
    {
        readonly int ByteCount;
        readonly ulong DurationSeconds;
        RedundancyContractType Redundancy;

        public ByteStorageRedundancyDuration(int byteCount, ulong durationSeconds, RedundancyContractType redundancy)
        {
            this.ByteCount = byteCount;
            this.DurationSeconds = durationSeconds;
            this.Redundancy = redundancy;
        }
    }
}
