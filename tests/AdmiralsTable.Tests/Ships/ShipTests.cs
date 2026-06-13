using AdmiralsTable.Domain.Common;
using AdmiralsTable.Domain.Ships;

namespace AdmiralsTable.Tests.Ships;

public class ShipTests
{
    private static ShipTemplate SampleTemplate() => new()
    {
        Type = "D7B",
        HullType = "D7",
        ShipClass = "Battlecruiser",
        Sc = 3,
        Faction = "Klingon",
        BpvCost = 140,
        MaxWarp = 30,
        MaxImp = 12,
        MaxDrnA = 2,
        MaxDrnB = 1,
        MaxDrnC = 0,
        MaxDrnG = 1,
        MaxDrnE = 0,
        Reloads = 2,
        MaxDrnAdd = 2,
        AddReloads = 2,
        MaxFuel = 10,
        MaxSupplies = 8,
        MineSpace = 3,
        MaxOpSpeed = OpSpeed.Warp2,
    };

    [Fact]
    public void DroneSpace_DerivedFromRacksAndReloads()
    {
        var t = SampleTemplate();
        // reloads * (4*(A+C+G+E) + 6*B) = 2 * (4*(2+0+1+0) + 6*1) = 2 * (12 + 6) = 36
        Assert.Equal(36, t.DroneSpace);
    }

    [Fact]
    public void AddSpace_DerivedFromAddRacksAndReloads()
    {
        var t = SampleTemplate();
        // MaxDrnAdd * 6 * AddReloads = 2 * 6 * 2 = 24
        Assert.Equal(24, t.AddSpace);
    }

    [Fact]
    public void ShipDrones_TotalSpace_UsesSlotSizes()
    {
        var d = new ShipDrones("test");
        d.SetAll(type1: 5, type4: 2, type6: 4, add: 8);
        // 5*1 + 2*2 + 4*0.5 + 8*0.5 = 5 + 4 + 2 + 4 = 15
        Assert.Equal(15, d.TotalSpace);
    }

    [Fact]
    public void ShipMines_TotalSpace_UsesMineSizes()
    {
        var m = new ShipMines("test") { SpaceL = 4, SpaceS = 8 };
        // 4*0.25 + 8*0.125 = 1 + 1 = 2
        Assert.Equal(2, m.TotalSpace);
    }

    [Fact]
    public void StandingMineLayouts_MatchSpec()
    {
        var layouts = LayoutGenerator.StandingMineLayouts(SampleTemplate()); // MineSpace = 3
        Assert.Equal(new[] { "Large", "Small", "Split" }, layouts.Select(l => l.LayoutName));
        Assert.Equal((12, 0), (layouts[0].SpaceL, layouts[0].SpaceS)); // 4*3, 0
        Assert.Equal((0, 24), (layouts[1].SpaceL, layouts[1].SpaceS)); // 0, 8*3
        Assert.Equal((6, 12), (layouts[2].SpaceL, layouts[2].SpaceS)); // 2*3, 4*3
    }

    [Fact]
    public void StandingDroneLayouts_IncludeDefensiveOnlyWhenGRacksPresent()
    {
        var withG = LayoutGenerator.StandingDroneLayouts(SampleTemplate()); // MaxDrnG = 1
        Assert.Contains(withG, l => l.LayoutName == "Defensive");

        var noG = SampleTemplate();
        noG.MaxDrnG = 0;
        Assert.DoesNotContain(LayoutGenerator.StandingDroneLayouts(noG), l => l.LayoutName == "Defensive");
    }

    [Fact]
    public void FromTemplate_StartsFullHealthWithId()
    {
        var ship = Ship.FromTemplate(SampleTemplate(), "IKV Death Talon");

        Assert.Equal("D7B", ship.Type);
        Assert.False(string.IsNullOrEmpty(ship.Id));
        Assert.Equal(30, ship.Warp.Max);
        Assert.Equal(30, ship.Warp.CMax);
        Assert.Equal(30, ship.Warp.Cur);
        Assert.Equal(36, ship.CDroneSpace);
        Assert.Equal(24, ship.CAdds);
        Assert.Equal(OpSpeed.Warp2, ship.CMaxOpSpeed);
    }

    [Fact]
    public void Normalize_ClampsCurAndCMaxIntoBounds()
    {
        var ship = Ship.FromTemplate(SampleTemplate(), "Test");
        ship.Warp.CMax = 999;  // above Max
        ship.Warp.Cur = 50;    // above CMax
        ship.CMaxOpSpeed = OpSpeed.Warp3; // above MaxOpSpeed (Warp2)
        ship.CAdds = 9999;

        ship.Normalize();

        Assert.Equal(30, ship.Warp.CMax);
        Assert.Equal(30, ship.Warp.Cur);
        Assert.Equal(OpSpeed.Warp2, ship.CMaxOpSpeed);
        Assert.Equal(ship.CAddSpace, ship.CAdds);
    }

    [Fact]
    public void Validate_FlagsLayoutOverflow()
    {
        var ship = Ship.FromTemplate(SampleTemplate(), "Test");
        ship.CDroneSpace = 10;
        ship.CDrones = new ShipDrones("Overloaded");
        ship.CDrones.SetAll(type1: 20); // 20 > 10

        Assert.NotEmpty(ship.Validate());
    }

    [Fact]
    public void PromoteDroneLayout_MovesChosenLayoutToFront()
    {
        var t = SampleTemplate();
        t.PromoteDroneLayout(new ShipDrones("All Type-IV"));
        t.PromoteDroneLayout(new ShipDrones("All Type-I"));

        Assert.Equal("All Type-I", t.DroneLayouts[0].LayoutName);
        Assert.Equal(2, t.DroneLayouts.Count); // no duplicate entries

        t.PromoteDroneLayout(new ShipDrones("All Type-IV")); // re-select existing
        Assert.Equal("All Type-IV", t.DroneLayouts[0].LayoutName);
        Assert.Equal(2, t.DroneLayouts.Count);
    }
}
