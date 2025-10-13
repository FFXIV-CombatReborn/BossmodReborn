namespace BossMod.Endwalker.VariantCriterion.C02AMR.C022Gorai;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(C022Gorai.ArenaCenter, 23f)], [new Square(C022Gorai.ArenaCenter, 20f)]);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Unenlightenment && Arena.Bounds.Radius > 20f)
        {
            _aoe = [new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.5d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x02 && state == 0x00020001u)
        {
            Arena.Bounds = C022Gorai.DefaultBounds;
            _aoe = [];
        }
    }
}
