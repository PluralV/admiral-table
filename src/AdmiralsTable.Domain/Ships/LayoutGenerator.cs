namespace AdmiralsTable.Domain.Ships;

/// <summary>
/// Produces the standing drone and mine layouts that always exist for a ship without any player
/// input, computed from current drone/mine space and rack counts (spec "Ships" section). The GUI
/// offers these alongside any custom layouts saved on the template.
/// </summary>
public static class LayoutGenerator
{
    public static List<ShipDrones> StandingDroneLayouts(double droneSpace, int drnA, int drnB, int drnC, int drnG, int reloads)
    {
        int space = (int)droneSpace;
        int adds = 8 * drnG;
        var layouts = new List<ShipDrones>();

        // All Type-I
        var allI = new ShipDrones("All Type-I");
        allI.SetAll(type1: space, add: adds);
        layouts.Add(allI);

        // All Type-IV
        var allIv = new ShipDrones("All Type-IV");
        allIv.SetAll(type4: space / 2, add: adds);
        layouts.Add(allIv);

        // I-IV Split
        int x = drnA + drnC + drnG;
        var split = new ShipDrones("I-IV Split");
        split.SetAll(type1: 2 * reloads * x + 2 * reloads * drnB, type4: reloads * x, add: adds);
        layouts.Add(split);

        // Defensive (only meaningful when G-racks are present)
        if (drnG > 0)
        {
            int defAdds = 24 * drnG;
            var defensive = new ShipDrones("Defensive");
            defensive.SetAll(type1: Math.Max(0, (int)(droneSpace - defAdds * 0.5)), add: defAdds);
            layouts.Add(defensive);
        }

        return layouts;
    }

    public static List<ShipMines> StandingMineLayouts(double mineSpace)
    {
        int space = (int)mineSpace;
        return new List<ShipMines>
        {
            new("Large") { SpaceL = 4 * space, SpaceS = 0 }, // default
            new("Small") { SpaceL = 0, SpaceS = 8 * space },
            new("Split") { SpaceL = 2 * space, SpaceS = 4 * space },
        };
    }

    /// <summary>Standing drone layouts from a template's baseline stats (full-health preview).</summary>
    public static List<ShipDrones> StandingDroneLayouts(ShipTemplate t) =>
        StandingDroneLayouts(t.DroneSpace, t.MaxDrnA, t.MaxDrnB, t.MaxDrnC, t.MaxDrnG, t.Reloads);

    /// <summary>Standing mine layouts from a template's baseline stats.</summary>
    public static List<ShipMines> StandingMineLayouts(ShipTemplate t) => StandingMineLayouts(t.MineSpace);

    /// <summary>Standing drone layouts from a ship's current (possibly damaged) stats.</summary>
    public static List<ShipDrones> StandingDroneLayouts(Ship s) =>
        StandingDroneLayouts(s.CDroneSpace, s.DrnA.Cur, s.DrnB.Cur, s.DrnC.Cur, s.DrnG.Cur, s.Reloads);

    /// <summary>Standing mine layouts from a ship's current (possibly damaged) stats.</summary>
    public static List<ShipMines> StandingMineLayouts(Ship s) => StandingMineLayouts(s.CMineSpace);
}
