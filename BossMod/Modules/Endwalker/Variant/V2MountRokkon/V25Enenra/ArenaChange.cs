namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V25Enenra;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20f, 21f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlagrantCombustion && Arena.Bounds.Radius > 20f)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 2.9d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x28 && state == 0x00020001u)
        {
            Arena.Bounds = V25Enenra.DefaultBounds;
            _aoe = [];
        }
    }
}
