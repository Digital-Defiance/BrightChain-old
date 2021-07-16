using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Helpers;
using BrightChain.Engine.Models.Blocks.DataObjects;

namespace BrightChain.Engine.Models.Blocks
{
    /// <summary>
    /// Input blocks to the whitener service that consist of purely CSPRNG data of the specified block size
    /// </summary>
    public class EmptyDummyBlock : Block, IComparable<EmptyDummyBlock>
    {
        public static ReadOnlyMemory<byte> NewEmptyBlockData(BlockSize blockSize)
        {
            var zeroBytes = new byte[BlockSizeMap.BlockSize(blockSize)];
            Array.Fill<byte>(array: zeroBytes, value: 0);
            return new ReadOnlyMemory<byte>(zeroBytes);
        }

        public EmptyDummyBlock(BlockParams blockArguments) :
            base(
                blockArguments: blockArguments,
                data: NewEmptyBlockData(blockArguments.BlockSize))
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
            return new EmptyDummyBlock(
blockArguments: blockArguments);
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
