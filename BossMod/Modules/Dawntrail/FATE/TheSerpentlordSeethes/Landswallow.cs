namespace BossMod.Dawntrail.FATE.Ttokrrone;

sealed class Landswallow(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly AOEShapeRect rect = new(68f, 13.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
        {
            aoes[0].Color = Colors.Danger;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is >= (uint)AID.LandSwallowTelegraph1 and <= (uint)AID.LandSwallowTelegraph6)
        {
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 0.4d)));
            if (_aoes.Count == 6)
            {
                _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is >= (uint)AID.Landswallow1 and <= (uint)AID.Landswallow4)
        {
            _aoes.RemoveAt(0);
        }
    }
}
