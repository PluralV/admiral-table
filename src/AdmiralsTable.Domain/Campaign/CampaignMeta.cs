namespace AdmiralsTable.Domain.Campaign;

/// <summary>
/// Campaign-wide bookkeeping owned by the (future) Campaign Management module. Stubbed for v1.0 —
/// only the calendar is used so far; victory points and other trackers are placeholders.
/// </summary>
public sealed class CampaignMeta
{
    public int Day { get; set; } = 1;
    public int Month { get; set; } = 1;

    /// <summary>Victory-point tallies by category (Campaign Management, future).</summary>
    public Dictionary<string, int> VictoryPoints { get; set; } = new();

    public string Notes { get; set; } = string.Empty;
}

/// <summary>Per-campaign configuration choices captured at creation (e.g. codename files).</summary>
public sealed class CampaignConfig
{
    public string TemplateName { get; set; } = string.Empty;
    public string? CodenameFile { get; set; }

    /// <summary>Free-form option overrides serialized with the campaign.</summary>
    public Dictionary<string, string> Options { get; set; } = new();
}
