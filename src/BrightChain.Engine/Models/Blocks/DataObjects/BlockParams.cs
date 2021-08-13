namespace BrightChain.Engine.Models.Blocks.DataObjects
{
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;

    /// <summary>
    /// Simple data object for passing block parameters
    /// </summary>
    public class BlockParams
    {
        public readonly BlockSize BlockSize;

        public readonly DateTime RequestTime;

        public readonly DateTime KeepUntilAtLeast;

        public readonly RedundancyContractType Redundancy;

        public readonly bool PrivateEncrypted;

        public readonly Type OriginalType;

        public BlockParams(BlockSize blockSize, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, bool privateEncrypted, Type originalType)
        {
            this.BlockSize = blockSize;
            this.RequestTime = requestTime;
            this.KeepUntilAtLeast = keepUntilAtLeast;
            this.Redundancy = redundancy;
            this.PrivateEncrypted = privateEncrypted;
            this.OriginalType = originalType;
        }

        public BlockParams Merge(BlockParams otherBlockParams)
        {
            if (otherBlockParams.BlockSize != this.BlockSize)
            {
                throw new BrightChainException("BlockSize mismatch");
            }

            return new BlockParams(
                blockSize: this.BlockSize,
                requestTime: this.RequestTime > otherBlockParams.RequestTime ? this.RequestTime : otherBlockParams.RequestTime,
                keepUntilAtLeast: (otherBlockParams.KeepUntilAtLeast > this.KeepUntilAtLeast) ? otherBlockParams.KeepUntilAtLeast : this.KeepUntilAtLeast,
                redundancy: (otherBlockParams.Redundancy > this.Redundancy) ? otherBlockParams.Redundancy : this.Redundancy,
                privateEncrypted: this.PrivateEncrypted || otherBlockParams.PrivateEncrypted,
                originalType: this.OriginalType);
        }
    }
}
