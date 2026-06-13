using System.Text.Json.Serialization;
using AdmiralsTable.Domain.Common;

namespace AdmiralsTable.Domain.Ships;

/// <summary>
/// A concrete ship in a campaign. It is self-contained: the static identity stats are copied
/// from its <see cref="ShipTemplate"/> at instantiation so the ship survives later edits or
/// deletion of the template. Box resources are tracked as <see cref="Gauge"/>s (Max/CMax/Cur);
/// damage reduces CMax, the current state reduces Cur. Call <see cref="Normalize"/> after edits.
/// </summary>
public sealed class Ship
{
    // --- Identity / display (copied from template) ---
    public string Type { get; set; } = string.Empty;
    public string HullType { get; set; } = string.Empty;
    public string ShipClass { get; set; } = string.Empty;
    public int Sc { get; set; }
    public string Faction { get; set; } = string.Empty;
    public bool IsBase { get; set; }
    public bool Special { get; set; }
    public bool Command { get; set; }
    public int BpvCost { get; set; }
    public double MoveCost { get; set; }
    public int DockingPoints { get; set; }
    public int Reloads { get; set; } = 2;
    public int AddReloads { get; set; } = 2;

    // --- Instance ---
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;

    /// <summary>Id of the task force this ship currently belongs to (see design note in plan).</summary>
    public string CurrentTf { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // --- Box resources (Max / CMax / Cur) ---
    public Gauge Warp { get; set; } = new();
    public Gauge Imp { get; set; } = new();
    public Gauge Apr { get; set; } = new();
    public Gauge Awr { get; set; } = new();
    public Gauge Scout { get; set; } = new();
    public Gauge Damcon { get; set; } = new();
    public Gauge Sensor { get; set; } = new();
    public Gauge DrnA { get; set; } = new();
    public Gauge DrnB { get; set; } = new();
    public Gauge DrnC { get; set; } = new();
    public Gauge DrnG { get; set; } = new();
    public Gauge DrnE { get; set; } = new();
    public Gauge DrnAdd { get; set; } = new();
    public Gauge Shuttles { get; set; } = new();

    public GaugeF Fuel { get; set; } = new();
    public GaugeF Supplies { get; set; } = new();

    // --- Storage spaces ---
    /// <summary>Baseline drone space (template).</summary>
    public double DroneSpace { get; set; }

    /// <summary>Current drone space after damage; usage is <see cref="CDrones"/>.TotalSpace.</summary>
    public double CDroneSpace { get; set; }

    public double MineSpace { get; set; }
    public double CMineSpace { get; set; }

    public int AddSpace { get; set; }
    public int CAddSpace { get; set; }

    /// <summary>Current number of loaded ADDs (defaults to <see cref="CAddSpace"/>).</summary>
    public int CAdds { get; set; }

    // --- Operational speed ---
    public OpSpeed MaxOpSpeed { get; set; } = OpSpeed.Warp3;
    public OpSpeed CMaxOpSpeed { get; set; } = OpSpeed.Warp3;

    // --- Loaded layouts ---
    public ShipDrones CDrones { get; set; } = new("Empty");
    public ShipMines CMines { get; set; } = new("Empty");

    /// <summary>Human-readable drone layout summary for tables (e.g. "5 Type-I, 8 ADD").</summary>
    [JsonIgnore]
    public string DroneSummary => CDrones.Describe();

    /// <summary>Human-readable mine layout summary for tables.</summary>
    [JsonIgnore]
    public string MineSummary => CMines.Describe();

    /// <summary>
    /// Builds a full-health ship from a template. Optional drone/mine layouts default to the
    /// template's first saved layout (or an empty layout if none).
    /// </summary>
    public static Ship FromTemplate(ShipTemplate t, string name, ShipDrones? drones = null, ShipMines? mines = null)
    {
        var ship = new Ship
        {
            Name = name,
            Id = Ids.New(name),
            Type = t.Type,
            HullType = t.HullType,
            ShipClass = t.ShipClass,
            Sc = t.Sc,
            Faction = t.Faction,
            IsBase = t.IsBase,
            Special = t.Special,
            Command = t.Command,
            BpvCost = t.BpvCost,
            MoveCost = t.MoveCost,
            DockingPoints = t.DockingPoints,
            Reloads = t.Reloads,
            AddReloads = t.AddReloads,

            Warp = new Gauge(t.MaxWarp),
            Imp = new Gauge(t.MaxImp),
            Apr = new Gauge(t.MaxApr),
            Awr = new Gauge(t.MaxAwr),
            Scout = new Gauge(t.MaxScout),
            Damcon = new Gauge(t.MaxDamcon),
            Sensor = new Gauge(t.MaxSensor),
            DrnA = new Gauge(t.MaxDrnA),
            DrnB = new Gauge(t.MaxDrnB),
            DrnC = new Gauge(t.MaxDrnC),
            DrnG = new Gauge(t.MaxDrnG),
            DrnE = new Gauge(t.MaxDrnE),
            DrnAdd = new Gauge(t.MaxDrnAdd),
            Shuttles = new Gauge(t.MaxShuttles),
            Fuel = new GaugeF(t.MaxFuel),
            Supplies = new GaugeF(t.MaxSupplies),

            DroneSpace = t.DroneSpace,
            CDroneSpace = t.DroneSpace,
            MineSpace = t.MineSpace,
            CMineSpace = t.MineSpace,
            AddSpace = t.AddSpace,
            CAddSpace = t.AddSpace,
            CAdds = t.AddSpace,

            MaxOpSpeed = t.MaxOpSpeed,
            CMaxOpSpeed = t.MaxOpSpeed,
        };

        ship.CDrones = drones?.Clone() ?? t.DroneLayouts.FirstOrDefault()?.Clone() ?? new ShipDrones("Empty");
        ship.CMines = mines?.Clone() ?? t.MineLayouts.FirstOrDefault()?.Clone() ?? new ShipMines("Empty");
        return ship;
    }

    private IEnumerable<Gauge> Gauges()
    {
        yield return Warp;
        yield return Imp;
        yield return Apr;
        yield return Awr;
        yield return Scout;
        yield return Damcon;
        yield return Sensor;
        yield return DrnA;
        yield return DrnB;
        yield return DrnC;
        yield return DrnG;
        yield return DrnE;
        yield return DrnAdd;
        yield return Shuttles;
    }

    /// <summary>Clamps every gauge and storage scalar back into legal bounds (0 ≤ Cur ≤ CMax ≤ Max).</summary>
    public void Normalize()
    {
        foreach (var gauge in Gauges())
        {
            gauge.Normalize();
        }

        Fuel.Normalize();
        Supplies.Normalize();

        CDroneSpace = Guard.Clamp(CDroneSpace, 0, DroneSpace);
        CMineSpace = Guard.Clamp(CMineSpace, 0, MineSpace);
        CAddSpace = Guard.Clamp(CAddSpace, 0, AddSpace);
        CAdds = Guard.Clamp(CAdds, 0, CAddSpace);

        if (CMaxOpSpeed > MaxOpSpeed)
        {
            CMaxOpSpeed = MaxOpSpeed;
        }
    }

    /// <summary>
    /// Returns any invariant violations that cannot be auto-clamped — currently a loaded layout whose
    /// total space exceeds the ship's current capacity. Empty when the ship is valid.
    /// </summary>
    public IReadOnlyList<string> Validate()
    {
        var issues = new List<string>();
        if (CDrones.TotalSpace > CDroneSpace)
        {
            issues.Add($"Drone layout '{CDrones.LayoutName}' uses {CDrones.TotalSpace:0.##} of {CDroneSpace:0.##} drone space.");
        }

        if (CMines.TotalSpace > CMineSpace)
        {
            issues.Add($"Mine layout '{CMines.LayoutName}' uses {CMines.TotalSpace:0.##} of {CMineSpace:0.##} mine space.");
        }

        return issues;
    }
}
