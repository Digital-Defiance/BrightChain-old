using System;
using System.IO;
using System.Linq;
using System.Text;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Helpers;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Hashes;
using ProtoBuf;

namespace BrightChain.Engine.Models.Blocks.Chains;

/// <summary>
///     Data container for serialization of objects into BrightChain.
/// </summary>
/// <typeparam name="T"></typeparam>
[ProtoContract]
public class ChainLinqObjectBlock<T>
    : IdentifiableBlock
    where T : new()
{
    public readonly T BlockObject;

    [ProtoMember(tag: 50)] public int ObjectDataLength;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChainLinqObjectBlock{T}" /> class.
    ///     TODO: are we using this?
    /// </summary>
    /// <param name="blockParams">Desired block parameters.</param>
    /// <param name="blockObject">Object serialized into this block.</param>
    /// <param name="next">Id of next block in chain.</param>
    public ChainLinqObjectBlock(BlockParams blockParams, T blockObject, BlockHash? next = null)
        : base(
            blockParams: blockParams,
            data: RandomDataHelper.DataFiller(
                inputData: ObjectToByteArray(
                    objectData: blockObject,
                    blockSize: blockParams.BlockSize,
                    totalLength: out var totalLength),
                blockSize: blockParams.BlockSize))
    {
        this.BlockObject = blockObject;
        this.ObjectDataLength = totalLength;
        this.Next = next;
        if (!this.ValidateOriginalType() || !this.ValidateCurrentTypeVsOriginal())
        {
            throw new BrightChainException(message: "Original type mismatch.");
        }
    }

    internal ChainLinqObjectBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        : base(blockParams: blockParams,
            data: data)
    {
        this.BlockObject = ByteArrayToObject<T>(byteArray: data.ToArray(),
            originalDataLength: this.ObjectDataLength);
    }

    /// <summary>
    ///     Gets or sets the hash of the next CBL in this CBL Chain.
    /// </summary>
    [ProtoMember(tag: 51)]
    public BlockHash Next { get; set; }

    /// <summary>
    ///     Convert an object to a Byte Array.
    /// </summary>
    public static ReadOnlyMemory<byte> ObjectToByteArray(T objectData, BlockSize blockSize, out int totalLength)
    {
        if (objectData == null)
        {
            totalLength = -1;
            return default;
        }

        var memoryStream = new MemoryStream();
        Serializer.Serialize(destination: memoryStream,
            instance: objectData);
        var finalBytes = Encoding.UTF8.GetBytes(chars: memoryStream.ToArray().Select(selector: c => (char)c).ToArray());
        if (finalBytes.Length >= BlockSizeMap.BlockSize(blockSize: blockSize))
        {
            throw new Exception(message: "Serialized data is too long for block. Use a larger block size.");
        }

        totalLength = finalBytes.Length;

        return RandomDataHelper.DataFiller(
            inputData: new ReadOnlyMemory<byte>(array: finalBytes),
            blockSize: blockSize);
    }

    public static T ByteArrayToObject(Type t, byte[] byteArray, int originalDataLength)
    {
        if (byteArray == null || !byteArray.Any())
        {
            return default;
        }

        if (originalDataLength > 0 && originalDataLength < byteArray.Length)
        {
            Array.Resize(array: ref byteArray,
                newSize: originalDataLength);
        }

        return (T)Serializer.Deserialize(type: t,
            source: new MemoryStream(buffer: byteArray));
    }

    /// <summary>
    ///     Convert a byte array to an Object of T.
    /// </summary>
    public static T ByteArrayToObject<T>(byte[] byteArray, int originalDataLength)
    {
        if (byteArray == null || !byteArray.Any())
        {
            return default;
        }

        if (originalDataLength > 0 && originalDataLength < byteArray.Length)
        {
            Array.Resize(array: ref byteArray,
                newSize: originalDataLength);
        }

        return Serializer.Deserialize<T>(source: new MemoryStream(buffer: byteArray));
    }

    public override void Dispose()
    {
    }

    public static ChainLinqObjectBlock<T> MakeBlock(BlockParams blockParams, T blockObject, BlockHash next = null)
    {
        return new ChainLinqObjectBlock<T>(
            blockParams: blockParams,
            blockObject: blockObject);
    }
}
