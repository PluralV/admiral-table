using AdmiralsTable.Domain.Campaign;
using AdmiralsTable.Persistence.Json;

namespace AdmiralsTable.Persistence.Repositories;

/// <summary>The campaign-template library (CAMPAIGN ANNEX), keyed by <see cref="CampaignTemplate.Name"/>.</summary>
public interface ICampaignTemplateRepository
{
    IReadOnlyList<CampaignTemplate> List();
    CampaignTemplate? Get(string name);
    void Save(CampaignTemplate template);
    void Delete(string name);
}

/// <summary>One JSON file per template under <c>campaigns/templates/</c>.</summary>
public sealed class JsonCampaignTemplateRepository : ICampaignTemplateRepository
{
    private readonly DataPaths _paths;

    public JsonCampaignTemplateRepository(DataPaths paths)
    {
        _paths = paths;
    }

    public IReadOnlyList<CampaignTemplate> List()
    {
        if (!Directory.Exists(_paths.CampaignTemplates))
        {
            return Array.Empty<CampaignTemplate>();
        }

        var templates = new List<CampaignTemplate>();
        foreach (var file in Directory.EnumerateFiles(_paths.CampaignTemplates, "*.json"))
        {
            try
            {
                templates.Add(JsonStore.Read<CampaignTemplate>(file));
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or System.Text.Json.JsonException)
            {
            }
        }

        return templates.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public CampaignTemplate? Get(string name)
    {
        string path = PathFor(name);
        return File.Exists(path) ? JsonStore.Read<CampaignTemplate>(path) : null;
    }

    public void Save(CampaignTemplate template) => JsonStore.Write(PathFor(template.Name), template);

    public void Delete(string name)
    {
        string path = PathFor(name);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string PathFor(string name) =>
        Path.Combine(_paths.CampaignTemplates, DataPaths.SafeFileName(name) + ".json");
}
