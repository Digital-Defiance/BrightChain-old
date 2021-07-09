using BrightChain.Enumerations;
using BrightChain.Interfaces;
using System;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Type box for the sha hashes
    /// </summary>
    public class BlockSignature : DataSignature, IDataSignature, IComparable<BlockSignature>
    {
        public BlockSignature(IBlock block) : base(block)
        {
        }

        public BlockSignature(ReadOnlyMemory<byte> dataBytes) : base(dataBytes)
        {
        }

        public BlockSignature(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes) : base(originalBlockSize, providedHashBytes)
        {
        }

        internal BlockSignature(BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes, bool computed = false) : base(originalBlockSize, providedHashBytes, computed)
        {
        }

        public int CompareTo(BlockSignature other) => throw new NotImplementedException();
    }
}