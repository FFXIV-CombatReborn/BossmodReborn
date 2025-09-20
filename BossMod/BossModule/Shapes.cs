using Clipper2Lib;

namespace BossMod;

[SkipLocalsInit]
public abstract record class Shape
{
    public const float MaxApproxError = CurveApprox.ScreenError;
    public const float Half = 0.5f;

    public abstract List<WDir> Contour(WPos center);

    public RelSimplifiedComplexPolygon ToPolygon(WPos center) => new((List<RelPolygonWithHoles>)[new(Contour(center))]);
}

[SkipLocalsInit]
public sealed record class Circle(WPos Center, float Radius) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var vertices = CurveApprox.Circle(Radius, MaxApproxError);
        var len = vertices.Length;
        var result = new List<WDir>(len);
        var offset = Center - center;
        for (var i = 0; i < len; ++i)
        {
            result.Add(vertices[i] + offset);
        }
        return result;
    }

    public override string ToString() => $"Circle:{Center},{Radius}";
}

// for custom polygons defined by an IReadOnlyList of vertices
[SkipLocalsInit]
public sealed record class PolygonCustom(IReadOnlyList<WPos> Vertices) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var vertices = Vertices;
        var count = vertices.Count;
        var result = new List<WDir>(count);
        for (var i = 0; i < count; ++i)
        {
            result.Add(vertices[i] - center);
        }
        return result;
    }

    public override string ToString()
    {
        var vertices = Vertices;
        var count = vertices.Count;
        var sb = new StringBuilder("PolygonCustom:", 14 + count * 9);
        for (var i = 0; i < count; ++i)
        {
            var vertex = vertices[i];
            sb.Append(vertex).Append(';');
        }
        --sb.Length;
        return sb.ToString();
    }
}

[SkipLocalsInit]
public sealed record class PolygonCustomRel(IReadOnlyList<WDir> Vertices) : Shape
{
    public override List<WDir> Contour(WPos center) => [.. Vertices];

    public override string ToString()
    {
        var vertices = Vertices;
        var count = vertices.Count;
        var sb = new StringBuilder("PolygonCustomRel:", 17 + count * 9);
        for (var i = 0; i < count; ++i)
        {
            var vertex = vertices[i];
            sb.Append(vertex).Append(';');
        }
        --sb.Length;
        return sb.ToString();
    }
}

[SkipLocalsInit]
public sealed record class Donut(WPos Center, float InnerRadius, float OuterRadius) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var vertices = CurveApprox.Donut(InnerRadius, OuterRadius, MaxApproxError);
        var len = vertices.Length;
        var result = new List<WDir>(len);
        var offset = Center - center;
        for (var i = 0; i < len; ++i)
        {
            result.Add(vertices[i] + offset);
        }
        return result;
    }

    public override string ToString() => $"Donut:{Center},{InnerRadius},{OuterRadius}";
}

// for rectangles defined by a center, halfwidth, halfheight and optionally rotation
[SkipLocalsInit]
public record class Rectangle(WPos Center, float HalfWidth, float HalfHeight, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var dir = Rotation != default ? Rotation.ToDirection() : new(default, 1f);
        var dx = dir.OrthoL() * HalfWidth;
        var dz = dir * HalfHeight;

        WDir[] vertices =
        [
            dx - dz,
            -dx - dz,
            -dx + dz,
            dx + dz
        ];

        var offset = Center - center;
        var result = new List<WDir>(4);
        for (var i = 0; i < 4; ++i)
        {
            result.Add(vertices[i] + offset);
        }
        return result;
    }

    public override string ToString() => $"Rectangle:{Center},{HalfWidth},{HalfHeight},{Rotation}";
}

// for rectangles defined by a start point, end point and halfwidth
[SkipLocalsInit]
public sealed record class RectangleSE(WPos Start, WPos End, float HalfWidth) : Rectangle(
    Center: new((Start.X + End.X) * Half, (Start.Z + End.Z) * Half),
    HalfWidth: HalfWidth,
    HalfHeight: (End - Start).Length() * Half,
    Rotation: new Angle(MathF.Atan2(End.X - Start.X, End.Z - Start.Z))
);

[SkipLocalsInit]
public sealed record class Square(WPos Center, float HalfSize, Angle Rotation = default) : Rectangle(Center, HalfSize, HalfSize, Rotation);

