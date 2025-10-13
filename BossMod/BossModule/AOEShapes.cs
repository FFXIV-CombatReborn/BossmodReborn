namespace BossMod;

[SkipLocalsInit]
public abstract class AOEShape(bool invertForbiddenZone)
{
    public bool InvertForbiddenZone = invertForbiddenZone;

    public abstract bool Check(WPos position, WPos origin, Angle rotation);
    public abstract void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default);
    public abstract void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f);
    public abstract ShapeDistance Distance(WPos origin, Angle rotation);
    public abstract ShapeDistance InvertedDistance(WPos origin, Angle rotation);

    public bool Check(WPos position, Actor? origin) => origin != null && Check(position, origin.Position, origin.Rotation);

    public void Draw(MiniArena arena, Actor? origin, uint color = default)
    {
        if (origin != null)
            Draw(arena, origin.Position, origin.Rotation, color);
    }

    public void Outline(MiniArena arena, Actor? origin, uint color = default)
    {
        if (origin != null)
            Outline(arena, origin.Position, origin.Rotation, color);
    }
}

[SkipLocalsInit]
public sealed class AOEShapeCone(float radius, Angle halfAngle, Angle directionOffset = default, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    public readonly float Radius = radius;
    public readonly Angle HalfAngle = halfAngle;
    public readonly Angle DirectionOffset = directionOffset;

    public override string ToString() => $"Cone: r={Radius:f3}, angle={HalfAngle * 2f}, off={DirectionOffset}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InCircleCone(origin, Radius, rotation + DirectionOffset, HalfAngle);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default) => arena.ZoneCone(origin, default, Radius, rotation + DirectionOffset, HalfAngle, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f) => arena.AddCone(origin, Radius, rotation + DirectionOffset, HalfAngle, color, thickness);
    public override ShapeDistance Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone
            ? new SDCone(origin, Radius, rotation + DirectionOffset, HalfAngle)
            : new SDInvertedCone(origin, Radius, rotation + DirectionOffset, HalfAngle);
    }
    public override ShapeDistance InvertedDistance(WPos origin, Angle rotation) => new SDInvertedCone(origin, Radius, rotation + DirectionOffset, HalfAngle);
}

[SkipLocalsInit]
public sealed class AOEShapeCircle(float radius, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    public readonly float Radius = radius;

    public override string ToString() => $"Circle: r={Radius:f3}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation = default) => position.InCircle(origin, Radius);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation = default, uint color = default) => arena.ZoneCircle(origin, Radius, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation = default, uint color = default, float thickness = 1f) => arena.AddCircle(origin, Radius, color, thickness);
    public override ShapeDistance Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone
            ? new SDCircle(origin, Radius)
            : new SDInvertedCircle(origin, Radius);
    }
    public override ShapeDistance InvertedDistance(WPos origin, Angle rotation) => new SDInvertedCircle(origin, Radius);
}

[SkipLocalsInit]
public sealed class AOEShapeDonut(float innerRadius, float outerRadius, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    public readonly float InnerRadius = innerRadius;
    public readonly float OuterRadius = outerRadius;

    public override string ToString() => $"Donut: r={InnerRadius:f3}-{OuterRadius:f3}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation = default) => position.InDonut(origin, InnerRadius, OuterRadius);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation = default, uint color = default) => arena.ZoneDonut(origin, InnerRadius, OuterRadius, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation = default, uint color = default, float thickness = 1f)
    {
        arena.AddCircle(origin, InnerRadius, color, thickness);
        arena.AddCircle(origin, OuterRadius, color, thickness);
    }
    public override ShapeDistance Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone
            ? new SDDonut(origin, InnerRadius, OuterRadius)
            : new SDInvertedDonut(origin, InnerRadius, OuterRadius);
    }
    public override ShapeDistance InvertedDistance(WPos origin, Angle rotation) => new SDInvertedDonut(origin, InnerRadius, OuterRadius);
}

