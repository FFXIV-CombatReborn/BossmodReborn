namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V22Moko;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly WPos ArenaCenter = new(-700f, 540f);
    private static readonly AOEShapeCustom square = new([new Square(ArenaCenter, 25f)], [new Square(ArenaCenter, 20f)]);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.KenkiRelease && Arena.Bounds.Radius > 20f)
        {
            _aoe = [new(square, Arena.Center, default, Module.CastFinishAt(spell, 2.1d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x5B && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoe = [];
        }
    }
}
