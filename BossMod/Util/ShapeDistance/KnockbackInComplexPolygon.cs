namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackInComplexPolygonAwayFromOrigin(WPos Center, WPos Origin, float Distance, RelSimplifiedComplexPolygon Polygon) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float distance = Distance;
    private readonly RelSimplifiedComplexPolygon polygon = Polygon;

    public override float Distance(WPos p)
    {
        if (polygon.Contains(p - center + distance * (p - origin).Normalized()))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInComplexPolygonFixedDirection(WPos Center, WDir Direction, RelSimplifiedComplexPolygon Polygon) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction;
    private readonly RelSimplifiedComplexPolygon polygon = Polygon;

    public override float Distance(WPos p)
    {
        if (polygon.Contains(p - center + direction))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInComplexPolygonAwayFromOriginPlusAOEAABBSquares(WPos Center, WPos Origin, float Distance, RelSimplifiedComplexPolygon Polygon, WPos[] AOEs, float HalfWidth, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly RelSimplifiedComplexPolygon polygon = Polygon;
    private readonly float distance = Distance;
    private readonly WPos[] aoes = AOEs;
    private readonly float halfWidth = HalfWidth;
    private readonly int len = Length;

    public override float Distance(WPos p)
    {
        var dir = distance * (p - origin).Normalized();
        var offsetCenter = p - center;
        var projected = p + dir;
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (projected.InSquare(aoes[i], halfWidth))
            {
                return default;
            }
        }
        if (polygon.Contains(offsetCenter + dir))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInComplexPolygonAwayFromOriginPlusAOECircles(WPos Center, WPos Origin, float Distance, RelSimplifiedComplexPolygon Polygon, WPos[] AOEs, float Radius, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly RelSimplifiedComplexPolygon polygon = Polygon;
    private readonly float distance = Distance;
    private readonly WPos[] aoes = AOEs;
    private readonly float radius = Radius;
    private readonly int len = Length;

    public override float Distance(WPos p)
    {
        var dir = distance * (p - origin).Normalized();
        var offsetCenter = p - center;
        var projected = p + dir;
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (projected.InCircle(aoes[i], radius))
            {
                return default;
            }
        }
        if (polygon.Contains(offsetCenter + dir))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInComplexPolygonAwayFromOriginPlusIntersectionTest(WPos Center, WPos Origin, float Distance, RelSimplifiedComplexPolygon Polygon) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float distance = Distance;
    private readonly RelSimplifiedComplexPolygon polygon = Polygon;

    public override float Distance(WPos p)
    {
        var offset = p - center;
        var dir = (p - origin).Normalized();
        // while doing a point in polygon test and intersection test seems like double the work, the intersection test is actually a lot slower than the PiP test, so this is a net positive to filter out some cells beforehand
        if (polygon.Contains(offset + distance * dir) && Intersect.RayPolygon(offset, dir, polygon) > distance)
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