[SkipLocalsInit]
public sealed record class Cross(WPos Center, float Length, float HalfWidth, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var dx = Rotation.ToDirection();
        var dy = dx.OrthoL();
        var dx1 = dx * Length;
        var dx2 = dx * HalfWidth;
        var dy1 = dy * Length;
        var dy2 = dy * HalfWidth;

        WDir[] vertices =
        [
            dx1 + dy2,
            dx2 + dy2,
            dx2 + dy1,
            -dx2 + dy1,
            -dx2 + dy2,
            -dx1 + dy2,
            -dx1 - dy2,
            -dx2 - dy2,
            -dx2 - dy1,
            dx2 - dy1,
            dx2 - dy2,
            dx1 - dy2
        ];

        var offset = Center - center;
        var result = new List<WDir>(12);
        for (var i = 0; i < 12; ++i)
        {
            result.Add(vertices[i] + offset);
        }
        return result;
    }

    public override string ToString() => $"Cross:{Center},{Length},{HalfWidth},{Rotation}";
}

// for polygons with edge count number of lines of symmetry, eg. pentagons, hexagons and octagons
[SkipLocalsInit]
public sealed record class Polygon(WPos Center, float Radius, int Edges, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var edges = Edges;
        var angleIncrement = Angle.DoublePI / edges;
        var initialRotation = Rotation.Rad;
        var radius = Radius;
        var vertices = new List<WDir>(edges);
        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;
        for (var i = 0; i < edges; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement + initialRotation);
            vertices.Add(new(radius * sin + offsetX, radius * cos + offsetZ));
        }
        return vertices;
    }

    public override string ToString() => $"Polygon:{Center},{Radius},{Edges},{Rotation}";
}

// for cones defined by radius, start angle and end angle
[SkipLocalsInit]
public record class Cone(WPos Center, float Radius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var points = CurveApprox.CircleSector(Radius, StartAngle, EndAngle, MaxApproxError);
        var len = points.Length;
        var vertices = new List<WDir>(len);
        var offset = Center - center;
        for (var i = 0; i < len; ++i)
        {
            vertices.Add(points[i] + offset);
        }
        return vertices;
    }

    public override string ToString() => $"Cone:{Center},{Radius},{StartAngle},{EndAngle}";
}

// for cones defined by radius, direction and half angle
[SkipLocalsInit]
public sealed record class ConeHA(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle) : Cone(Center, Radius, CenterDir - HalfAngle, CenterDir + HalfAngle);

// for donut segments defined by inner and outer radius, direction, start angle and end angle
[SkipLocalsInit]
public record class DonutSegment(WPos Center, float InnerRadius, float OuterRadius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var vertices = CurveApprox.DonutSector(InnerRadius, OuterRadius, StartAngle, EndAngle, MaxApproxError);
        var len = vertices.Length;
        var result = new List<WDir>(len);
        var offset = Center - center;
        for (var i = 0; i < len; ++i)
        {
            result.Add(vertices[i] + offset);
        }
        return result;
    }

    public override string ToString() => $"DonutSegment:{Center},{InnerRadius},{OuterRadius},{StartAngle},{EndAngle}";
}

// for donut segments defined by inner and outer radius, direction and half angle
[SkipLocalsInit]
public sealed record class DonutSegmentHA(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle) : DonutSegment(Center, InnerRadius, OuterRadius,
CenterDir - HalfAngle, CenterDir + HalfAngle);

// Approximates a cone with a customizable number of edges for the circle arc - with 1 edge this turns into a triangle, 2 edges result in a parallelogram
[SkipLocalsInit]
public sealed record class ConeV(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle, int Edges) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var edges = Edges;
        var angleIncrement = 2f * HalfAngle.Rad / edges;
        var startAngle = CenterDir.Rad - HalfAngle.Rad;
        var vertices = new List<WDir>(edges + 2);
        var radius = Radius;
        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;
        var e1 = edges + 1;
        for (var i = 0; i < e1; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(startAngle + i * angleIncrement);
            vertices.Add(new(radius * sin + offsetX, radius * cos + offsetZ));
        }
        vertices.Add(offset);
        return vertices;
    }

    public override string ToString() => $"ConeV:{Center},{Radius},{CenterDir},{HalfAngle},{Edges}";
}

