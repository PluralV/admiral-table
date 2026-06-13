namespace AdmiralsTable.Domain.Maps;

/// <summary>
/// A single hex on the campaign map. The hex name is not stored here (it is derived
/// from the index by <see cref="Map.NameOf"/>); adjacency is precomputed at build time
/// because it is hit constantly (turn-plot validation, movement). <see cref="Features"/>
/// carries special-hex tags (settlement, nebula, ...) applied from the campaign template.
/// </summary>
public sealed class Hex
{
    public const int NeighborCount = 6;

    /// <summary>Flat-array index of this hex within its <see cref="Map"/>.</summary>
    public int Index { get; }

    /// <summary>
    /// The six neighbor indices in fixed order [E, N, NE, W, SW, S]; -1 where a
    /// neighbor would fall off the map edge.
    /// </summary>
    public int[] Adjacent { get; }

    /// <summary>Special-hex tags (case-insensitive); empty for an ordinary hex.</summary>
    public HashSet<string> Features { get; }

    public Hex(int index, int[] adjacent, IEnumerable<string>? features = null)
    {
        if (adjacent.Length != NeighborCount)
        {
            throw new ArgumentException($"Adjacency must have exactly {NeighborCount} entries.", nameof(adjacent));
        }

        Index = index;
        Adjacent = adjacent;
        Features = features is null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(features, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Neighbor indices that exist (excludes the -1 edge sentinels).</summary>
    public IEnumerable<int> ValidNeighbors() => Adjacent.Where(n => n >= 0);
}
