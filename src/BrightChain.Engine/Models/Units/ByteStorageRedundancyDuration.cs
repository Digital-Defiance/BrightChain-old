using BrightChain.Engine.Enumerations;

namespace BrightChain.Engine.Models.Units;

/// <summary>
///     Struct to house the fields for the RedundancyContract. Per block.
/// </summary>
public struct ByteStorageRedundancyDuration
{
    private readonly int ByteCount;
    private readonly ulong DurationSeconds;
    private readonly RedundancyContractType Redundancy;

    public ByteStorageRedundancyDuration(int byteCount, ulong durationSeconds, RedundancyContractType redundancy)
    {
        this.ByteCount = byteCount;
        this.DurationSeconds = durationSeconds;
        this.Redundancy = redundancy;
    }
}
