using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using AdmiralsTable.Domain.Maps;

namespace AdmiralsTable.Persistence.Json;

/// <summary>
/// Serializes a <see cref="Map"/> compactly as { rows, columns, specialHexes } and rebuilds it via
/// <see cref="Map.Build"/> on read. This avoids persisting the (fully derivable) hex/adjacency array,
/// which would be enormous for large maps.
/// </summary>
public sealed class MapJsonConverter : JsonConverter<Map>
{
    public override Map Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        int rows = root.GetProperty("rows").GetInt32();
        int columns = root.GetProperty("columns").GetInt32();

        Dictionary<int, IEnumerable<string>>? specials = null;
        if (root.TryGetProperty("specialHexes", out var sh) && sh.ValueKind == JsonValueKind.Object)
        {
            specials = new Dictionary<int, IEnumerable<string>>();
            foreach (var prop in sh.EnumerateObject())
            {
                int index = int.Parse(prop.Name, CultureInfo.InvariantCulture);
                var tags = prop.Value.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToList();
                specials[index] = tags;
            }
        }

        return Map.Build(rows, columns, specials);
    }

    public override void Write(Utf8JsonWriter writer, Map map, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("rows", map.Rows);
        writer.WriteNumber("columns", map.Columns);

        writer.WritePropertyName("specialHexes");
        writer.WriteStartObject();
        foreach (var hex in map.Hexes)
        {
            if (hex.Features.Count == 0)
            {
                continue;
            }

            writer.WritePropertyName(hex.Index.ToString(CultureInfo.InvariantCulture));
            writer.WriteStartArray();
            foreach (var feature in hex.Features)
            {
                writer.WriteStringValue(feature);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
