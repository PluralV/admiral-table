using AdmiralsTable.Domain.Common;
using AdmiralsTable.Domain.Fleet;

namespace AdmiralsTable.Domain.Contacts;

/// <summary>One ship-class "bucket" in a contact's identified lineup.</summary>
public sealed class ShipLineupEntry
{
    public string HullType { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>
/// A non-friendly task force encountered on the map. Knowledge is gated by <see cref="IdLevel"/>:
/// more detail (movement cost, ship count, lineup) becomes editable as the level rises. The contact
/// owns a <see cref="TaskForce"/> describing what is known of its speed/posture; that task force
/// mirrors the contact's name/abbreviation/pos via <see cref="SyncTaskForce"/>.
/// </summary>
public sealed class Contact
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AbbreviatedName { get; set; } = string.Empty;
    public IdLevel IdLevel { get; set; } = IdLevel.L0;

    /// <summary>Hex index of the contact's position, or -1 when out of sight.</summary>
    public int Pos { get; set; } = -1;

    /// <summary>Observed task force (speed, posture). Inherits name/abbreviation/pos from the contact.</summary>
    public TaskForce TaskForce { get; set; } = new();

    /// <summary>Total movement cost of the task force; meaningful at id level ≥ 1.</summary>
    public double TotalMc { get; set; }

    /// <summary>Total number of ships; meaningful at id level ≥ 2.</summary>
    public int TotalShips { get; set; }

    /// <summary>Ship count by hull type; meaningful at id level ≥ 3.</summary>
    public List<ShipLineupEntry> ShipLineup { get; set; } = new();

    /// <summary>Hex this contact was last seen at; updated whenever <see cref="Pos"/> changes.</summary>
    public int LastSeen { get; set; } = -1;

    public bool CanEditTotalMc => IdLevel.Rank() >= 1;
    public bool CanEditTotalShips => IdLevel.Rank() >= 2;
    public bool CanEditShipLineup => IdLevel.Rank() >= 3;

    public static Contact Create(string name, int pos)
    {
        var contact = new Contact
        {
            Id = Ids.New(name),
            Name = name,
            AbbreviatedName = TaskForce.DefaultAbbreviation(name),
            Pos = pos,
            TaskForce = TaskForce.Create(name, pos, friendly: false),
        };
        contact.SyncTaskForce();
        return contact;
    }

    /// <summary>Moves the contact to <paramref name="newPos"/>, recording the previous hex as last-seen.</summary>
    public void MoveTo(int newPos)
    {
        if (Pos >= 0)
        {
            LastSeen = Pos;
        }

        Pos = newPos;
        TaskForce.Pos = newPos;
    }

    /// <summary>Updates last-seen to the current hex and sets the contact out of sight (pos = -1).</summary>
    public void MoveOutOfSight() => MoveTo(-1);

    /// <summary>Pushes the contact's identity (name/abbreviation/pos) onto its observed task force.</summary>
    public void SyncTaskForce()
    {
        TaskForce.Name = Guard.Cap(Name, 20);
        TaskForce.Abbreviation = Guard.Cap(AbbreviatedName, 5);
        TaskForce.Pos = Pos;
        TaskForce.Friendly = false;
    }
}
