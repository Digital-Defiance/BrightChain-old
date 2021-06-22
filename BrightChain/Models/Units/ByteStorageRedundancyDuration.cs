using BrightChain.Enumerations;
namespace BrightChain.Models.Units
{
    /// <summary>
    /// Struct to house the fields for the RedundancyContract
    /// </summary>
    public struct ByteStorageRedundancyDuration
    {
        readonly ulong ByteCount;
        readonly ulong DurationSeconds;
        RedundancyContractType Redundancy;

        public ByteStorageRedundancyDuration(ulong byteCount, ulong durationSeconds, RedundancyContractType redundancy)
        {
            this.ByteCount = byteCount;
            this.DurationSeconds = durationSeconds;
            this.Redundancy = redundancy;
        }
    }
}
