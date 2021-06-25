using BrightChain.Models.Units;
using System;

namespace BrightChain.Models.Contracts
{
    /// <summary>
    /// Contract for the minimum amount of time required to store a given block
    /// </summary>
    public struct StorageDurationContract
    {
        public readonly DateTime RequestTime;
        public readonly DateTime KeepUntilAtLeast;
        public readonly int ByteCount;


        public StorageDurationContract(DateTime requestTime, DateTime keepUntilAtLeast, int byteCount)
        {
            this.RequestTime = requestTime;
            this.KeepUntilAtLeast = keepUntilAtLeast;
            this.ByteCount = byteCount;
        }

        public double Duration =>
            KeepUntilAtLeast.Subtract(RequestTime).TotalSeconds;

        public ByteStorageDuration ByteStorageDuration =>
            new ByteStorageDuration(
                byteCount: ByteCount,
                durationSeconds: (ulong)Duration);

        public readonly bool DoNotStore =>
            this.KeepUntilAtLeast.Equals(DateTime.MinValue);

        public readonly bool NonExpiring =>
            this.KeepUntilAtLeast.Equals(DateTime.MaxValue);

        public bool Equals(StorageDurationContract other) =>
            this.RequestTime.Equals(other.RequestTime) &&
            this.KeepUntilAtLeast.Equals(other.KeepUntilAtLeast) &&
            this.ByteCount.Equals(other.ByteCount);

        public override bool Equals(object other) =>
            other is StorageDurationContract && (StorageDurationContract)other == this;

        public static bool operator ==(StorageDurationContract a, StorageDurationContract b) =>
            a.RequestTime == b.RequestTime &&
            a.KeepUntilAtLeast == b.KeepUntilAtLeast &&
            a.ByteCount == b.ByteCount;

        public static bool operator !=(StorageDurationContract a, StorageDurationContract b) =>
            !a.Equals(b);
    }
}
