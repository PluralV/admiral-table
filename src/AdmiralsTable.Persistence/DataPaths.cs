namespace AdmiralsTable.Persistence;

/// <summary>
/// Resolves the on-disk data layout under a configurable root (mirrors the spec's module
/// subdirectories). Distinct from the source-code layout: this is where JSON data lives at runtime.
/// </summary>
public sealed class DataPaths
{
    public DataPaths(string root)
    {
        Root = root;
    }

    public string Root { get; }

    public string Ships => Path.Combine(Root, "ships");
    public string ShipTemplates => Path.Combine(Ships, "templates");
    public string Campaigns => Path.Combine(Root, "campaigns");
    public string CampaignTemplates => Path.Combine(Campaigns, "templates");
    public string FleetManagement => Path.Combine(Root, "fleet-management");
    public string Contacts => Path.Combine(Root, "contacts");
    public string CampaignManagement => Path.Combine(Root, "campaign-management");

    /// <summary>Creates every data directory if it does not already exist.</summary>
    public void EnsureCreated()
    {
        foreach (var dir in new[]
        {
            Root, Ships, ShipTemplates, Campaigns, CampaignTemplates,
            FleetManagement, Contacts, CampaignManagement,
        })
        {
            Directory.CreateDirectory(dir);
        }
    }

    /// <summary>Replaces characters that are invalid in a file name with '_'.</summary>
    public static string SafeFileName(string name)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        string cleaned = new string(chars).Trim();
        return string.IsNullOrEmpty(cleaned) ? "unnamed" : cleaned;
    }
}
