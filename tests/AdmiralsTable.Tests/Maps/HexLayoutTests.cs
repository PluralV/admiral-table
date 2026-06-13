using AdmiralsTable.Domain.Maps;

namespace AdmiralsTable.Tests.Maps;

public class HexLayoutTests
{
    private const double Size = 30;

    [Fact]
    public void EveryNeighborCenterIsEquidistant_LockingLayoutToAdjacency()
    {
        var map = Map.Build(9, 11);
        double expected = HexLayout.NeighborDistance(Size);

        foreach (var hex in map.Hexes)
        {
            (double X, double Y) center = CenterOf(map, hex.Index);
            foreach (int neighbor in hex.Adjacent.Where(n => n >= 0))
            {
                (double X, double Y) other = CenterOf(map, neighbor);
                double distance = Math.Sqrt(
                    Math.Pow(center.X - other.X, 2) + Math.Pow(center.Y - other.Y, 2));
                Assert.Equal(expected, distance, precision: 6);
            }
        }
    }

    [Fact]
    public void Dimensions_FollowPointyTopHex()
    {
        Assert.Equal(Math.Sqrt(3) * Size, HexLayout.Width(Size), precision: 9);
        Assert.Equal(2 * Size, HexLayout.Height(Size), precision: 9);
    }

    [Fact]
    public void PointsInBox_HasSixVertices()
    {
        string points = HexLayout.PointsInBox(Size);
        Assert.Equal(6, points.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
    }

    private static (double X, double Y) CenterOf(Map map, int index)
    {
        int row = index / map.Columns;
        int col = index % map.Columns;
        return (HexLayout.CenterX(row, col, Size), HexLayout.CenterY(row, col, Size));
    }
}
