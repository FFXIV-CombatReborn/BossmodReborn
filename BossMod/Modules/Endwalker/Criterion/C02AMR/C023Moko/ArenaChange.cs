namespace BossMod.Endwalker.VariantCriterion.C02AMR.C023Moko;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(C023Moko.ArenaCenter, 25f)], [new Square(C023Moko.ArenaCenter, 20f)]);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NKenkiRelease or (uint)AID.SKenkiRelease && Arena.Bounds.Radius > 20f)
        {
            _aoe = [new(square, Arena.Center, default, Module.CastFinishAt(spell, 2.1d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x2B && state == 0x00020001u)
        {
            Arena.Bounds = C023Moko.DefaultBounds;
            _aoe = [];
        }
    }
}
