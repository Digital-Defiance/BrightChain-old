using BrightChain.Enumerations;
using BrightChain.Models.Blocks;
using BrightChain.Models.Units;

namespace BrightChain.Models.Contracts
{
    /// <summary>
    /// Contract for the minimum amount of redundancy desired for a given block
    /// </summary>
    public struct RedundancyContract
    {
        public readonly StorageDurationContract StorageContract;
        public readonly RedundancyContractType RedundancyContractType;

        public RedundancyContract(StorageDurationContract storageDurationContract, RedundancyContractType redundancy)
        {
            this.StorageContract = storageDurationContract;
            this.RedundancyContractType = redundancy;
        }

        public double Cost =>
            ComputeCost(byteCount: this.StorageContract.ByteCount,
                durationSeconds: (ulong)this.StorageContract.Duration,
                redundancy: this.RedundancyContractType);

        public readonly ByteStorageRedundancyDuration RedundancyDuration =>
            new ByteStorageRedundancyDuration(
                byteCount: this.StorageContract.ByteCount,
                durationSeconds: (ulong)this.StorageContract.Duration,
                redundancy: this.RedundancyContractType);

        public static double ComputeCost(int byteCount, ulong durationSeconds, RedundancyContractType redundancy)
        {
            return
                ((ulong)byteCount) * durationSeconds * ByteStorageRedundancyDurationCostMap.Cost(BlockSizeMap.BlockSize(byteCount), redundancy);
        }

        public bool Equals(RedundancyContract other) =>
            this.StorageContract.Equals(other.StorageContract) &&
            this.RedundancyContractType == other.RedundancyContractType;

        public static bool operator ==(RedundancyContract a, RedundancyContract b) =>
            a.StorageContract == b.StorageContract &&
            a.RedundancyContractType == b.RedundancyContractType;

        public static bool operator !=(RedundancyContract a, RedundancyContract b) =>
            !(a == b);
    }
}
