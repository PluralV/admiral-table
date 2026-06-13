using AdmiralsTable.Domain.Maps;

namespace AdmiralsTable.Tests.Maps;

public class MapTests
{
    [Theory]
    [InlineData(1000, 1000, 0, "00010001")] // spec example
    [InlineData(10, 100, 0, "001001")]      // spec example: uneven magnitudes pad to 3
    [InlineData(10, 100, 999, "010100")]    // last hex of the 10x100 map
    [InlineData(3, 3, 0, "11")]
    [InlineData(3, 3, 8, "33")]
    public void NameOf_PadsToConsistentWidth(int rows, int columns, int index, string expected)
    {
        var map = Map.Build(rows, columns);
        Assert.Equal(expected, map.NameOf(index));
    }

    [Theory]
    [InlineData(1000, 1000, 4)]
    [InlineData(10, 100, 3)]
    [InlineData(100, 10, 3)]
    [InlineData(3, 3, 1)]
    public void DigitWidth_IsDigitsOfLargerDimension(int rows, int columns, int expectedWidth)
    {
        Assert.Equal(expectedWidth, Map.Build(rows, columns).DigitWidth);
    }

    [Fact]
    public void Adjacency_InteriorHex_HasAllSixNeighbors()
    {
        // 3x3 offset grid, center index 4 (odd row) → [NW, NE, E, SE, SW, W]
        var map = Map.Build(3, 3);
        Assert.Equal(new[] { 0, 1, 5, 7, 6, 3 }, map.GetByIndex(4).Adjacent);
    }

    [Theory]
    [InlineData(0, new[] { -1, -1, 1, 4, 3, -1 })]  // top-left corner (even row)
    [InlineData(2, new[] { -1, -1, -1, -1, 5, 1 })] // top-right corner (even row)
    [InlineData(6, new[] { 3, 4, 7, -1, -1, -1 })]  // bottom-left corner (even row)
    [InlineData(8, new[] { 5, -1, -1, -1, -1, 7 })] // bottom-right corner (even row)
    public void Adjacency_Corners_UseMinusOneSentinels(int index, int[] expected)
    {
        var map = Map.Build(3, 3);
        Assert.Equal(expected, map.GetByIndex(index).Adjacent);
    }

    [Fact]
    public void Adjacency_IsSymmetric_OppositeDirections()
    {
        // Direction pairs in our [NW, NE, E, SE, SW, W] order: NW<->SE, NE<->SW, E<->W.
        var map = Map.Build(7, 9);
        (int dir, int opposite)[] pairs = { (0, 3), (1, 4), (2, 5) };

        foreach (var hex in map.Hexes)
        {
            foreach (var (dir, opposite) in pairs)
            {
                int neighbor = hex.Adjacent[dir];
                if (neighbor >= 0)
                {
                    Assert.Equal(hex.Index, map.GetByIndex(neighbor).Adjacent[opposite]);
                }
            }
        }
    }

    [Fact]
    public void NameAndIndex_RoundTrip()
    {
        var map = Map.Build(12, 34);
        for (int i = 0; i < map.Size; i++)
        {
            string name = map.NameOf(i);
            Assert.Equal(i, map.IndexOfName(name));
            Assert.Equal(i, map.GetByName(name).Index);
        }
    }

    [Fact]
    public void GetByName_OutOfBounds_Throws()
    {
        var map = Map.Build(3, 3); // names are 2 digits, valid 11..33
        Assert.Throws<ArgumentOutOfRangeException>(() => map.GetByName("44"));
        Assert.Throws<ArgumentException>(() => map.GetByName("123")); // wrong length
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(5, 0)]
    [InlineData(-1, 4)]
    public void Build_NonPositiveDimensions_Throws(int rows, int columns)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Map.Build(rows, columns));
    }

    [Fact]
    public void Build_AppliesSpecialHexFeatures()
    {
        var specials = new Dictionary<int, IEnumerable<string>>
        {
            [4] = new[] { "settlement", "nebula" },
        };
        var map = Map.Build(3, 3, specials);

        Assert.Contains("settlement", map.GetByIndex(4).Features);
        Assert.Contains("nebula", map.GetByIndex(4).Features);
        Assert.Empty(map.GetByIndex(0).Features);
    }
}
