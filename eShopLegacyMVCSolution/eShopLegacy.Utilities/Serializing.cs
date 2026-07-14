using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;

namespace eShopLegacy.Utilities
{
    public class Serializing
    {
        /// <summary>
        /// Serializes an object to a stream using System.Text.Json (replaces BinaryFormatter which was removed in .NET 9).
        /// </summary>
        public Stream SerializeBinary(object input)
        {
            var stream = new MemoryStream();
            JsonSerializer.Serialize(stream, input, input.GetType());
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// Deserializes an object from a stream using System.Text.Json.
        /// </summary>
        public object DeserializeBinary(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
