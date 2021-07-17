namespace BrightChain.Engine.Models.Blocks
{
    using System;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;

    /// <summary>
    /// Type box for the sha hashes.
    /// </summary>
    public class BlockHash : DataHash, IDataHash, IComparable<BlockHash>, IEquatable<BlockHash>
    {
        /// <summary>
        /// Size in bits of the hash.
        /// </summary>
        public new const int HashSize = 256;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockHash"/> class.
        /// </summary>
        /// <param name="block">Source block to compute data hash from.</param>
        public BlockHash(IBlock block)
            : base(dataBytes: block.Data)
        {
            this.BlockSize = BlockSizeMap.BlockSize(block.Data.Length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockHash"/> class.
        /// </summary>
        /// <param name="blockType">Block type of the underlying block.</param>
        /// <param name="originalBlockSize">Block size of the block the hash was computed from.</param>
        /// <param name="providedHashBytes">Hash bytes to accept as the hash.</param>
        /// <param name="computed">A boolean value indicating whether the source bytes were computed internally or externally (false).</param>
        public BlockHash(Type blockType, BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes, bool computed)
            : base(providedHashBytes: providedHashBytes, sourceDataLength: BlockSizeMap.BlockSize(originalBlockSize), computed: computed)
        {
            if (!blockType.Equals(typeof(Block)))
            {
                throw new BrightChainException("Block Type must be Block or descendant.");
            }
            this.BlockType = blockType;
            this.BlockSize = originalBlockSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockHash"/> class.
        /// </summary>
        /// <param name="blockType">Block type of the underlying block.</param>
        /// <param name="dataBytes">Data to compute hash from.</param>
        public BlockHash(Type blockType, ReadOnlyMemory<byte> dataBytes)
            : base(dataBytes)
        {
            this.BlockSize = BlockSizeMap.BlockSize(dataBytes.Length);
            this.BlockType = blockType;
        }

        /// <summary>
        /// Gets a value indicating the block type of the underlying block.
        /// </summary>
        public Type BlockType { get; }

        /// <summary>
        /// Gets a BlockSize enum of the source block.
        /// </summary>
        public BlockSize BlockSize { get; }

        public static bool operator ==(BlockHash a, BlockHash b) =>
             a.SourceDataLength == b.SourceDataLength && ReadOnlyMemoryComparer<byte>.Compare(a.HashBytes, b.HashBytes) == 0;

        public static bool operator ==(ReadOnlyMemory<byte> a, BlockHash b) =>
            a.Length == b.SourceDataLength && ReadOnlyMemoryComparer<byte>.Compare(b.HashBytes, a) == 0;

        public static bool operator !=(ReadOnlyMemory<byte> b, BlockHash a) =>
            !(b == a);

        public static bool operator !=(BlockHash a, BlockHash b)
            => !a.Equals(b);

        /// <summary>
        /// Compares the raw bytes of the hash with a BlockHash classed as a plain object.
        /// </summary>
        /// <param name="obj">Should be of BlockHash type.</param>
        /// <returns>Returns a boolean indicating whether the bytes are the same in both objects.</returns>
        public override bool Equals(object obj) =>
            obj is BlockHash blockHash ? blockHash.SourceDataLength == this.SourceDataLength && ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, blockHash.HashBytes) == 0 : false;

        /// <summary>
        /// Computes and returns the hash code for the HashBytes in this object.
        /// </summary>
        /// <returns>Returns the hash code for the HashBytes in this object.</returns>
        public override int GetHashCode() =>
            this.HashBytes.GetHashCode();

        /// <summary>
        /// Compares the raw bytes of the hash.
        /// </summary>
        /// <param name="other">Other BlockHash to compare bytes with.</param>
        /// <returns>Returns a standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
        public int CompareTo(BlockHash other) =>
            other.SourceDataLength == this.SourceDataLength ? ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes) : other.SourceDataLength > this.SourceDataLength ? -1 : 1;

        /// <summary>
        /// Returns a boolean whether the two objects contain the same series of bytes.
        /// </summary>
        /// <param name="other">Other BlockHash to compare bytes with.</param>
        /// <returns>Returns the standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
        public bool Equals(BlockHash other) =>
            other.SourceDataLength == this.SourceDataLength && ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes) == 0;
    }
}
