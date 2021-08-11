using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;

namespace BrightChain.Engine.Factories
{
    public class HashJsonFactory : JsonConverterFactory
    {
        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(DataHash) || typeToConvert == typeof(BlockHash);
        }

        public override JsonConverter CreateConverter(
            Type type,
            JsonSerializerOptions options)
        {
            var isBlock = type == typeof(BlockHash);
            if (type == typeof(DataHash) || isBlock)
            {
                return new DataHashConverter(options, isBlock);
            }

            throw new Exception();
        }

        private class DataHashConverter :
           JsonConverter<DataHash>
        {
            private bool isBlock;

            public DataHashConverter(JsonSerializerOptions options, bool isBlock)
            {
                this.isBlock = isBlock;
            }

            public override DataHash Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                DataHash dataHash = null;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return dataHash;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    var firstPropertyName = reader.GetString();
                    if (firstPropertyName == "s")
                    {
                        this.isBlock = true;
                    }
                    else if (firstPropertyName != "l")
                    {
                        throw new JsonException();
                    }

                    reader.Read();

                    long dataLength = this.isBlock ? reader.GetInt32() : reader.GetInt64();

                    reader.Read();

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    var secondPropertyName = reader.GetString();
                    if (secondPropertyName != "h")
                    {
                        throw new JsonException();
                    }

                    reader.Read();

                    string stringHash = reader.GetString();

                    if (this.isBlock)
                    {
                        dataHash = new BlockHash(
                            blockType: typeof(Block),
                            originalBlockSize: BlockSizeMap.BlockSize((int)dataLength),
                            providedHashBytes: StringToByteArray(stringHash),
                            computed: false);
                    }
                    else
                    {
                        dataHash = new DataHash(
                            providedHashBytes: StringToByteArray(stringHash),
                            sourceDataLength: dataLength,
                            computed: false);
                    }
                }

                throw new JsonException();
            }

            public override void Write(
                Utf8JsonWriter writer,
                DataHash dataHash,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                if (dataHash is BlockHash blockHash)
                {
                    writer.WriteNumber("s", BlockSizeMap.BlockSize(blockHash.BlockSize));
                }
                else
                {
                    writer.WriteNumber("l", dataHash.SourceDataLength);
                }

                writer.WriteString("h", dataHash.ToString().Replace("-", string.Empty).ToLower(culture: System.Globalization.CultureInfo.InvariantCulture));
                writer.WriteEndObject();
            }
        }
    }
}
