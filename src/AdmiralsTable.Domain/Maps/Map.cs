using System.Globalization;

namespace AdmiralsTable.Domain.Maps;

/// <summary>
/// The campaign hex map: a row-major one-dimensional array of <see cref="Hex"/>.
/// Only <see cref="Rows"/>, <see cref="Columns"/> and special-hex features need to be
/// persisted — the hex array and adjacency are rebuilt deterministically by <see cref="Build"/>.
/// </summary>
public sealed class Map
{
    private readonly Hex[] _hexes;

    private Map(int rows, int columns, Hex[] hexes, int digitWidth)
    {
        Rows = rows;
        Columns = columns;
        _hexes = hexes;
        DigitWidth = digitWidth;
    }

    public int Rows { get; }
    public int Columns { get; }
    public int Size => Rows * Columns;

    /// <summary>
    /// Zero-pad width used for both the row and column segment of a hex name. Equal to the
    /// digit count of max(rows, columns) so names never become ambiguous (e.g. 10x100 → "001001").
    /// </summary>
    public int DigitWidth { get; }

    public IReadOnlyList<Hex> Hexes => _hexes;

    /// <summary>
    /// Builds a map of the given dimensions. <paramref name="specialHexes"/> maps a hex index to
    /// its feature tags (settlement, nebula, ...), typically taken from the campaign template.
    /// </summary>
    public static Map Build(int rows, int columns, IReadOnlyDictionary<int, IEnumerable<string>>? specialHexes = null)
    {
        if (rows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), rows, "Rows must be positive.");
        }

        if (columns <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(columns), columns, "Columns must be positive.");
        }

        int size = rows * columns;
        int digitWidth = DigitsOf(Math.Max(rows, columns));
        var hexes = new Hex[size];

        for (int i = 0; i < size; i++)
        {
            IEnumerable<string>? features = null;
            specialHexes?.TryGetValue(i, out features);
            hexes[i] = new Hex(i, ComputeAdjacency(i, rows, columns), features);
        }

        return new Map(rows, columns, hexes, digitWidth);
    }

    /// <summary>The six neighbor indices for <paramref name="i"/> in order [NW, NE, E, SE, SW, W]; -1 off-map.</summary>
    /// <remarks>
    /// Offset ("even-r") pointy-top hex grid matching the game: even rows (0-based) are shifted half a
    /// hex to the right, so a column descends in a left/right zigzag (e.g. 0101 is up-right of 0201,
    /// which is up-left of 0301). The two diagonal neighbors therefore depend on row parity; E and W
    /// are the same for every row. This deliberately replaces the axial formula in specs.md lines 39–45.
    /// </remarks>
    private static int[] ComputeAdjacency(int i, int rows, int columns)
    {
        int row = i / columns;
        int col = i % columns;
        bool even = row % 2 == 0;

        int upDownCol = even ? col + 1 : col - 1; // the column the "outer" diagonals reach toward

        return new[]
        {
            Neighbor(row - 1, even ? col : upDownCol, rows, columns), // NW
            Neighbor(row - 1, even ? upDownCol : col, rows, columns), // NE
            Neighbor(row, col + 1, rows, columns),                    // E
            Neighbor(row + 1, even ? upDownCol : col, rows, columns), // SE
            Neighbor(row + 1, even ? col : upDownCol, rows, columns), // SW
            Neighbor(row, col - 1, rows, columns),                    // W
        };
    }

    private static int Neighbor(int row, int col, int rows, int columns) =>
        row < 0 || row >= rows || col < 0 || col >= columns ? -1 : row * columns + col;

    /// <summary>Direct getter by flat-array index.</summary>
    public Hex GetByIndex(int index)
    {
        if (index < 0 || index >= Size)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "Hex index out of range.");
        }

        return _hexes[index];
    }

    /// <summary>Getter by the human-readable hex name (e.g. "001001").</summary>
    public Hex GetByName(string name) => GetByIndex(IndexOfName(name));

    /// <summary>The human-readable name of the hex at <paramref name="index"/> (zero-padded row+column).</summary>
    public string NameOf(int index)
    {
        if (index < 0 || index >= Size)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "Hex index out of range.");
        }

        int row = index / Columns + 1;
        int col = index % Columns + 1;
        return Pad(row) + Pad(col);
    }

    /// <summary>Translates a hex name back into its flat-array index.</summary>
    public int IndexOfName(string name)
    {
        (int row, int col) = ParseName(name);
        return (row - 1) * Columns + (col - 1);
    }

    /// <summary>Non-throwing name lookup, convenient for search-as-you-type position pickers.</summary>
    public bool TryIndexOfName(string name, out int index)
    {
        try
        {
            index = IndexOfName(name);
            return true;
        }
        catch (FormatException)
        {
            index = -1;
            return false;
        }
        catch (ArgumentException)
        {
            index = -1;
            return false;
        }
    }

    private (int Row, int Col) ParseName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (name.Length != DigitWidth * 2)
        {
            throw new ArgumentException($"Hex name '{name}' must be {DigitWidth * 2} digits.", nameof(name));
        }

        int row = int.Parse(name[..DigitWidth], CultureInfo.InvariantCulture);
        int col = int.Parse(name[DigitWidth..], CultureInfo.InvariantCulture);

        if (row < 1 || row > Rows || col < 1 || col > Columns)
        {
            throw new ArgumentOutOfRangeException(nameof(name), name, "Hex name is outside the map bounds.");
        }

        return (row, col);
    }

    private string Pad(int value) => value.ToString(CultureInfo.InvariantCulture).PadLeft(DigitWidth, '0');

    private static int DigitsOf(int value) => value.ToString(CultureInfo.InvariantCulture).Length;
}
