using System;

namespace BrightChain.Engine.Interfaces
{
    public interface IBlockHash : IFormattable
    {
        /// <summary>
        /// raw bytes of the hash value
        /// </summary>
        ReadOnlyMemory<byte> HashBytes { get; }
        /// <summary>
        /// Whether the hash value was computed from data or given from bytes. A computed hash is verified.
        /// </summary>
        bool Computed { get; }
    }
}
