namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

sealed class SidewiseSpark(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(60f, 90f.Degrees());
    private static readonly AOEShapeRect rect = new(40f, 8f);
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEInstance[] _visible = new AOEInstance[2];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var first = _aoes[0];
        var sameTime = count > 1 && first.Activation == _aoes[1].Activation;
        _visible[0] = first with { Color = count == 1 || sameTime ? default : Colors.Danger };
        if (max > 1)
            _visible[1] = _aoes[1] with { Risky = sameTime };
        return _visible.AsSpan(0, max);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SidewiseSpark1:
            case (uint)AID.SidewiseSpark2:
            case (uint)AID.SidewiseSpark3:
            case (uint)AID.SidewiseSpark4:
            case (uint)AID.SidewiseSpark5:
            case (uint)AID.SidewiseSpark6:
                AddOrReplace(caster, cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
                break;
            case (uint)AID.Burst:
                AddOrReplace(caster, rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.WickedReplica)
        {
            var offset = id switch
            {
                0x11D8 => 90f.Degrees(),
                0x11D6 => -90f.Degrees(),
                _ => default
            };
            if (offset != default)
                AddOrReplace(actor, cone, actor.Position, actor.Rotation + offset, WorldState.FutureTime(8d));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SidewiseSpark1:
            case (uint)AID.SidewiseSpark2:
            case (uint)AID.SidewiseSpark3:
            case (uint)AID.SidewiseSpark4:
            case (uint)AID.SidewiseSpark5:
            case (uint)AID.SidewiseSpark6:
            case (uint)AID.Burst:
                var index = IndexOf(caster.InstanceID);
                if (index >= 0)
                    _aoes.RemoveAt(index);
                break;
        }
    }

    private int IndexOf(ulong actorID)
    {
        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_aoes[i].ActorID == actorID)
                return i;
        }
        return -1;
    }

    private void AddOrReplace(Actor caster, AOEShape shape, WPos origin, Angle rotation, DateTime activation)
    {
        var aoe = new AOEInstance(shape, origin, rotation, activation, actorID: caster.InstanceID);
        var index = IndexOf(caster.InstanceID);
        if (index >= 0)
            _aoes[index] = aoe;
        else
            _aoes.Add(aoe);
        _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
    }
}
