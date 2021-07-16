using System;
using BrightChain.Engine.Helpers;
using BrightChain.Engine.Models.Blocks.DataObjects;

namespace BrightChain.Engine.Models.Blocks
{
    /// <summary>
    /// Input blocks to the whitener service that consist of purely CSPRNG data of the specified block size
    /// </summary>
    public class RandomDataBlock : Block, IComparable<EmptyDummyBlock>
    {
        public RandomDataBlock(BlockParams blockArguments) :
            base(blockArguments: blockArguments,
                data: RandomDataHelper.RandomReadOnlyBytes(BlockSizeMap.BlockSize(blockArguments.BlockSize)))
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
        public override Block NewBlock(BlockParams blockArguments, ReadOnlyMemory<byte> _)
        {
            return new RandomDataBlock(blockArguments: blockArguments);
        }

        public int CompareTo(EmptyDummyBlock other)
        {
            return ReadOnlyMemoryComparer<byte>.Compare(Data, other.Data);
        }

        public override void Dispose()
        {

        }
    }
}
