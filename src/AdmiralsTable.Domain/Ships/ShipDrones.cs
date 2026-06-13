using System.Text.Json.Serialization;

namespace AdmiralsTable.Domain.Ships;

/// <summary>Slots in a drone storage layout. Backing array index == (int) value.</summary>
public enum DroneSlot
{
    Type1 = 0, // size 1
    Type2 = 1, // size 1
    Type3 = 2, // size 1
    Type4 = 3, // size 2
    Type5 = 4, // size 2
    Type6 = 5, // size 0.5
    Add = 6,   // size 0.5
}

/// <summary>
/// The drones held in a ship's storage for a named layout. Space counts are mutated only
/// through <see cref="SetSpace"/>/<see cref="SetAll"/>; <see cref="TotalSpace"/> is always
/// derived so it cannot drift out of sync with the counts.
/// </summary>
public sealed class ShipDrones
{
    public const int SlotCount = 7;

    // Drone sizes by slot: Type1-3 = 1, Type4-5 = 2, Type6 & ADD = 0.5.
    private static readonly double[] Sizes = { 1, 1, 1, 2, 2, 0.5, 0.5 };

    public ShipDrones()
    {
    }

    public ShipDrones(string layoutName)
    {
        LayoutName = layoutName;
    }

    public string LayoutName { get; set; } = string.Empty;

    /// <summary>Counts per <see cref="DroneSlot"/>, indexed by slot value.</summary>
    public int[] Spaces { get; set; } = new int[SlotCount];

    [JsonIgnore]
    public double TotalSpace
    {
        get
        {
            double total = 0;
            for (int i = 0; i < SlotCount; i++)
            {
                total += Spaces[i] * Sizes[i];
            }

            return total;
        }
    }

    public int Get(DroneSlot slot) => Spaces[(int)slot];

    /// <summary>Sets a single slot (negatives clamped to 0). Total space follows automatically.</summary>
    public void SetSpace(DroneSlot slot, int value) => Spaces[(int)slot] = value < 0 ? 0 : value;

    public void SetAll(int type1 = 0, int type2 = 0, int type3 = 0, int type4 = 0, int type5 = 0, int type6 = 0, int add = 0)
    {
        SetSpace(DroneSlot.Type1, type1);
        SetSpace(DroneSlot.Type2, type2);
        SetSpace(DroneSlot.Type3, type3);
        SetSpace(DroneSlot.Type4, type4);
        SetSpace(DroneSlot.Type5, type5);
        SetSpace(DroneSlot.Type6, type6);
        SetSpace(DroneSlot.Add, add);
    }

    /// <summary>Human-readable summary for tables, e.g. "5 Type-I, 2 Type-IV, 8 ADD".</summary>
    public string Describe()
    {
        string[] labels = { "Type-I", "Type-II", "Type-III", "Type-IV", "Type-V", "Type-VI", "ADD" };
        var parts = new List<string>();
        for (int i = 0; i < SlotCount; i++)
        {
            if (Spaces[i] > 0)
            {
                parts.Add($"{Spaces[i]} {labels[i]}");
            }
        }

        return parts.Count == 0 ? "Empty" : string.Join(", ", parts);
    }

    public ShipDrones Clone() => new(LayoutName) { Spaces = (int[])Spaces.Clone() };
}
