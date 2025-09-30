namespace BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NShowOfStrength or (uint)AID.SShowOfStrength && Arena.Bounds.Radius > 20f)
        {
            _aoe = [new(V1SildihnSubterrane.V14ZelessGah.VCZelessGah.Rect, Arena.Center, default, Module.CastFinishAt(spell, 0.8d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x02 && state == 0x00080004u)
        {
            Arena.Bounds = new ArenaBoundsRect(15f, 20f);
            _aoe = [];
        }
    }
}
