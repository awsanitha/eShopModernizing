using System.IO;
using System.Text.Json;

namespace eShopLegacy.Utilities
{
    /// <summary>
    /// Provides serialization utilities.
    /// Note: Migrated from BinaryFormatter (removed in .NET 5+) to System.Text.Json.
    /// The serialized format is now JSON instead of binary.
    /// </summary>
    public class Serializing
    {
        public Stream SerializeBinary(object input)
        {
            var stream = new MemoryStream();
            JsonSerializer.Serialize(stream, input, input.GetType());
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public object DeserializeBinary(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                // Return as dynamic object (JsonElement)
                return JsonSerializer.Deserialize<object>(json);
            }
        }
    }
}