// Approximates a donut segment with a customizable number of edges per circle arc
[SkipLocalsInit]
public sealed record class DonutSegmentV(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle, int Edges) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var edges = Edges;
        var angleIncrement = 2f * HalfAngle.Rad / edges;
        var startAngle = CenterDir.Rad - HalfAngle.Rad;
        var n = Edges + 1;
        var vertices = new WDir[2 * edges + 2];
        var innerRadius = InnerRadius;
        var outerRadius = OuterRadius;
        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;
        var indexInner = 2 * edges + 1;
        for (var i = 0; i < n; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(startAngle + i * angleIncrement);
            vertices[i] = new(outerRadius * sin + offsetX, outerRadius * cos + offsetZ);
            vertices[indexInner - i] = new(innerRadius * sin + offsetX, innerRadius * cos + offsetZ);
        }

        return [.. vertices];
    }

    public override string ToString() => $"DonutSegmentV:{Center},{InnerRadius},{OuterRadius},{CenterDir},{HalfAngle},{Edges}";
}

// Approximates a donut with a customizable number of edges per circle arc
[SkipLocalsInit]
public sealed record class DonutV(WPos Center, float InnerRadius, float OuterRadius, int Edges, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var edges = Edges;
        var angleIncrement = Angle.DoublePI / edges;
        var vertices = new WDir[2 * edges + 2];
        var initialRotation = Rotation.Rad;
        var innerRadius = InnerRadius;
        var outerRadius = OuterRadius;
        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;
        var indexInner = 2 * edges + 1;
        for (var i = 0; i < edges; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement + initialRotation);

            vertices[i] = new(outerRadius * sin + offsetX, outerRadius * cos + offsetZ);
            vertices[indexInner - i] = new(innerRadius * sin + offsetX, innerRadius * cos + offsetZ);
        }
        // ensure closed polygons, copy first vertices of each ring
        vertices[edges] = vertices[0];
        vertices[edges + 1] = vertices[indexInner];
        return [.. vertices];
    }

    public override string ToString() => $"DonutV:{Center},{InnerRadius},{OuterRadius},{Edges}";
}

// Approximates an ellipse with a customizable number of edges
[SkipLocalsInit]
public sealed record class Ellipse(WPos Center, float HalfWidth, float HalfHeight, int Edges, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var angleIncrement = Angle.DoublePI / Edges;
        var (sinRotation, cosRotation) = ((float, float))Math.SinCos(Rotation.Rad);
        var vertices = new List<WDir>(Edges);
        var halfWidth = HalfWidth;
        var halfHeight = HalfHeight;
        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;
        for (var i = 0; i < Edges; ++i)
        {
            var currentAngle = i * angleIncrement;
            var (sin, cos) = ((float, float))Math.SinCos(currentAngle);
            var x = halfWidth * cos;
            var y = halfHeight * sin;
            vertices.Add(new(x * cosRotation - y * sinRotation + offsetX, x * sinRotation + y * cosRotation + offsetZ));
        }
        return vertices;
    }

    public override string ToString() => $"Ellipse:{Center},{HalfWidth},{HalfHeight},{Edges},{Rotation}";
}

// Capsule shape defined by center, halfheight, halfwidth (radius), rotation, and number of edges. in this case the halfheight is the distance from capsule center to semicircle centers,
// the edges are per semicircle
[SkipLocalsInit]
public sealed record class Capsule(WPos Center, float HalfHeight, float HalfWidth, int Edges, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var vertices = new WDir[2 * Edges];
        var angleIncrement = MathF.PI / Edges;
        var (sinRot, cosRot) = ((float, float))Math.SinCos(Rotation.Rad);
        var halfWidth = HalfWidth;
        var halfHeight = HalfHeight;
        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;
        for (var i = 0; i < Edges; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement);
            var halfWidthCos = halfWidth * cos;
            var halfWidthSin = halfWidth * sin + halfHeight;
            var rxTop = halfWidthCos * cosRot - halfWidthSin * sinRot + offsetX;
            var ryTop = halfWidthCos * sinRot + halfWidthSin * cosRot + offsetZ;
            vertices[i] = new(rxTop, ryTop);
            var rxBot = -rxTop;
            var ryBot = -ryTop;
            vertices[Edges + i] = new(rxBot, ryBot);
        }
        return [.. vertices];
    }

    public override string ToString() => $"Capsule:{Center},{HalfHeight},{HalfWidth},{Rotation},{Edges}";
}
