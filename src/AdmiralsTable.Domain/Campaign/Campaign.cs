using AdmiralsTable.Domain.Contacts;
using AdmiralsTable.Domain.Fleet;
using AdmiralsTable.Domain.Maps;
using AdmiralsTable.Domain.Ships;

namespace AdmiralsTable.Domain.Campaign;

/// <summary>
/// The top-level campaign aggregate. Every piece of campaign state hangs off this object, which is
/// loaded, edited in memory, and serialized (or discarded) as a single unit. Future modules
/// (Campaign Management) attach to <see cref="Meta"/>.
/// </summary>
public sealed class Campaign
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Serialized compactly (rows/columns/specials) via a converter; rebuilt on load.</summary>
    public Map Map { get; set; } = Map.Build(1, 1);

    public List<TaskForce> TaskForces { get; set; } = new();
    public List<Ship> Ships { get; set; } = new();
    public List<Minefield> Minefields { get; set; } = new();
    public List<Contact> Contacts { get; set; } = new();

    public CampaignMeta Meta { get; set; } = new();
    public CampaignConfig Config { get; set; } = new();

    /// <summary>Creates an empty campaign from a template (builds the map and applies special hexes).</summary>
    public static Campaign CreateNew(string name, CampaignTemplate template)
    {
        var specials = template.SpecialHexes.ToDictionary(
            h => h.Index,
            h => (IEnumerable<string>)h.Features);

        return new Campaign
        {
            Name = name,
            Map = Map.Build(template.MapRows, template.MapColumns, specials),
            Config = new CampaignConfig { TemplateName = template.Name },
        };
    }

    /// <summary>
    /// Re-applies every domain invariant after a load or a batch of edits: clamps ship gauges,
    /// caps task-force/minefield fields, syncs contact identities, and refreshes TF speed caches.
    /// </summary>
    public void NormalizeAll()
    {
        var fleet = new FleetService();

        foreach (var ship in Ships)
        {
            ship.Normalize();
        }

        foreach (var tf in TaskForces)
        {
            tf.Normalize();
        }

        foreach (var minefield in Minefields)
        {
            minefield.Normalize();
        }

        foreach (var contact in Contacts)
        {
            contact.SyncTaskForce();
            contact.TaskForce.Normalize();
        }

        fleet.RecomputeAll(TaskForces, Ships);
    }
}
