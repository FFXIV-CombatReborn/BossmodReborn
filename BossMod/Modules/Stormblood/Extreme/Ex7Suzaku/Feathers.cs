namespace BossMod.Stormblood.Extreme.Ex7Suzaku;

sealed class ScarletPlumeTailFeather(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(5);
    private static readonly AOEShapeCircle circle = new(9f);
    private static readonly uint[] _feathers = [(uint)OID.ScarletTailFeather, (uint)OID.ScarletPlume];
    private readonly int party = module.Raid.WithoutSlot(true, false, false).Length;
    private readonly RekindleP1 _spread = module.FindComponent<RekindleP1>()!;
    private BitMask _target;
    private readonly List<WPos> plumeCache = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WingAndAPrayerTailFeather)
            AOEs.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WingAndAPrayerTailFeather)
            AOEs.Clear();
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.PrimaryTarget && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _target[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.PrimaryTarget && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _target[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ScarletPlume)
        {
            plumeCache.Add(actor.Position);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        var feathers = Module.Enemies((uint)OID.ScarletTailFeather);
        var count = feathers.Count;
        if (count != 0)
        {
            for (var i = 0; i < count; ++i)
            {
                if (feathers[i].IsTargetable)
                {
                    hints.Add("Kill birds outside of AOEs!");
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var birds = Module.Enemies((uint)OID.ScarletLady);
        var feathers = Module.Enemies(_feathers);
        var countB = birds.Count;
        var countF = feathers.Count;

        var countT = hints.PotentialTargets.Count;
        for (var i = 0; i < countT; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.ScarletTailFeather => AIHints.Enemy.PriorityInvincible,
                (uint)OID.ScarletPlume => 1,
                _ => 0
            };
        }

        for (var i = 0; i < countB; ++i)
        {
            var b = birds[i];
            if (b.IsDead)
                continue;
            var notInAOE = true;
            for (var j = 0; j < countF; ++j)
            {
                var f = feathers[j];
                if (f.IsDead)
                    continue;
                if (b.Position.InCircle(f.Position, 9f))
                {
                    notInAOE = false;
                    hints.SetPriority(b, AIHints.Enemy.PriorityForbidden);
                    break;
                }
            }
            if (notInAOE)
                hints.SetPriority(b, 1);
        }
        if (_spread.IsSpreadTarget(actor))
        {
            if (party == 8)
            {
                for (var i = 0; i < countB; ++i)
                {
                    var b = birds[i];
                    if (!b.IsDead)
                        continue;
                    hints.GoalZones.Add(AIHints.GoalProximity(b.Position, 7.12f, 100f));
                }
            }
            else // unsync we can just take all 4 birds
            {
                hints.GoalZones.Add(AIHints.GoalProximity(Arena.Center, 7.12f, 100f));
            }
        }
        else if (_target[slot])
        {
            var count = plumeCache.Count;
            for (var i = 0; i < count; ++i)
            {
                hints.GoalZones.Add(AIHints.GoalProximity(Ex7Suzaku.ArenaCenter - 20f * (plumeCache[i] - Ex7Suzaku.ArenaCenter).Normalized(), 5f, 100f));
            }
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
