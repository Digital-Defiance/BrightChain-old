namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System;
    using System.Collections.Generic;
using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text.Json;
    using global::BrightChain.Engine.Attributes;
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
        where T : ISerializable
    {
        public static JsonSerializerOptions NewSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                Converters =
                {
                    new HashJsonFactory(),
                },
            };
        }

        public static ReadOnlyMemory<byte> SerializeObjectThroughDictionaryToMemory(T objectData, out int length, BlockHash next = null)
        {
            var dictionary = new Dictionary<string, object>()
            {
                { "_t", typeof(T).AssemblyQualifiedName },
                { "Data", objectData },
                { "Next", next },
            };

            string jsonData = JsonSerializer.Serialize(dictionary, NewSerializerOptions());
            var data = new ReadOnlyMemory<byte>(jsonData.Select(c => (byte)c).ToArray());
            length = data.Length;
            return data;
        }

        public bool ValidateBlockDictionary(Dictionary<string, object> dictionary)
        {
            var type = (string)dictionary["_t"];

            if (!dictionary.ContainsKey("_t") || type != typeof(T).AssemblyQualifiedName || !Block.ValidateType(type, typeof(T)))
            {
                return false;
            }

            if (!dictionary.ContainsKey("Data") || dictionary["Data"].GetType().IsAssignableFrom(this.GetType()))
            {
                return false;
            }

            if (!dictionary.ContainsKey("Previous") || (dictionary["Previous"] is not null && dictionary["Previous"] is not BlockHash))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainLinqObjectBlock{T}"/> class.
        /// TODO: are we using this?
        /// </summary>
        /// <param name="blockParams">Desired block parameters.</param>
        /// <param name="blockObject">Object serialized into this block.</param>
        /// <param name="serializedData">Data of serialized object.</param>
        /// <param name="next">Id of next block in chain.</param>
        public ChainLinqObjectBlock(ChainLinqBlockParams blockParams, T blockObject, BlockHash? next = null)
            : base(
                  blockParams: blockParams,
                  data: global::BrightChain.Engine.Helpers.RandomDataHelper.DataFiller(
                    inputData: SerializeObjectThroughDictionaryToMemory(
                        objectData: blockObject,
                        length: out int dataLength,
                        next: next),
                    blockSize: blockParams.BlockSize))
        {
            this._blockObject = blockObject;
            this._next = null;
            this._length = dataLength;
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
            this._length = persistedData.Length;

            var jsonString = new string(persistedData.ToArray().Select(c => (char)c).ToArray());
            object blockDataObject = JsonSerializer.Deserialize(jsonString, typeof(Dictionary<string, object>), NewSerializerOptions());

            Dictionary<string, object> blockDictionary = (Dictionary<string, object>)blockDataObject;
            if (!this.ValidateBlockDictionary(blockDictionary))
            {
                throw new BrightChainException("Block deserialization error");
            }

            this._blockObject = (T)blockDictionary["Data"];
            this._next = (BlockHash)blockDictionary["Next"];
        }

        public override void Dispose()
        {
            this._next = null;
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

        /// <summary>
        /// Loaded block data.
        /// </summary>
        public T BlockObject => this._blockObject;

        private readonly T _blockObject;
        private BlockHash _next;
        private readonly long _length;

        [BrightChainBlockData]
        public BlockHash Next
        {
            get => this._next;
            set => this._next = value;
        }
    }
}
