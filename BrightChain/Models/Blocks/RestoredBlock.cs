using BrightChain.Enumerations;
using System;

namespace BrightChain.Models.Blocks
{
    public class RestoredBlock : Block
    {
        public RestoredBlock(BlockSize blockSize, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data) : base(blockSize: blockSize, requestTime, keepUntilAtLeast, redundancy, data)
        {
        }

        public override void Dispose()
        {

        }

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) => throw new NotImplementedException();
    }
}
