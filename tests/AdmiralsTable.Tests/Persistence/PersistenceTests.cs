using System.Text;
using System.Text.Json;
using AdmiralsTable.Domain.Campaign;
using AdmiralsTable.Domain.Common;
using AdmiralsTable.Domain.Contacts;
using AdmiralsTable.Domain.Fleet;
using AdmiralsTable.Domain.Maps;
using AdmiralsTable.Domain.Ships;
using AdmiralsTable.Persistence;
using AdmiralsTable.Persistence.Json;
using AdmiralsTable.Persistence.Repositories;
using CampaignModel = AdmiralsTable.Domain.Campaign.Campaign;

namespace AdmiralsTable.Tests.Persistence;

public sealed class PersistenceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "admiral-table-tests", Guid.NewGuid().ToString("N"));
    private readonly DataPaths _paths;

    public PersistenceTests()
    {
        _paths = new DataPaths(_root);
        _paths.EnsureCreated();
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void Map_SerializesCompactly_AndRebuildsAdjacency()
    {
        var specials = new Dictionary<int, IEnumerable<string>> { [4] = new[] { "nebula" } };
        var map = Map.Build(5, 6, specials);

        string json = JsonSerializer.Serialize(map, JsonStore.Options);
        var restored = JsonSerializer.Deserialize<Map>(json, JsonStore.Options)!;

        // Compact: the huge hex/adjacency array is not in the JSON.
        Assert.DoesNotContain("adjacent", json, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(map.Rows, restored.Rows);
        Assert.Equal(map.Columns, restored.Columns);
        Assert.Contains("nebula", restored.GetByIndex(4).Features);
        Assert.Equal(map.GetByIndex(10).Adjacent, restored.GetByIndex(10).Adjacent);
    }

    [Fact]
    public void Campaign_FullRoundTrip_PreservesGraphAndCrossReferences()
    {
        var template = new CampaignTemplate
        {
            Name = "PQ-17 Scenario 1",
            MapRows = 10,
            MapColumns = 10,
            SpecialHexes = { new SpecialHexSpec { Index = 12, Features = { "settlement" } } },
        };

        var campaign = CampaignModel.CreateNew("My Campaign", template);
        var tf = TaskForce.Create("Home Fleet", pos: 12);
        var ship = Ship.FromTemplate(
            new ShipTemplate { Type = "D7", MaxWarp = 30, MaxOpSpeed = OpSpeed.Warp2 },
            "IKV Foo");
        ship.CurrentTf = tf.Id;
        ship.Warp.Cur = 22;
        ship.CDrones.SetAll(type1: 4);
        var contact = Contact.Create("Convoy", pos: 30);
        contact.IdLevel = IdLevel.L2;
        contact.TotalShips = 6;
        campaign.TaskForces.Add(tf);
        campaign.Ships.Add(ship);
        campaign.Contacts.Add(contact);
        campaign.Minefields.Add(Minefield.Create("Choke Point", pos: 45, strength: 3));

        var repo = new JsonCampaignRepository(_paths);
        repo.Save(campaign);
        var loaded = repo.Load("My Campaign");

        Assert.Equal(10, loaded.Map.Rows);
        Assert.Contains("settlement", loaded.Map.GetByIndex(12).Features);

        var loadedShip = Assert.Single(loaded.Ships);
        Assert.Equal("IKV Foo", loadedShip.Name);
        Assert.Equal(22, loadedShip.Warp.Cur);
        Assert.Equal(4, loadedShip.CDrones.Get(DroneSlot.Type1));

        var loadedTf = Assert.Single(loaded.TaskForces);
        Assert.Equal(loadedTf.Id, loadedShip.CurrentTf);          // cross-reference intact
        Assert.Equal(OpSpeed.Warp2, loadedTf.MaxOpSpeed);          // recomputed on load

        var loadedContact = Assert.Single(loaded.Contacts);
        Assert.Equal(6, loadedContact.TotalShips);
        Assert.Equal(3, Assert.Single(loaded.Minefields).Strength);
    }

    [Fact]
    public void ShipTemplateRepository_SaveListGetDelete()
    {
        var repo = new JsonShipTemplateRepository(_paths);
        repo.Save(new ShipTemplate { Type = "D7B", ShipClass = "BC" });
        repo.Save(new ShipTemplate { Type = "CA", ShipClass = "Cruiser" });

        Assert.Equal(2, repo.List().Count);
        Assert.Equal("BC", repo.Get("D7B")!.ShipClass);

        repo.Delete("D7B");
        Assert.Null(repo.Get("D7B"));
        Assert.Single(repo.List());
    }

    [Fact]
    public void ShipTemplateRepository_ImportCsv_ParsesAndPersists()
    {
        const string csv = "type,hull_type,class,max_warp,reloads,max_drn_b,max_op_speed\n" +
                           "D7B,D7,Battlecruiser,30,2,1,2\n" +
                           "FF,F5,Frigate,18,2,0,1\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var repo = new JsonShipTemplateRepository(_paths);
        var imported = repo.ImportCsv(stream);

        Assert.Equal(2, imported.Count);
        var d7b = repo.Get("D7B")!;
        Assert.Equal("D7", d7b.HullType);
        Assert.Equal(30, d7b.MaxWarp);
        Assert.Equal(OpSpeed.Warp2, d7b.MaxOpSpeed);
        // reloads * (4*(A+C+G+E) + 6*B) = 2 * (0 + 6*1) = 12
        Assert.Equal(12, d7b.DroneSpace);
    }
}
