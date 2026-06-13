using AdmiralsTable.Persistence.Json;
using CampaignModel = AdmiralsTable.Domain.Campaign.Campaign;

namespace AdmiralsTable.Persistence.Repositories;

/// <summary>Serialized campaigns under <c>campaigns/</c>, one JSON file per campaign.</summary>
public interface ICampaignRepository
{
    IReadOnlyList<string> ListNames();
    bool Exists(string name);
    CampaignModel Load(string name);
    void Save(CampaignModel campaign);
    void Delete(string name);
}

/// <summary>
/// Loads/saves the whole campaign object graph as one JSON document. On load the campaign is
/// re-normalized so every domain invariant holds even if the file was hand-edited.
/// </summary>
public sealed class JsonCampaignRepository : ICampaignRepository
{
    private readonly DataPaths _paths;

    public JsonCampaignRepository(DataPaths paths)
    {
        _paths = paths;
    }

    public IReadOnlyList<string> ListNames()
    {
        if (!Directory.Exists(_paths.Campaigns))
        {
            return Array.Empty<string>();
        }

        return Directory.EnumerateFiles(_paths.Campaigns, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public bool Exists(string name) => File.Exists(PathFor(name));

    public CampaignModel Load(string name)
    {
        var campaign = JsonStore.Read<CampaignModel>(PathFor(name));
        campaign.NormalizeAll();
        return campaign;
    }

    public void Save(CampaignModel campaign) => JsonStore.Write(PathFor(campaign.Name), campaign);

    public void Delete(string name)
    {
        string path = PathFor(name);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string PathFor(string name) =>
        Path.Combine(_paths.Campaigns, DataPaths.SafeFileName(name) + ".json");
}
