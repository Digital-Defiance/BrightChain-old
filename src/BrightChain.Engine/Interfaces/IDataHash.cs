namespace BrightChain.Engine.Interfaces
{
    using System;

    /// <summary>
    /// Type box interface for data hash results.
    /// </summary>
    public interface IDataHash : IFormattable
    {
        /// <summary>
        /// Size in bits of the hash.
        /// </summary>
        const int HashSize = 0;

        /// <summary>
        /// Gets the raw bytes of the hash value.
        /// </summary>
        ReadOnlyMemory<byte> HashBytes { get; }

        /// <summary>
        /// Gets a long containing the length of the source data the hash was computed on.
        /// </summary>
        public long SourceDataLength { get; }

        /// <summary>
        /// Gets a value indicating whether the hash value was computed from data or given from bytes. A computed hash is verified.
        /// </summary>
        bool Computed { get; }
    }
}
