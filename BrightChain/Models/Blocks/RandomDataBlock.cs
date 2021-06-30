using BrightChain.Enumerations;
using BrightChain.Helpers;
using System;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Input blocks to the whitener service that consist of purely CSPRNG data of the specified block size
    /// </summary>
    public class RandomDataBlock : Block, IComparable<EmptyDummyBlock>
    {
        public RandomDataBlock(BlockSize blockSize, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy) :
            base(
                blockSize: blockSize,
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                redundancy: redundancy,
                data: RandomDataHelper.RandomReadOnlyBytes(BlockSizeMap.BlockSize(blockSize)))
        { }

        /// <summary>
        /// replace incoming data (will be empty byte array to fit conventions) with random data
        /// </summary>
        /// <param name="requestTime"></param>
        /// <param name="keepUntilAtLeast"></param>
        /// <param name="redundancy"></param>
        /// <param name="_"></param>
        /// <param name="allowCommit"></param>
        /// <returns></returns>
        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> _, bool allowCommit) => new RandomDataBlock(
blockSize: this.BlockSize,
requestTime: requestTime,
keepUntilAtLeast: keepUntilAtLeast,
redundancy: redundancy);

        public int CompareTo(EmptyDummyBlock other) => ReadOnlyMemoryComparer<byte>.Compare(this.Data, other.Data);

        public override void Dispose()
        {

        }
    }
}