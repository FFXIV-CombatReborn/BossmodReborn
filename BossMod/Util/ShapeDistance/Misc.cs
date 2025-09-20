namespace BossMod;

[SkipLocalsInit]
public sealed class SDDeepDungeonLOS(Bitmap Map, WPos Origin) : ShapeDistance
{
    private readonly Bitmap map = Map;
    private readonly WPos origin = Origin;
    private readonly float pixelSize = Map.PixelSize;

    public override float Distance(WPos p)
    {
        var offset = (p - origin) / pixelSize;
        return map[(int)offset.X, (int)offset.Z] ? -10f : 10f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDBlockedAreaT01Caduceus(ShapeDistance[] platformShapes, (int lower, int upper)[] highEdges, ShapeDistance[] highEdgeShapes, float actorY, float[] platformHeights) : ShapeDistance
{
    private readonly ShapeDistance[] _platformShapes = platformShapes;
    private readonly (int lower, int upper)[] _highEdges = highEdges;
    private readonly ShapeDistance[] _highEdgeShapes = highEdgeShapes;
    private readonly float _actorY = actorY;
    private readonly float[] _platformHeights = platformHeights;

    public override float Distance(WPos p)
    {
        var res = float.MaxValue;

        for (var i = 0; i < 13; ++i)
        {
            res = Math.Min(res, _platformShapes[i].Distance(p));
        }

        res = -res; // invert

        for (var i = 0; i < 5; ++i)
        {
            var e = _highEdges[i];
            var f = _highEdgeShapes[i];

            if (_actorY + 0.1f < _platformHeights[e.upper])
            {
                res = Math.Min(res, f.Distance(p));
            }
        }
        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
