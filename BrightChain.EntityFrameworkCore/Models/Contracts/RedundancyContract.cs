using BrightChain.Enumerations;
using BrightChain.Models.Blocks;
using BrightChain.Models.Units;
using System.Text.Json.Serialization;

namespace BrightChain.Models.Contracts
{
    /// <summary>
    /// Contract for the minimum amount of redundancy desired for a given block
    /// </summary>
    public struct RedundancyContract
    {
        public StorageDurationContract StorageContract { get; set; }
        public RedundancyContractType RedundancyContractType { get; set; }

        [JsonConstructor]
        public RedundancyContract(StorageDurationContract StorageContract, RedundancyContractType RedundancyContractType)
        {
            this.StorageContract = StorageContract;
            this.RedundancyContractType = RedundancyContractType;
        }

        [JsonIgnore]
        public double Cost => ComputeCost(byteCount: this.StorageContract.ByteCount,
                durationSeconds: (ulong)this.StorageContract.Duration,
                redundancy: this.RedundancyContractType);

        [JsonIgnore]
        public readonly ByteStorageRedundancyDuration RedundancyDuration => new ByteStorageRedundancyDuration(
                byteCount: this.StorageContract.ByteCount,
                durationSeconds: (ulong)this.StorageContract.Duration,
                redundancy: this.RedundancyContractType);


        public static double ComputeCost(int byteCount, ulong durationSeconds, RedundancyContractType redundancy) => ((ulong)byteCount) * durationSeconds * ByteStorageRedundancyDurationCostMap.Cost(BlockSizeMap.BlockSize(byteCount), redundancy);

        public bool Equals(RedundancyContract other) => this.StorageContract.Equals(other.StorageContract) &&
this.RedundancyContractType.Equals(this.RedundancyContractType);

        public static bool operator ==(RedundancyContract a, RedundancyContract b) => a.StorageContract.Equals(b.StorageContract) &&
a.RedundancyContractType.Equals(b.RedundancyContractType);

        public static bool operator !=(RedundancyContract a, RedundancyContract b) => !a.Equals(b);

        public override bool Equals(object obj) => (obj is RedundancyContract) &&
this.StorageContract.Equals(((RedundancyContract)obj).StorageContract) &&
this.RedundancyContractType.Equals(((RedundancyContract)obj).RedundancyContractType);

        public override int GetHashCode() => throw new System.NotImplementedException();
    }
}
