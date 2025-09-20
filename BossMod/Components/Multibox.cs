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

    public float GenericMaxDistance()
    {
        return 0.75f + (2 * Arena.Bounds.MapResolution); // TODO: Arena.Bounds.MapResolution is sometimes too small, but how big is big enough??
    }

    public void AddGenericGoalDestination(AIHints hints, WPos destination, float maxWeight = 100)
    {
        hints.GoalZones.Add(hints.GoalProximity(destination, GenericMaxDistance(), maxWeight));
    }

    public void AddGenericMTNorthHint(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (assignment == PartyRolesConfig.Assignment.MT && actor.InstanceID == Raid.Player()?.InstanceID)
        {
            var pos = Module.PrimaryActor.Position + (Module.PrimaryActor.HitboxRadius * 180.Degrees().ToDirection());
            hints.GoalZones.Add(hints.GoalProximity(pos, GenericMaxDistance(), 1));
        }
    }

    // TODO: possible auto rotation integration to add goal zones for positionals
}
