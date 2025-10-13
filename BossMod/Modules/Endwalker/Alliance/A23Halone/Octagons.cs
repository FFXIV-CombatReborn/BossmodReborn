namespace BossMod.Endwalker.Alliance.A23Halone;

// TODO: assign alliances members to a specific octagon. in duty finder it is usually:
// NW (Octagon3): Alliance A
// NE (Octagon1): Alliance C
// S (Octagon2): Alliance B
class Octagons(BossModule module) : Components.GenericAOEs(module)
{
    private const float InnerRadius = 11.125f; // radii adjusted for hitbox radius
    private const float OuterRadius = 13.45f;
    private const int Vertices = 8;
    private static readonly WPos[] spears = [new(-686f, 592f), new(-700f, 616.2f), new(-714f, 592f)];
    private static readonly Angle[] angle = [-37.5f.Degrees(), 22.5f.Degrees(), 37.5f.Degrees()];
    private static readonly Polygon[] shapes = [new(spears[0], InnerRadius, Vertices, angle[0]),
    new(spears[0], OuterRadius, Vertices, angle[0]), new(spears[1], InnerRadius, Vertices, angle[1]),
    new(spears[1], OuterRadius, Vertices, angle[1]), new(spears[2], InnerRadius, Vertices, angle[2]),
    new(spears[2], OuterRadius, Vertices, angle[2])];
    private static readonly Shape[] baseArena = [new Circle(A23Halone.ArenaCenter, 29.5f)];
    private readonly List<Polygon> octagonsInner = [shapes[0], shapes[2], shapes[4]], octagonsOuter = [shapes[1], shapes[3], shapes[5]];

    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCustom customShape = new([new Square(A23Halone.ArenaCenter, 29.5f)], [shapes[0], shapes[2], shapes[4]]); // using a square should be less cpu intensive, gets clipped with arena border anyway

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        //x07 = south, x06 = east, x05 = west x00020001 walls activate, x00200004 disappear
        // telegraph - 0x00100008
        var update = false;
        switch (state)
        {
            case 0x00100008u when index == 0x07:
                _aoe = [new(customShape, Arena.Center, default, WorldState.FutureTime(9d))];
                break;
            case 0x00020001u when index == 0x07:
                update = true;
                _aoe = [];
                break;
            case 0x00200004u:
                RemoveOctagons(index);
                update = true;
                break;
        }
        if (update)
        {
            Arena.Bounds = new ArenaBoundsCustom(baseArena, [.. octagonsOuter], [.. octagonsInner]);
        }
    }

    private void RemoveOctagons(byte index)
    {
        switch (index)
        {
            case 0x06:
                octagonsInner.Remove(shapes[0]);
                octagonsOuter.Remove(shapes[1]);
                break;
            case 0x07:
                octagonsInner.Remove(shapes[2]);
                octagonsOuter.Remove(shapes[3]);
                break;
            case 0x05:
                octagonsInner.Remove(shapes[4]);
                octagonsOuter.Remove(shapes[5]);
                break;
        }
    }
}
