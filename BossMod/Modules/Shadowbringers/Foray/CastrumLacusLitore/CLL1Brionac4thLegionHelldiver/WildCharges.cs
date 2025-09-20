namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL1Brionac4thLegionHelldiver;

sealed class WildCharges(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private readonly DetermineArena _arena = module.FindComponent<DetermineArena>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_arena.IsBrionacArena)
            return CollectionsMarshal.AsSpan(_aoes);
        else
            return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var halfWidth = spell.Action.ID switch
        {
            (uint)AID.LinearDive => 3f,
            (uint)AID.DiveFormation => 5f,
            _ => default
        };
        if (halfWidth != default)
        {
            var dir = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), halfWidth, invertForbiddenZone: true), caster.Position.Quantized(), Angle.FromDirection(dir), Module.CastFinishAt(spell), Colors.SafeFromAOE));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.DiveFormation or (uint)AID.LinearDive)
            _aoes.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_arena.IsBrionacArena)
            return;
        var count = _aoes.Count;
        if (count != 0)
        {
            var forbidden = new ShapeDistance[count];
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref _aoes.Ref(i);
                forbidden[i] = aoe.Shape.Distance(aoe.Origin, aoe.Rotation);
            }
            hints.AddForbiddenZone(new SDIntersection(forbidden), _aoes.Ref(0).Activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_arena.IsBrionacArena)
            return;
        var count = _aoes.Count;
        if (count == 0)
            return;
        var risky = true;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (aoe.Check(actor.Position))
            {
                risky = false;
                break;
            }
        }
        hints.Add("Share damage inside wildcharge!", risky);
    }
}
