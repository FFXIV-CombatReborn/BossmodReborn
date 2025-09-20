namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V13Gladiator;

sealed class RushOfMight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCone cone = new(60f, 90f.Degrees());
    private readonly RackAndRuin _aoe = module.FindComponent<RackAndRuin>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = _aoe.Casters.Count != 0 ? 1 : count;
        var time = WorldState.CurrentTime;
        ref var aoe0 = ref aoes[0];
        if (max > 1)
        {
            aoe0.Color = Colors.Danger;
        }
        aoe0.Risky = aoe0.Activation.AddSeconds(-5d) <= time;
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.RushOfMightFront or (uint)AID.RushOfMightBack)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), risky: false));
            if (_aoes.Count == 2)
            {
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                ref var aoe1 = ref aoes[0];
                ref var aoe2 = ref aoes[1];
                if (aoe1.Activation > aoe2.Activation)
                {
                    (aoe1, aoe2) = (aoe2, aoe1);
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.RushOfMightFront or (uint)AID.RushOfMightBack)
        {
            _aoes.RemoveAt(0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count != 2)
        {
            return;
        }
        // make ai stay close to middle of the aoes to ensure successfully dodging the combo
        ref var aoe = ref _aoes.Ref(0);
        if (aoe.Risky)
        {
            hints.AddForbiddenZone(new SDInvertedRect(aoe.Origin, aoe.Rotation, 2f, 2f, 40f), aoe.Activation.AddSeconds(2d));
        }
    }
}
