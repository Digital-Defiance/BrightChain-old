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
            this.OriginalAssemblyTypeString = typeof(ZeroVectorBlock).AssemblyQualifiedName;
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

        public int CompareTo(ZeroVectorBlock other)
        {
            return this.StoredData.CompareTo(other.StoredData);
        }

        public override void Dispose()
        {

        }
    }
}
