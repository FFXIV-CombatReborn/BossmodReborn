namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V23Gorai;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly WPos ArenaCenter = new(741f, -190f);
    public static readonly WPos octagonCenter = new(731f, -200f);
    private static readonly Square[] defaultSquare = [new Square(ArenaCenter, 20f)];
    private static readonly AOEShapeCustom square = new([new Square(ArenaCenter, 23f)], defaultSquare);
    private static readonly Angle rotation = 22.5f.Degrees();
    private static readonly ArenaBoundsCustom octagonTrap = new(defaultSquare, [new Polygon(octagonCenter, 8.5f, 8, rotation)],
    [new Polygon(octagonCenter, 7.5f, 8, rotation)]);
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
        if (index == 0x16 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoe = [];
        }
        else if (index == 0x3D)
        {
            if (state == 0x00020001u)
            {
                Arena.Bounds = octagonTrap;
            }
            else if (state == 0x00080004u)
            {
                Arena.Bounds = new ArenaBoundsSquare(20f);
            }
        }
    }
}
