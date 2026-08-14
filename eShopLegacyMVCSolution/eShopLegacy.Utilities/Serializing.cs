using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace eShopLegacy.Utilities
{
    public class Serializing
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public Stream SerializeBinary(object input)
        {
            var json = JsonConvert.SerializeObject(input, _settings);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public object DeserializeBinary(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject(json, _settings)!;
        }
    }
}
