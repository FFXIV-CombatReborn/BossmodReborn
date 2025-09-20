namespace BossMod;

// 2d vector that represents world-space direction on XZ plane
[SkipLocalsInit]
public readonly struct WDir(float x, float z)
{
    public readonly float X = x;
    public readonly float Z = z;
    public WDir(Vector2 v) : this(v.X, v.Y) { }
    public readonly Vector2 ToVec2() => new(X, Z);
    public readonly Vector3 ToVec3(float y = default) => new(X, y, Z);
    public readonly Vector4 ToVec4(float y = default, float w = default) => new(X, y, Z, w);
    public readonly WPos ToWPos() => new(X, Z);

    public static bool operator ==(WDir left, WDir right) => left.X == right.X && left.Z == right.Z;
    public static bool operator !=(WDir left, WDir right) => left.X != right.X || left.Z != right.Z;
    public static WDir operator +(WDir a, WDir b) => new(a.X + b.X, a.Z + b.Z);
    public static WDir operator -(WDir a, WDir b) => new(a.X - b.X, a.Z - b.Z);
    public static WDir operator -(WDir a) => new(-a.X, -a.Z);
    public static WDir operator -(WDir a, WPos b) => new(a.X - b.X, a.Z - b.Z);
    public static WDir operator *(WDir a, float b) => new(a.X * b, a.Z * b);
    public static WDir operator *(float a, WDir b) => new(a * b.X, a * b.Z);
    public static WDir operator /(WDir a, float b)
    {
        var invB = 1f / b;
        return new(a.X * invB, a.Z * invB);
    }

    public readonly WDir Abs() => new(Math.Abs(X), Math.Abs(Z));
    public readonly WDir Sign() => new(Math.Sign(X), Math.Sign(Z));
    public readonly WDir OrthoL() => new(Z, -X); // CCW, same length
    public readonly WDir OrthoR() => new(-Z, X); // CW, same length
    public readonly WDir MirrorX() => new(-X, Z);
    public readonly WDir MirrorZ() => new(X, -Z);
    public readonly float Dot(WDir a) => X * a.X + Z * a.Z;
    public readonly float Cross(WDir b) => X * b.Z - Z * b.X;
    public readonly WDir Rotate(WDir dir)
    {
        var dirZ = dir.Z;
        var dirX = dir.X;
        return new(X * dirZ + Z * dirX, Z * dirZ - X * dirX);
    }
    public readonly WDir Rotate(Angle dir) => Rotate(dir.ToDirection());
    public readonly float LengthSq() => X * X + Z * Z;
    public readonly float Length() => MathF.Sqrt(LengthSq());
    public readonly WDir Normalized()
    {
        var length = MathF.Sqrt(X * X + Z * Z);
        return length > 0f ? this / length : default;
    }
    public readonly bool AlmostEqual(WDir b, float eps) => Math.Abs(X - b.X) <= eps && Math.Abs(Z - b.Z) <= eps;
    public readonly WDir Scaled(float multiplier) => new(X * multiplier, Z * multiplier);
    public readonly WDir Rounded() => new(MathF.Round(X), MathF.Round(Z));
    public readonly WDir Rounded(float precision) => Scaled(1f / precision).Rounded().Scaled(precision);
    public readonly WDir Floor() => new(MathF.Floor(X), MathF.Floor(Z));
    public readonly Angle ToAngle() => new(MathF.Atan2(X, Z));

    public override readonly string ToString() => $"({X:f3}, {Z:f3})";
    public readonly bool Equals(WDir other) => this == other;
    public override readonly bool Equals(object? obj) => obj is WDir other && Equals(other);
    public override readonly int GetHashCode() => (X, Z).GetHashCode(); // TODO: this is a hack, the default should be good enough, but for whatever reason (X, -Z).GetHashCode() == (-X, Z).GetHashCode()...

    // area checks, assuming this is an offset from shape's center
    public readonly bool InRect(WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var dotDir = Dot(direction);
        var dotNormal = Dot(direction.OrthoL());
        return dotDir >= -lenBack && dotDir <= lenFront && Math.Abs(dotNormal) <= halfWidth;
    }

    public readonly bool InCross(WDir direction, float length, float halfWidth)
    {
        var dotDir = Dot(direction);
        var absDotNormal = Math.Abs(Dot(direction.OrthoL()));
        var inVerticalArm = dotDir >= -length && dotDir <= length && absDotNormal <= halfWidth;
        var inHorizontalArm = dotDir >= -halfWidth && dotDir <= halfWidth && absDotNormal <= length;
        return inVerticalArm || inHorizontalArm;
    }
}

