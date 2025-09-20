namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackFixedDirectionAgainstSafewalls(WDir Direction, Components.GenericKnockback.SafeWall[] SafeWalls, float Distance, int Length) : ShapeDistance
{
    private readonly WDir direction = Direction;
    private readonly float distance = Distance;
    private readonly Components.GenericKnockback.SafeWall[] safeWalls = SafeWalls;
    private readonly int length = Length;

    public override float Distance(WPos p)
    {
        for (var i = 0; i < length; ++i)
        {
            ref readonly var w = ref safeWalls[i];
            if (Intersect.RaySegment(p, direction, w.Vertex1, w.Vertex2) < distance)
            {
                return 1f;
            }
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackFixedDirectionAgainstSafewallsPlusRectAOE(WDir Direction, Components.GenericKnockback.SafeWall[] SafeWalls, float Distance, int Length, WPos RectOrigin, WDir RectDirection, float LengthFront, float HalfWidth) : ShapeDistance
{
    private readonly WDir direction = Direction;
    private readonly float distance = Distance;
    private readonly Components.GenericKnockback.SafeWall[] safeWalls = SafeWalls;
    private readonly int length = Length;
    private readonly WPos rectOrigin = RectOrigin;
    private readonly WDir rectDirection = RectDirection;
    private readonly float lengthFront = LengthFront;
    private readonly float halfWidth = HalfWidth;

    public override float Distance(WPos p)
    {
        for (var i = 0; i < length; ++i)
        {
            ref readonly var w = ref safeWalls[i];
            var intersect = Intersect.RaySegment(p, direction, w.Vertex1, w.Vertex2);
            if (intersect < distance)
            {
                if ((p + intersect * direction).InRect(rectOrigin, rectDirection, lengthFront, default, halfWidth))
                {
                    return default;
                }
                return 1f;
            }
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackWithWallsAwayFromOriginMultiAimIntoDonuts((WPos Origin, WDir Direction)[] Knockbacks, int LengthKnockbacks, float RectLengthFront, float RectHalfWidth, float Distance, WPos[] DonutOrigins, float DonutInnerRadius, int LengthDonuts) : ShapeDistance
{
    private readonly (WPos origin, WDir direction)[] knockbacks = Knockbacks;
    private readonly int lenKBs = LengthKnockbacks;
    private readonly float rectLengthFront = RectLengthFront;
    private readonly float rectHalfWidth = RectHalfWidth;
    private readonly float distance = Distance;
    private readonly WPos[] donutOrigins = DonutOrigins;
    private readonly float donutInnerRadius = DonutInnerRadius;
    private readonly int lenDonuts = LengthDonuts;

    public override float Distance(WPos p)
    {
        for (var i = 0; i < lenKBs; ++i)
        {
            ref var kb = ref knockbacks[i];
            var origin = kb.origin;
            if (p.InRect(origin, kb.direction, rectLengthFront, default, rectHalfWidth))
            {
                var projected = p + distance * (origin - p).Normalized();
                for (var j = 0; j < lenDonuts; ++j)
                {
                    if (projected.InCircle(donutOrigins[j], donutInnerRadius))
                    {
                        return 1f;
                    }
                }
                return default;
            }
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
