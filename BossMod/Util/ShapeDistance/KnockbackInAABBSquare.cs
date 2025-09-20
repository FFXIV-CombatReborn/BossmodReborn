namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareLeftRightAlongXAxisPlusAOECircles(WPos Center, float Distance, float HalfWidth, WPos[] AOEs, float Radius, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly float originX = Center.X;
    private readonly WDir dir1 = new(default, Distance);
    private readonly WDir dir2 = new(default, -Distance);
    private readonly float halfWidth = HalfWidth;
    private readonly WPos[] aoes = AOEs;
    private readonly float radius = Radius;
    private readonly int len = Length;

    public override float Distance(WPos p)
    {
        var projected = p + (p.X > originX ? dir1 : dir2);
        for (var i = 0; i < len; ++i)
        {
            if (projected.InCircle(aoes[i], radius))
            {
                return default;
            }
        }
        if (projected.InSquare(center, halfWidth))
        {
            return 1f;
        }

        return default;
    }
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareAwayFromOrigin(WPos Center, WPos Origin, float Distance, float HalfWidth) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;

    public override float Distance(WPos p)
    {
        if ((p + distance * (p - origin).Normalized()).InSquare(center, halfWidth))
        {
            return 1f;
        }
        return default;
    }
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareAwayFromOriginPlusRectAOE(WPos Center, WPos Origin, float Distance, float HalfWidth, WPos RectOrigin, WDir RectDirection, float LengthFront, float RectHalfWidth) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;
    private readonly WPos rectOrigin = RectOrigin;
    private readonly WDir rectDirection = RectDirection;
    private readonly float lengthFront = LengthFront;
    private readonly float rectHalfWidth = RectHalfWidth;

    public override float Distance(WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        if (projected.InSquare(center, halfWidth) && projected.InRect(rectOrigin, rectDirection, lengthFront, default, rectHalfWidth))
        {
            return 1f;
        }
        return default;
    }
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareAwayFromOriginIntoCircle(WPos Center, WPos Origin, float Distance, float HalfWidth, WPos CircleOrigin, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;
    private readonly WPos circleOrigin = CircleOrigin;
    private readonly float radius = Radius;

    public override float Distance(WPos p)
    {
        var projected = p + distance * (p - origin);
        if (projected.InCircle(circleOrigin, radius) && projected.InSquare(center, halfWidth))
        {
            return 1f;
        }
        return default;
    }
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareFixedDirection(WPos Center, WDir Direction, float HalfWidth) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly float halfWidth = HalfWidth;

    public override float Distance(WPos p)
    {
        if ((p + direction).InSquare(center, halfWidth))
        {
            return 1f;
        }
        return default;
    }
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareFixedDirectionPlusAOECircle(WPos Center, WDir Direction, float HalfWidth, WPos CircleOrigin, float Radius) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly float halfWidth = HalfWidth;
    private readonly WPos circleOrigin = CircleOrigin;
    private readonly float radius = Radius;

    public override float Distance(WPos p)
    {
        var projected = p + direction;
        if (projected.InSquare(center, halfWidth) && !projected.InCircle(circleOrigin, radius))
        {
            return 1f;
        }
        return default;
    }
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareFixedDirectionPlusAOECircles(WPos Center, WDir Direction, float HalfWidth, WPos[] Origins, float Radius, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly float halfWidth = HalfWidth;
    private readonly WPos[] origins = Origins;
    private readonly float radius = Radius;
    private readonly int len = Length;

    public override float Distance(WPos p)
    {
        var projected = p + direction;
        for (var i = 0; i < len; ++i)
        {
            if (projected.InCircle(origins[i], radius))
            {
                return default;
            }
        }
        if (projected.InSquare(center, halfWidth))
        {
            return 1f;
        }
        return default;
    }
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareAwayFromOriginPlusAOECirclesMixedRadii(WPos Center, WPos Origin, float Distance, float HalfWidth, (WPos Origin, float Radius)[] AOEs, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;
    private readonly (WPos origin, float radius)[] aoes = AOEs;
    private readonly int len = Length;

    public override float Distance(WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        if (!projected.InSquare(center, halfWidth))
        {
            return default;
        }

        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (projected.InCircle(aoe.origin, aoe.radius))
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
public sealed class SDKnockbackInAABBSquareAwayFromOriginPlusAOECirclesMixedRadiiPlusAvoidShape(WPos Center, WPos Origin, float Distance, float HalfWidth, (WPos Origin, float Radius)[] AOEs, int Length, ShapeDistance Shape) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;
    private readonly (WPos origin, float radius)[] aoes = AOEs;
    private readonly int len = Length;
    private readonly ShapeDistance shape = Shape;

    public override float Distance(WPos p)
    {
        var dist = shape.Distance(p);
        if (dist <= 0)
        {
            return dist;
        }

        var projected = p + distance * (p - origin).Normalized();
        if (!projected.InSquare(center, halfWidth))
        {
            return default;
        }

        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (projected.InCircle(aoe.origin, aoe.radius))
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
public sealed class SDKnockbackInAABBSquareAwayFromOriginPlusAOECircles(WPos Center, WPos Origin, float Distance, float HalfWidth, WPos[] AOEs, float Radius, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;
    private readonly WPos[] aoes = AOEs;
    private readonly float radius = Radius;
    private readonly int len = Length;

    public override float Distance(WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        for (var i = 0; i < len; ++i)
        {
            if (projected.InCircle(aoes[i], radius))
            {
                return default;
            }
        }
        if (projected.InSquare(center, halfWidth))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareAwayFromOriginPlusAOERects(WPos Center, WPos Origin, float Distance, float HalfWidth, (WPos Origin, WDir Direction)[] AOEs, float LengthFront, float RectHalfWidth, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float distance = Distance;
    private readonly (WPos origin, WDir direction)[] aoes = AOEs;
    private readonly float lenFront = LengthFront;
    private readonly float rectHalfWidth = RectHalfWidth;
    private readonly int len = Length;

    public override float Distance(WPos p)
    {
        var projected = p + distance * (p - origin).Normalized();
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (projected.InRect(aoe.origin, aoe.direction, lenFront, default, rectHalfWidth))
            {
                return default;
            }
        }
        if (projected.InSquare(center, halfWidth))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareFixedDirectionPlusMixedAOEs(WPos Center, WDir Direction, float HalfWidth, Components.GenericAOEs.AOEInstance[] AOEs, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly float halfWidth = HalfWidth;
    private readonly Components.GenericAOEs.AOEInstance[] aoes = AOEs;
    private readonly int len = Length;

    public override float Distance(WPos p)
    {
        var projected = p + direction;
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Check(projected))
            {
                return default;
            }
        }
        if (projected.InSquare(center, halfWidth))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
