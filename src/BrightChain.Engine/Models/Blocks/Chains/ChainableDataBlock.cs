namespace BrightChain.Engine.Models.Blocks.Chains
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using BrightChain.Engine.Attributes;
    using BrightChain.Engine.Extensions;
    using BrightChain.Engine.Factories;
    using BrightChain.Engine.Models.Blocks.DataObjects;

    public class ChainableDataBlock : TransactableBlock
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

        public static ReadOnlyMemory<byte> SerializeDictionaryToMemory(Dictionary<string, object> dictionary)
        {
            string jsonData = JsonSerializer.Serialize(dictionary, NewSerializerOptions());
            var readonlyChars = jsonData.AsMemory();
            return new ReadOnlyMemory<byte>(readonlyChars.ToArray().Select(c => (byte)c).ToArray());
        }

        public ChainableDataBlock(ChainableDataBlockParams blockParams, Dictionary<string, object> blockData)
            : base(blockParams, BrightChain.Engine.Helpers.RandomDataHelper.DataFiller(inputData: SerializeDictionaryToMemory(blockData), blockSize: blockParams.BlockSize))
        {
            this.BlockData = blockData;
            this.Previous = blockParams.Previous;
            this.Next = blockParams.Next;
        }

        public ChainableDataBlock(ChainableDataBlockParams blockParams, ReadOnlyMemory<byte> persistedData)
        {
            var jsonString = new string(persistedData.ToArray().Select(c => (char)c).ToArray());
            object blockDataObject = JsonSerializer.Deserialize(jsonString, typeof(Dictionary<string, object>), NewSerializerOptions());

            Dictionary<string, object> blockDictionary = (Dictionary<string, object>)blockDataObject;

            // now separate additional params
            List<PropertyInfo> loadableParams = new List<PropertyInfo>();
            foreach (string key in blockDictionary.Keys)
            {
                var keyProperty = this.GetType().GetProperty(key);
                var valueObject = blockDictionary[key];
                var keyValue = (valueObject is null) ? null : ((JsonElement)valueObject).ToObject(keyProperty.PropertyType, NewSerializerOptions());
                Dictionary<string, object> dataDictionary = new Dictionary<string, object>();
                foreach (PropertyInfo prop in this.GetType().GetProperties())
                {
                    if (prop.Name != key)
                    {
                        continue;
                    }

                    foreach (object attr in prop.GetCustomAttributes(true))
                    {
                        if (attr is BrightChainBlockDataAttribute)
                        {
                            var value = blockDictionary[keyProperty.Name];
                            blockDictionary.Remove(keyProperty.Name);
                            prop.SetValue(this, value);
                            loadableParams.Add(prop);
                            break;
                        }
                    }
                }
            }

            this.BlockData = blockDictionary;
        }

        public override void Dispose()
        {
            this.BlockData = null;
            this.Previous = null;
            this.Next = null;
        }

        public override ChainableDataBlock NewBlock(BlockParams blockParams, ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loaded block data.
        /// </summary>
        public Dictionary<string, object> BlockData { get; private set; }

        /// <summary>
        /// Gets or sets the BlockHash of the previous CBL in this CBL Chain.
        /// </summary>
        [BrightChainBlockData]

        public BlockHash Previous { get; private set; }

        /// <summary>
        /// Gets or sets the hash of the next CBL in this CBL Chain.
        /// </summary>
        [BrightChainBlockData]
        public BlockHash Next { get; private set; }
    }
}
