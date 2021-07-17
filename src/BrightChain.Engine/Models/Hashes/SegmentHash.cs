namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;

    /// <summary>
    /// Type box for the sha hashes.
    /// </summary>
    public class SegmentHash : DataHash, IDataHash, IComparable<SegmentHash>, IEquatable<SegmentHash>
    {
        /// <summary>
        /// Size in bits of the hash.
        /// </summary>
        public new const int HashSize = 256;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentHash"/> class.
        /// </summary>
        /// <param name="providedHashBytes">Hash bytes to accept as the hash.</param>
        /// <param name="sourceDataLength">Long indicating the length of the source the hash was computed from.</param>
        /// <param name="computed">A boolean value indicating whether the source bytes were computed internally or externally (false).</param>
        public SegmentHash(ReadOnlyMemory<byte> providedHashBytes, long sourceDataLength, bool computed)
            : base(providedHashBytes: providedHashBytes, sourceDataLength: sourceDataLength, computed: computed)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentHash"/> class.
        /// </summary>
        /// <param name="blockType">Block type of the underlying block.</param>
        /// <param name="dataBytes">Data to compute hash from.</param>
        public SegmentHash(ReadOnlyMemory<byte> dataBytes)
            : base(dataBytes)
        {
        }

        /// <summary>
        /// Compares the raw bytes of the hash.
        /// </summary>
        /// <param name="other">Other BlockHash to compare bytes with.</param>
        /// <returns>Returns a standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
        /// TODO: verify -1/1 correctness
        public int CompareTo(SegmentHash other) =>
            other.SourceDataLength == this.SourceDataLength ? ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes) : (this.SourceDataLength > other.SourceDataLength ? -1 : 1);

        /// <summary>
        /// Returns a boolean whether the two objects contain the same series of bytes.
        /// </summary>
        /// <param name="other">Other BlockHash to compare bytes with.</param>
        /// <returns>Returns the standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
        public bool Equals(SegmentHash other) =>
            !(other is null) ? other.SourceDataLength == this.SourceDataLength && ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes) == 0 : false;
    }
}
