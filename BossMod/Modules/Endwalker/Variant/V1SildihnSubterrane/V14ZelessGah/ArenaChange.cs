namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V14ZelessGah;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShowOfStrength && Arena.Bounds.Radius > 20f)
        {
            _aoe = [new(VCZelessGah.Rect, Arena.Center, default, Module.CastFinishAt(spell, 0.8d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x14 && state == 0x00080004u)
        {
            Arena.Bounds = new ArenaBoundsRect(15f, 20f);
            _aoe = [];
        }
    }
}
