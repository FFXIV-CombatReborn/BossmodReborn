namespace BossMod.Dawntrail.Raid.M08NHowlingBlade;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeDonut donut = new(12f, 17f);
    private readonly List<Polygon> pillars = new(3);
    private static readonly WPos[] pillarPositions = [new(100f, 88.5f), new(109.959f, 94.25f), new(109.917f, 105.619f),
    new(100f, 111.5f), new(90.075f, 105.619f), new(90.041f, 94.25f)];
    private static readonly Polygon[] pillarPolygons =
    [
        new(pillarPositions[0], 3.5f, 20), // north, ENVC 0x08
        new(pillarPositions[0], 3.5f, 20, -55f.Degrees()), // north, ENVC 0x09
        new(pillarPositions[0], 3.5f, 20, -30f.Degrees()), // north, ENVC 0x0A
        new(pillarPositions[2], 3.5f, 20, -175f.Degrees()), // southeast, ENVC 0x0B
        new(pillarPositions[2], 3.5f, 20, -150f.Degrees()), // southeast, ENVC 0x0C
        new(pillarPositions[2], 3.5f, 20, -120f.Degrees()), // southeast, ENVC 0x0D
        new(pillarPositions[4], 3.5f, 20, 89.98f.Degrees()), // southwest, ENVC 0x0E
        new(pillarPositions[4], 3.5f, 20, 105f.Degrees()), // southwest, ENVC 0x0F
        new(pillarPositions[4], 3.5f, 20, 75f.Degrees()), // southwest, ENVC 0x10
        new(pillarPositions[1], 3.5f, 20, -125f.Degrees()), // northeast, ENVC 0x11
        new(pillarPositions[1], 3.5f, 20, -60f.Degrees()), // northeast, ENVC 0x12
        new(pillarPositions[1], 3.5f, 20, -89.98f.Degrees()), // northeast, ENVC 0x13
        new(pillarPositions[3], 3.5f, 20, 125f.Degrees()), // south, ENVC 0x14
        new(pillarPositions[3], 3.5f, 20), // south, ENVC 0x15
        new(pillarPositions[3], 3.5f, 20, -150f.Degrees()), // south, ENVC 0x16
        new(pillarPositions[5], 3.5f, 20, 5f.Degrees()), // northwest, ENVC 0x17
        new(pillarPositions[5], 3.5f, 20, 60f.Degrees()), // northwest, ENVC 0x18
        new(pillarPositions[5], 3.5f, 20, 30f.Degrees()), // northwest, ENVC 0x19
    ];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            if (state == 0x00200010u)
                _aoe = [new(donut, Arena.Center, default, WorldState.FutureTime(11.2d))];
            else if (state == 0x00020001u)
            {
                _aoe = [];
                Arena.Bounds = M08NHowlingBlade.EndArena;
            }
        }
        else if (index is >= 0x08 and <= 0x19)
        {
            if (state == 0x00020001u)
            {
                pillars.Add(pillarPolygons[index - 0x08]);
                if (pillars.Count == 3)
                {
                    var arena = new ArenaBoundsCustom(M08NHowlingBlade.EndArenaPolygon, [.. pillars]);
                    Arena.Bounds = arena;
                    Arena.Center = arena.Center;
                }
            }
            else if (state == 0x00200004u && Arena.Bounds != M08NHowlingBlade.EndArena)
            {
                pillars.Clear();
                Arena.Bounds = M08NHowlingBlade.EndArena;
                Arena.Center = M08NHowlingBlade.ArenaCenter;
            }
        }
    }
}
