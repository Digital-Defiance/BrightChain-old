namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using global::BrightChain.Engine.Attributes;
    using global::BrightChain.Engine.Enumerations;
    using global::BrightChain.Engine.Exceptions;
    using global::BrightChain.Engine.Factories;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;
    using global::BrightChain.Engine.Models.Hashes;

    /// <summary>
    /// Data container for serialization of objects into BrightChain.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChainLinqObjectBlock<T>
        : SourceBlock
        where T : new()
    {
        /// <summary>
        /// Convert an object to a Byte Array.
        /// </summary>
        public static byte[] ObjectToByteArray(object objectData, BlockSize blockSize)
        {
            if (objectData == null)
            {
                return default;
            }

            var finalBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(objectData, GetJsonSerializerOptions()));
            if (finalBytes.Length >= BlockSizeMap.BlockSize(blockSize))
            {
                throw new Exception("Serialized data is too long for block. Use a larger block size.");
            }

            var newLength = finalBytes.Length + 1;
            Array.Resize(ref finalBytes, newLength);
            finalBytes[newLength - 1] = 0; // null term

            return finalBytes;
        }

        /// <summary>
        /// Convert a byte array to an Object of T.
        /// </summary>
        public static T ByteArrayToObject<T>(byte[] byteArray)
        {
            if (byteArray == null || !byteArray.Any())
            {
                return default;
            }

            var nullIndex = Array.IndexOf(byteArray, 0);
            if (nullIndex <= 0)
            {
                throw new BrightChainException("Null terminator not found");
            }

            Array.Resize(ref byteArray, nullIndex);

            return JsonSerializer.Deserialize<T>(byteArray, GetJsonSerializerOptions());
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions()
            {
                PropertyNamingPolicy = null,
                WriteIndented = false,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new HashJsonFactory(),
                },
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainLinqObjectBlock{T}"/> class.
        /// TODO: are we using this?
        /// </summary>
        /// <param name="blockParams">Desired block parameters.</param>
        /// <param name="blockObject">Object serialized into this block.</param>
        /// <param name="next">Id of next block in chain.</param>
        public ChainLinqObjectBlock(ChainLinqBlockParams blockParams, T blockObject, BlockHash? next = null)
            : base(
                  blockParams: blockParams,
                  data: global::BrightChain.Engine.Helpers.RandomDataHelper.DataFiller(
                    inputData: ObjectToByteArray(
                        objectData: blockObject,
                        blockSize: blockParams.BlockSize),
                    blockSize: blockParams.BlockSize))
        {
            this.BlockObject = blockObject;
            this.Next = next;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainLinqObjectBlock{T}"/> class.
        /// Constructor for deserialization from storage.
        /// </summary>
        /// <param name="blockParams"></param>
        /// <param name="persistedData"></param>
        public ChainLinqObjectBlock(ChainLinqBlockParams blockParams, ReadOnlyMemory<byte> persistedData)
            : base(blockParams, persistedData)
        {
            this.BlockObject = ByteArrayToObject<T>(persistedData.ToArray());
        }

        public override void Dispose()
        {
        }

        public static ChainLinqObjectBlock<T> MakeBlock(ChainLinqBlockParams blockParams, T blockObject, BlockHash next = null)
        {
            return new ChainLinqObjectBlock<T>(
                blockParams: blockParams,
                blockObject: blockObject);
        }

        public override ChainLinqObjectBlock<T> NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            return new ChainLinqObjectBlock<T>(
blockParams: new ChainLinqBlockParams(
blockParams: blockParams),
persistedData: data);
        }

        public readonly T BlockObject;

        /// <summary>
        /// Gets or sets the hash of the next CBL in this CBL Chain.
        /// </summary>
        [BrightChainMetadata]
        public BlockHash Next { get; set; }
    }
}
