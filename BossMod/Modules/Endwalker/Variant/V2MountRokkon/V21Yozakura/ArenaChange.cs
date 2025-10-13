namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V21Yozakura;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square1 = new([new Square(V21Yozakura.ArenaCenter1, 23f)], [new Square(V21Yozakura.ArenaCenter1, 20f)]);
    private static readonly AOEShapeCustom square2 = new([new Square(V21Yozakura.ArenaCenter3, 23f)], [new Square(V21Yozakura.ArenaCenter3, 20f)]);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GloryNeverlasting && Arena.Bounds.Radius > 20f)
        {
            void AddAOE(AOEShape shape) => _aoe = [new(shape, Arena.Center, default, Module.CastFinishAt(spell, 3.7d))];
            var center = Arena.Center;
            if (center == V21Yozakura.ArenaCenter1)
            {
                AddAOE(square1);
            }
            else if (center == V21Yozakura.ArenaCenter3)
            {
                AddAOE(square2);
            }
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is 0x35 or 0x36 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoe = [];
        }
    }
}
