namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P7GigaflaresEdge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(20f); // TODO: verify falloff

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.GigaflaresEdgeAOE1 or (uint)AID.GigaflaresEdgeAOE2 or (uint)AID.GigaflaresEdgeAOE3)
        {
            _aoes.Add(new(_shape, caster.Position, default, Module.CastFinishAt(spell)));
            _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.GigaflaresEdgeAOE1 or (uint)AID.GigaflaresEdgeAOE2 or (uint)AID.GigaflaresEdgeAOE3)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}
