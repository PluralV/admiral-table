namespace AdmiralsTable.Domain.Fleet;

/// <summary>
/// A task force's operational posture. <see cref="Warp"/> is normally derived from whether member
/// ships are running on warp; the rest are player-set. Base/shadow targets are stored as ids.
/// </summary>
public sealed class Posture
{
    public bool Cloak { get; set; }
    public bool Warp { get; set; }
    public bool SensorActive { get; set; }
    public bool Logistics { get; set; }

    /// <summary>True if in port at a base.</summary>
    public bool InPort { get; set; }

    /// <summary>Id of the base task force this TF is in port at when <see cref="InPort"/> is true.</summary>
    public string? BaseId { get; set; }

    public bool Shadowing { get; set; }

    /// <summary>Id of the enemy contact being shadowed when <see cref="Shadowing"/> is true.</summary>
    public string? ShadowTargetId { get; set; }

    public Posture Clone() => new()
    {
        Cloak = Cloak,
        Warp = Warp,
        SensorActive = SensorActive,
        Logistics = Logistics,
        InPort = InPort,
        BaseId = BaseId,
        Shadowing = Shadowing,
        ShadowTargetId = ShadowTargetId,
    };
}
