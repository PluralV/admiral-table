using AdmiralsTable.Domain.Common;

namespace AdmiralsTable.Domain.Ships;

/// <summary>
/// An integer ship resource tracked as three values: <see cref="Max"/> (the template
/// ceiling), <see cref="CMax"/> (current ceiling after damage) and <see cref="Cur"/>
/// (current amount). Invariant: 0 ≤ Cur ≤ CMax ≤ Max, enforced by <see cref="Normalize"/>.
/// </summary>
public sealed class Gauge
{
    public Gauge()
    {
    }

    public Gauge(int max)
    {
        Max = CMax = Cur = max;
    }

    public int Max { get; set; }
    public int CMax { get; set; }
    public int Cur { get; set; }

    public void Normalize()
    {
        if (Max < 0)
        {
            Max = 0;
        }

        CMax = Guard.Clamp(CMax, 0, Max);
        Cur = Guard.Clamp(Cur, 0, CMax);
    }

    public Gauge Clone() => new() { Max = Max, CMax = CMax, Cur = Cur };
}

/// <summary>Floating-point counterpart of <see cref="Gauge"/> (fuel, supplies).</summary>
public sealed class GaugeF
{
    public GaugeF()
    {
    }

    public GaugeF(double max)
    {
        Max = CMax = Cur = max;
    }

    public double Max { get; set; }
    public double CMax { get; set; }
    public double Cur { get; set; }

    public void Normalize()
    {
        if (Max < 0)
        {
            Max = 0;
        }

        CMax = Guard.Clamp(CMax, 0, Max);
        Cur = Guard.Clamp(Cur, 0, CMax);
    }

    public GaugeF Clone() => new() { Max = Max, CMax = CMax, Cur = Cur };
}
