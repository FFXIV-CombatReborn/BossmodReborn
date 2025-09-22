namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackInComplexPolygonAwayFromOrigin(WPos Center, WPos Origin, float Distance, RelSimplifiedComplexPolygon Polygon) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float distance = Distance;
    private readonly RelSimplifiedComplexPolygon polygon = Polygon;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos p)
    {
        return !polygon.Contains(p - center + distance * (p - origin).Normalized());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInComplexPolygonFixedDirection(WPos Center, WDir Direction, RelSimplifiedComplexPolygon Polygon) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction;
    private readonly RelSimplifiedComplexPolygon polygon = Polygon;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos p)
    {
        return !polygon.Contains(p - center + direction);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p) => Contains(p) ? 0f : 1f;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos p)
    {
        var dir = distance * (p - origin).Normalized();
        if (!polygon.Contains(p - center + dir))
        {
            return true;
        }

        var projected = p + dir;
        for (var i = 0; i < len; ++i)
        {
            if (projected.InSquare(aoes[i], halfWidth))
            {
                return true;
            }
        }
        return false;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos p)
    {
        var dir = distance * (p - origin).Normalized();
        if (!polygon.Contains(p - center + dir))
        {
            return true;
        }

        var projected = p + dir;
        for (var i = 0; i < len; ++i)
        {
            if (projected.InCircle(aoes[i], radius))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p) => Contains(p) ? 0f : 1f;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos p)
    {
        var offset = p - center;
        var dir = (p - origin).Normalized();
        return !polygon.Contains(offset + distance * dir) || Intersect.RayPolygon(offset, dir, polygon) < distance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
