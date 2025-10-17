using BossMod.AI;

namespace BossMod.Components;

public class MultiboxComponent(BossModule module) : BossComponent(module)
{
    internal readonly AIConfig _config = Service.Config.Get<AIConfig>();
    internal readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();

    internal static class Cardinal
    {
        public static readonly Angle North = 0.Degrees();
        public static readonly Angle NorthEast = 45.Degrees();
        public static readonly Angle East = 90.Degrees();
        public static readonly Angle SouthEast = 135.Degrees();
        public static readonly Angle South = 180.Degrees();
        public static readonly Angle SouthWest = 225.Degrees();
        public static readonly Angle West = 270.Degrees();
        public static readonly Angle NorthWest = 315.Degrees();
    }

    public float GenericMaxDistance() => 0.75f + (2 * Arena.Bounds.MapResolution); // TODO: Arena.Bounds.MapResolution is sometimes too small, but how big is big enough??

    public void AddGenericGoalDestination(AIHints hints, WPos destination, float maxWeight = 100) => hints.GoalZones.Add(AIHints.GoalProximity(destination, GenericMaxDistance(), maxWeight));

    public void AddGenericMTNorthHint(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (assignment == PartyRolesConfig.Assignment.MT && actor.InstanceID == Raid.Player()?.InstanceID)
        {
            var pos = Module.PrimaryActor.Position + (Module.PrimaryActor.HitboxRadius * 180.Degrees().ToDirection());
            hints.GoalZones.Add(AIHints.GoalProximity(pos, GenericMaxDistance(), 1));
        }
    }

    public Dictionary<PartyRolesConfig.Assignment, WDir> GenericSpreadAroundBoss(List<PartyRolesConfig.Assignment> activeRoles, float baseRadius, bool useAbsoluteNorth, Angle? bossRotation = null)
    {
        var positions = new Dictionary<PartyRolesConfig.Assignment, WDir>();
        float rangedDistance = baseRadius * 1.5f;

        if (activeRoles.Count == 0)
            return positions;

        var rotation = useAbsoluteNorth ? 0.Degrees() : (bossRotation ?? Module.PrimaryActor.Rotation);
        var tanks = activeRoles.Where(r => r is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT).OrderBy(r => (int)r).ToList();
        var melees = activeRoles.Where(r => r is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2).OrderBy(r => (int)r).ToList();
        var ranged = activeRoles.Where(r => r is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2 or PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2).OrderBy(r => (int)r).ToList();
        var meleeCount = tanks.Count + melees.Count;
        var rangedCount = ranged.Count;

        // MT is always at boss facing direction if present
        if (tanks.Contains(PartyRolesConfig.Assignment.MT))
        {
            positions[PartyRolesConfig.Assignment.MT] = baseRadius * (rotation + Cardinal.North).ToDirection();
        }

        if (meleeCount == 4) // All melee comp (ex: MT/OT/M1/M2)
        {
            // MT front, OT under boss, M1/M2 at SW/SE for positionals
            if (tanks.Contains(PartyRolesConfig.Assignment.OT))
                positions[PartyRolesConfig.Assignment.OT] = new WDir(0, 0); // Under boss
            if (melees.Contains(PartyRolesConfig.Assignment.M1))
                positions[PartyRolesConfig.Assignment.M1] = baseRadius * (rotation + Cardinal.SouthWest).ToDirection();
            if (melees.Contains(PartyRolesConfig.Assignment.M2))
                positions[PartyRolesConfig.Assignment.M2] = baseRadius * (rotation + Cardinal.SouthEast).ToDirection();
        }
        else if (meleeCount == 3 && rangedCount == 1) // 3 melee + 1 ranged
        {
            // the single ranged under the boss
            if (ranged.Count > 0)
                positions[ranged[0]] = new WDir(0, 0); // Under boss

            // melees at SW/SE
            var meleesToPlace = new List<PartyRolesConfig.Assignment>();
            if (tanks.Contains(PartyRolesConfig.Assignment.OT))
                meleesToPlace.Add(PartyRolesConfig.Assignment.OT);
            meleesToPlace.AddRange(melees);

            if (meleesToPlace.Count >= 1)
                positions[meleesToPlace[0]] = baseRadius * (rotation + Cardinal.SouthWest).ToDirection();
            if (meleesToPlace.Count >= 2)
                positions[meleesToPlace[1]] = baseRadius * (rotation + Cardinal.SouthEast).ToDirection();
        }
        else if (meleeCount == 2 && rangedCount == 2) // Standard comp (2 melee, 2 ranged)
        {
            // non-MT melee behind boss
            if (tanks.Contains(PartyRolesConfig.Assignment.OT))
                positions[PartyRolesConfig.Assignment.OT] = baseRadius * (rotation + Cardinal.South).ToDirection();
            else if (melees.Count > 0)
                positions[melees[0]] = baseRadius * (rotation + Cardinal.South).ToDirection();

            // ranged at Northern intercardinals
            if (ranged.Count >= 1)
            {
                var r1 = ranged.FirstOrDefault(r => r is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.H1);
                if (r1 != default)
                    positions[r1] = rangedDistance * (rotation + Cardinal.NorthWest).ToDirection();
                else
                    positions[ranged[0]] = rangedDistance * (rotation + Cardinal.NorthWest).ToDirection();
            }
            if (ranged.Count >= 2)
            {
                var r2 = ranged.FirstOrDefault(r => r is PartyRolesConfig.Assignment.R2 or PartyRolesConfig.Assignment.H2);
                if (r2 != default)
                    positions[r2] = rangedDistance * (rotation + Cardinal.NorthEast).ToDirection();
                else if (positions.Values.Any(p => p.AlmostEqual(rangedDistance * (rotation + Cardinal.NorthWest).ToDirection(), 0.1f)))
                    positions[ranged[1]] = rangedDistance * (rotation + Cardinal.NorthEast).ToDirection();
                else
                    positions[ranged[0]] = rangedDistance * (rotation + Cardinal.NorthEast).ToDirection();
            }
        }
        else // 1 melee, 3 ranged or anything else - cardinals
        {
            // melee at front
            if (tanks.Contains(PartyRolesConfig.Assignment.OT))
                positions[PartyRolesConfig.Assignment.OT] = baseRadius * (rotation + Cardinal.North).ToDirection();
            else if (melees.Count > 0)
                positions[melees[0]] = baseRadius * (rotation + Cardinal.North).ToDirection();

            // ranged at other cardinals
            var cardinals = new[] { Cardinal.South, Cardinal.West, Cardinal.East };
            for (int index = 0; index < Math.Min(ranged.Count, 3); index++)
            {
                positions[ranged[index]] = baseRadius * (rotation + cardinals[index]).ToDirection();
            }
        }

        return positions;
    }

    // TODO: possible auto rotation integration to add goal zones for positionals
}
