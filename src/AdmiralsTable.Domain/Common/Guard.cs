namespace AdmiralsTable.Domain.Common;

/// <summary>
/// Small helpers for enforcing the numeric and string invariants that recur across
/// the domain (clamped current/max values, capped string lengths).
/// </summary>
public static class Guard
{
    public static int Clamp(int value, int min, int max) =>
        value < min ? min : value > max ? max : value;

    public static double Clamp(double value, double min, double max) =>
        value < min ? min : value > max ? max : value;

    /// <summary>Trims <paramref name="value"/> to at most <paramref name="maxLength"/> characters.</summary>
    public static string Cap(string? value, int maxLength)
    {
        value ??= string.Empty;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
