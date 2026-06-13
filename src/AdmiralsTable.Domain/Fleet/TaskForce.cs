using AdmiralsTable.Domain.Common;

namespace AdmiralsTable.Domain.Fleet;

/// <summary>
/// A grouping of ships and/or bases at a single hex. Ships reference their task force by
/// <see cref="Id"/> via <c>Ship.CurrentTf</c>. <see cref="MaxOpSpeed"/> is a cache of the slowest
/// member ship's speed and is refreshed by <c>FleetService</c> whenever membership changes.
/// </summary>
public sealed class TaskForce
{
    public const int TurnPlotLength = 3;

    public string Id { get; set; } = string.Empty;

    /// <summary>Name used in order files (≤ 12 chars).</summary>
    public string Callsign { get; set; } = string.Empty;

    public bool Friendly { get; set; } = true;

    /// <summary>Abbreviated name (≤ 5 chars); defaults to the initials of <see cref="Name"/>.</summary>
    public string Abbreviation { get; set; } = string.Empty;

    /// <summary>Name displayed in records (≤ 20 chars).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Index of the hex the task force occupies.</summary>
    public int Pos { get; set; } = -1;

    public Posture Posture { get; set; } = new();

    /// <summary>Cached maximum speed = the lowest <c>CMaxOpSpeed</c> of any member ship.</summary>
    public OpSpeed MaxOpSpeed { get; set; } = OpSpeed.Warp3;

    public OpSpeed OpSpeed { get; set; } = OpSpeed.Stop;

    /// <summary>The hexes this TF will move to over the turn's three impulses (-1 = unset).</summary>
    public int[] TurnPlot { get; set; } = { -1, -1, -1 };

    /// <summary>Creates a task force with an id and an abbreviation defaulted from the name initials.</summary>
    public static TaskForce Create(string name, int pos, bool friendly = true)
    {
        return new TaskForce
        {
            Id = Ids.New(name),
            Name = Guard.Cap(name, 20),
            Abbreviation = DefaultAbbreviation(name),
            Pos = pos,
            Friendly = friendly,
        };
    }

    /// <summary>First letter of each whitespace-separated word in the name, capped at 5 chars.</summary>
    public static string DefaultAbbreviation(string name)
    {
        var initials = name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(word => char.ToUpperInvariant(word[0]));
        return Guard.Cap(new string(initials.ToArray()), 5);
    }

    /// <summary>Caps the length-limited string fields back to their spec maxima and fixes the plot length.</summary>
    public void Normalize()
    {
        Callsign = Guard.Cap(Callsign, 12);
        Abbreviation = Guard.Cap(Abbreviation, 5);
        Name = Guard.Cap(Name, 20);

        if (TurnPlot.Length != TurnPlotLength)
        {
            var fixedPlot = new[] { -1, -1, -1 };
            Array.Copy(TurnPlot, fixedPlot, Math.Min(TurnPlot.Length, TurnPlotLength));
            TurnPlot = fixedPlot;
        }
    }
}
