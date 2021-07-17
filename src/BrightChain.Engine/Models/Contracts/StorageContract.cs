using System;
using System.Text.Json.Serialization;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Units;

namespace BrightChain.Engine.Models.Contracts
{
    /// <summary>
    /// Contract for the minimum amount of time required to store a given block
    /// </summary>
    public struct StorageContract
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

        public RedundancyContractType RedundancyContractType { get; set; }


        [JsonConstructor]
        public StorageContract(DateTime RequestTime, DateTime KeepUntilAtLeast, int ByteCount, bool PrivateEncrypted, RedundancyContractType redundancyContractType)
        {
            this.RequestTime = RequestTime;
            this.KeepUntilAtLeast = KeepUntilAtLeast;
            this.ByteCount = ByteCount;
            this.PrivateEncrypted = PrivateEncrypted;
            this.RedundancyContractType = redundancyContractType;
        }

        public static bool operator ==(StorageContract a, StorageContract b) =>
            a.RequestTime == b.RequestTime &&
            a.KeepUntilAtLeast == b.KeepUntilAtLeast &&
            a.ByteCount == b.ByteCount &&
            a.PrivateEncrypted == b.PrivateEncrypted &&
            a.RedundancyContractType == b.RedundancyContractType;

        public static bool operator !=(StorageContract a, StorageContract b) => !(a == b);

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

        public bool Equals(StorageContract other) =>
            this == other;

        public override bool Equals(object other) =>
            other is StorageContract storageContract && storageContract == this;

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
