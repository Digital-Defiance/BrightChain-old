using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;
using NeuralFabric.Models.Hashes;

namespace BrightChain.Engine.Factories;

public class HashJsonFactory : JsonConverterFactory
{
    public static JsonSerializerOptions NewSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Converters = {new HashJsonFactory()},
        };
    }

    private static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(start: 0,
                count: hex.Length)
            .Where(predicate: x => x % 2 == 0)
            .Select(selector: x => Convert.ToByte(value: hex.Substring(startIndex: x,
                    length: 2),
                fromBase: 16))
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
            return new DataHashConverter(options: options,
                isBlock: isBlock);
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

                var dataLength = this.isBlock ? reader.GetInt32() : reader.GetInt64();

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

                var stringHash = reader.GetString();

                if (this.isBlock)
                {
                    dataHash = new BlockHash(
                        blockType: typeof(Block),
                        originalBlockSize: BlockSizeMap.BlockSize(blockSize: (int)dataLength),
                        providedHashBytes: StringToByteArray(hex: stringHash),
                        computed: false);
                }
                else
                {
                    dataHash = new DataHash(
                        providedHashBytes: StringToByteArray(hex: stringHash),
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
                writer.WriteNumber(propertyName: "s",
                    value: BlockSizeMap.BlockSize(blockSize: blockHash.BlockSize));
            }
            else
            {
                writer.WriteNumber(propertyName: "l",
                    value: dataHash.SourceDataLength);
            }

            writer.WriteString(propertyName: "h",
                value: dataHash.ToString().Replace(oldValue: "-",
                    newValue: string.Empty).ToLower(culture: CultureInfo.InvariantCulture));
            writer.WriteEndObject();
        }
    }
}
