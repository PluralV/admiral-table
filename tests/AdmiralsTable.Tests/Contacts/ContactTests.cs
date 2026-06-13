using AdmiralsTable.Domain.Common;
using AdmiralsTable.Domain.Contacts;

namespace AdmiralsTable.Tests.Contacts;

public class ContactTests
{
    [Fact]
    public void Create_SetsIdAndSyncsTaskForce()
    {
        var contact = Contact.Create("Red Squadron", pos: 7);

        Assert.False(string.IsNullOrEmpty(contact.Id));
        Assert.Equal("Red Squadron", contact.TaskForce.Name);
        Assert.Equal("RS", contact.TaskForce.Abbreviation);
        Assert.Equal(7, contact.TaskForce.Pos);
        Assert.False(contact.TaskForce.Friendly);
    }

    [Fact]
    public void MoveTo_RecordsPreviousHexAsLastSeen()
    {
        var contact = Contact.Create("Ghost", pos: 7);
        contact.MoveTo(10);

        Assert.Equal(10, contact.Pos);
        Assert.Equal(7, contact.LastSeen);
        Assert.Equal(10, contact.TaskForce.Pos);
    }

    [Fact]
    public void MoveOutOfSight_StoresPositionAndClearsPos()
    {
        var contact = Contact.Create("Ghost", pos: 7);
        contact.MoveOutOfSight();

        Assert.Equal(-1, contact.Pos);
        Assert.Equal(7, contact.LastSeen);
    }

    [Theory]
    [InlineData(IdLevel.L0, false, false, false)]
    [InlineData(IdLevel.L1, true, false, false)]
    [InlineData(IdLevel.L2, true, true, false)]
    [InlineData(IdLevel.L3, true, true, true)]
    [InlineData(IdLevel.L4, true, true, true)]
    public void EditGating_FollowsIdLevel(IdLevel level, bool mc, bool ships, bool lineup)
    {
        var contact = Contact.Create("X", pos: 0);
        contact.IdLevel = level;

        Assert.Equal(mc, contact.CanEditTotalMc);
        Assert.Equal(ships, contact.CanEditTotalShips);
        Assert.Equal(lineup, contact.CanEditShipLineup);
    }
}