// 2d vector that represents world-space position on XZ plane
[SkipLocalsInit]
public readonly struct WPos(float x, float z)
{
    public readonly float X = x;
    public readonly float Z = z;
    public WPos(Vector2 v) : this(v.X, v.Y) { }
    public readonly Vector2 ToVec2() => new(X, Z);
    public readonly Vector3 ToVec3(float y = 0) => new(X, y, Z);
    public readonly Vector4 ToVec4(float y = 0, float w = 0) => new(X, y, Z, w);
    public readonly WDir ToWDir() => new(X, Z);

    public static bool operator ==(WPos left, WPos right) => left.X == right.X && left.Z == right.Z;
    public static bool operator !=(WPos left, WPos right) => left.X != right.X || left.Z != right.Z;
    public static WPos operator *(WPos a, float b) => new(a.X * b, a.Z * b);
    public static WPos operator +(WPos a, float b) => new(a.X + b, a.Z + b);
    public static WPos operator /(WPos a, int b)
    {
        var invB = 1f / b;
        return new(a.X * invB, a.Z * invB);
    }

    public static WPos operator /(WPos a, float b)
    {
        var invB = 1f / b;
        return new(a.X * invB, a.Z * invB);
    }
    public static WPos operator +(WPos a, WDir b) => new(a.X + b.X, a.Z + b.Z);
    public static WPos operator +(WDir a, WPos b) => new(a.X + b.X, a.Z + b.Z);
    public static WPos operator -(WPos a, WDir b) => new(a.X - b.X, a.Z - b.Z);
    public static WDir operator -(WPos a, WPos b) => new(a.X - b.X, a.Z - b.Z);

    public readonly bool AlmostEqual(WPos b, float eps) => Math.Abs(X - b.X) <= eps && Math.Abs(Z - b.Z) <= eps;
    public readonly WPos Scaled(float multiplier) => new(X * multiplier, Z * multiplier);
    public readonly WPos Rounded() => new(MathF.Round(X), MathF.Round(Z));
    public readonly WPos Rounded(float precision) => Scaled(1f / precision).Rounded().Scaled(precision);
    public static WPos Lerp(WPos from, WPos to, float progress) => new(from.ToVec2() * (1f - progress) + to.ToVec2() * progress);

    public readonly WPos Quantized()  // AOEs are getting clamped to the center of a grid cell, if spell.LocXZ can't be used, you can correct the position with this method
    {
        const float gridSize = 2000f / 65535f;
        const float gridSizeInv = 1f / gridSize;
        return new(((int)MathF.Round(X * gridSizeInv) - 0.5f) * gridSize, ((int)MathF.Round(Z * gridSizeInv) - 0.5f) * gridSize);
    }

    public static WPos RotateAroundOrigin(float rotateByDegrees, WPos origin, WPos point)
    {
        var (sin, cos) = ((float, float))Math.SinCos(rotateByDegrees * Angle.DegToRad);
        var deltaX = point.X - origin.X;
        var deltaZ = point.Z - origin.Z;
        var rotatedX = cos * deltaX - sin * deltaZ;
        var rotatedZ = sin * deltaX + cos * deltaZ;
        return new(origin.X + rotatedX, origin.Z + rotatedZ);
    }

    public static WPos[] GenerateRotatedVertices(WPos center, WPos[] vertices, float rotationAngle)
    {
        var len = vertices.Length;
        var rotatedVertices = new WPos[len];
        for (var i = 0; i < len; ++i)
            rotatedVertices[i] = RotateAroundOrigin(rotationAngle, center, vertices[i]);
        return rotatedVertices;
    }

    public override readonly string ToString() => $"[{X:f3}, {Z:f3}]";
    public readonly bool Equals(WPos other) => this == other;
    public override readonly bool Equals(object? obj) => obj is WPos other && Equals(other);
    public override readonly int GetHashCode() => (X, Z).GetHashCode(); // TODO: this is a hack, the default should be good enough, but for whatever reason (X, -Z).GetHashCode() == (-X, Z).GetHashCode()...

    // area checks
    public readonly bool InTri(WPos v1, WPos v2, WPos v3)
    {
        var s = (v2.X - v1.X) * (Z - v1.Z) - (v2.Z - v1.Z) * (X - v1.X);
        var t = (v3.X - v2.X) * (Z - v2.Z) - (v3.Z - v2.Z) * (X - v2.X);
        if ((s < 0f) != (t < 0f) && s != 0f && t != 0f)
            return false;
        var d = (v1.X - v3.X) * (Z - v3.Z) - (v1.Z - v3.Z) * (X - v3.X);
        return d == 0f || (d < 0f) == (s + t <= 0f);
    }

    public readonly bool InRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth) => (this - origin).InRect(direction, lenFront, lenBack, halfWidth);
    public readonly bool InRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => (this - origin).InRect(direction.ToDirection(), lenFront, lenBack, halfWidth);
    public readonly bool InRect(WPos origin, WDir startToEnd, float halfWidth)
    {
        var len = startToEnd.Length();
        return InRect(origin, startToEnd / len, len, default, halfWidth);
    }
    public readonly bool InRect(WPos origin, WPos end, float halfWidth)
    {
        return InRect(origin, end - origin, halfWidth);
    }

    public readonly bool InSquare(WPos origin, float halfWidth, Angle rotation) => (this - origin).InRect(rotation.ToDirection(), halfWidth, halfWidth, halfWidth);
    public readonly bool InSquare(WPos origin, float halfWidth, WDir rotation) => (this - origin).InRect(rotation, halfWidth, halfWidth, halfWidth);

    // for AABB squares and rects
    public readonly bool InSquare(WPos origin, float halfWidth) => Math.Abs(X - origin.X) <= halfWidth && Math.Abs(Z - origin.Z) <= halfWidth;
    public readonly bool InRect(WPos origin, float halfWidth, float halfHeight) => Math.Abs(X - origin.X) <= halfWidth && Math.Abs(Z - origin.Z) <= halfHeight;

    public readonly bool InCross(WPos origin, Angle direction, float length, float halfWidth) => (this - origin).InCross(direction.ToDirection(), length, halfWidth);
    public readonly bool InCross(WPos origin, WDir direction, float length, float halfWidth) => (this - origin).InCross(direction, length, halfWidth);

    public readonly bool InCircle(WPos origin, float radius) => (this - origin).LengthSq() <= radius * radius;
    public readonly bool InDonut(WPos origin, float innerRadius, float outerRadius) => InCircle(origin, outerRadius) && !InCircle(origin, innerRadius);

    public readonly bool InCone(WPos origin, WDir direction, Angle halfAngle) => (this - origin).Normalized().Dot(direction) >= halfAngle.Cos();
    public readonly bool InCone(WPos origin, Angle direction, Angle halfAngle) => InCone(origin, direction.ToDirection(), halfAngle);

    public readonly bool InCircleCone(WPos origin, float radius, WDir direction, Angle halfAngle) => InCircle(origin, radius) && InCone(origin, direction, halfAngle);
    public readonly bool InCircleCone(WPos origin, float radius, Angle direction, Angle halfAngle) => InCircle(origin, radius) && InCone(origin, direction, halfAngle);

    public readonly bool InDonutCone(WPos origin, float innerRadius, float outerRadius, WDir direction, Angle halfAngle) => InDonut(origin, innerRadius, outerRadius) && InCone(origin, direction, halfAngle);
    public readonly bool InDonutCone(WPos origin, float innerRadius, float outerRadius, Angle direction, Angle halfAngle) => InDonut(origin, innerRadius, outerRadius) && InCone(origin, direction, halfAngle);

    public readonly bool InCapsule(WPos origin, WDir direction, float radius, float length)
    {
        var offset = this - origin;
        var t = Math.Clamp(offset.Dot(direction), 0f, length);
        var proj = origin + t * direction;
        return (this - proj).LengthSq() <= radius * radius;
    }
}
