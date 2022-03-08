using System;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks;
using FASTER.core;
using NeuralFabric.Models.Hashes;
using ProtoBuf;

namespace BrightChain.Engine.Models.Hashes;

/// <summary>
///     Type box for the sha hashes.
/// </summary>
[ProtoContract]
public class BlockHash : DataHash, IDataHash, IComparable<BlockHash>, IEquatable<BlockHash>, IFasterEqualityComparer<BlockHash>
{
    /// <summary>
    ///     Size in bits of the hash.
    /// </summary>
    public new const int HashSize = 256;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockHash" /> class.
    /// </summary>
    /// <param name="block">Source block to compute data hash from.</param>
    public BlockHash(Block block)
        : base(dataBytes: block.Bytes)
    {
        if (block is not RootBlock)
        {
            var detectedSize = BlockSizeMap.BlockSize(blockSize: block.Bytes.Length);
            if (detectedSize != block.BlockSize)
            {
                throw new BrightChainValidationException(
                    element: nameof(detectedSize),
                    message: "Detected block size did not match specified block size");
            }

            this.BlockSize = block.BlockSize;
            this.BlockType = block.GetType();
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockHash" /> class.
    /// </summary>
    /// <param name="blockType">Block type of the underlying block.</param>
    /// <param name="originalBlockSize">Block size of the block the hash was computed from.</param>
    /// <param name="providedHashBytes">Hash bytes to accept as the hash.</param>
    /// <param name="computed">A boolean value indicating whether the source bytes were computed internally or externally (false).</param>
    public BlockHash(Type blockType, BlockSize originalBlockSize, ReadOnlyMemory<byte> providedHashBytes, bool computed)
        : base(providedHashBytes: providedHashBytes,
            sourceDataLength: BlockSizeMap.BlockSize(blockSize: originalBlockSize),
            computed: computed)
    {
        if (!typeof(Block).IsAssignableFrom(c: blockType))
        {
            throw new BrightChainException(message: "Block Type must be Block or descendant.");
        }

        this.BlockType = blockType;
        this.BlockSize = originalBlockSize;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockHash" /> class.
    /// </summary>
    /// <param name="blockType">Block type of the underlying block.</param>
    /// <param name="dataBytes">Data to compute hash from.</param>
    public BlockHash(Type blockType, ReadOnlyMemory<byte> dataBytes)
        : base(dataBytes: dataBytes)
    {
        this.BlockSize = BlockSizeMap.BlockSize(blockSize: dataBytes.Length);
        this.BlockType = blockType;
    }

    /// <summary>
    ///     Gets a value indicating the block type of the underlying block.
    /// </summary>
    [ProtoMember(tag: 20)]
    public Type BlockType { get; }

    /// <summary>
    ///     Gets a BlockSize enum of the source block.
    /// </summary>
    [ProtoMember(tag: 21)]
    public BlockSize BlockSize { get; }

    public string Base58 =>
        SimpleBase.Base58.Bitcoin.Encode(bytes: this.HashBytes.ToArray());

    public uint Crc32 =>
        NeuralFabric.Helpers.Crc32.ComputeChecksum(bytes: this.HashBytes.ToArray());

    public ulong Crc64 =>
        NeuralFabric.Helpers.Crc64.ComputeChecksum(bytes: this.HashBytes.ToArray());

    public string Base58Crc64 =>
        SimpleBase.Base58.Bitcoin.Encode(bytes: BitConverter.GetBytes(value: this.Crc64));

    /// <summary>
    ///     Compares the raw bytes of the hash.
    /// </summary>
    /// <param name="other">Other BlockHash to compare bytes with.</param>
    /// <returns>Returns a standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
    public int CompareTo(BlockHash other)
    {
        return other.SourceDataLength == this.SourceDataLength ? NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(
                ar1: this.HashBytes,
                ar2: other.HashBytes) :
            other.SourceDataLength > this.SourceDataLength ? -1 : 1;
    }

    /// <summary>
    ///     Returns a boolean whether the two objects contain the same series of bytes.
    /// </summary>
    /// <param name="other">Other BlockHash to compare bytes with.</param>
    /// <returns>Returns the standard comparison result, -1, 0, 1 for less than, equal, greater than.</returns>
    public bool Equals(BlockHash other)
    {
        return other.SourceDataLength == this.SourceDataLength && NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(
            ar1: this.HashBytes,
            ar2: other.HashBytes) == 0;
    }

    public long GetHashCode64(ref BlockHash k)
    {
        return (long)NeuralFabric.Helpers.Crc64.ComputeChecksum(bytes: this.HashBytes.ToArray());
    }

    public bool Equals(ref BlockHash k1, ref BlockHash k2)
    {
        return !(k2 is null)
            ? k2.SourceDataLength == k1.SourceDataLength && NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(ar1: k1.HashBytes,
                ar2: k2.HashBytes) == 0
            : false;
    }

    public static bool operator ==(BlockHash a, BlockHash b)
    {
        return a.SourceDataLength == b.SourceDataLength && NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(ar1: a.HashBytes,
            ar2: b.HashBytes) == 0;
    }

    public static bool operator ==(ReadOnlyMemory<byte> a, BlockHash b)
    {
        return a.Length == b.SourceDataLength && NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(ar1: b.HashBytes,
            ar2: a) == 0;
    }

    public static bool operator !=(ReadOnlyMemory<byte> b, BlockHash a)
    {
        return !(b == a);
    }

    public static bool operator !=(BlockHash a, BlockHash b)
    {
        return !a.Equals(other: b);
    }

    /// <summary>
    ///     Compares the raw bytes of the hash with a BlockHash classed as a plain object.
    /// </summary>
    /// <param name="obj">Should be of BlockHash type.</param>
    /// <returns>Returns a boolean indicating whether the bytes are the same in both objects.</returns>
    public override bool Equals(object obj)
    {
        return obj is BlockHash blockHash
            ? blockHash.SourceDataLength == this.SourceDataLength && NeuralFabric.Helpers.ReadOnlyMemoryComparer<byte>.Compare(
                ar1: this.HashBytes,
                ar2: blockHash.HashBytes) == 0
            : false;
    }

    public override int GetHashCode()
    {
        return (int)NeuralFabric.Helpers.Crc32.ComputeChecksum(bytes: this.HashBytes.ToArray());
    }
}
