namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class KnuckleSandwich(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(9f), new AOEShapeCircle(18f), new AOEShapeCircle(27f), new AOEShapeDonut(9f, 60f),
    new AOEShapeDonut(18f, 60f), new AOEShapeDonut(27f, 60f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }

        var time = WorldState.CurrentTime;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count == 2)
        {
            ref var aoe0 = ref aoes[0];
            ref var aoe1 = ref aoes[1];
            var delay = aoe1.Shape is AOEShapeDonut donut ? donut.InnerRadius * 0.25d : default;
            aoe0.Risky = aoe0.Activation.AddSeconds(-delay) <= time;
        }

        return aoes[..1];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // extra ai hint: stay close to the edge of the first aoe
        if (_aoes.Count == 2 && _aoes.Ref(1).Shape is AOEShapeDonut donut)
        {
            ref var aoe = ref _aoes.Ref(0);
            hints.AddForbiddenZone(new SDInvertedCircle(aoe.Origin, donut.InnerRadius + 2f), aoe.Activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.KnuckleSandwich1 => _shapes[0],
            (uint)AID.KnuckleSandwich2 => _shapes[1],
            (uint)AID.KnuckleSandwich3 => _shapes[2],
            (uint)AID.BrittleImpact1 => _shapes[3],
            (uint)AID.BrittleImpact2 => _shapes[4],
            (uint)AID.BrittleImpact3 => _shapes[5],
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
            _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.KnuckleSandwich1:
            case (uint)AID.KnuckleSandwich2:
            case (uint)AID.KnuckleSandwich3:
            case (uint)AID.BrittleImpact1:
            case (uint)AID.BrittleImpact2:
            case (uint)AID.BrittleImpact3:
                ++NumCasts;
                if (_aoes.Count != 0)
                    _aoes.RemoveAt(0);
                break;
        }
    }
}
