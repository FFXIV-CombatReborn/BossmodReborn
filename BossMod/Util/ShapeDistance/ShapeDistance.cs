namespace BossMod;

// shapes can be defined by distance from point to shape's border; distance is positive for points outside shape and negative for points inside shape
// union is min, intersection is max
// NOTE: some of these are not returning the true distance, for example knockback related SDs and return only 0 for forbidden and 1 for allowed. best to add 1y safety margin to cover all points in a cell

[SkipLocalsInit]
public abstract class ShapeDistance
{
    public const float Epsilon = 1e-5f;

    public abstract float Distance(in WPos p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool Contains(in WPos p) => Distance(p) <= 0f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NearlyZero(float v) => Math.Abs(v) <= Epsilon;
}
