using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdmiralsTable.Persistence.Json;

/// <summary>
/// Central System.Text.Json configuration and small file read/write helpers shared by every
/// repository. Enums are written as strings; the <see cref="MapJsonConverter"/> keeps maps compact.
/// </summary>
public static class JsonStore
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    public static void Write<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(value, Options));
    }

    public static T Read<T>(string path)
    {
        string json = File.ReadAllText(path);
        T? value = JsonSerializer.Deserialize<T>(json, Options);
        return value ?? throw new InvalidDataException($"Failed to deserialize {typeof(T).Name} from '{path}'.");
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new MapJsonConverter());
        return options;
    }
}
