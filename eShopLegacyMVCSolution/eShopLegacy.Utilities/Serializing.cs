using System.Text.Json;

namespace eShopLegacy.Utilities;

public class Serializing
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public Stream SerializeBinary(object input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, input, input.GetType(), Options);
        stream.Position = 0;
        return stream;
    }

    public object DeserializeBinary(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        stream.Position = 0;
        using var document = JsonDocument.Parse(stream);
        return document.RootElement.Clone();
    }
}
