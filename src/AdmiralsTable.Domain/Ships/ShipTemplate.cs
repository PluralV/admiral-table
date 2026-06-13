using System.Text.Json.Serialization;
using AdmiralsTable.Domain.Common;

namespace AdmiralsTable.Domain.Ships;

/// <summary>
/// A ship "type" in the shared template library — the immutable baseline stats a concrete
/// <see cref="Ship"/> is instantiated from. <see cref="Type"/> is the primary key. Starbases
/// are templates with <see cref="IsBase"/> = true.
/// </summary>
public sealed class ShipTemplate
{
    public string Type { get; set; } = string.Empty;
    public string HullType { get; set; } = string.Empty;
    public bool IsBase { get; set; }

    /// <summary>The ship class string (spec field "class").</summary>
    public string ShipClass { get; set; } = string.Empty;

    /// <summary>Size class, usually 1-4.</summary>
    public int Sc { get; set; }

    public string Faction { get; set; } = string.Empty;
    public int BpvCost { get; set; }

    /// <summary>Warp needed to move one hex in SFB.</summary>
    public double MoveCost { get; set; }

    public int MaxWarp { get; set; }
    public int MaxImp { get; set; }
    public int MaxApr { get; set; }
    public int MaxAwr { get; set; }
    public int MaxScout { get; set; }
    public int MaxDamcon { get; set; }
    public int MaxSensor { get; set; }

    public int MaxDrnA { get; set; }
    public int MaxDrnB { get; set; }
    public int MaxDrnC { get; set; }
    public int MaxDrnG { get; set; }
    public int MaxDrnE { get; set; }

    /// <summary>Drone rack reloads.</summary>
    public int Reloads { get; set; } = 2;

    public int MaxDrnAdd { get; set; }

    /// <summary>ADD rack reloads.</summary>
    public int AddReloads { get; set; } = 2;

    public int MaxShuttles { get; set; }
    public double MaxFuel { get; set; }
    public double MaxSupplies { get; set; }

    /// <summary>Space available for drones, derived from rack counts and reloads.</summary>
    [JsonIgnore]
    public double DroneSpace => Reloads * (4 * (MaxDrnA + MaxDrnC + MaxDrnG + MaxDrnE) + 6 * MaxDrnB);

    /// <summary>Space available for mines.</summary>
    public double MineSpace { get; set; }

    /// <summary>Space available for ADDs in ADD racks, derived from rack counts and reloads.</summary>
    [JsonIgnore]
    public int AddSpace => MaxDrnAdd * 6 * AddReloads;

    public OpSpeed MaxOpSpeed { get; set; } = OpSpeed.Warp3;

    public bool Special { get; set; }
    public bool Command { get; set; }

    /// <summary>Saved drone layouts; the first entry is the default offered on the next instantiation.</summary>
    public List<ShipDrones> DroneLayouts { get; set; } = new();

    /// <summary>Saved mine layouts; the first entry is the default offered on the next instantiation.</summary>
    public List<ShipMines> MineLayouts { get; set; } = new();

    /// <summary>How many ships can be docked to this ship at once.</summary>
    public int DockingPoints { get; set; }

    /// <summary>
    /// Records the chosen drone layout as the default for the next instantiation by moving it to the
    /// front of <see cref="DroneLayouts"/> (replacing any existing entry with the same name).
    /// </summary>
    public void PromoteDroneLayout(ShipDrones layout)
    {
        DroneLayouts.RemoveAll(l => string.Equals(l.LayoutName, layout.LayoutName, StringComparison.OrdinalIgnoreCase));
        DroneLayouts.Insert(0, layout.Clone());
    }

    /// <summary>Mine-layout counterpart of <see cref="PromoteDroneLayout"/>.</summary>
    public void PromoteMineLayout(ShipMines layout)
    {
        MineLayouts.RemoveAll(l => string.Equals(l.LayoutName, layout.LayoutName, StringComparison.OrdinalIgnoreCase));
        MineLayouts.Insert(0, layout.Clone());
    }
}
