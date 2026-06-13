using AdmiralsTable.Domain.Ships;
using AdmiralsTable.Persistence.Csv;
using AdmiralsTable.Persistence.Json;

namespace AdmiralsTable.Persistence.Repositories;

/// <summary>The shared ship-template library (SHIP ANNEX), keyed by <see cref="ShipTemplate.Type"/>.</summary>
public interface IShipTemplateRepository
{
    IReadOnlyList<ShipTemplate> List();
    ShipTemplate? Get(string type);
    void Save(ShipTemplate template);
    void Delete(string type);
    IReadOnlyList<ShipTemplate> ImportCsv(Stream csv);
}

/// <summary>One JSON file per template under <c>ships/templates/</c>.</summary>
public sealed class JsonShipTemplateRepository : IShipTemplateRepository
{
    private readonly DataPaths _paths;

    public JsonShipTemplateRepository(DataPaths paths)
    {
        _paths = paths;
    }

    public IReadOnlyList<ShipTemplate> List()
    {
        if (!Directory.Exists(_paths.ShipTemplates))
        {
            return Array.Empty<ShipTemplate>();
        }

        var templates = new List<ShipTemplate>();
        foreach (var file in Directory.EnumerateFiles(_paths.ShipTemplates, "*.json"))
        {
            try
            {
                templates.Add(JsonStore.Read<ShipTemplate>(file));
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or System.Text.Json.JsonException)
            {
                // Skip a malformed/locked template file rather than failing the whole listing.
            }
        }

        return templates.OrderBy(t => t.Type, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public ShipTemplate? Get(string type)
    {
        string path = PathFor(type);
        return File.Exists(path) ? JsonStore.Read<ShipTemplate>(path) : null;
    }

    public void Save(ShipTemplate template) => JsonStore.Write(PathFor(template.Type), template);

    public void Delete(string type)
    {
        string path = PathFor(type);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public IReadOnlyList<ShipTemplate> ImportCsv(Stream csv)
    {
        var imported = ShipTemplateCsv.Parse(csv);
        foreach (var template in imported)
        {
            Save(template);
        }

        return imported;
    }

    private string PathFor(string type) =>
        Path.Combine(_paths.ShipTemplates, DataPaths.SafeFileName(type) + ".json");
}
