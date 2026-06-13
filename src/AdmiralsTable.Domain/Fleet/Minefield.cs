using AdmiralsTable.Domain.Common;

namespace AdmiralsTable.Domain.Fleet;

/// <summary>An operational minefield placed on a hex, strength 1-4.</summary>
public sealed class Minefield
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Pos { get; set; } = -1;
    public int Strength { get; set; } = 1;

    public static Minefield Create(string name, int pos, int strength)
    {
        return new Minefield
        {
            Id = Ids.New(name),
            Name = Guard.Cap(name, 20),
            Pos = pos,
            Strength = Guard.Clamp(strength, 1, 4),
        };
    }

    public void Normalize()
    {
        Name = Guard.Cap(Name, 20);
        Strength = Guard.Clamp(Strength, 1, 4);
    }
}
