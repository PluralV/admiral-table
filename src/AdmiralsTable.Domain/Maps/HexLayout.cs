using System.Globalization;

namespace AdmiralsTable.Domain.Maps;

/// <summary>
/// Pure pixel-geometry for rendering the hex map. Matches the offset ("even-r") pointy-top grid in
/// <see cref="Map.ComputeAdjacency"/> (neighbor order [NW, NE, E, SE, SW, W]): even rows are shifted
/// half a hex to the right, so a hex's six neighbors render exactly as its six adjacent hexes.
/// </summary>
public static class HexLayout
{
    /// <summary>Bounding-box width of a hex (vertex-to-vertex across the flats): √3 · size.</summary>
    public static double Width(double size) => Math.Sqrt(3) * size;

    /// <summary>Bounding-box height of a hex (point-to-point): 2 · size.</summary>
    public static double Height(double size) => 2 * size;

    /// <summary>Pixel X of a hex center; even rows are offset half a hex to the right.</summary>
    public static double CenterX(int row, int col, double size) =>
        size * Math.Sqrt(3) * (col + (row % 2 == 0 ? 0.5 : 0.0));

    /// <summary>Pixel Y of a hex center for the given row/column.</summary>
    public static double CenterY(int row, int col, double size) => size * 1.5 * row;

    /// <summary>Distance between the centers of two adjacent hexes (= the bounding-box width).</summary>
    public static double NeighborDistance(double size) => Width(size);

    /// <summary>
    /// The six pointy-top hex vertices as a WPF <c>Points</c> string, expressed inside a
    /// Width×Height box (so a polygon placed at the cell's top-left renders correctly).
    /// </summary>
    public static string PointsInBox(double size)
    {
        double w = Width(size);
        double h = Height(size);
        double s = size;

        (double X, double Y)[] points =
        {
            (w / 2, 0),
            (w, s / 2),
            (w, 3 * s / 2),
            (w / 2, h),
            (0, 3 * s / 2),
            (0, s / 2),
        };

        return string.Join(" ", points.Select(p =>
            string.Create(CultureInfo.InvariantCulture, $"{p.X},{p.Y}")));
    }
}
