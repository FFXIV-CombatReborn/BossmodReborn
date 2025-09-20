namespace BossMod.RealmReborn.Raid.T01Caduceus;

// we have 12 hexagonal platforms and 1 octagonal; sorted S to N, then E to W - so entrance platform has index 0, octagonal (NW) platform has index 12
class Platforms(BossModule module) : BossComponent(module)
{
    private const float HexaPlatformSide = 9f;
    private const float OctaPlatformLong = 13f;
    private const float OctaPlatformShort = 7f;
    private const float HexaCenterToSideCornerX = HexaPlatformSide * 0.8660254f; // sqrt(3) / 2
    private const float HexaCenterToSideCornerZ = HexaPlatformSide * 0.5f;
    private const float HexaNeighbourDistX = HexaCenterToSideCornerX * 2f;
    private const float HexaNeighbourDistZ = HexaPlatformSide * 1.5f;

    private static readonly WPos closestPlatformCenter = new(0.6f, -374f); // (0,0) on hexa grid
    private static readonly (int, int)[] hexaPlatforms = [(0, 0), (0, 1), (1, 1), (0, 2), (1, 2), (2, 2), (3, 2), (0, 3), (1, 3), (2, 3), (1, 4), (2, 4)];
    private static readonly (int, int) octaPlatform = (3, 4);
    public static readonly WPos[] hexaPlatformCenters = CreateHexaPlatformCenters();
    private static WPos[] CreateHexaPlatformCenters()
    {
        var centers = new WPos[12];
        for (var i = 0; i < 12; ++i)
        {
            centers[i] = HexaCenter(hexaPlatforms[i]);
        }
        return centers;
    }
    private static readonly WDir OctaCenterOffset = 0.5f * new WDir(OctaPlatformShort, OctaPlatformLong - HexaPlatformSide);
    private static readonly WPos OctaPlatformCenter = HexaCenter(octaPlatform) - OctaCenterOffset;

    // it is possible to move from platform if height difference is < 0.5, or jump if height difference is < 2
    public static readonly float[] PlatformHeights = [4.5f, 0.9f, 0.5f, -0.7f, -0.3f, 0.1f, 0.5f, 1.7f, 1.3f, 0.9f, 2.1f, 2.5f, 4.9f];
    private static readonly (int lower, int upper)[] highEdges = [(1, 0), (3, 7), (4, 7), (9, 12), (11, 12)];
    private static readonly (int lower, int upper)[] jumpEdges = [(3, 1), (4, 1), (4, 2), (4, 8), (5, 8), (5, 9), (8, 10), (8, 11), (9, 11)];

    private static readonly BitMask allPlatforms = new(0x1FFF);

    private static WPos HexaCenter((int x, int y) c) => closestPlatformCenter - new WDir(c.x * HexaNeighbourDistX + ((c.y & 1) != 0 ? HexaCenterToSideCornerX : 0), c.y * HexaNeighbourDistZ);
    private static readonly WDir[] HexaCornerOffsets = [
        new(HexaCenterToSideCornerX, -HexaCenterToSideCornerZ),
        new(HexaCenterToSideCornerX, HexaCenterToSideCornerZ),
        new(0, HexaPlatformSide),
        new(-HexaCenterToSideCornerX, HexaCenterToSideCornerZ),
        new(-HexaCenterToSideCornerX, -HexaCenterToSideCornerZ),
        new(0, -HexaPlatformSide)
    ];

    public static readonly ShapeDistance[] PlatformShapes = InitializePlatformShapes();
    private static readonly ShapeDistance[] highEdgeShapes = InitializeHighEdgeShapes();

    private static ShapeDistance[] InitializePlatformShapes()
    {
        var platformShapes = new ShapeDistance[13];
        for (var i = 0; i < 13; ++i)
        {
            platformShapes[i] = new SDConvexPolygon(PlatformPoly(i), true);
        }
        return platformShapes;
    }

    private static ShapeDistance[] InitializeHighEdgeShapes()
    {
        var highEdgeShapes = new ShapeDistance[5];
        for (var i = 0; i < 5; ++i)
        {
            var e = HexaEdge(highEdges[i].lower, highEdges[i].upper);
            highEdgeShapes[i] = new SDRect(e.Item1, e.Item2, 0);
        }
        return highEdgeShapes;
    }

    private static readonly (WPos p, WDir d, float l)[] jumpEdgeSegments = GenerateSegments();

