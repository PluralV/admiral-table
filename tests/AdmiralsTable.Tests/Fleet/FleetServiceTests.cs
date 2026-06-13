using AdmiralsTable.Domain.Common;
using AdmiralsTable.Domain.Fleet;
using AdmiralsTable.Domain.Maps;
using AdmiralsTable.Domain.Ships;

namespace AdmiralsTable.Tests.Fleet;

public class FleetServiceTests
{
    private readonly FleetService _fleet = new();

    private static Ship ShipIn(string tfId, OpSpeed speed, bool isBase = false) => new()
    {
        Id = Ids.New("s"),
        CurrentTf = tfId,
        CMaxOpSpeed = speed,
        MaxOpSpeed = speed,
        IsBase = isBase,
    };

    [Fact]
    public void RecomputeMaxOpSpeed_UsesSlowestMember_AndClampsCurrentSpeed()
    {
        var tf = TaskForce.Create("Strike", pos: 5);
        tf.OpSpeed = OpSpeed.Warp3;
        var ships = new List<Ship>
        {
            ShipIn(tf.Id, OpSpeed.Warp3),
            ShipIn(tf.Id, OpSpeed.WarpCrawl), // slowest
        };

        _fleet.RecomputeMaxOpSpeed(tf, ships);

        Assert.Equal(OpSpeed.WarpCrawl, tf.MaxOpSpeed);
        Assert.Equal(OpSpeed.WarpCrawl, tf.OpSpeed); // clamped down from Warp3
    }

    [Fact]
    public void RecomputeMaxOpSpeed_EmptyTaskForce_DefaultsToWarp3()
    {
        var tf = TaskForce.Create("Empty", pos: 0);
        _fleet.RecomputeMaxOpSpeed(tf, Array.Empty<Ship>());
        Assert.Equal(OpSpeed.Warp3, tf.MaxOpSpeed);
    }

    [Fact]
    public void CanReassign_RejectsDifferentHex()
    {
        var source = TaskForce.Create("A", pos: 5);
        var target = TaskForce.Create("B", pos: 9);
        var ship = ShipIn(source.Id, OpSpeed.Warp2);

        Assert.False(_fleet.CanReassign(ship, source, target, new[] { ship }).Allowed);
    }

    [Fact]
    public void CanReassign_RejectsBaseLeavingItsTaskForce()
    {
        var source = TaskForce.Create("Base TF", pos: 5);
        var target = TaskForce.Create("Other", pos: 5);
        var baseShip = ShipIn(source.Id, OpSpeed.Stop, isBase: true);

        Assert.False(_fleet.CanReassign(baseShip, source, target, new[] { baseShip }).Allowed);
    }

    [Fact]
    public void CanReassign_RejectsJoiningABasesTaskForce()
    {
        var source = TaskForce.Create("Mobile", pos: 5);
        var target = TaskForce.Create("Base TF", pos: 5);
        var mover = ShipIn(source.Id, OpSpeed.Warp2);
        var baseShip = ShipIn(target.Id, OpSpeed.Stop, isBase: true);

        Assert.False(_fleet.CanReassign(mover, source, target, new[] { mover, baseShip }).Allowed);
    }

    [Fact]
    public void Reassign_MovesShipAndRefreshesSpeeds()
    {
        var source = TaskForce.Create("A", pos: 5);
        var target = TaskForce.Create("B", pos: 5);
        var fast = ShipIn(source.Id, OpSpeed.Warp3);
        var slow = ShipIn(source.Id, OpSpeed.WarpCrawl);
        var ships = new List<Ship> { fast, slow };
        var tfs = new List<TaskForce> { source, target };
        _fleet.RecomputeAll(tfs, ships);

        var result = _fleet.Reassign(slow, source, target, tfs, ships);

        Assert.True(result.Allowed);
        Assert.Equal(target.Id, slow.CurrentTf);
        Assert.Equal(OpSpeed.Warp3, source.MaxOpSpeed);     // slow one left
        Assert.Equal(OpSpeed.WarpCrawl, target.MaxOpSpeed); // slow one arrived
    }

    [Fact]
    public void TurnPlotOptions_FirstCellIsNeighborsOfPosition_LaterCellsChain()
    {
        var map = Map.Build(5, 5);
        var tf = TaskForce.Create("Mover", pos: 12); // center

        var cell0 = _fleet.TurnPlotOptions(map, tf, 0);
        Assert.Equal(map.GetByIndex(12).ValidNeighbors().OrderBy(x => x), cell0.OrderBy(x => x));

        // cell 1 empty until cell 0 chosen
        Assert.Empty(_fleet.TurnPlotOptions(map, tf, 1));
        tf.TurnPlot[0] = cell0[0];
        Assert.NotEmpty(_fleet.TurnPlotOptions(map, tf, 1));
    }

    [Fact]
    public void SetShadow_LocksSpeedAndClearsPlot()
    {
        var map = Map.Build(5, 5);
        var tf = TaskForce.Create("Shadow", pos: 12);
        tf.TurnPlot[0] = 7;

        _fleet.SetShadow(tf, "contact-id", OpSpeed.Warp1);

        Assert.True(tf.Posture.Shadowing);
        Assert.Equal("contact-id", tf.Posture.ShadowTargetId);
        Assert.Equal(OpSpeed.Warp1, tf.OpSpeed);
        Assert.Equal(new[] { -1, -1, -1 }, tf.TurnPlot);
        Assert.Empty(_fleet.TurnPlotOptions(map, tf, 0)); // plot locked while shadowing
    }
}
