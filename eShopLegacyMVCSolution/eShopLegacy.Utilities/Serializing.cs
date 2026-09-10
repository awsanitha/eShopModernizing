using System.IO;
using System.Text;
using System.Text.Json;

namespace eShopLegacy.Utilities
{
    public class Serializing
    {
        /// <summary>
        /// Serializes an object to a JSON stream.
        /// Replaces the legacy BinaryFormatter-based serialization which is not supported on .NET 5+.
        /// </summary>
        public Stream SerializeBinary(object input)
        {
            var json = JsonSerializer.Serialize(input);
            var bytes = Encoding.UTF8.GetBytes(json);
            return new MemoryStream(bytes);
        }

        /// <summary>
        /// Deserializes a JSON stream to an object.
        /// Replaces the legacy BinaryFormatter-based deserialization which is not supported on .NET 5+.
        /// </summary>
        public object DeserializeBinary(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var json = reader.ReadToEnd();
                return JsonSerializer.Deserialize<object>(json);
            }
        }
    }
}
