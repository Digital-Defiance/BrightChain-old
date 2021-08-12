namespace BrightChain.Engine.Models.Contracts
{
    using System.Text.Json.Serialization;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Models.Units;

    /// <summary>
    /// Contract for the minimum amount of time required to store a given block.
    /// </summary>
    [Serializable]
    public struct StorageContract
    {
        /// <summary>
        /// Gets the Date/Time the block was received by the network.
        /// </summary>
        public DateTime RequestTime { get; internal set; }

        /// <summary>
        /// Gets the Minimum date the block will be preserved until.
        /// </summary>
        public DateTime KeepUntilAtLeast { get; internal set; }

        /// <summary>
        /// Gets the Number of bytes stored in this block.
        /// </summary>
        public int ByteCount { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the data is being stored for public use.
        /// Factors into cost and other matters later on.
        /// </summary>
        public bool PrivateEncrypted { get; internal set; }

        /// <summary>
        /// Gets the contracted durability requirements.
        /// </summary>
        public RedundancyContractType RedundancyContractType { get; internal set; }

        public StorageContract(DateTime RequestTime, DateTime KeepUntilAtLeast, int ByteCount, bool PrivateEncrypted, RedundancyContractType redundancyContractType)
        {
            this.RequestTime = RequestTime;
            this.KeepUntilAtLeast = KeepUntilAtLeast;
            this.ByteCount = ByteCount;
            this.PrivateEncrypted = PrivateEncrypted;
            this.RedundancyContractType = redundancyContractType;
        }

        public static bool operator ==(StorageContract a, StorageContract b)
        {
            return a.RequestTime == b.RequestTime &&
                a.KeepUntilAtLeast == b.KeepUntilAtLeast &&
                a.ByteCount == b.ByteCount &&
                a.PrivateEncrypted == b.PrivateEncrypted &&
                a.RedundancyContractType == b.RedundancyContractType;
        }

        public static bool operator !=(StorageContract a, StorageContract b)
        {
            return !(a == b);
        }

        public double Duration => this.KeepUntilAtLeast.Subtract(this.RequestTime).TotalSeconds;

        public ByteStorageDuration ByteStorageDuration => new ByteStorageDuration(
                byteCount: this.ByteCount,
                durationSeconds: (ulong)this.Duration);

        public readonly bool DoNotStore => this.KeepUntilAtLeast.Equals(DateTime.MinValue);

        public readonly bool NonExpiring => this.KeepUntilAtLeast.Equals(DateTime.MaxValue);

        public bool Equals(StorageContract other)
        {
            return this == other;
        }

        public override bool Equals(object other)
        {
            return other is StorageContract storageContract && storageContract == this;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
