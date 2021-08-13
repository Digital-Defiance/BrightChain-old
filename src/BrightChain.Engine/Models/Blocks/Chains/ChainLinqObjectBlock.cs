namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Linq;
    using System.Text;
    using global::BrightChain.Engine.Enumerations;
    using global::BrightChain.Engine.Exceptions;
    using global::BrightChain.Engine.Helpers;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;
    using global::BrightChain.Engine.Models.Hashes;
    using ProtoBuf;

    /// <summary>
    /// Data container for serialization of objects into BrightChain.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ProtoContract]
    public class ChainLinqObjectBlock<T>
        : SourceBlock
        where T : new()
    {
        /// <summary>
        /// Convert an object to a Byte Array.
        /// </summary>
        public static ReadOnlyMemory<byte> ObjectToByteArray(T objectData, BlockSize blockSize, out int totalLength)
        {
            if (objectData == null)
            {
                totalLength = -1;
                return default;
            }

            var memoryStream = new MemoryStream();
            Serializer.Serialize<T>(destination: memoryStream, instance: objectData);
            memoryStream.Position = 0;
            var finalBytes = Encoding.UTF8.GetBytes(chars: memoryStream.GetBuffer().Select(c => (char)c).ToArray());
            if (finalBytes.Length >= BlockSizeMap.BlockSize(blockSize))
            {
                throw new Exception("Serialized data is too long for block. Use a larger block size.");
            }

            totalLength = finalBytes.Length;

            return Helpers.RandomDataHelper.DataFiller(
                inputData: new ReadOnlyMemory<byte>(finalBytes),
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
                Array.Resize(ref byteArray, originalDataLength);
            }

            return (T)Serializer.Deserialize(type: t, new MemoryStream(byteArray));
        }

        /// <summary>
        /// Convert a byte array to an Object of T.
        /// </summary>
        public static T ByteArrayToObject<T>(byte[] byteArray, int originalDataLength)
        {
            if (byteArray == null || !byteArray.Any())
            {
                return default;
            }

            if (originalDataLength > 0 && originalDataLength < byteArray.Length)
            {
                Array.Resize(ref byteArray, originalDataLength);
            }

            return Serializer.Deserialize<T>(new MemoryStream(byteArray));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainLinqObjectBlock{T}"/> class.
        /// TODO: are we using this?
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
                        totalLength: out int totalLength),
                    blockSize: blockParams.BlockSize))
        {
            this.BlockObject = blockObject;
            this.ObjectDataLength = totalLength;
            this.Next = next;
            if (this.OriginalType != this.GetType().AssemblyQualifiedName)
            {
                throw new BrightChainException("Original type mismatch.");
            }
        }

        internal ChainLinqObjectBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
            : base(blockParams: blockParams, data: data)
        {
            this.BlockObject = ByteArrayToObject<T>(data.ToArray(), this.ObjectDataLength);
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

        public override ChainLinqObjectBlock<T> NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }

        public readonly T BlockObject;

        [ProtoMember(50)]
        public int ObjectDataLength;

        /// <summary>
        /// Gets or sets the hash of the next CBL in this CBL Chain.
        /// </summary>
        [ProtoMember(51)]
        public BlockHash Next { get; set; }
    }
}
