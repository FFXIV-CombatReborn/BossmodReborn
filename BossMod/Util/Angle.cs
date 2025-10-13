namespace BossMod;

// wrapper around float, stores angle in radians, provides type-safety and convenience
// when describing rotation in world, common convention is 0 for 'south'/'down'/(0, -1) and increasing counterclockwise - so +90 is 'east'/'right'/(1, 0)

[SkipLocalsInit]
public readonly struct Angle(float rad)
{
    public readonly float Rad = rad;
    public const float RadToDeg = 180f / MathF.PI;
    public const float DegToRad = MathF.PI / 180f;
    public const float HalfPi = MathF.PI / 2f;
    public const float DoublePI = MathF.Tau;

    public static readonly Angle[] AnglesIntercardinals = [-45.003f.Degrees(), 44.998f.Degrees(), 134.999f.Degrees(), -135.005f.Degrees()];
    public static readonly Angle[] AnglesCardinals = [-90.004f.Degrees(), -0.003f.Degrees(), 180f.Degrees(), 89.999f.Degrees()];

    public readonly float Deg => Rad * RadToDeg;

    public static Angle FromDirection(WDir dir) => new(MathF.Atan2(dir.X, dir.Z));
    public readonly WDir ToDirection()
    {
        var (sin, cos) = ((float, float))Math.SinCos(Rad);
        return new(sin, cos);
    }

    public static bool operator ==(Angle left, Angle right) => left.Rad == right.Rad;
    public static bool operator !=(Angle left, Angle right) => left.Rad != right.Rad;
    public static Angle operator +(Angle a, Angle b) => new(a.Rad + b.Rad);
    public static Angle operator -(Angle a, Angle b) => new(a.Rad - b.Rad);
    public static Angle operator -(Angle a) => new(-a.Rad);
    public static Angle operator *(Angle a, float b) => new(a.Rad * b);
    public static Angle operator *(float a, Angle b) => new(a * b.Rad);
    public static Angle operator /(Angle a, float b) => new(a.Rad / b);
    public static bool operator >(Angle a, Angle b) => a.Rad > b.Rad;
    public static bool operator <(Angle a, Angle b) => a.Rad < b.Rad;
    public static bool operator >=(Angle a, Angle b) => a.Rad >= b.Rad;
    public static bool operator <=(Angle a, Angle b) => a.Rad <= b.Rad;

    public readonly Angle Abs() => new(Math.Abs(Rad));
    public readonly float Sin() => (float)Math.Sin(Rad);
    public readonly float Cos() => (float)Math.Cos(Rad);
    public readonly float Tan() => (float)Math.Tan(Rad);
    public static Angle Atan2(float y, float x) => new(MathF.Atan2(y, x));
    public static Angle Asin(float x) => new((float)Math.Asin(x));
    public static Angle Acos(float x) => new((float)Math.Acos(x));
    public readonly Angle Round(float roundToNearestDeg) => (MathF.Round(Deg / roundToNearestDeg) * roundToNearestDeg).Degrees();

    public readonly Angle Normalized()
    {
        var r = Rad;
        while (r < -MathF.PI)
            r += DoublePI;
        while (r > MathF.PI)
            r -= DoublePI;
        return new(r);
    }

    public readonly bool AlmostEqual(Angle other, float epsRad) => Math.Abs((this - other).Normalized().Rad) <= epsRad;

    // closest distance to move from this angle to destination (== 0 if equal, >0 if moving in positive/CCW dir, <0 if moving in negative/CW dir)
    public readonly Angle DistanceToAngle(Angle other) => (other - this).Normalized();

    // returns 0 if angle is within range, positive value if min is closest, negative if max is closest
    public readonly Angle DistanceToRange(Angle min, Angle max)
    {
        var width = (max - min) * 0.5f;
        var midDist = DistanceToAngle((min + max) * 0.5f);
        return midDist.Rad > width.Rad ? midDist - width : midDist.Rad < -width.Rad ? midDist + width : default;
    }

    // closest direction in range to this angle
    public readonly Angle ClosestInRange(Angle min, Angle max)
    {
        var width = (max - min) * 0.5f;
        var midDist = DistanceToAngle((min + max) * 0.5f);
        return midDist.Rad > width.Rad ? min : midDist.Rad < -width.Rad ? max : this;
    }

    public override readonly string ToString() => Deg.ToString("f3", System.Globalization.CultureInfo.InvariantCulture);
    public readonly bool Equals(Angle other) => Rad == other.Rad;
    public override readonly bool Equals(object? obj) => obj is Angle other && Equals(other);
    public override readonly int GetHashCode() => Rad.GetHashCode();
}

[SkipLocalsInit]
public static class AngleExtensions
{
    public static Angle Radians(this float radians) => new(radians);
    public static Angle Degrees(this float degrees) => new(degrees * Angle.DegToRad);
    public static Angle Degrees(this int degrees) => new(degrees * Angle.DegToRad);
}

[SkipLocalsInit]
public static class CosPI
{
    public const float Pi8th = 1.082392f; // 1 / Math.Cos(Math.PI / 8)
    public const float Pi16th = 1.019591f; // 1 / Math.Cos(Math.PI / 16)
    public const float Pi20th = 1.012465f; // 1 / Math.Cos(Math.PI / 20)
    public const float Pi28th = 1.006328f; // 1 / Math.Cos(Math.PI / 28)
    public const float Pi32th = 1.004839f; // 1 / Math.Cos(Math.PI / 32)
    public const float Pi36th = 1.00382f; // 1 / Math.Cos(Math.PI / 36)
    public const float Pi40th = 1.0030922f; // 1 / Math.Cos(Math.PI / 40)
    public const float Pi48th = 1.0021457f; // 1 / Math.Cos(Math.PI / 48)
    public const float Pi60th = 1.0013723f; // 1 / Math.Cos(Math.PI / 60)
    public const float Pi64th = 1.001206f; // 1 / Math.Cos(Math.PI / 64)
    public const float Pi148th = 1.000225f; // 1 / Math.Cos(Math.PI / 148)
}
