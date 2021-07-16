using System.Text.Json.Serialization;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Units;

namespace BrightChain.Engine.Models.Contracts
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
        public double Cost => ComputeCost(byteCount: StorageContract.ByteCount,
                durationSeconds: (ulong)StorageContract.Duration,
                redundancy: RedundancyContractType);

        [JsonIgnore]
        public readonly ByteStorageRedundancyDuration RedundancyDuration => new ByteStorageRedundancyDuration(
                byteCount: StorageContract.ByteCount,
                durationSeconds: (ulong)StorageContract.Duration,
                redundancy: RedundancyContractType);


        public static double ComputeCost(int byteCount, ulong durationSeconds, RedundancyContractType redundancy)
        {
            return ((ulong)byteCount) * durationSeconds * ByteStorageRedundancyDurationCostMap.Cost(BlockSizeMap.BlockSize(byteCount), redundancy);
        }

        public bool Equals(RedundancyContract other)
        {
            return StorageContract.Equals(other.StorageContract) &&
RedundancyContractType.Equals(RedundancyContractType);
        }

        public static bool operator ==(RedundancyContract a, RedundancyContract b)
        {
            return a.StorageContract.Equals(b.StorageContract) &&
a.RedundancyContractType.Equals(b.RedundancyContractType);
        }

        public static bool operator !=(RedundancyContract a, RedundancyContract b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return (obj is RedundancyContract) &&
StorageContract.Equals(((RedundancyContract)obj).StorageContract) &&
RedundancyContractType.Equals(((RedundancyContract)obj).RedundancyContractType);
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}
