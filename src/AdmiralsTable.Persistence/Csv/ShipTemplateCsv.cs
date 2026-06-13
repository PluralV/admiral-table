using System.Globalization;
using System.Text;
using AdmiralsTable.Domain.Common;
using AdmiralsTable.Domain.Ships;

namespace AdmiralsTable.Persistence.Csv;

/// <summary>
/// Parses ship templates from a header-based CSV (the SHIP ANNEX "import CSV" feature). Header names
/// are matched case-, space- and underscore-insensitively; unknown columns and the derived
/// drone/add space columns are ignored. Each row needs at least a non-empty <c>type</c>.
/// </summary>
public static class ShipTemplateCsv
{
    public static IReadOnlyList<ShipTemplate> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        if (lines.Count < 2)
        {
            return Array.Empty<ShipTemplate>();
        }

        string[] headers = SplitLine(lines[0]).Select(Normalize).ToArray();
        var templates = new List<ShipTemplate>();

        for (int row = 1; row < lines.Count; row++)
        {
            var cells = SplitLine(lines[row]);
            var template = new ShipTemplate();
            for (int col = 0; col < headers.Length && col < cells.Count; col++)
            {
                ApplyField(template, headers[col], cells[col].Trim());
            }

            if (!string.IsNullOrWhiteSpace(template.Type))
            {
                templates.Add(template);
            }
        }

        return templates;
    }

    private static void ApplyField(ShipTemplate t, string header, string value)
    {
        switch (header)
        {
            case "type": t.Type = value; break;
            case "hulltype": t.HullType = value; break;
            case "isbase": t.IsBase = Bool(value); break;
            case "class": t.ShipClass = value; break;
            case "sc": t.Sc = Int(value); break;
            case "faction": t.Faction = value; break;
            case "bpvcost": t.BpvCost = Int(value); break;
            case "movecost": t.MoveCost = Double(value); break;
            case "maxwarp": t.MaxWarp = Int(value); break;
            case "maximp": t.MaxImp = Int(value); break;
            case "maxapr": t.MaxApr = Int(value); break;
            case "maxawr": t.MaxAwr = Int(value); break;
            case "maxscout": t.MaxScout = Int(value); break;
            case "maxdamcon": t.MaxDamcon = Int(value); break;
            case "maxsensor": t.MaxSensor = Int(value); break;
            case "maxdrna": t.MaxDrnA = Int(value); break;
            case "maxdrnb": t.MaxDrnB = Int(value); break;
            case "maxdrnc": t.MaxDrnC = Int(value); break;
            case "maxdrng": t.MaxDrnG = Int(value); break;
            case "maxdrne": t.MaxDrnE = Int(value); break;
            case "reloads": t.Reloads = Int(value); break;
            case "maxdrnadd": t.MaxDrnAdd = Int(value); break;
            case "addreloads": t.AddReloads = Int(value); break;
            case "maxshuttles": t.MaxShuttles = Int(value); break;
            case "maxfuel": t.MaxFuel = Double(value); break;
            case "maxsupplies": t.MaxSupplies = Double(value); break;
            case "minespace": t.MineSpace = Double(value); break;
            case "maxopspeed": t.MaxOpSpeed = OpSpeedValue(value); break;
            case "special": t.Special = Bool(value); break;
            case "command": t.Command = Bool(value); break;
            case "dockingpoints": t.DockingPoints = Int(value); break;
            default: break; // unknown / derived column
        }
    }

    /// <summary>Lowercases and strips everything but letters and digits for fuzzy header matching.</summary>
    private static string Normalize(string header)
    {
        var sb = new StringBuilder(header.Length);
        foreach (char c in header)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString();
    }

    private static int Int(string v) => int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out int r) ? r : 0;

    private static double Double(string v) => double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out double r) ? r : 0;

    private static bool Bool(string v) =>
        v.Equals("true", StringComparison.OrdinalIgnoreCase) ||
        v.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
        v == "1";

    private static OpSpeed OpSpeedValue(string v)
    {
        try
        {
            return OpSpeedExtensions.FromDisplay(v);
        }
        catch (ArgumentOutOfRangeException)
        {
            return Enum.TryParse<OpSpeed>(v, ignoreCase: true, out var parsed) ? parsed : OpSpeed.Warp3;
        }
    }

    /// <summary>Splits one CSV line, honoring double-quoted fields and escaped ("") quotes.</summary>
    private static List<string> SplitLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }
}
