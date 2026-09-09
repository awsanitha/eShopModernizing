using System.IO;
using System.Text;
using System.Text.Json;

namespace eShopLegacy.Utilities
{
    public class Serializing
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes the given object to a UTF-8 JSON stream.
        /// </summary>
        public Stream SerializeBinary(object input)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(input, input.GetType(), _jsonOptions);
            return new MemoryStream(bytes);
        }

        /// <summary>
        /// Deserializes a UTF-8 JSON stream back to a dynamic object.
        /// </summary>
        public object? DeserializeBinary(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = ReadAllBytes(stream);
            return JsonSerializer.Deserialize<object>(bytes, _jsonOptions);
        }

        private static byte[] ReadAllBytes(Stream stream)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
