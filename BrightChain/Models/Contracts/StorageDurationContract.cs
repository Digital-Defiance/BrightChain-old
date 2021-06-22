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
        public readonly ulong ByteCount;


        public StorageDurationContract(DateTime requestTime, DateTime keepUntilAtLeast, ulong byteCount)
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
    }
}
