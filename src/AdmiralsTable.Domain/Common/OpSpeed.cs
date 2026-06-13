namespace AdmiralsTable.Domain.Common;

/// <summary>
/// Operational speeds for SFB PQ-17, slowest to fastest. The underlying integer
/// value preserves ordering (so speeds compare with normal int comparison), while
/// the warp speeds are surfaced to the UI as the strings "1", "2", "3".
/// </summary>
public enum OpSpeed
{
    Stop = 0,
    ImpulseCrawl = 1,
    WarpCrawl = 2,
    Warp1 = 3,
    Warp2 = 4,
    Warp3 = 5,
}

public static class OpSpeedExtensions
{
    /// <summary>Human-readable label used on the GUI.</summary>
    public static string Display(this OpSpeed speed) => speed switch
    {
        OpSpeed.Stop => "0",
        OpSpeed.ImpulseCrawl => "IMP",
        OpSpeed.WarpCrawl => "WPC",
        OpSpeed.Warp1 => "1",
        OpSpeed.Warp2 => "2",
        OpSpeed.Warp3 => "3",
        _ => speed.ToString(),
    };

    /// <summary>Parses a display label back into an <see cref="OpSpeed"/>.</summary>
    public static OpSpeed FromDisplay(string label) => label.Trim().ToUpperInvariant() switch
    {
        "0" => OpSpeed.Stop,
        "IMP" => OpSpeed.ImpulseCrawl,
        "WPC" => OpSpeed.WarpCrawl,
        "1" => OpSpeed.Warp1,
        "2" => OpSpeed.Warp2,
        "3" => OpSpeed.Warp3,
        _ => throw new ArgumentOutOfRangeException(nameof(label), label, "Unknown op speed label."),
    };

    /// <summary>All speeds at or below <paramref name="max"/>, slowest first — for speed dropdowns.</summary>
    public static IEnumerable<OpSpeed> AtOrBelow(OpSpeed max)
    {
        foreach (OpSpeed speed in Enum.GetValues<OpSpeed>())
        {
            if (speed <= max)
            {
                yield return speed;
            }
        }
    }
}
