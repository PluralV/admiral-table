namespace AdmiralsTable.Domain.Common;

/// <summary>
/// Identification level of an enemy contact (rules: U, 0, 1, 2, 3, 4). Higher
/// levels unlock more editable fields on the contact. Most contacts start at L0.
/// </summary>
public enum IdLevel
{
    Unknown,
    L0,
    L1,
    L2,
    L3,
    L4,
}

public static class IdLevelExtensions
{
    public static string Display(this IdLevel level) => level switch
    {
        IdLevel.Unknown => "U",
        IdLevel.L0 => "0",
        IdLevel.L1 => "1",
        IdLevel.L2 => "2",
        IdLevel.L3 => "3",
        IdLevel.L4 => "4",
        _ => level.ToString(),
    };

    public static IdLevel FromDisplay(string label) => label.Trim().ToUpperInvariant() switch
    {
        "U" => IdLevel.Unknown,
        "0" => IdLevel.L0,
        "1" => IdLevel.L1,
        "2" => IdLevel.L2,
        "3" => IdLevel.L3,
        "4" => IdLevel.L4,
        _ => throw new ArgumentOutOfRangeException(nameof(label), label, "Unknown id level label."),
    };

    /// <summary>Numeric rank (Unknown = -1, L0..L4 = 0..4) for the id_level &gt;= n gating rules.</summary>
    public static int Rank(this IdLevel level) => level == IdLevel.Unknown ? -1 : (int)level - 1;
}
