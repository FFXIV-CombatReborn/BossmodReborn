namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOrigin(WPos Center, WPos Origin, float Distance, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float radius = Radius;
    private readonly float distance = Distance;

    public override float Distance(WPos p)
    {
        if ((p + distance * (p - origin).Normalized()).InCircle(center, radius))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOriginMixedAOEs(WPos Center, WPos Origin, float Distance, float Radius, Components.GenericAOEs.AOEInstance[] AOEs, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float radius = Radius;
    private readonly float distance = Distance;
    private readonly Components.GenericAOEs.AOEInstance[] aoes = AOEs;
    private readonly int len = Length;

    public override float Distance(WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Check(projected))
            {
                return default;
            }
        }
        if (projected.InCircle(center, radius))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOriginPlusAOERects(WPos Center, WPos Origin, float Distance, float Radius, (WPos Origin, WDir Direction)[] AOEs, float LengthFront, float HalfWidth, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float radius = Radius;
    private readonly float distance = Distance;
    private readonly (WPos origin, WDir direction)[] aoes = AOEs;
    private readonly float lenFront = LengthFront;
    private readonly float halfWidth = HalfWidth;
    private readonly int len = Length;

    public override float Distance(WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (projected.InRect(aoe.origin, aoe.direction, lenFront, default, halfWidth))
            {
                return default;
            }
        }
        if (projected.InCircle(center, radius))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleFixedDirection(WPos Center, WDir Direction, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction;
    private readonly float radius = Radius;

    public override float Distance(WPos p)
    {
        if ((p + direction).InCircle(center, radius))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOriginPlusMixedAOEsPlusSingleCircleIntersection(WPos Center, WPos Origin, float Radius, float Distance, SDUnion AOEs, WPos CircleOrigin, float CircleRadius, bool Pull) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float radius = Radius;
    private readonly float distance = Distance;
    private readonly SDUnion aoes = AOEs;
    private readonly WPos circleOrigin = CircleOrigin;
    private readonly float circleRadius = CircleRadius;
    private readonly bool pull = Pull;

    public override float Distance(WPos p)
    {
        var dir = (p - origin).Normalized();
        var kb = pull ? -dir : dir;
        var projected = p + distance * kb;
        if (!projected.InCircle(center, radius) || Intersect.RayCircle(p - circleOrigin, kb, circleRadius, distance))
        {
            return default;
        }
        return aoes.Distance(projected);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleFixedDirectionAndAwayFromOrigin(WPos Center, WPos Origin, WDir Direction, float Distance, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly WDir direction = Direction; // direction including distance for the fixed direction kb
    private readonly float radius = Radius;
    private readonly float distance = Distance; // distance for the away from origin kb

    public override float Distance(WPos p)
    {
        if ((p + direction).InCircle(center, radius) && (p + distance * (p - origin).Normalized()).InCircle(center, radius))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOriginIntoDirection(WPos Center, WPos Origin, float Distance, float ArenaRadius, WDir Direction, Angle Tolerance) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float arenaRadius = ArenaRadius;
    private readonly float distance = Distance;
    private readonly WDir direction = Direction;
    private readonly Angle tolerance = Tolerance;

    public override float Distance(WPos p)
    {
        var projected = p + distance * (p - origin);
        if (projected.InCircle(center, arenaRadius) && projected.InCone(origin, direction, tolerance))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInCircleAwayFromOriginPlusAOECircles(WPos Center, WPos Origin, float Distance, float Radius, WPos[] AOEs, float AOERadius, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float radius = Radius;
    private readonly float distance = Distance;
    private readonly WPos[] aoes = AOEs;
    private readonly float aoeRadius = AOERadius;
    private readonly int len = Length;

    public override float Distance(WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        if (!projected.InCircle(center, radius))
        {
            return default;
        }
        for (var i = 0; i < len; ++i)
        {
            if (projected.InCircle(aoes[i], aoeRadius))
            {
                return default;
            }
        }
        return 1f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackFixedDirectionIntoCircle(WDir Direction, WPos CircleOrigin, float Radius) : ShapeDistance
{
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly WPos circleOrigin = CircleOrigin;
    private readonly float radius = Radius;

    public override float Distance(WPos p)
    {
        if ((p + direction).InCircle(circleOrigin, radius))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
