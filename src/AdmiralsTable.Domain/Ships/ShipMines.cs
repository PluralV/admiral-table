using System.Text.Json.Serialization;

namespace AdmiralsTable.Domain.Ships;

/// <summary>
/// The mines held in a ship's storage for a named layout. Large mines are size 0.25,
/// small mines size 0.125. <see cref="TotalSpace"/> is derived from the counts.
/// </summary>
public sealed class ShipMines
{
    public const double LargeSize = 0.25;
    public const double SmallSize = 0.125;

    public ShipMines()
    {
    }

    public ShipMines(string layoutName)
    {
        LayoutName = layoutName;
    }

    public string LayoutName { get; set; } = string.Empty;

    /// <summary>Number of large mines (size 0.25).</summary>
    public int SpaceL { get; set; }

    /// <summary>Number of small mines (size 0.125).</summary>
    public int SpaceS { get; set; }

    [JsonIgnore]
    public double TotalSpace => SpaceL * LargeSize + SpaceS * SmallSize;

    public void SetSpace(int large, int small)
    {
        SpaceL = large < 0 ? 0 : large;
        SpaceS = small < 0 ? 0 : small;
    }

    /// <summary>Human-readable summary for tables, e.g. "4 Large, 8 Small".</summary>
    public string Describe()
    {
        var parts = new List<string>();
        if (SpaceL > 0)
        {
            parts.Add($"{SpaceL} Large");
        }

        if (SpaceS > 0)
        {
            parts.Add($"{SpaceS} Small");
        }

        return parts.Count == 0 ? "Empty" : string.Join(", ", parts);
    }

    public ShipMines Clone() => new(LayoutName) { SpaceL = SpaceL, SpaceS = SpaceS };
}
