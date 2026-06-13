namespace AdmiralsTable.Domain.Campaign;

/// <summary>A special hex declared by a campaign template (settlement, nebula, ...).</summary>
public sealed class SpecialHexSpec
{
    public int Index { get; set; }
    public List<string> Features { get; set; } = new();
}

/// <summary>Fleet-building constraints for a campaign.</summary>
public sealed class FleetConstraints
{
    /// <summary>Maximum total build point value across the fleet.</summary>
    public int TotalBpv { get; set; }

    /// <summary>Per-ship-class build point value caps.</summary>
    public Dictionary<string, int> BpvByClass { get; set; } = new();

    /// <summary>Hex indices where ships may be deployed.</summary>
    public List<int> DeploymentHexes { get; set; } = new();
}

/// <summary>
/// Reusable blueprint for a new campaign: map dimensions, fleet constraints and special hexes.
/// Stored as JSON in <c>campaigns/templates/</c>.
/// </summary>
public sealed class CampaignTemplate
{
    public string Name { get; set; } = string.Empty;
    public int MapRows { get; set; } = 1;
    public int MapColumns { get; set; } = 1;
    public FleetConstraints Fleet { get; set; } = new();
    public List<SpecialHexSpec> SpecialHexes { get; set; } = new();
}