    private static (WPos p, WDir d, float l)[] GenerateSegments()
    {
        var segments = new (WPos p, WDir d, float l)[9];

        for (var i = 0; i < 9; ++i)
        {
            ref readonly var e = ref jumpEdges[i];
            var edge = HexaEdge(e.lower, e.upper);
            var direction = (edge.Item2 - edge.Item1).Normalized();
            var length = (edge.Item2 - edge.Item1).Length();
            segments[i] = (edge.Item1, direction, length);
        }
        return segments;
    }

    private static WPos[] HexaPoly(WPos center)
    {
        var result = new WPos[6];
        for (var i = 0; i < 6; ++i)
        {
            result[i] = center + HexaCornerOffsets[i];
        }
        return result;
    }

    private static WPos[] OctaPoly()
    {
        return
        [
            OctaPlatformCenter + new WDir(OctaCenterOffset.X, -OctaCenterOffset.Z - HexaPlatformSide),
            OctaPlatformCenter + new WDir(OctaCenterOffset.X + HexaCenterToSideCornerX, -OctaCenterOffset.Z - HexaCenterToSideCornerZ),
            OctaPlatformCenter + new WDir(OctaCenterOffset.X + HexaCenterToSideCornerX, +OctaCenterOffset.Z + HexaCenterToSideCornerZ),
            OctaPlatformCenter + new WDir(OctaCenterOffset.X, +OctaCenterOffset.Z + HexaPlatformSide),
            OctaPlatformCenter - new WDir(OctaCenterOffset.X, -OctaCenterOffset.Z - HexaPlatformSide),
            OctaPlatformCenter - new WDir(OctaCenterOffset.X + HexaCenterToSideCornerX, -OctaCenterOffset.Z - HexaCenterToSideCornerZ),
            OctaPlatformCenter - new WDir(OctaCenterOffset.X + HexaCenterToSideCornerX, +OctaCenterOffset.Z + HexaCenterToSideCornerZ),
            OctaPlatformCenter - new WDir(OctaCenterOffset.X, +OctaCenterOffset.Z + HexaPlatformSide)
        ];
    }

    private static WPos[] PlatformPoly(int index) => index < 12 ? HexaPoly(hexaPlatformCenters[index]) : OctaPoly();

    private static (WPos, WPos) HexaEdge(int from, int to)
    {
        var (x1, y1) = from < 12 ? hexaPlatforms[from] : octaPlatform;
        var (x2, y2) = to < 12 ? hexaPlatforms[to] : octaPlatform;
        var (o1, o2) = (x2 - x1, y2 - y1, y1 & 1) switch
        {
            (-1, 0, _) => (0, 1),
            (-1, -1, 0) or (0, -1, 1) => (1, 2),
            (0, -1, 0) or (1, -1, 1) => (2, 3),
            (1, 0, _) => (3, 4),
            (0, 1, 0) or (1, 1, 1) => (4, 5),
            (-1, 1, 0) or (0, 1, 1) => (5, 0),
            _ => (0, 0)
        };
        var c = HexaCenter((x1, y1));
        return (c + HexaCornerOffsets[o1], c + HexaCornerOffsets[o2]);
    }

    private static bool IntersectJumpEdge(WPos p, WDir d, float l)
    {
        for (var i = 0; i < 9; ++i)
        {
            var e = jumpEdgeSegments[i];
            var n = e.d.OrthoL();
            var dirDot = d.Dot(n);
            if (dirDot < 0.05f)
                continue;

            var ts = n.Dot(e.p - p) / dirDot;
            if (ts < 0 || ts > l)
                continue;

            var te = d.OrthoL().Dot(p - e.p) / e.d.Dot(d.OrthoL());
            if (te >= 0 && te <= e.l)
                return true;
        }
        return false;
    }

    public BitMask ActivePlatforms;
    public DateTime ExplosionAt;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(new SDBlockedAreaT01Caduceus(PlatformShapes, highEdges, highEdgeShapes, actor.PosRot.Y, PlatformHeights));

        if (actor.PrevPosition != actor.Position)
        {
            hints.WantJump = IntersectJumpEdge(actor.Position, (actor.Position - actor.PrevPosition).Normalized(), 2.5f);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var i in (ActivePlatforms ^ allPlatforms).SetBits())
            Arena.AddPolygon(PlatformPoly(i), Colors.Border);
        foreach (var i in ActivePlatforms.SetBits())
            Arena.AddPolygon(PlatformPoly(i), Colors.Enemy);
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (actor.OID == (uint)OID.Platform)
        {
            var i = Array.FindIndex(hexaPlatformCenters, c => actor.Position.InCircle(c, 2));
            if (i == -1)
                i = 12;
            var active = state == 2;
            ActivePlatforms[i] = active;
            if (active)
                ExplosionAt = WorldState.FutureTime(6d);
        }
    }
}
