using System.IO;
using System.Text.Json;

namespace eShopLegacy.Utilities
{
    /// <summary>
    /// Serialization utilities.
    /// BinaryFormatter has been replaced with System.Text.Json for .NET 5+ compatibility.
    /// </summary>
    public class Serializing
    {
        public Stream SerializeBinary(object input)
        {
            var stream = new MemoryStream();
            JsonSerializer.Serialize(stream, input);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public object? DeserializeBinary(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return JsonSerializer.Deserialize<object>(stream);
        }
    }
}
