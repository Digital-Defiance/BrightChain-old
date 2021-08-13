namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Hashes;
    using ProtoBuf;

    /// <summary>
    /// Type box for the sha hashes.
    /// </summary>
    [ProtoContract]
    public class BlockSignature : DataSignature, IDataSignature, IComparable<BlockSignature>
    {
        public BlockSignature(IBlock block)
            : base(block)
        {
        }

        public BlockSignature(ReadOnlyMemory<byte> dataBytes)
            : base(dataBytes)
        {
        }

        public BlockSignature(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes)
            : base(originalBlockSize, providedHashBytes)
        {
        }

        internal BlockSignature(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes, bool computed = false)
            : base(originalBlockSize, providedHashBytes, computed)
        {
        }

        public int CompareTo(BlockSignature other)
        {
            throw new NotImplementedException();
        }
    }
}
