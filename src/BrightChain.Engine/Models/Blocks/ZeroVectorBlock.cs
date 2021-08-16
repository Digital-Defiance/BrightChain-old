namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Models.Blocks.DataObjects;
    using ProtoBuf;

    /// <summary>
    /// Input blocks to the whitener service that consist of purely CSPRNG data of the specified block size
    /// </summary>
    [ProtoContract]
    public class ZeroVectorBlock : Block, IComparable<ZeroVectorBlock>
    {
        public static ReadOnlyMemory<byte> NewZeroVectorBlockData(BlockSize blockSize)
        {
            var zeroBytes = new byte[BlockSizeMap.BlockSize(blockSize)];
            Array.Fill<byte>(array: zeroBytes, value: 0);
            return new ReadOnlyMemory<byte>(zeroBytes);
        }

        public ZeroVectorBlock(BlockParams blockParams)
            : base(
                blockParams: blockParams,
                data: NewZeroVectorBlockData(blockParams.BlockSize))
        {
            this.OriginalType = typeof(ZeroVectorBlock).AssemblyQualifiedName;
        }

        public ZeroVectorBlock(BlockSize blockSize)
            : base(
                  blockParams: new BlockParams(
                    blockSize: blockSize,
                    requestTime: DateTime.Now,
                    keepUntilAtLeast: DateTime.MaxValue,
                    redundancy: RedundancyContractType.Unknown,
                    privateEncrypted: false,
                    originalType: typeof(ZeroVectorBlock)),
                  data: NewZeroVectorBlockData(blockSize))
        {
        }

        /// <summary>
        /// replace incoming data (will be empty byte array to fit conventions) with random data
        /// </summary>
        /// <param name="requestTime"></param>
        /// <param name="keepUntilAtLeast"></param>
        /// <param name="redundancy"></param>
        /// <param name="_"></param>
        /// <param name="allowCommit"></param>
        /// <returns></returns>
        public override Block NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> _)
        {
            return new ZeroVectorBlock(
blockParams: blockParams);
        }

        public int CompareTo(ZeroVectorBlock other)
        {
            return this.StoredData.CompareTo(other.StoredData);
        }

        public override void Dispose()
        {

        }
    }
}
