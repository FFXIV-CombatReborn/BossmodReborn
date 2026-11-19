
namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

// // // Technically I've seen two survive aetherosote, but we want the highlighting for fewer than 3 in a stack
sealed class AetherocharAetherosote(BossModule module) : Components.IconStackSpread(module, stackIcon: (uint)IconID.Aetherosote, spreadIcon: (uint)IconID.Aetherochar, stackAID: (uint)AID.Aetherosote, spreadAID: (uint)AID.Aetherochar, stackRadius: 6f, spreadRadius: 6f, minStackSize: 3, maxStackSize: 3, activationDelay: 6.5d)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        // for some reason, we can't use stackIcon and spreadIcon here, so we have to use the AID values directly
        if (iconID is (uint)IconID.Aetherosote or (uint)IconID.Aetherochar)
        {
            var raid = Raid.WithoutSlot(false, true, true);
            if (iconID == (uint)IconID.Aetherosote)
            {
                // Stack always targets healers
                foreach (var member in raid)
                {
                    if (member.Role == Role.Healer)
                    {
                        AddStack(member, WorldState.FutureTime(ActivationDelay));
                    }
                }
            }
            else
            {
                // Spread targets any 3, but we can't tell which
                foreach (var member in raid)
                {
                    if (member.Role != Role.Tank)
                    {
                        AddSpread(member, WorldState.FutureTime(ActivationDelay));
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var aid = spell.Action.ID;
        if (aid == StackAction)
        {
            var id = spell.MainTargetID;
            if (MaxCasts != 1 && Stacks.Count == 1 && Stacks.Ref(0).Target.InstanceID != id && WorldState.Actors.Find(id) is Actor newTarget) // multi hit stack target died and new target got selected
            {
                Stacks.Ref(0).Target = newTarget;
            }
            if (++CastCounter == MaxCasts)
            {
                var count = Stacks.Count;
                var stacks = CollectionsMarshal.AsSpan(Stacks);
                for (var i = 0; i < count; ++i)
                {
                    if (stacks[i].Target.InstanceID == id)
                    {
                        ++NumFinishedStacks;
                        CastCounter = 0;
                        Stacks.RemoveAt(i);
                        return;
                    }
                }
                // stack not found, probably due to being self targeted
                if (count != 0)
                {
                    ++NumFinishedStacks;
                    CastCounter = 0;
                    Stacks.RemoveAt(0);
                }
            }
        }
        else if (aid == SpreadAction)
        {
            Spreads.Clear();
        }
    }
}

sealed class AetherialRay(BossModule module) : Components.GenericBaitAway(module, (uint)AID.AetherialRay, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private static readonly AOEShapeCone cone = new(50f, 22.5f.Degrees());
    private readonly List<Actor> _trains = module.Enemies((uint)OID.GhostTrain);
    private Angle _nextRotation;
    private WPos? _nextSource;
    private DateTime _nextActivation;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Distance)
        {
            _nextRotation = status.Extra switch
            {
                0x578 => 170f.Degrees(),
                0x960 => 106f.Degrees(),
                _ => default
            };
            if (_nextRotation == default)
            {
                ReportError($"Unrecognized status extra {status.Extra:X} on Ghost Train, can't predict its final position");
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Horn)
        {
            if (_trains.Count != 0)
            {
                var center = Arena.Center;
                var angle = Angle.FromDirection(_trains[0].Position - center) + _nextRotation;
                _nextSource = center + 25f * angle.ToDirection();
            }
            _nextActivation = WorldState.FutureTime(9.7d);
        }
        else if (iconID == (uint)IconID.AetherialRay && _nextSource is WPos source)
        {
            CurrentBaits.Add(new(source, actor, cone, _nextActivation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

sealed class RunawayTrain(BossModule module) : Components.RaidwideInstant(module, (uint)AID.RunawayTrain, 15.2d, "Choo choo, the train is coming! (Raidwide)")
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.RunawayTrainVisual2)
        {
            Activation = WorldState.FutureTime(Delay);
        }
    }
}
