using BrightChain.Enumerations;
using System;

namespace BrightChain.Models.Blocks.DataObjects
{
    /// <summary>
    /// Simple data object for passing block parameters
    /// </summary>
    public class BlockParams
    {
        public readonly BlockSize BlockSize;
        public readonly DateTime RequestTime;
        public readonly DateTime KeepUntilAtLeast;
        public readonly RedundancyContractType Redundancy;
        public readonly bool AllowCommit;
        public readonly bool PrivateEncrypted;

        public BlockParams(BlockSize blockSize, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, bool allowCommit, bool privateEncrypted)
        {
            this.BlockSize = blockSize;
            this.RequestTime = requestTime;
            this.KeepUntilAtLeast = keepUntilAtLeast;
            this.Redundancy = redundancy;
            this.AllowCommit = allowCommit;
            this.PrivateEncrypted = privateEncrypted;
        }
    }
}