[SkipLocalsInit]
public sealed class AOEShapeDonutSector(float innerRadius, float outerRadius, Angle halfAngle, Angle directionOffset = default, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    public readonly float InnerRadius = innerRadius;
    public readonly float OuterRadius = outerRadius;
    public readonly Angle HalfAngle = halfAngle;
    public readonly Angle DirectionOffset = directionOffset;

    public override string ToString() => $"Donut sector: r={InnerRadius:f3}-{OuterRadius:f3}, angle={HalfAngle * 2f}, off={DirectionOffset}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InDonutCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default) => arena.ZoneCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f) => arena.AddDonutCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle, color, thickness);
    public override ShapeDistance Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone
            ? new SDDonutSector(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle)
            : new SDInvertedDonutSector(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
    }
    public override ShapeDistance InvertedDistance(WPos origin, Angle rotation) => new SDInvertedDonutSector(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
}

[SkipLocalsInit]
public sealed class AOEShapeRect(float lengthFront, float halfWidth, float lengthBack = default, Angle directionOffset = default, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    public readonly float LengthFront = lengthFront;
    public readonly float HalfWidth = halfWidth;
    public readonly float LengthBack = lengthBack;
    public readonly Angle DirectionOffset = directionOffset;

    public override string ToString() => $"Rect: l={LengthFront:f3}+{LengthBack:f3}, w={HalfWidth * 2f}, off={DirectionOffset}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InRect(origin, rotation, LengthFront, LengthBack, HalfWidth);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default) => arena.ZoneRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f) => arena.AddRect(origin, (rotation + DirectionOffset).ToDirection(), LengthFront, LengthBack, HalfWidth, color, thickness);
    public override ShapeDistance Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone
            ? new SDRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth)
            : new SDInvertedRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth);
    }
    public override ShapeDistance InvertedDistance(WPos origin, Angle rotation) => new SDInvertedRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth);

}

[SkipLocalsInit]
public sealed class AOEShapeCross(float length, float halfWidth, Angle directionOffset = default, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    public readonly float Length = length;
    public readonly float HalfWidth = halfWidth;
    public readonly Angle DirectionOffset = directionOffset;

    public override string ToString() => $"Cross: l={Length:f3}, w={HalfWidth * 2f}, off={DirectionOffset}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InCross(origin, rotation + DirectionOffset, Length, HalfWidth);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default) => arena.ZoneCross(origin, rotation + DirectionOffset, Length, HalfWidth, ContourPoints(origin, rotation), color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f)
    {
        var points = ContourPoints(origin, rotation);
        for (var i = 0; i < 12; ++i)
        {
            arena.PathLineTo(points[i]);
        }
        MiniArena.PathStroke(true, color, thickness);
    }

    private WPos[] ContourPoints(WPos origin, Angle rotation, float offset = default)
    {
        var dx = (rotation + DirectionOffset).ToDirection();
        var dy = dx.OrthoL();

        var lengthOffset = Length + offset;
        var halfWidthOffset = HalfWidth + offset;

        var dxLength = dx * lengthOffset;
        var dxWidth = dx * halfWidthOffset;
        var dyLength = dy * lengthOffset;
        var dyWidth = dy * halfWidthOffset;

        return
        [
            origin + dxLength - dyWidth,
            origin + dxWidth - dyWidth,
            origin + dxWidth - dyLength,
            origin - dxWidth - dyLength,
            origin - dxWidth - dyWidth,
            origin - dxLength - dyWidth,
            origin - dxLength + dyWidth,
            origin - dxWidth + dyWidth,
            origin - dxWidth + dyLength,
            origin + dxWidth + dyLength,
            origin + dxWidth + dyWidth,
            origin + dxLength + dyWidth
        ];
    }

    public override ShapeDistance Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone
            ? new SDCross(origin, rotation + DirectionOffset, Length, HalfWidth)
            : new SDInvertedCross(origin, rotation + DirectionOffset, Length, HalfWidth);
    }
    public override ShapeDistance InvertedDistance(WPos origin, Angle rotation) => new SDInvertedCross(origin, rotation + DirectionOffset, Length, HalfWidth);
}

[SkipLocalsInit]
public sealed class AOEShapeTriCone(float sideLength, Angle halfAngle, Angle directionOffset = default, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    public readonly float SideLength = sideLength;
    public readonly Angle HalfAngle = halfAngle;
    public readonly Angle DirectionOffset = directionOffset;

    public override string ToString() => $"TriCone: side={SideLength:f3}, angle={HalfAngle * 2f}, off={DirectionOffset}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InTri(origin, origin + SideLength * (rotation + DirectionOffset + HalfAngle).ToDirection(), origin + SideLength * (rotation + DirectionOffset - HalfAngle).ToDirection());
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default) => arena.ZoneTri(origin, origin + SideLength * (rotation + DirectionOffset + HalfAngle).ToDirection(), origin + SideLength * (rotation + DirectionOffset - HalfAngle).ToDirection(), color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f) => arena.AddTriangle(origin, origin + SideLength * (rotation + DirectionOffset + HalfAngle).ToDirection(), origin + SideLength * (rotation + DirectionOffset - HalfAngle).ToDirection(), color, thickness);

    public override ShapeDistance Distance(WPos origin, Angle rotation)
    {
        var rotoffset = rotation + DirectionOffset;
        var direction1 = SideLength * (rotoffset + HalfAngle).ToDirection();
        var direction2 = SideLength * (rotoffset - HalfAngle).ToDirection();
        var shape = new RelTriangle(default, direction1, direction2);
        return !InvertForbiddenZone ? new SDTri(origin, shape) : new SDInvertedTri(origin, shape);
    }
    public override ShapeDistance InvertedDistance(WPos origin, Angle rotation)
    {
        var rotoffset = rotation + DirectionOffset;
        var direction1 = SideLength * (rotoffset + HalfAngle).ToDirection();
        var direction2 = SideLength * (rotoffset - HalfAngle).ToDirection();
        var shape = new RelTriangle(default, direction1, direction2);
        return new SDInvertedTri(origin, shape);
    }
}

