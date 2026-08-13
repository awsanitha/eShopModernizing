using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;

namespace eShopLegacy.Utilities
{
    public class Serializing
    {
        public Stream SerializeBinary(object input)
        {
            var serializer = new DataContractSerializer(input.GetType());
            var stream = new MemoryStream();
            using (var writer = XmlDictionaryWriter.CreateBinaryWriter(stream, null, null, ownsStream: false))
            {
                serializer.WriteObject(writer, input);
                writer.Flush();
            }
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public object DeserializeBinary(Stream stream)
        {
            // Without knowing the type at compile time, return the raw bytes as a fallback.
            // Callers that need typed deserialization should use the generic overload.
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new MemoryStream())
            {
                stream.CopyTo(reader);
                return reader.ToArray();
            }
        }

        public T DeserializeBinary<T>(Stream stream)
        {
            var serializer = new DataContractSerializer(typeof(T));
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
            {
                return (T)serializer.ReadObject(reader);
            }
        }
    }
}
