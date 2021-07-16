using System;
using System.Text.Json.Serialization;
using BrightChain.Engine.Models.Units;

namespace BrightChain.Engine.Models.Contracts
{
    /// <summary>
    /// Contract for the minimum amount of time required to store a given block
    /// </summary>
    public struct StorageDurationContract
    {
        /// <summary>
        /// Date/Time the block was received by the network
        /// </summary>
        public DateTime RequestTime { get; internal set; }

        /// <summary>
        /// Minimum date the block will be preserved until
        /// </summary>
        public DateTime KeepUntilAtLeast { get; internal set; }

        /// <summary>
        /// Number of bytes stored in this block
        /// </summary>
        public int ByteCount { get; internal set; }

        /// <summary>
        /// Whether the data is being stored for public use.
        /// Factors into cost and other matters later on.
        /// </summary>
        public bool PrivateEncrypted { get; internal set; }

        [JsonConstructor]
        public StorageDurationContract(DateTime RequestTime, DateTime KeepUntilAtLeast, int ByteCount, bool PrivateEncrypted)
        {
            this.RequestTime = RequestTime;
            this.KeepUntilAtLeast = KeepUntilAtLeast;
            this.ByteCount = ByteCount;
            this.PrivateEncrypted = PrivateEncrypted;
        }

        [JsonIgnore]
        public double Duration => KeepUntilAtLeast.Subtract(RequestTime).TotalSeconds;

        [JsonIgnore]
        public ByteStorageDuration ByteStorageDuration => new ByteStorageDuration(
                byteCount: ByteCount,
                durationSeconds: (ulong)Duration);

        [JsonIgnore]
        public readonly bool DoNotStore => KeepUntilAtLeast.Equals(DateTime.MinValue);

        [JsonIgnore]
        public readonly bool NonExpiring => KeepUntilAtLeast.Equals(DateTime.MaxValue);

        public bool Equals(StorageDurationContract other)
        {
            return RequestTime.Equals(other.RequestTime) &&
KeepUntilAtLeast.Equals(other.KeepUntilAtLeast) &&
ByteCount.Equals(other.ByteCount);
        }

        public override bool Equals(object other)
        {
            return other is StorageDurationContract && (StorageDurationContract)other == this;
        }

        public static bool operator ==(StorageDurationContract a, StorageDurationContract b)
        {
            return a.RequestTime == b.RequestTime &&
a.KeepUntilAtLeast == b.KeepUntilAtLeast &&
a.ByteCount == b.ByteCount;
        }

        public static bool operator !=(StorageDurationContract a, StorageDurationContract b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
