﻿namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using global::BrightChain.Engine.Attributes;
using global::BrightChain.Engine.Enumerations;
    using global::BrightChain.Engine.Exceptions;
    using global::BrightChain.Engine.Factories;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;
    using global::BrightChain.Engine.Services;

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

        public static ReadOnlyMemory<byte> SerializeObjectThroughDictionaryToMemory(T objectData, BlockHash next = null)
        {
            SerializationInfo info = new SerializationInfo(typeof(StrongNameKeyPair), new FormatterConverter());
            StreamingContext context = new StreamingContext();
            objectData.GetObjectData(info, context);

            var dictionary = new Dictionary<string, object>()
            {
                { "_t", typeof(T).FullName },
                { "Data", objectData },
                { "Next", next },
            };

            string jsonData = JsonSerializer.Serialize(dictionary, NewSerializerOptions());
            var readonlyChars = jsonData.AsMemory();
            return new ReadOnlyMemory<byte>(readonlyChars.ToArray().Select(c => (byte)c).ToArray());
        }

        public bool ValidateBlockDictionary(Dictionary<string, object> dictionary)
        {
            if (!dictionary.ContainsKey("_t") || (string)dictionary["_t"] != typeof(T).FullName)
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

        public ChainLinqObjectBlock(ChainLinqBlockParams blockParams, T blockObject, ReadOnlyMemory<byte> serializedData)
            : base(
                  blockParams: blockParams,
                  data: global::BrightChain.Engine.Helpers.RandomDataHelper.DataFiller(
                    inputData: serializedData,
                    blockSize: blockParams.BlockSize))
        {
            this._blockObject = blockObject;
            this._next = blockParams.Next;
            this._length = serializedData.Length;
        }

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
            var serialized = ChainLinqObjectBlock<T>.SerializeObjectThroughDictionaryToMemory(
                objectData: blockObject,
                next: next);

            return new ChainLinqObjectBlock<T>(
                blockParams: new Models.Blocks.DataObjects.ChainLinqBlockParams(
                    blockParams: blockParams,
                    next: next),
                serializedData: serialized,
                blockObject: blockObject);
        }

        public static BrightChain MakeChain(BrightBlockService brightBlockService, ChainLinqBlockParams blockParams, IEnumerable<T> blockObjects)
        {
            long i = 0;
            ChainLinqObjectBlock<T>[] blocks = new ChainLinqObjectBlock<T>[blockObjects.Count()];
            foreach (var blockObject in blockObjects)
            {
                blocks[i++] = MakeBlock(
                    blockParams: blockParams,
                    blockObject: blockObject,
                    next: null);
            }

            return ChainLinq<T>.BrightenAll(brightBlockService, blocks);
        }

        public override ChainLinqObjectBlock<T> NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data) =>
                new ChainLinqObjectBlock<T>(
                    blockParams: new ChainLinqBlockParams(
                        blockParams: blockParams),
                    persistedData: data);

        /// <summary>
        /// Loaded block data.
        /// </summary>
        public T BlockObject { get => this._blockObject; }

        private T _blockObject;
        private BlockHash _next;
        private long _length;

        [BrightChainBlockData]
        public BlockHash Next
        {
            get => this._next;
            set
            {
                this._next = value;
            }
        }
    }
}