using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using BrightChain.Engine.Models.Blocks;

namespace BrightChain.Engine.Factories
{
    public class BlockHashJsonFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return (typeToConvert == typeof(BlockHash));
        }

        public override JsonConverter CreateConverter(
            Type type,
            JsonSerializerOptions options)
        {
            return new BlockHashConverter(options);
        }

        private class BlockHashConverter :
            JsonConverter<BlockHash>
        {
            public BlockHashConverter(JsonSerializerOptions options)
            {
            }

            private static byte[] StringToByteArray(string hex)
            {
                return Enumerable.Range(0, hex.Length)
.Where(x => x % 2 == 0)
.Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
.ToArray();
            }

            public override BlockHash Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                BlockHash blockHash = null;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return blockHash;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    var firstPropertyName = reader.GetString();
                    if (firstPropertyName != "s")
                    {
                        throw new JsonException();
                    }

                    reader.Read();

                    var blockSize = reader.GetInt32();

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

                    blockHash = new BlockHash(blockType: typeof(Block), originalBlockSize: BlockSizeMap.BlockSize(blockSize), providedHashBytes: StringToByteArray(stringHash), computed: false);
                }

                throw new JsonException();
            }

            public override void Write(
                Utf8JsonWriter writer,
                BlockHash blockHash,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteNumber("s", BlockSizeMap.BlockSize(blockHash.BlockSize));
                writer.WriteString("h", blockHash.ToString().Replace("-", "").ToLower(culture: System.Globalization.CultureInfo.InvariantCulture));
                writer.WriteEndObject();
            }
        }
    }
}
