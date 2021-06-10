using BrightChain.Enumerations;
using BrightChain.Models.Units;

namespace BrightChain.Models.Contracts
{
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

        public static double ComputeCost(ulong byteCount, ulong durationSeconds, RedundancyContractType redundancy)
        {
            // TODO: probably not a linear map. It's cheaper to store more bytes in bulk probably?
            return
                byteCount * durationSeconds * ByteStorageRedundancyDurationCostMap.Cost(redundancy);
        }
    }
}
