namespace BossMod.Dawntrail.Alliance.A22UltimaOmega;

sealed class OmegaBlaster(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly AOEShapeCone cone = new(50f, 90f.Degrees());
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OmegaBlasterFirst or (uint)AID.OmegaBlasterSecond)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count > 1)
            {
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                ref var aoe1 = ref aoes[0];
                ref var aoe2 = ref aoes[1];
                if (aoe1.Activation > aoe2.Activation)
                {
                    (aoe1, aoe2) = (aoe2, aoe1);
                }
                aoe2.Origin += 5f * aoe2.Rotation.ToDirection();
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.OmegaBlasterFirst or (uint)AID.OmegaBlasterSecond)
        {
            _aoes.RemoveAt(0);
            if (_aoes.Count == 1)
            {
                ref var aoe = ref CollectionsMarshal.AsSpan(_aoes)[0];
                aoe.Origin -= 5f * aoe.Rotation.ToDirection();
            }
        }
    }
}
