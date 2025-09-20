namespace BossMod.Endwalker.Trial.T08Asura;

class SixBladedKhadga(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly Angle a180 = 180f.Degrees();
    private static readonly AOEShapeCone cone = new(20f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
            {
                if (aoes[0].Rotation.AlmostEqual(aoe.Rotation + a180, Angle.DegToRad))
                    aoe.Risky = false;
            }
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.KhadgaTelegraph1:
            case (uint)AID.KhadgaTelegraph2:
            case (uint)AID.KhadgaTelegraph3:
                _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 11.9f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.Khadga1:
                case (uint)AID.Khadga2:
                case (uint)AID.Khadga3:
                case (uint)AID.Khadga4:
                case (uint)AID.Khadga5:
                case (uint)AID.Khadga6:
                    _aoes.RemoveAt(0);
                    break;
            }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoes.Count;
        if (count == 0)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
        if (count > 1)
        {
            var aoe = _aoes[0];
            // stay close to the middle if there is more than one aoe left
            hints.AddForbiddenZone(new SDInvertedCircle(aoe.Origin, 3f), aoe.Activation);
        }
    }
}