[SkipLocalsInit]
public sealed class AOEShapeCapsule(float radius, float length, Angle directionOffset = default, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    public readonly float Radius = radius;
    public readonly float Length = length;
    public readonly Angle DirectionOffset = directionOffset;

    public override string ToString() => $"Capsule: radius={Radius:f3}, length={Length}, off={DirectionOffset}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InCapsule(origin, (rotation + DirectionOffset).ToDirection(), Radius, Length);

    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default) => arena.ZoneCapsule(origin, (rotation + DirectionOffset).ToDirection(), Radius, Length, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f)
    => arena.AddCapsule(origin, (rotation + DirectionOffset).ToDirection(), Radius, Length, color, thickness);
    public override ShapeDistance Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone ? new SDCapsule(origin, rotation, Length, Radius) : new SDInvertedCapsule(origin, rotation, Length, Radius);
    }
    public override ShapeDistance InvertedDistance(WPos origin, Angle rotation) => new SDInvertedCapsule(origin, rotation, Length, Radius);
}

public enum OperandType
{
    Union,
    Xor,
    Intersection,
    Difference
}

// shapes1 for unions, shapes 2 for shapes for XOR/intersection with shapes1, differences for shapes that get subtracted after previous operations
// always create a new instance of AOEShapeCustom if something other than the invertforbiddenzone changes
// if the origin of the AOE can change, edit the origin default value to prevent cache issues
[SkipLocalsInit]
public sealed class AOEShapeCustom : AOEShape
{
    private readonly IReadOnlyList<Shape> Shapes1;
    private readonly IReadOnlyList<Shape> DifferenceShapes;
    private readonly IReadOnlyList<Shape> Shapes2;
    private readonly int hashkey;
    public RelSimplifiedComplexPolygon Polygon;
    private SDPolygonWithHolesBase shapeDistance;
    private bool isShapeDistanceInitialized;
    private readonly OperandType Operand;
    private readonly WPos Origin;

    public AOEShapeCustom(IReadOnlyList<Shape> shapes1, IReadOnlyList<Shape>? differenceShapes = null, IReadOnlyList<Shape>? shapes2 = null, OperandType operand = OperandType.Union, WPos origin = default, bool invertForbiddenZone = false) : base(invertForbiddenZone)
    {
        Shapes1 = shapes1;
        DifferenceShapes = differenceShapes ?? [];
        Shapes2 = shapes2 ?? [];
        Origin = origin;
        Operand = operand;
        hashkey = CreateCacheKey(Shapes1, Shapes2, DifferenceShapes, Operand, Origin);
    }

    private static readonly Dictionary<int, RelSimplifiedComplexPolygon> cache = [];
    private static readonly LinkedList<int> cacheOrder = [];

    private void AddToCache(RelSimplifiedComplexPolygon value)
    {
        if (cache.Count >= 50)
        {
            var lruKey = cacheOrder.Last?.Value;
            if (lruKey != null)
            {
                cache.Remove(lruKey.Value);
                cacheOrder.RemoveLast();
            }
        }
        cache[hashkey] = value;
        cacheOrder.Remove(hashkey);
        cacheOrder.AddFirst(hashkey);
    }

    public override string ToString() => $"Custom AOE shape: hashkey={hashkey}, ifz={InvertForbiddenZone}";

