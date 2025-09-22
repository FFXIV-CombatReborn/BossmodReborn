namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackInAABBRectFixedDirection(WPos Center, WDir Direction, float HalfWidth, float HalfHeight) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction;
    private readonly float halfWidth = HalfWidth;
    private readonly float halfHeight = HalfHeight;

    public override float Distance(WPos p)
    {
        if ((p + direction).InRect(center, halfWidth, halfHeight))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos p)
    {
        return Distance(p) <= 0f;
    }
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBRectAwayFromOrigin(WPos Center, WPos Origin, float Distance, float HalfWidth, float HalfHeight) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WPos origin = Origin;
    private readonly float halfWidth = HalfWidth;
    private readonly float halfHeight = HalfHeight;
    private readonly float distance = Distance;

    public override float Distance(WPos p)
    {
        if ((p + distance * (p - origin).Normalized()).InRect(center, halfWidth, halfHeight))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos p)
    {
        return Distance(p) <= 0f;
    }
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBRectLeftRightAlongZAxis(WPos Center, float Distance, float HalfWidth, float HalfHeight) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly float originZ = Center.Z;
    private readonly WDir dir1 = new(default, Distance);
    private readonly WDir dir2 = new(default, -Distance);
    private readonly float halfWidth = HalfWidth;
    private readonly float halfHeight = HalfHeight;

    public override float Distance(WPos p)
    {
        if ((p + (p.Z > originZ ? dir1 : dir2)).InRect(center, halfWidth, halfHeight))
        {
            return 1f;
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos p)
    {
        return Distance(p) <= 0f;
    }
}

[SkipLocalsInit]
public sealed class SDKnockbackInAABBRectLeftRightAlongZAxisPlusAOERects(WPos Center, float Distance, float HalfWidth, float HalfHeight, (WPos Origin, WDir Direction)[] AOEs, float LengthFront, float RectHalfWidth, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly float originZ = Center.Z;
    private readonly WDir dir1 = new(default, Distance);
    private readonly WDir dir2 = new(default, -Distance);
    private readonly float halfWidth = HalfWidth;
    private readonly float halfHeight = HalfHeight;
    private readonly (WPos origin, WDir direction)[] aoes = AOEs;
    private readonly float lenFront = LengthFront;
    private readonly float rectHalfWidth = RectHalfWidth;
    private readonly int len = Length;

    public override float Distance(WPos p)
    {
        var projected = p + (p.Z > originZ ? dir1 : dir2);

        if (!projected.InRect(center, halfWidth, halfHeight))
        {
            return default;
        }

        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (projected.InRect(aoe.origin, aoe.direction, lenFront, default, rectHalfWidth))
            {
                return default;
            }
        }

        return 1f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos p)
    {
        return Distance(p) <= 0f;
    }
}
