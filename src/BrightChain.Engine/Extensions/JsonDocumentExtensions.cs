
namespace BrightChain.Engine.Extensions
{
    using System;
    using System.Buffers;
    using System.Text.Json;

    public static class JsonDocumentExtensions
    {
        #region https://stackoverflow.com/a/61047681/4009129
        public static T ToObject<T>(this JsonElement element, JsonSerializerOptions options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                element.WriteTo(writer);
            }

            return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
        }

        public static T ToObject<T>(this JsonDocument document, JsonSerializerOptions options = null)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return document.RootElement.ToObject<T>(options);
        }

        public static object ToObject(this JsonElement element, Type returnType, JsonSerializerOptions options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                element.WriteTo(writer);
            }

            return JsonSerializer.Deserialize(bufferWriter.WrittenSpan, returnType, options);
        }

        public static object ToObject(this JsonDocument document, Type returnType, JsonSerializerOptions options = null)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return document.RootElement.ToObject(returnType, options);
        }
        #endregion
    }
}
