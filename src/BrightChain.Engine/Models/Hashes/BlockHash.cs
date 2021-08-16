namespace BrightChain.Engine.Models.Hashes
{
    using System;
    using BrightChain.Engine.Enumerations;
    using BrightChain.Engine.Exceptions;
    using BrightChain.Engine.Helpers;
    using BrightChain.Engine.Interfaces;
    using BrightChain.Engine.Models.Blocks;
    using DamienG.Security.Cryptography;
    using FASTER.core;
    using ProtoBuf;

    /// <summary>
    /// Type box for the sha hashes.
    /// </summary>
    [ProtoContract]
    public class BlockHash : DataHash, IDataHash, IComparable<BlockHash>, IEquatable<BlockHash>, IFasterEqualityComparer<BlockHash>
    {
        /// <summary>
        /// Size in bits of the hash.
        /// </summary>
        public new const int HashSize = 256;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockHash"/> class.
        /// </summary>
        /// <param name="block">Source block to compute data hash from.</param>
        public BlockHash(Block block)
            : base(dataBytes: block.Bytes)
        {
            if (block is not RootBlock)
            {
                var detectedSize = BlockSizeMap.BlockSize(block.Bytes.Length);
                if (detectedSize != block.BlockSize)
                {
                    throw new BrightChainValidationException(
                        element: nameof(detectedSize),
                        message: "Detected block size did not match specified block size");
                }

                this.BlockSize = block.BlockSize;
            }
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
            if (!typeof(Block).IsAssignableFrom(blockType))
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
        [ProtoMember(20)]
        public Type BlockType { get; }

        /// <summary>
        /// Gets a BlockSize enum of the source block.
        /// </summary>
        [ProtoMember(21)]
        public BlockSize BlockSize { get; }

        public static bool operator ==(BlockHash a, BlockHash b)
        {
            return a.SourceDataLength == b.SourceDataLength && Helpers.ReadOnlyMemoryComparer<byte>.Compare(a.HashBytes, b.HashBytes) == 0;
        }

        public static bool operator ==(ReadOnlyMemory<byte> a, BlockHash b)
        {
            return a.Length == b.SourceDataLength && Helpers.ReadOnlyMemoryComparer<byte>.Compare(b.HashBytes, a) == 0;
        }

        public static bool operator !=(ReadOnlyMemory<byte> b, BlockHash a)
        {
            return !(b == a);
        }

        public static bool operator !=(BlockHash a, BlockHash b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Compares the raw bytes of the hash with a BlockHash classed as a plain object.
        /// </summary>
        /// <param name="obj">Should be of BlockHash type.</param>
        /// <returns>Returns a boolean indicating whether the bytes are the same in both objects.</returns>
        public override bool Equals(object obj)
        {
            return obj is BlockHash blockHash ? blockHash.SourceDataLength == this.SourceDataLength && Helpers.ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, blockHash.HashBytes) == 0 : false;
        }

        /// <summary>
        /// Compares the raw bytes of the hash.
        /// </summary>
        /// <param name="other">Other BlockHash to compare bytes with.</param>
        /// <returns>Returns a standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
        public int CompareTo(BlockHash other)
        {
            return other.SourceDataLength == this.SourceDataLength ? Helpers.ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes) : other.SourceDataLength > this.SourceDataLength ? -1 : 1;
        }

        /// <summary>
        /// Returns a boolean whether the two objects contain the same series of bytes.
        /// </summary>
        /// <param name="other">Other BlockHash to compare bytes with.</param>
        /// <returns>Returns the standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
        public bool Equals(BlockHash other)
        {
            return other.SourceDataLength == this.SourceDataLength && Helpers.ReadOnlyMemoryComparer<byte>.Compare(this.HashBytes, other.HashBytes) == 0;
        }

        public override int GetHashCode()
        {
            return (int)Crc32.ComputeNewChecksum(this.HashBytes.ToArray());
        }

        public long GetHashCode64(ref BlockHash k)
        {
            return (long)Crc64Iso.Compute(this.HashBytes.ToArray());
        }

        public bool Equals(ref BlockHash k1, ref BlockHash k2)
        {
            return !(k2 is null) ? k2.SourceDataLength == k1.SourceDataLength && Helpers.ReadOnlyMemoryComparer<byte>.Compare(k1.HashBytes, k2.HashBytes) == 0 : false;
        }
    }
}
