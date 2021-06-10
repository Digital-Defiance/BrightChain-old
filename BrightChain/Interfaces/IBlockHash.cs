using System;

namespace BrightChain.Interfaces
{
    public interface IBlockHash : IFormattable
    {
        ReadOnlyMemory<byte> HashBytes { get; }
    }
}