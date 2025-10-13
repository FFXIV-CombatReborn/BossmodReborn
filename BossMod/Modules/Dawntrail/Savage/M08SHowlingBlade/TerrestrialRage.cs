namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class HeavensearthSuspendedStone(BossModule module) : Components.IconStackSpread(module, (uint)IconID.Heavensearth, (uint)IconID.SuspendedStone, (uint)AID.Heavensearth, (uint)AID.SuspendedStone, 6f, 6f, 5.1d, 4, 4)
{
    private BitMask forbidden;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == SpreadIcon) // stack and spreads can appear in any order during the same frame
        {
            AddSpread(actor, WorldState.FutureTime(ActivationDelay));
            forbidden.Set(Raid.FindSlot(targetID));
            if (Stacks.Count != 0)
            {
                Stacks.Ref(0).ForbiddenPlayers = forbidden;
            }
        }
        else if (iconID == StackIcon)
        {
            AddStack(actor, WorldState.FutureTime(ActivationDelay), forbidden);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == SpreadAction)
        {
            forbidden = default;
        }
    }
}

sealed class FangedCharge(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40f, 3f);
    private readonly List<AOEInstance> _aoes = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref readonly var aoe0 = ref aoes[0];
        var deadline = aoe0.Activation.AddSeconds(1d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D2 && actor.OID == (uint)OID.GleamingFang2)
        {
            AddAOE();
            AddAOE(180f.Degrees());
        }
        void AddAOE(Angle offset = default)
        => _aoes.Add(new(rect, actor.Position.Quantized(), actor.Rotation + offset, WorldState.FutureTime(6d)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FangedCharge)
        {
            var count = _aoes.Count - 1;
            var pos = caster.Position;
            for (var i = count; i >= 0; --i)
            {
                if (_aoes[i].Origin.AlmostEqual(pos, 1f))
                {
                    _aoes.RemoveAt(i);
                }
            }
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.FangedCharge)
        {
            _aoes.RemoveAt(0);
            ++NumCasts;
        }
    }
}

sealed class Shadowchase(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40f, 4f);
    private readonly List<AOEInstance> _aoes = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D1 && actor.OID == (uint)OID.HowlingBladeShadow)
        {
            _aoes.Add(new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(3.1d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shadowchase)
        {
            ++NumCasts;
        }
    }
}
