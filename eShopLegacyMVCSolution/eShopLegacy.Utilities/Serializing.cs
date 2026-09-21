using System.IO;
using System.Xml.Serialization;

namespace eShopLegacy.Utilities
{
    public class Serializing
    {
        public Stream SerializeBinary(object input)
        {
            var stream = new MemoryStream();
            var serializer = new XmlSerializer(input.GetType());
            serializer.Serialize(stream, input);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public object? DeserializeBinary(System.Type type, Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var serializer = new XmlSerializer(type);
            return serializer.Deserialize(stream);
        }
    }
}