    public RelSimplifiedComplexPolygon GetCombinedPolygon(WPos origin)
    {
        if (cache.TryGetValue(hashkey, out var cachedResult))
        {
            cacheOrder.Remove(hashkey);
            cacheOrder.AddFirst(hashkey);
            return Polygon = cachedResult;
        }

        var shapes1 = CreateOperandFromShapes(Shapes1, origin);

        var clipper = new PolygonClipper();
        if (Shapes2 == null)
        {
            if (DifferenceShapes != null)
            {
                Polygon = clipper.Difference(shapes1, CreateOperandFromShapes(DifferenceShapes, origin));
                AddToCache(Polygon);
                return Polygon;
            }
            else
            {
                Polygon = clipper.Simplify(shapes1);
                AddToCache(Polygon);
                return Polygon;
            }
        }
        else
        {
            Polygon = clipper.Simplify(shapes1);
            if (Operand is OperandType.Intersection or OperandType.Xor)
            {
                var count = Shapes2.Count;
                for (var i = 0; i < count; ++i)
                {
                    var shape = Shapes2[i];
                    var singleShapeOperand = CreateOperandFromShape(shape, origin);
                    var operand = new PolygonClipper.Operand(Polygon);
                    switch (Operand)
                    {
                        case OperandType.Intersection:
                            Polygon = clipper.Intersect(operand, singleShapeOperand);
                            break;
                        case OperandType.Xor:
                            Polygon = clipper.Xor(operand, singleShapeOperand);
                            break;
                    }
                }
            }
            Polygon = DifferenceShapes != null ? clipper.Difference(new PolygonClipper.Operand(Polygon), CreateOperandFromShapes(DifferenceShapes, origin)) : Polygon;
            if (Operand == OperandType.Union)
            {
                Polygon = clipper.Union(CreateOperandFromShapes(Shapes2, origin), new PolygonClipper.Operand(Polygon));
            }
            AddToCache(Polygon);
            return Polygon;
        }
    }

    private static PolygonClipper.Operand CreateOperandFromShape(Shape shape, WPos origin)
    {
        var operand = new PolygonClipper.Operand();
        operand.AddPolygon(shape.ToPolygon(origin));
        return operand;
    }

    private static PolygonClipper.Operand CreateOperandFromShapes(IReadOnlyList<Shape>? shapes, WPos origin)
    {
        var operand = new PolygonClipper.Operand();
        if (shapes != null)
        {
            var count = shapes.Count;
            for (var i = 0; i < count; ++i)
            {
                operand.AddPolygon(shapes[i].ToPolygon(origin));
            }
        }
        return operand;
    }

    public override bool Check(WPos position, WPos origin, Angle rotation)
    {
        return (Polygon ?? GetCombinedPolygon(origin)).Contains(position - origin);
    }

    private static int CreateCacheKey(IReadOnlyList<Shape> shapes1, IReadOnlyList<Shape> shapes2, IReadOnlyList<Shape> differenceShapes, OperandType operand, WPos origin)
    {
        var hashCode = new HashCode();
        var count1 = shapes1.Count;
        var count2 = shapes2.Count;
        var count3 = differenceShapes.Count;
        for (var i = 0; i < count1; ++i)
        {
            hashCode.Add(shapes1[i].GetHashCode());
        }
        for (var i = 0; i < count2; ++i)
        {
            hashCode.Add(shapes2[i].GetHashCode());
        }
        for (var i = 0; i < count3; ++i)
        {
            hashCode.Add(differenceShapes[i].GetHashCode());
        }
        hashCode.Add(operand);
        hashCode.Add(origin);
        return hashCode.ToHashCode();
    }

    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default)
    {
        arena.ZoneRelPoly(hashkey, Polygon ?? GetCombinedPolygon(origin), color);
    }

    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f)
    {
        var combinedPolygon = Polygon ?? GetCombinedPolygon(origin);
        arena.AddComplexPolygon(origin, combinedPolygon, color, thickness);
    }

    public override ShapeDistance Distance(WPos origin, Angle rotation)
    {
        if (!isShapeDistanceInitialized)
        {
            shapeDistance = new SDPolygonWithHolesBase(origin, Polygon ?? GetCombinedPolygon(origin));
            isShapeDistanceInitialized = true;
        }
        return InvertForbiddenZone ? new SDInvertedPolygonWithHoles(shapeDistance) : new SDPolygonWithHoles(shapeDistance);
    }

    public override ShapeDistance InvertedDistance(WPos origin, Angle rotation)
    {
        if (!isShapeDistanceInitialized)
        {
            shapeDistance = new SDPolygonWithHolesBase(origin, Polygon ?? GetCombinedPolygon(origin));
            isShapeDistanceInitialized = true;
        }
        return new SDInvertedPolygonWithHoles(shapeDistance);
    }

    public AOEShapeCustom Clone() => (AOEShapeCustom)MemberwiseClone();
}
