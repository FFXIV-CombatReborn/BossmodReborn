namespace BossMod;

[SkipLocalsInit]
public sealed class SDComplexPolygonInvertedContains(RelSimplifiedComplexPolygon Polygon, WPos Center) : ShapeDistance
{
    private readonly RelSimplifiedComplexPolygon polygon = Polygon;
    private readonly WPos center = Center;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos p)
    {
        return polygon.Contains(p - center);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public readonly struct SDPolygonWithHolesBase
{
    private readonly RelSimplifiedComplexPolygon _polygon;
    private readonly float _originX, _originZ;
    private readonly Edge[] _edges;
    private readonly SpatialIndex _spatialIndex;

    public SDPolygonWithHolesBase(WPos origin, RelSimplifiedComplexPolygon polygon)
    {
        _originX = origin.X;
        _originZ = origin.Z;
        _polygon = polygon;
        var edgeCount = 0;
        var countPolygonParts = polygon.Parts.Count;
        for (var i = 0; i < countPolygonParts; ++i)
        {
            var part = polygon.Parts[i];
            edgeCount += part.ExteriorEdges.Length;
            var lenPolygonHoles = part.Holes.Length;
            for (var j = 0; j < lenPolygonHoles; ++j)
                edgeCount += part.InteriorEdges(j).Length;
        }
        _edges = new Edge[edgeCount];
        var edgeIndex = 0;
        for (var i = 0; i < countPolygonParts; ++i)
        {
            var part = polygon.Parts[i];
            var exteriorEdges = GetEdges(part.Exterior, origin);
            var exteriorCount = exteriorEdges.Length;
            Array.Copy(exteriorEdges, 0, _edges, edgeIndex, exteriorCount);
            edgeIndex += exteriorCount;
            var lenPolygonHoles = part.Holes.Length;
            for (var j = 0; j < lenPolygonHoles; ++j)
            {
                var holeEdges = GetEdges(part.Interior(j), origin);
                var holeEdgesCount = holeEdges.Length;
                Array.Copy(holeEdges, 0, _edges, edgeIndex, holeEdgesCount);
                edgeIndex += holeEdgesCount;
            }
        }
        _spatialIndex = new(_edges);

        static Edge[] GetEdges(ReadOnlySpan<WDir> vertices, WPos origin)
        {
            var count = vertices.Length;

            if (count == 0)
            {
                return [];
            }

            var edges = new Edge[count];

            var prev = vertices[count - 1];
            var originX = origin.X;
            var originZ = origin.Z;

            for (var i = 0; i < count; ++i)
            {
                var curr = vertices[i];
                var prevX = prev.X;
                var prevZ = prev.Z;
                edges[i] = new(originX + prevX, originZ + prevZ, curr.X - prevX, curr.Z - prevZ);
                prev = curr;
            }

            return edges;
        }
    }

    public readonly float Distance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        if (_polygon.Contains(new(pX - _originX, pZ - _originZ))) // NOTE: our usecase doesn't care about distance inside of the polygon, so we can short circuit here
        {
            return default;
        }
        var minDistanceSq = float.MaxValue;

        var indices = _spatialIndex.Query(pX, pZ);
        var len = indices.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref _edges[indices[i]];
            var edgeAx = edge.Ax;
            var edgeAy = edge.Ay;
            var edgeDx = edge.Dx;
            var edgeDy = edge.Dy;
            var t = Math.Clamp(((pX - edgeAx) * edgeDx + (pZ - edgeAy) * edgeDy) * edge.InvLengthSq, default, 1f);
            var distX = pX - (edgeAx + t * edgeDx);
            var distY = pZ - (edgeAy + t * edgeDy);

            minDistanceSq = Math.Min(minDistanceSq, distX * distX + distY * distY);
        }
        return MathF.Sqrt(minDistanceSq);
    }

    public readonly float DistanceInverted(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        if (!_polygon.Contains(new(pX - _originX, pZ - _originZ))) // NOTE: our usecase doesn't care about distance outside of the polygon, so we can short circuit here
        {
            return default;
        }
        var minDistanceSq = float.MaxValue;

        var indices = _spatialIndex.Query(pX, pZ);
        var len = indices.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref _edges[indices[i]];
            var edgeAx = edge.Ax;
            var edgeAy = edge.Ay;
            var edgeDx = edge.Dx;
            var edgeDy = edge.Dy;
            var t = Math.Clamp(((pX - edgeAx) * edgeDx + (pZ - edgeAy) * edgeDy) * edge.InvLengthSq, default, 1f);
            var distX = pX - (edgeAx + t * edgeDx);
            var distY = pZ - (edgeAy + t * edgeDy);

            minDistanceSq = Math.Min(minDistanceSq, distX * distX + distY * distY);
        }
        return MathF.Sqrt(minDistanceSq);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        return _polygon.Contains(new(pX - _originX, pZ - _originZ));
    }
}

[SkipLocalsInit]
public sealed class SDPolygonWithHoles(SDPolygonWithHolesBase core) : ShapeDistance
{
    private readonly SDPolygonWithHolesBase _core = core;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        return _core.Distance(p);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos p)
    {
        return _core.Contains(p);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDInvertedPolygonWithHoles(SDPolygonWithHolesBase core) : ShapeDistance
{
    private readonly SDPolygonWithHolesBase _core = core;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        return _core.DistanceInverted(p);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos p)
    {
        return !_core.Contains(p);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
