namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;

    /// <summary>
    /// Type box for the specially (fudged) root "hash".
    /// </summary>
    public class RootHash : BlockHash, IDataHash, IComparable<RootHash>, IEquatable<RootHash>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RootHash"/> class.
        /// </summary>
        /// <param name="blockSize">Size of the root block.</param>
        public RootHash(BlockSize blockSize)
            : base(blockType: typeof(RootBlock), originalBlockSize: blockSize, providedHashBytes: EmptyHashBytes(), computed: false)
        {
        }

        private static ReadOnlyMemory<byte> EmptyHashBytes()
        {
            var emptyHashBytes = new byte[BlockHash.HashSizeBytes];
            Array.Fill<byte>(emptyHashBytes, 0);
            return new ReadOnlyMemory<byte>(emptyHashBytes);
        }

        /// <summary>
        /// Compares the raw bytes of the hash.
        /// </summary>
        /// <param name="other">Other BlockHash to compare bytes with.</param>
        /// <returns>Returns a standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
        /// TODO: verify -1/1 correctness
        public int CompareTo(RootHash other)
        {
            return other.SourceDataLength == this.SourceDataLength ? ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes) : (this.SourceDataLength > other.SourceDataLength ? -1 : 1);
        }

        /// <summary>
        /// Returns a boolean whether the two objects contain the same series of bytes.
        /// </summary>
        /// <param name="other">Other BlockHash to compare bytes with.</param>
        /// <returns>Returns the standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
        public bool Equals(RootHash other)
        {
            return !(other is null) ? other.SourceDataLength == this.SourceDataLength && ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes) == 0 : false;
        }
    }
}
