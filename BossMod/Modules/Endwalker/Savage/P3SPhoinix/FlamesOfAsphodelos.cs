namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to flames of asphodelos mechanic
class FlamesOfAsphodelos(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(60f, 30f.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 4 ? 4 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes)[..max];
        var min = count > 2 ? 2 : count;
        for (var i = 0; i < min; ++i)
        {
            ref var aoe = ref aoes[i];
            if (max > 2)
                aoe.Color = Colors.Danger;
            aoe.Risky = true;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FlamesOfAsphodelosAOE1 or (uint)AID.FlamesOfAsphodelosAOE2 or (uint)AID.FlamesOfAsphodelosAOE3)
        {
            _aoes.Add(new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell), risky: false));
            if (_aoes.Count == 6)
                _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.FlamesOfAsphodelosAOE1 or (uint)AID.FlamesOfAsphodelosAOE2 or (uint)AID.FlamesOfAsphodelosAOE3)
            _aoes.RemoveAt(0);
    }
}
