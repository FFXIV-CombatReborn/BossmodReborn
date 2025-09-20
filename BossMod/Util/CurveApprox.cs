namespace BossMod;

// a bunch of utilities for approximating curves with line segments
// we need them, since clipping and rendering works with polygons
[SkipLocalsInit]
public static class CurveApprox
{
    public const float ScreenError = 0.05f;
    private static readonly Angle a90 = 90f.Degrees(), a270 = 270f.Degrees(), a360 = 360f.Degrees();

    public static int CalculateCircleSegments(float radius, Angle angularLength, float maxError)
    {
        // select max angle such that tesselation error is smaller than desired
        // error = R * (1 - cos(phi/2)) => cos(phi/2) = 1 - error/R
        var tessAngle = 2 * MathF.Acos(1 - Math.Min(maxError / radius, 1));
        var tessNumSegments = (int)MathF.Ceiling(angularLength.Rad / tessAngle);
        tessNumSegments = (tessNumSegments + 1) & ~1;
        return Math.Clamp(tessNumSegments, 4, 512);
    }

    // return polygon points approximating full circle; implicitly closed path - last point is not included
    // winding: points are in CCW order
    public static WDir[] Circle(float Radius, float maxError)
    {
        var radius = Radius;
        var numSegments = CalculateCircleSegments(radius, a360, maxError);
        var angleIncrement = (Angle.DoublePI / numSegments).Radians();
        var points = new WDir[numSegments];
        for (var i = 0; i < numSegments; ++i) // note: do not include last point
        {
            points[i] = PolarToCartesian(radius, i * angleIncrement);
        }
        return points;
    }

    public static WDir[] CircleArc(float Radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var length = angleEnd - angleStart;
        var radius = Radius;
        var numSegments = CalculateCircleSegments(radius, length.Abs(), maxError);
        var angleIncrement = length / numSegments;
        var points = new WDir[numSegments + 1];
        for (var i = 0; i <= numSegments; ++i)
        {
            var angle = angleStart + i * angleIncrement;
            points[i] = PolarToCartesian(radius, angle);
        }
        return points;
    }

    // return polygon points approximating circle sector; implicitly closed path - center + arc
    public static WDir[] CircleSector(float radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var arcPoints = CircleArc(radius, angleStart, angleEnd, maxError);
        var length = arcPoints.Length;
        var points = new WDir[length + 1];
        points[0] = default;
        Array.Copy(arcPoints, 0, points, 1, length);
        return points;
    }

    // return polygon points approximating full donut; implicitly closed path - outer arc + inner arc
    public static WDir[] Donut(float innerRadius, float outerRadius, float maxError)
    {
        var outerCircle = Circle(outerRadius, maxError);
        var innerCircle = Circle(innerRadius, maxError);
        var outerLength = outerCircle.Length;
        var innerLength = innerCircle.Length;
        var points = new WDir[outerLength + innerLength + 2];

        for (var i = 0; i < outerLength; ++i)
        {
            points[i] = outerCircle[i];
        }

        points[outerLength] = PolarToCartesian(outerRadius, default);
        points[outerLength + 1] = PolarToCartesian(innerRadius, default);
        var index = outerLength + 2;
        var innerAdj = innerLength - 1;
        for (var i = innerAdj; i >= 0; --i)
        {
            points[index++] = innerCircle[i];
        }

        return points;
    }

    // return polygon points approximating donut sector; implicitly closed path - outer arc + inner arc
    public static WDir[] DonutSector(float innerRadius, float outerRadius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var outerArc = CircleArc(outerRadius, angleStart, angleEnd, maxError);
        var innerArc = CircleArc(innerRadius, angleEnd, angleStart, maxError);
        var outerLength = outerArc.Length;
        var innerLength = innerArc.Length;
        var totalPoints = outerLength + innerLength;
        var points = new WDir[totalPoints];

        for (var i = 0; i < outerLength; ++i)
        {
            points[i] = outerArc[i];
        }

        for (var i = 0; i < innerLength; ++i)
        {
            points[outerLength + i] = innerArc[i];
        }

        return points;
    }

    // return polygon points for rectangle - it's not really a curve, but whatever...
    public static WDir[] Rect(WDir dx, WDir dz)
    {
        return
        [
            dx - dz,
            dx + dz,
            -dx + dz,
            -dx - dz
        ];
    }

    public static WDir[] Rect(WDir dirZ, float halfWidth, float halfHeight)
    {
        var dx = dirZ.OrthoL() * halfWidth;
        var dz = dirZ * halfHeight;
        return Rect(dx, dz);
    }

    // for angles, we use standard FF convention: 0 is 'south'/down/(0, -r), and then increases clockwise
    private static WDir PolarToCartesian(float r, Angle phi) => r * phi.ToDirection();

    public static WDir[] Capsule(WDir dir, float length, float radius, float maxError)
    {
        var p0 = default(WDir);
        var p1 = length * dir;
        var dirPerp = dir.OrthoL();
        var angleDir = Angle.FromDirection(dir);
        var angleStartP1 = angleDir - a90;
        var angleEnd = angleDir + a90;
        var angleEndP0 = angleDir + a270;
        var radiusDirPerp = radius * dirPerp;

        var arcP1 = CircleArc(radius, angleStartP1, angleEnd, maxError);
        var arcP0 = CircleArc(radius, angleEnd, angleEndP0, maxError);

        var arcP0Length = arcP0.Length;
        var arcP1Length = arcP1.Length;
        var totalPoints = 4 + arcP0Length + arcP1Length;
        var points = new WDir[totalPoints];
        var index = 0;

        points[index++] = p0 + radiusDirPerp;
        points[index++] = p1 + radiusDirPerp;

        for (var i = 0; i < arcP1Length; ++i)
        {
            points[index++] = p1 + arcP1[i];
        }

        points[index++] = p1 - radiusDirPerp;
        points[index++] = p0 - radiusDirPerp;

        for (var i = 0; i < arcP0Length; ++i)
        {
            points[index++] = p0 + arcP0[i];
        }

        return points;
    }
}
