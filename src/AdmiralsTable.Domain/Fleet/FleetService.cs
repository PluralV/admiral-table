using AdmiralsTable.Domain.Common;
using AdmiralsTable.Domain.Maps;
using AdmiralsTable.Domain.Ships;

namespace AdmiralsTable.Domain.Fleet;

/// <summary>Outcome of a reassignment check.</summary>
public readonly record struct ReassignResult(bool Allowed, string? Reason)
{
    public static ReassignResult Ok() => new(true, null);

    public static ReassignResult No(string reason) => new(false, reason);
}

/// <summary>
/// Stateless operations over task forces and their member ships: speed caching, reassignment rules
/// (co-location and base constraints), turn-plot adjacency, and shadowing. Collections are passed in
/// so the service stays easy to test and free of campaign state.
/// </summary>
public sealed class FleetService
{
    /// <summary>Ships that currently belong to <paramref name="tf"/>.</summary>
    public IEnumerable<Ship> MembersOf(TaskForce tf, IEnumerable<Ship> ships) =>
        ships.Where(s => s.CurrentTf == tf.Id);

    public bool ContainsBase(TaskForce tf, IEnumerable<Ship> ships) =>
        MembersOf(tf, ships).Any(s => s.IsBase);

    /// <summary>
    /// Sets <paramref name="tf"/>.MaxOpSpeed to the slowest member ship's CMaxOpSpeed (Warp3 if empty)
    /// and clamps the current OpSpeed to that ceiling.
    /// </summary>
    public void RecomputeMaxOpSpeed(TaskForce tf, IEnumerable<Ship> ships)
    {
        var members = MembersOf(tf, ships).ToList();
        tf.MaxOpSpeed = members.Count == 0 ? OpSpeed.Warp3 : members.Min(s => s.CMaxOpSpeed);

        if (tf.OpSpeed > tf.MaxOpSpeed)
        {
            tf.OpSpeed = tf.MaxOpSpeed;
        }
    }

    public void RecomputeAll(IEnumerable<TaskForce> taskForces, IEnumerable<Ship> ships)
    {
        var shipList = ships as ICollection<Ship> ?? ships.ToList();
        foreach (var tf in taskForces)
        {
            RecomputeMaxOpSpeed(tf, shipList);
        }
    }

    /// <summary>
    /// Whether <paramref name="ship"/> may be reassigned from <paramref name="source"/> into the
    /// existing <paramref name="target"/>: target must share the source's hex, a base may never join
    /// another task force, and no ship may join a base's task force.
    /// </summary>
    public ReassignResult CanReassign(Ship ship, TaskForce source, TaskForce target, IEnumerable<Ship> ships)
    {
        if (target.Id == source.Id)
        {
            return ReassignResult.No("Ship is already in that task force.");
        }

        if (target.Pos != source.Pos)
        {
            return ReassignResult.No("Target task force is in a different hex.");
        }

        if (ship.IsBase)
        {
            return ReassignResult.No("A base must stay in its own task force.");
        }

        if (ContainsBase(target, ships))
        {
            return ReassignResult.No("Cannot add ships to a base's task force.");
        }

        return ReassignResult.Ok();
    }

    /// <summary>Moves <paramref name="ship"/> into <paramref name="target"/> and refreshes speed caches.</summary>
    public ReassignResult Reassign(Ship ship, TaskForce source, TaskForce target, IReadOnlyList<TaskForce> taskForces, IReadOnlyList<Ship> ships)
    {
        var check = CanReassign(ship, source, target, ships);
        if (!check.Allowed)
        {
            return check;
        }

        ship.CurrentTf = target.Id;
        RecomputeMaxOpSpeed(source, ships);
        RecomputeMaxOpSpeed(target, ships);
        return ReassignResult.Ok();
    }

    /// <summary>
    /// Creates a new task force at the ship's current hex and moves the ship into it. Used by the
    /// REASSIGN "create new task force" option; this is also the only way a base changes task force.
    /// </summary>
    public TaskForce MoveToNewTaskForce(Ship ship, string name, TaskForce source, ICollection<TaskForce> taskForces, IReadOnlyList<Ship> ships)
    {
        var tf = TaskForce.Create(name, source.Pos, source.Friendly);
        taskForces.Add(tf);
        ship.CurrentTf = tf.Id;
        RecomputeMaxOpSpeed(source, ships);
        RecomputeMaxOpSpeed(tf, ships);
        return tf;
    }

    /// <summary>
    /// Candidate hexes for turn-plot cell <paramref name="cell"/> (0-2): hexes adjacent to the TF
    /// position for cell 0, or adjacent to the previous plotted hex otherwise. Empty while shadowing
    /// (the plot is locked) or until the previous cell is set.
    /// </summary>
    public IReadOnlyList<int> TurnPlotOptions(Map map, TaskForce tf, int cell)
    {
        if (tf.Posture.Shadowing || cell < 0 || cell >= TaskForce.TurnPlotLength)
        {
            return Array.Empty<int>();
        }

        int from = cell == 0 ? tf.Pos : tf.TurnPlot[cell - 1];
        if (from < 0 || from >= map.Size)
        {
            return Array.Empty<int>();
        }

        return map.GetByIndex(from).ValidNeighbors().ToList();
    }

    /// <summary>
    /// Begins shadowing a contact: locks speed to the contact's op speed and clears the turn plot.
    /// </summary>
    public void SetShadow(TaskForce tf, string contactId, OpSpeed contactOpSpeed)
    {
        tf.Posture.Shadowing = true;
        tf.Posture.ShadowTargetId = contactId;
        tf.OpSpeed = contactOpSpeed;
        tf.TurnPlot = new[] { -1, -1, -1 };
    }

    public void ClearShadow(TaskForce tf)
    {
        tf.Posture.Shadowing = false;
        tf.Posture.ShadowTargetId = null;
    }
}
