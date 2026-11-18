namespace BossMod.Components;

// utility component for encounters where a stack/spread mechanic has predefined safe spots
// the generic solver has no chance of saving the player when the entire party has spreads and is trying to dodge away from each other
// subclasses return a (destination, radius) pair per stack/spread target; radius 0 means "stand exactly here", null means "use generic behaviour" for that target
// (generic: a spread target moves away from everyone else and the rest avoid it; a stack target looks for a partner and the rest join any stack with room)
public abstract class StrictStackSpread(BossModule module, bool raidwideOnResolve = true, bool includeDeadTargets = false) : GenericStackSpread(module, raidwideOnResolve, includeDeadTargets)
{
    // zones that position a player relative to other targets (keep away from them, join their stack) are tagged with this, so that players with their own destination can drop them; high bits keep it clear of actor instance IDs
    private ulong Tag => 0xF000000000000000ul | (uint)RuntimeHelpers.GetHashCode(this);
    private readonly List<ShapeDistance> _joinZones = [];
    private readonly List<ShapeDistance> _buddyZones = [];

    public virtual (WPos, float)? GetStackDestination(int order, Actor target) => null;
    public virtual (WPos, float)? GetSpreadDestination(int order, Actor target) => null;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!EnableHints)
        {
            return;
        }

        var tag = Tag;
        var hasDestination = false;
        var isSpreadTarget = IsSpreadTarget(actor);
        var isStackTarget = IsStackTarget(actor);
        (int, Actor)[]? party = null; // only needed for targets without a destination

        var spreads = CollectionsMarshal.AsSpan(Spreads);
        var order = 0;
        for (var i = 0; i < spreads.Length; ++i)
        {
            ref var spread = ref spreads[i];
            var target = spread.Target;
            if (!IncludeDeadTargets && target.IsDead)
            {
                continue;
            }
            var ix = order++;
            if (RaidwideOnResolve)
            {
                hints.AddPredictedDamage(SlotMask(target), spread.Activation);
            }

            if (GetSpreadDestination(ix, target) is var (dest, destRadius))
            {
                if (target == actor)
                {
                    hasDestination = true;
                    hints.AddForbiddenZone(DestinationZone(dest, destRadius, actor), spread.Activation);
                }
                else
                {
                    hints.AddForbiddenZone(new SDCircle(dest, spread.Radius + MathF.Max(destRadius, 0.1f)), spread.Activation, tag);
                }
            }
            else if (target == actor)
            {
                // generic: get away from everyone who isn't busy with a spread of their own
                party ??= Raid.WithSlot(IncludeDeadTargets);
                var radius = spread.Radius + ExtraAISpreadThreshold;
                for (var j = 0; j < party.Length; ++j)
                {
                    var p = party[j].Item2;
                    if (p != actor && !IsSpreadTarget(p))
                    {
                        hints.AddForbiddenZone(new SDCircle(p.Position, radius), spread.Activation, tag);
                    }
                }
            }
            else
            {
                // generic: stay out of the spread
                hints.AddForbiddenZone(new SDCircle(target.Position, spread.Radius + ExtraAISpreadThreshold), spread.Activation, tag);
            }
        }

        // a player who should stack gets a single "inside any of these stacks" zone; one inverted circle per stack would demand standing in all of them at once
        _joinZones.Clear();
        var joinActivation = DateTime.MaxValue;
        var stacks = CollectionsMarshal.AsSpan(Stacks);
        order = 0;
        for (var i = 0; i < stacks.Length; ++i)
        {
            ref var stack = ref stacks[i];
            var target = stack.Target;
            if (!IncludeDeadTargets && target.IsDead)
            {
                continue;
            }
            var ix = order++;
            if (GetStackDestination(ix, target) is var (dest, destRadius))
            {
                if (RaidwideOnResolve)
                {
                    hints.AddPredictedDamage(Raid.WithSlot().MaskInRadius(dest, stack.Radius), stack.Activation, AIHints.PredictedDamageType.Shared);
                }

                var margin = MathF.Max(destRadius, 0.1f);
                if (target == actor)
                {
                    hasDestination = true;
                    hints.AddForbiddenZone(DestinationZone(dest, destRadius, actor), stack.Activation);
                }
                else if (stack.ForbiddenPlayers[slot] || isSpreadTarget || isStackTarget)
                {
                    hints.AddForbiddenZone(new SDCircle(dest, stack.Radius + margin), stack.Activation, tag);
                }
                else
                {
                    _joinZones.Add(new SDInvertedCircle(dest, stack.Radius - margin));
                    joinActivation = stack.Activation < joinActivation ? stack.Activation : joinActivation;
                }
                continue;
            }

            if (RaidwideOnResolve)
            {
                hints.AddPredictedDamage(Raid.WithSlot().MaskInRadius(target.Position, stack.Radius), stack.Activation, AIHints.PredictedDamageType.Shared);
            }

            if (target == actor)
            {
                // generic: try to get next to someone who isn't a target of anything else
                party ??= Raid.WithSlot(IncludeDeadTargets);
                _buddyZones.Clear();
                for (var j = 0; j < party.Length; ++j)
                {
                    var (s, p) = party[j];
                    if (p != actor && !stack.ForbiddenPlayers[s] && !IsSpreadTarget(p) && !IsStackTarget(p))
                    {
                        _buddyZones.Add(new SDInvertedCircle(p.Position, stack.Radius * 0.5f));
                    }
                }
                if (_buddyZones.Count != 0)
                {
                    hints.AddForbiddenZone(new SDIntersection([.. _buddyZones]), stack.Activation, tag);
                }
            }
            else if (stack.ForbiddenPlayers[slot] || isSpreadTarget || isStackTarget)
            {
                // generic: stay out; another stack's target keeps double distance so the two stacks don't overlap
                hints.AddForbiddenZone(new SDCircle(target.Position, isStackTarget ? 2f * stack.Radius : stack.Radius), stack.Activation, tag);
            }
            else
            {
                // generic: join if there is room, otherwise stay out
                var numInside = stack.NumInside(Module);
                if (stack.IsInside(actor) ? numInside <= stack.MaxSize : numInside < stack.MaxSize)
                {
                    _joinZones.Add(new SDInvertedCircle(target.Position, stack.Radius));
                    joinActivation = stack.Activation < joinActivation ? stack.Activation : joinActivation;
                }
                else
                {
                    hints.AddForbiddenZone(new SDCircle(target.Position, stack.Radius), stack.Activation, tag);
                }
            }
        }

        var joinCount = _joinZones.Count;
        if (joinCount == 1)
        {
            hints.AddForbiddenZone(_joinZones[0], joinActivation, tag);
        }
        else if (joinCount > 1)
        {
            hints.AddForbiddenZone(new SDOutsideOfUnion([.. _joinZones]), joinActivation, tag);
        }

        // players that have their own destination ignore the tagged zones (keeping away from or joining other targets)
        if (hasDestination)
        {
            var zones = hints.ForbiddenZones;
            for (var i = zones.Count - 1; i >= 0; --i)
            {
                if (zones[i].Source == tag)
                {
                    zones.RemoveAt(i);
                }
            }
        }
    }

    private ShapeDistance DestinationZone(WPos dest, float destRadius, Actor actor)
        => destRadius == 0f ? new SDPrecisePosition(dest, new(0f, 1f), Arena.Bounds.MapResolution, actor.Position, 0.1f) : new SDInvertedCircle(dest, destRadius);

    private BitMask SlotMask(Actor target)
    {
        var slot = Raid.FindSlot(target.InstanceID);
        return slot >= 0 ? BitMask.Build(slot) : default;
    }
}

public abstract class StrictUniformStackSpread(BossModule module, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool raidwideOnResolve = true, bool includeDeadTargets = false)
    : StrictStackSpread(module, raidwideOnResolve, includeDeadTargets)
{
    public float StackRadius = stackRadius;
    public float SpreadRadius = spreadRadius;
    public int MinStackSize = minStackSize;
    public int MaxStackSize = maxStackSize;

    public void AddStack(Actor target, DateTime activation = default, BitMask forbiddenPlayers = default) => Stacks.Add(new(target, StackRadius, MinStackSize, MaxStackSize, activation, forbiddenPlayers));
    public void AddSpread(Actor target, DateTime activation = default) => Spreads.Add(new(target, SpreadRadius, activation));
}
