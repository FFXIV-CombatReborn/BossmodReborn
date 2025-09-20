namespace BossMod;

[SkipLocalsInit]
public sealed class SDHalfPlane : ShapeDistance
{
    private readonly float normalX, normalZ, bias;

    public SDHalfPlane(WPos Point, WDir Normal)
    {
        var normal = Normal;
        normalX = normal.X;
        normalZ = normal.Z;
        var point = Point;
        bias = normalX * point.X + normalZ * point.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p) => normalX * p.X + normalZ * p.Z - bias;

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        var rowstart = rowStart;
        var dx_ = dx;
        // s(p) = n·p − bias. Only need values at endpoints.
        var s0 = normalX * rowstart.X + normalZ * rowstart.Z - bias;
        var delta = width * (normalX * dx_.X + normalZ * dx_.Z);
        var s1 = s0 + delta;
        var min = s0 < s1 ? s0 : s1;
        return min <= cushion + Epsilon; // include tangency
    }
}

[SkipLocalsInit]
public sealed class SDCircle : ShapeDistance
{
    private readonly float originX, originZ, radius;

    public SDCircle(WPos Origin, float Radius)
    {
        var origin = Origin;
        originX = origin.X;
        originZ = origin.Z;
        radius = Radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        return MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ) - radius;
    }

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        // Segment vs disk test using closest-point distance
        var rowstartX = rowStart.X;
        var rowstartZ = rowStart.Z;
        var aX = rowstartX - originX;
        var aZ = rowstartZ - originZ;
        var b = rowStart + width * dx;
        var dX = b.X - rowstartX;
        var dZ = b.Z - rowstartZ;
        var A = dX * dX + dZ * dZ;
        var R = radius + cushion;
        var R2 = R * R;
        if (A <= Epsilon)
        {
            var d2 = aX * aX + aZ * aZ;
            return d2 <= R2 + Epsilon;
        }
        var t = -(aX * dX + aZ * dZ) / A; // projection of center onto segment
        if (t < 0f)
        {
            t = 0f;
        }
        else if (t > 1f)
        {
            t = 1f;
        }
        var cX = aX + t * dX;
        var cZ = aZ + t * dZ;
        var minD2 = cX * cX + cZ * cZ;
        return minD2 <= R2 + Epsilon;
    }
}

[SkipLocalsInit]
public sealed class SDInvertedCircle : ShapeDistance
{
    private readonly float originX, originZ, radius;

    public SDInvertedCircle(WPos Origin, float Radius)
    {
        var origin = Origin;
        originX = origin.X;
        originZ = origin.Z;
        radius = Radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        return radius - MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
    }

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        // Outside of disk: segment intersects unless fully inside the (inflated) disk.
        var aX = rowStart.X - originX;
        var aZ = rowStart.Z - originZ;
        var b = rowStart + width * dx;
        var bX = b.X - originX;
        var bZ = b.Z - originZ;
        var R = radius + cushion;
        var R2 = R * R;
        var aIn = (aX * aX + aZ * aZ) <= R2 + Epsilon;
        var bIn = (bX * bX + bZ * bZ) <= R2 + Epsilon;
        return !(aIn && bIn);
    }
}

[SkipLocalsInit]
public sealed class SDDonut : ShapeDistance
{
    private readonly float originX, originZ, innerRadius, outerRadius;

    public SDDonut(WPos Origin, float InnerRadius, float OuterRadius)
    {
        var origin = Origin;
        originX = origin.X;
        originZ = origin.Z;
        innerRadius = InnerRadius;
        outerRadius = OuterRadius;
    }

    public override float Distance(WPos p)
    {
        // intersection of outer circle and inverted inner circle
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        var distOuter = distOrigin - outerRadius;
        var distInner = innerRadius - distOrigin;
        return distOuter > distInner ? distOuter : distInner;
    }

    public override bool RowIntersectsShape(WPos rowStart, WDir dxVec, float width, float cushion = default)
    {
        // Segment vs annulus [Rin, Rout] with cushion ⇒ [max(0,Rin-c), Rout+c]
        var rowstartX = rowStart.X;
        var rowstartZ = rowStart.Z;
        var aX = rowstartX - originX;
        var aZ = rowstartZ - originZ;
        var b = rowStart + width * dxVec;
        var dX = b.X - rowstartX;
        var dZ = b.Z - rowstartZ;
        var A = dX * dX + dZ * dZ;

        var Rout = outerRadius + cushion;
        var Rout2 = Rout * Rout;
        var Rin = Math.Max(0f, innerRadius - cushion);
        var Rin2 = Rin * Rin;

        // closest-point distance^2 to center
        float minD2;
        if (A <= Epsilon)
        {
            minD2 = aX * aX + aZ * aZ;
        }
        else
        {
            var t = -(aX * dX + aZ * dZ) / A;
            if (t < 0f)
            {
                t = 0f;
            }
            else if (t > 1f)
            {
                t = 1f;
            }
            var cX = aX + t * dX;
            var cZ = aZ + t * dZ;
            minD2 = cX * cX + cZ * cZ;
        }
        var hitsOuter = minD2 <= Rout2 + Epsilon;
        if (!hitsOuter)
        {
            return false;
        }

        // fully inside inner disk? (convex ⇒ endpoints suffice)
        var aInInner = (aX * aX + aZ * aZ) <= Rin2 + Epsilon;
        var bX = b.X - originX;
        var bZ = b.Z - originZ;
        var bInInner = (bX * bX + bZ * bZ) <= Rin2 + Epsilon;
        return !aInInner || !bInInner;
    }
}

[SkipLocalsInit]
public sealed class SDInvertedDonut : ShapeDistance
{
    private readonly float originX, originZ, innerRadius, outerRadius;

    public SDInvertedDonut(WPos Origin, float InnerRadius, float OuterRadius)
    {
        var origin = Origin;
        originX = origin.X;
        originZ = origin.Z;
        innerRadius = InnerRadius;
        outerRadius = OuterRadius;
    }

    public override float Distance(WPos p)
    {
        // intersection of outer circle and inverted inner circle
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        var distOuter = distOrigin - outerRadius;
        var distInner = innerRadius - distOrigin;
        return distOuter > distInner ? -distOuter : -distInner;
    }

    public override bool RowIntersectsShape(WPos rowStart, WDir dxVec, float width, float cushion = default)
    {
        // Inverted annulus = outside outer disk ∪ inside inner disk.
        // Segment hits inverted region unless it is fully inside the inflated annulus.
        var rowstartX = rowStart.X;
        var rowstartZ = rowStart.Z;
        var aX = rowstartX - originX;
        var aZ = rowstartZ - originZ;
        var b = rowStart + width * dxVec;
        var bX_ = b.X;
        var bZ_ = b.Z;
        var bX = bX_ - originX;
        var bZ = bZ_ - originZ;
        var dX = bX_ - rowstartX;
        var dZ = bZ_ - rowstartZ;
        var A = dX * dX + dZ * dZ;

        var Rout = outerRadius + cushion;
        var Rout2 = Rout * Rout;
        var Rin = Math.Max(0f, innerRadius - cushion);
        var Rin2 = Rin * Rin;

        // Fully inside outer disk? (convex ⇒ endpoints-inside suffice)
        var aInOuter = (aX * aX + aZ * aZ) <= Rout2 + Epsilon;
        var bInOuter = (bX * bX + bZ * bZ) <= Rout2 + Epsilon;
        if (!(aInOuter && bInOuter))
        {
            return true; // touches inverted outside
        }

        // Intersects inner disk? If yes, not fully inside annulus ⇒ intersects inverted
        float minD2;
        if (A <= Epsilon)
        {
            minD2 = aX * aX + aZ * aZ;
        }
        else
        {
            var t = -(aX * dX + aZ * dZ) / A;
            if (t < 0f)
            {
                t = 0f;
            }
            else if (t > 1f)
            {
                t = 1f;
            }
            var cX = aX + t * dX;
            var cZ = aZ + t * dZ;
            minD2 = cX * cX + cZ * cZ;
        }
        var hitsInner = minD2 <= Rin2 + Epsilon; // include tangency
        if (hitsInner)
        {
            return true;
        }

        // fully inside annulus
        return false;
    }
}

[SkipLocalsInit]
public sealed class SDCone : ShapeDistance
{
    private readonly float originX, originZ, coneFactor, radius, nlX, nlZ, nrX, nrZ;

    public SDCone(WPos Origin, float Radius, Angle CenterDir, Angle HalfAngle)
    {
        originX = Origin.X;
        originZ = Origin.Z;
        radius = Radius;
        var halfAngle = HalfAngle;
        coneFactor = halfAngle.Rad > Angle.HalfPi ? -1f : 1f;
        var centerDir = CenterDir;
        var nl = coneFactor * (centerDir + halfAngle).ToDirection().OrthoL();
        var nr = coneFactor * (centerDir - halfAngle).ToDirection().OrthoR();
        nlX = nl.X;
        nlZ = nl.Z;
        nrX = nr.X;
        nrZ = nr.Z;
    }

    // for <= 180-degree cone: result = intersection of circle and two half-planes with normals pointing outside cone sides
    // for > 180-degree cone: result = intersection of circle and negated intersection of two half-planes with inverted normals
    // both normals point outside
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        var distOuter = distOrigin - radius;
        var distLeft = pXoriginX * nlX + pZoriginZ * nlZ;
        var distRight = pXoriginX * nrX + pZoriginZ * nrZ;

        var maxSideDist = distLeft > distRight ? distLeft : distRight;
        var conef = coneFactor * maxSideDist;
        return distOuter > conef ? distOuter : conef;
    }

    // rotated grid row: p(s) = rowStart + s*dx, s∈[s0,s1]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        return SegmentIntersectsCone(rowStart, rowStart + width * dx, cushion);
    }

    public bool SegmentIntersectsCone(WPos a, WPos b, float cushion = default)
    {
        // Shift to cone origin
        var a_ = a;
        var aX_ = a_.X;
        var aZ_ = a_.Z;
        var b_ = b;
        var ax = aX_ - originX;
        var az = aZ_ - originZ;
        var dx = b_.X - aX_;
        var dz = b_.Z - aZ_;

        // 1) Intersect with disk of radius (r+c)
        var r = radius + cushion;
        var A = dx * dx + dz * dz;
        if (A <= Epsilon)
        {
            // Point test (degenerate segment)
            return Distance(a) <= cushion + Epsilon;
        }
        var B = 2f * (ax * dx + az * dz);
        var C = ax * ax + az * az - r * r;
        var disc = B * B - 4f * A * C;
        if (disc < -Epsilon)
        {
            return false;
        }
        var sqrtD = MathF.Sqrt(disc < 0f ? 0f : disc);
        var inv2A = 0.5f / A;
        var t1 = (-B - sqrtD) * inv2A;
        var t2 = (-B + sqrtD) * inv2A;
        if (t1 > t2)
        {
            (t2, t1) = (t1, t2);
        }
        var tmin = Math.Max(0f, t1);
        var tmax = Math.Min(1f, t2);
        if (tmax <= tmin + Epsilon)
        {
            return false; // misses disk entirely
        }

        // 2) Angular constraint(s)
        if (coneFactor > 0f)
        {
            // <= 180° : AND of two half-planes n·((a-o) + t d) <= cushion
            return ClipHalfplaneLE(nlX, nlZ, ax, az, dx, dz, cushion, ref tmin, ref tmax)
            && ClipHalfplaneLE(nrX, nrZ, ax, az, dx, dz, cushion, ref tmin, ref tmax)
            && tmax > tmin + Epsilon;
        }
        else
        {
            // > 180° : inside if (HL >= -c) OR (HR >= -c); compute each interval on [0,1] and OR them
            float l0 = 0f, l1 = 1f;
            var lOk = ClipHalfplaneGE(nlX, nlZ, ax, az, dx, dz, -cushion, ref l0, ref l1);
            float r0 = 0f, r1 = 1f;
            var rOk = ClipHalfplaneGE(nrX, nrZ, ax, az, dx, dz, -cushion, ref r0, ref r1);
            if (!lOk && !rOk)
            {
                return false;
            }
            // Intersect each with disk window and test overlap
            return lOk && Math.Min(l1, tmax) > Math.Max(l0, tmin) + Epsilon || rOk && Math.Min(r1, tmax) > Math.Max(r0, tmin) + Epsilon;
        }
    }

    // Solve n·((a-o) + t d) <= c for t in [tmin,tmax]
    private static bool ClipHalfplaneLE(float nX, float nZ, float ax, float az, float dx, float dz, float c, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az; // value at t=0
        var s1 = nX * dx + nZ * dz; // derivative along segment
        if (NearlyZero(s1))
        {
            return s0 <= c + Epsilon; // all or nothing
        }
        var bound = (c - s0) / s1; // t at which equality holds
        if (s1 > 0f)
        {
            tmax = Math.Min(tmax, bound);
        }
        else
        {
            tmin = Math.Max(tmin, bound);
        }
        return tmax > tmin + Epsilon;
    }

    // Solve n·((a-o) + t d) >= v  for t
    private static bool ClipHalfplaneGE(float nX, float nZ, float ax, float az, float dx, float dz, float v, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (NearlyZero(s1))
        {
            if (s0 >= v - Epsilon)
            {
                return true; // whole [0,1]
            }
            tmin = 1f;
            tmax = 0f;
            return false; // empty
        }
        var bound = (v - s0) / s1;
        if (s1 > 0f)
        {
            tmin = Math.Max(tmin, bound);
        }
        else
        {
            tmax = Math.Min(tmax, bound);
        }
        return tmax > tmin + Epsilon;
    }
}

[SkipLocalsInit]
public sealed class SDInvertedCone : ShapeDistance
{
    private readonly float originX, originZ, coneFactor, radius, nlX, nlZ, nrX, nrZ;

    public SDInvertedCone(WPos Origin, float Radius, Angle CenterDir, Angle HalfAngle)
    {
        originX = Origin.X;
        originZ = Origin.Z;
        radius = Radius;
        var halfAngle = HalfAngle;
        coneFactor = halfAngle.Rad > Angle.HalfPi ? -1f : 1f;
        var centerDir = CenterDir;
        var nl = coneFactor * (centerDir + halfAngle).ToDirection().OrthoL();
        var nr = coneFactor * (centerDir - halfAngle).ToDirection().OrthoR();
        nlX = nl.X;
        nlZ = nl.Z;
        nrX = nr.X;
        nrZ = nr.Z;
    }

    // for <= 180-degree cone: result = intersection of circle and two half-planes with normals pointing outside cone sides
    // for > 180-degree cone: result = intersection of circle and negated intersection of two half-planes with inverted normals
    // both normals point outside
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        var distOuter = distOrigin - radius;
        var distLeft = pXoriginX * nlX + pZoriginZ * nlZ;
        var distRight = pXoriginX * nrX + pZoriginZ * nrZ;

        var maxSideDist = distLeft > distRight ? distLeft : distRight;
        var conef = coneFactor * maxSideDist;
        return distOuter > conef ? -distOuter : -conef;
    }

    // inverted cones very rarely do not intersect a row, so we always return true
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDDonutSector : ShapeDistance
{
    private readonly float originX, originZ, coneFactor, innerRadius, outerRadius, nlX, nlZ, nrX, nrZ;

    public SDDonutSector(WPos Origin, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle)
    {
        originX = Origin.X;
        originZ = Origin.Z;
        innerRadius = InnerRadius;
        outerRadius = OuterRadius;
        var halfAngle = HalfAngle;
        coneFactor = halfAngle.Rad > Angle.HalfPi ? -1f : 1f;
        var centerDir = CenterDir;
        var a90 = 90f.Degrees();
        var nl = coneFactor * (centerDir + halfAngle + a90).ToDirection();
        var nr = coneFactor * (centerDir - halfAngle - a90).ToDirection();
        nlX = nl.X;
        nlZ = nl.Z;
        nrX = nr.X;
        nrZ = nr.Z;
    }

    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        var distOuter = distOrigin - outerRadius;
        var distInner = innerRadius - distOrigin;
        var distLeft = pXoriginX * nlX + pZoriginZ * nlZ;
        var distRight = pXoriginX * nrX + pZoriginZ * nrZ;

        var maxRadial = distOuter > distInner ? distOuter : distInner;
        var maxCone = distLeft > distRight ? distLeft : distRight;
        var conef = coneFactor * maxCone;
        return maxRadial > conef ? maxRadial : conef;
    }

    // Row cull with exact annulus∩wedge clipping on the segment
    public override bool RowIntersectsShape(WPos rowStart, WDir dxVec, float width, float cushion = default)
    {
        var rowstart = rowStart;
        var rowstartX = rowstart.X;
        var rowstartZ = rowstart.Z;
        var ax = rowstartX - originX;
        var az = rowstartZ - originZ;
        var b = rowstart + width * dxVec;
        var dx = b.X - rowstartX;
        var dz = b.Z - rowstartZ;
        var A = dx * dx + dz * dz;
        if (A <= Epsilon)
        {
            // degenerate: point test
            return Distance(rowStart) <= cushion + Epsilon;
        }

        // radial annulus window(s)
        var Rout = outerRadius + cushion;
        var Rout2 = Rout * Rout;
        var Rin = Math.Max(0f, innerRadius - cushion);
        var Rin2 = Rin * Rin;
        var B = 2f * (ax * dx + az * dz);
        var Couter = ax * ax + az * az - Rout2;
        var DiscOuter = B * B - 4f * A * Couter;
        if (DiscOuter < -Epsilon)
        {
            return false; // misses outer disk entirely
        }
        var sqrtOut = MathF.Sqrt(DiscOuter < 0f ? 0f : DiscOuter);
        var inv2A = 0.5f / A;
        var tOut1 = (-B - sqrtOut) * inv2A;
        var tOut2 = (-B + sqrtOut) * inv2A;
        if (tOut1 > tOut2)
        {
            (tOut2, tOut1) = (tOut1, tOut2);
        }
        var u0 = Math.Max(0f, tOut1);
        var u1 = Math.Min(1f, tOut2);
        if (u1 <= u0 + Epsilon)
        {
            return false;
        }

        // inner disk interval (forbidden) to subtract
        var Cinner = ax * ax + az * az - Rin2;
        var DiscInner = B * B - 4f * A * Cinner;
        var hasInner = DiscInner >= -Epsilon;
        float v0 = 0f, v1 = 0f;
        if (hasInner)
        {
            var sqrtIn = MathF.Sqrt(DiscInner < 0f ? 0f : DiscInner);
            var tIn1 = (-B - sqrtIn) * inv2A;
            var tIn2 = (-B + sqrtIn) * inv2A;
            if (tIn1 > tIn2)
            {
                (tIn2, tIn1) = (tIn1, tIn2);
            }
            v0 = Math.Max(0f, tIn1);
            v1 = Math.Min(1f, tIn2);
            if (v1 <= v0 + Epsilon)
            {
                hasInner = false; // tangent/empty
            }
        }

        // Subtract [v0,v1] from [u0,u1] to get up to two annulus intervals
        Span<(float a, float b)> annulus = stackalloc (float a, float b)[2];
        var annN = 0;
        void Add(float a, float b, Span<(float, float)> annulus)
        {
            if (b > a + Epsilon)
                annulus[annN++] = (a, b);
        }

        if (!hasInner)
        {
            Add(u0, u1, annulus);
        }
        else
        {
            var a0 = u0;
            var a1 = u1;
            var b0 = v0;
            var b1 = v1;
            if (b1 <= a0 || b0 >= a1)
            {
                Add(a0, a1, annulus);
            }
            else if (b0 <= a0 && b1 >= a1)
            {
                // fully removed
            }
            else if (b0 <= a0)
            {
                Add(b1, a1, annulus);
            }
            else if (b1 >= a1)
            {
                Add(a0, b0, annulus);
            }
            else
            {
                Add(a0, b0, annulus);
                Add(b1, a1, annulus);
            }
        }
        if (annN == 0)
        {
            return false;
        }

        // --- wedge constraints ---
        if (coneFactor > 0f)
        {
            // <= 180° : intersection of two half-planes n·((a-o) + t d) <= cushion
            for (var i = 0; i < annN; ++i)
            {
                float t0 = annulus[i].a, t1 = annulus[i].b;
                if (!ClipHalfplaneLE(nlX, nlZ, ax, az, dx, dz, cushion, ref t0, ref t1))
                {
                    continue;
                }
                if (!ClipHalfplaneLE(nrX, nrZ, ax, az, dx, dz, cushion, ref t0, ref t1))
                {
                    continue;
                }
                if (t1 > t0 + Epsilon)
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            // > 180° : union of two half-planes n·((a-o) + t d) >= -cushion
            float l0 = 0f, l1 = 1f;
            var lOk = ClipHalfplaneGE(nlX, nlZ, ax, az, dx, dz, -cushion, ref l0, ref l1);
            float r0 = 0f, r1 = 1f;
            var rOk = ClipHalfplaneGE(nrX, nrZ, ax, az, dx, dz, -cushion, ref r0, ref r1);
            if (!lOk && !rOk)
            {
                return false;
            }
            for (var i = 0; i < annN; ++i)
            {
                var a0 = annulus[i].a;
                var a1 = annulus[i].b;
                if (lOk && Math.Min(l1, a1) > Math.Max(l0, a0) + Epsilon)
                {
                    return true;
                }
                if (rOk && Math.Min(r1, a1) > Math.Max(r0, a0) + Epsilon)
                {
                    return true;
                }
            }
            return false;
        }
    }

    private static bool ClipHalfplaneLE(float nX, float nZ, float ax, float az, float dx, float dz, float c, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (Math.Abs(s1) <= Epsilon)
        {
            return s0 <= c + Epsilon;
        }
        var bound = (c - s0) / s1;
        if (s1 > 0f)
        {
            tmax = Math.Min(tmax, bound);
        }
        else
        {
            tmin = Math.Max(tmin, bound);
        }
        return tmax > tmin + Epsilon;
    }

    private static bool ClipHalfplaneGE(float nX, float nZ, float ax, float az, float dx, float dz, float v, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (Math.Abs(s1) <= Epsilon)
        {
            return s0 >= v - Epsilon;
        }
        var bound = (v - s0) / s1;
        if (s1 > 0f)
        {
            tmin = Math.Max(tmin, bound);
        }
        else
        {
            tmax = Math.Min(tmax, bound);
        }
        return tmax > tmin + Epsilon;
    }
}

[SkipLocalsInit]
public sealed class SDInvertedDonutSector : ShapeDistance
{
    private readonly float originX, originZ, coneFactor, innerRadius, outerRadius, nlX, nlZ, nrX, nrZ;

    public SDInvertedDonutSector(WPos Origin, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle)
    {
        originX = Origin.X;
        originZ = Origin.Z;
        innerRadius = InnerRadius;
        outerRadius = OuterRadius;
        var halfAngle = HalfAngle;
        coneFactor = halfAngle.Rad > Angle.HalfPi ? -1f : 1f;
        var centerDir = CenterDir;
        var a90 = 90f.Degrees();
        var nl = coneFactor * (centerDir + halfAngle + a90).ToDirection();
        var nr = coneFactor * (centerDir - halfAngle - a90).ToDirection();
        nlX = nl.X;
        nlZ = nl.Z;
        nrX = nr.X;
        nrZ = nr.Z;
    }

    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        var distOuter = distOrigin - outerRadius;
        var distInner = innerRadius - distOrigin;
        var distLeft = pXoriginX * nlX + pZoriginZ * nlZ;
        var distRight = pXoriginX * nrX + pZoriginZ * nrZ;

        var maxRadial = distOuter > distInner ? distOuter : distInner;
        var maxCone = distLeft > distRight ? distLeft : distRight;
        var conef = coneFactor * maxCone;
        return maxRadial > conef ? -maxRadial : -conef;
    }

    // inverted donut segments very rarely do not intersect a row, so we always return true
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDTri : ShapeDistance
{
    private readonly float n1X, n1Z, n2X, n2Z, n3X, n3Z, aX, aZ, bX, bZ, cX, cZ;

    public SDTri(WPos Origin, RelTriangle Triangle)
    {
        var tri = Triangle;
        var ab = tri.B - tri.A;
        var bc = tri.C - tri.B;
        var ca = tri.A - tri.C;
        var n1 = ab.OrthoL().Normalized();
        var n2 = bc.OrthoL().Normalized();
        var n3 = ca.OrthoL().Normalized();
        if (ab.Cross(bc) < 0f)
        {
            n1 = -n1;
            n2 = -n2;
            n3 = -n3;
        }
        var origin = Origin;
        var a = origin + tri.A;
        var b = origin + tri.B;
        var c = origin + tri.C;

        n1X = n1.X;
        n1Z = n1.Z;
        n2X = n2.X;
        n2Z = n2.Z;
        n3X = n3.X;
        n3Z = n3.Z;
        aX = a.X;
        aZ = a.Z;
        bX = b.X;
        bZ = b.Z;
        cX = c.X;
        cZ = c.Z;
    }

    public override float Distance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        var d1 = n1X * (pX - aX) + n1Z * (pZ - aZ);
        var d2 = n2X * (pX - bX) + n2Z * (pZ - bZ);
        var d3 = n3X * (pX - cX) + n3Z * (pZ - cZ);
        var max1 = d1 > d2 ? d1 : d2;
        return max1 > d3 ? max1 : d3;
    }

    // Row-cull: clip segment by 3 half-planes (convex triangle inflated by cushion)
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        var rowstart = rowStart;
        var a0x = rowstart.X;
        var a0z = rowstart.Z;
        var b = rowstart + width * dx;
        var dX = b.X - a0x;
        var dZ = b.Z - a0z;
        float tmin = 0f, tmax = 1f;

        return ClipLE(n1X, n1Z, a0x - aX, a0z - aZ, dX, dZ, cushion, ref tmin, ref tmax)
        && ClipLE(n2X, n2Z, a0x - bX, a0z - bZ, dX, dZ, cushion, ref tmin, ref tmax)
        && ClipLE(n3X, n3Z, a0x - cX, a0z - cZ, dX, dZ, cushion, ref tmin, ref tmax)
        && tmax > tmin + Epsilon;
    }

    private static bool ClipLE(float nX, float nZ, float vx, float vz, float dx, float dz, float value, ref float tmin, ref float tmax)
    {
        var s0 = nX * vx + nZ * vz;
        var s1 = nX * dx + nZ * dz;
        if (NearlyZero(s1))
        {
            return s0 <= value + Epsilon; // why: constant along the row
        }
        var bound = (value - s0) / s1;
        if (s1 > 0f)
        {
            tmax = Math.Min(tmax, bound);
        }
        else
        {
            tmin = Math.Max(tmin, bound);
        }
        return tmax > tmin + Epsilon;
    }
}

[SkipLocalsInit]
public sealed class SDInvertedTri : ShapeDistance
{
    private readonly float n1X, n1Z, n2X, n2Z, n3X, n3Z, aX, aZ, bX, bZ, cX, cZ;

    public SDInvertedTri(WPos Origin, RelTriangle Triangle)
    {
        var tri = Triangle;
        var ab = tri.B - tri.A;
        var bc = tri.C - tri.B;
        var ca = tri.A - tri.C;
        var n1 = ab.OrthoL().Normalized();
        var n2 = bc.OrthoL().Normalized();
        var n3 = ca.OrthoL().Normalized();
        if (ab.Cross(bc) < 0f)
        {
            n1 = -n1;
            n2 = -n2;
            n3 = -n3;
        }
        var origin = Origin;
        var a = origin + tri.A;
        var b = origin + tri.B;
        var c = origin + tri.C;

        n1X = n1.X;
        n1Z = n1.Z;
        n2X = n2.X;
        n2Z = n2.Z;
        n3X = n3.X;
        n3Z = n3.Z;
        aX = a.X;
        aZ = a.Z;
        bX = b.X;
        bZ = b.Z;
        cX = c.X;
        cZ = c.Z;
    }

    public override float Distance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        var d1 = n1X * (pX - aX) + n1Z * (pZ - aZ);
        var d2 = n2X * (pX - bX) + n2Z * (pZ - bZ);
        var d3 = n3X * (pX - cX) + n3Z * (pZ - cZ);
        var max1 = d1 > d2 ? d1 : d2;
        return max1 > d3 ? -max1 : -d3;
    }

    // inverted triangles very rarely do not intersect a row, so we always return true
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDRect : ShapeDistance
{
    private readonly float originX, originZ, dirX, dirZ, normalX, normalZ, lenFront, lenBack, halfWidth;

    public SDRect(WPos Origin, Angle Direction, float LenFront, float LenBack, float HalfWidth) : this(Origin, Direction.ToDirection(), LenFront, LenBack, HalfWidth) { }

    public SDRect(WPos From, WPos To, float HalfWidth)
    {
        var from = From;
        var dir = To - from;
        var l = dir.Length();
        var normalizedDir = dir / l;
        var normal = normalizedDir.OrthoL();

        originX = from.X;
        originZ = from.Z;
        dirX = normalizedDir.X;
        dirZ = normalizedDir.Z;
        normalX = normal.X;
        normalZ = normal.Z;
        lenFront = l;
        lenBack = default;
        halfWidth = HalfWidth;
    }

    public SDRect(WPos Origin, WDir Dir, float LenFront, float LenBack, float HalfWidth)
    {
        // dir points outside far side
        var dir = Dir;
        var normal = dir.OrthoL(); // points outside left side
        var origin = Origin;
        originX = origin.X;
        originZ = origin.Z;
        dirX = dir.X;
        dirZ = dir.Z;
        normalX = normal.X;
        normalZ = normal.Z;
        lenFront = LenFront;
        lenBack = LenBack;
        halfWidth = HalfWidth;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distParr = pXoriginX * dirX + pZoriginZ * dirZ;
        var distOrtho = pXoriginX * normalX + pZoriginZ * normalZ;
        var distFront = distParr - lenFront;
        var distBack = -distParr - lenBack;
        var distLeft = distOrtho - halfWidth;
        var distRight = -distOrtho - halfWidth;

        var maxParr = distFront > distBack ? distFront : distBack;
        var maxOrtho = distLeft > distRight ? distLeft : distRight;

        return maxParr > maxOrtho ? maxParr : maxOrtho;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        return SegmentIntersectsRect(rowStart, rowStart + width * dx, cushion);
    }

    private bool SegmentIntersectsRect(WPos a, WPos b, float cushion)
    {
        // Translate to rectangle origin
        var ax = a.X - originX;
        var az = a.Z - originZ;
        var dxs = b.X - a.X;
        var dzs = b.Z - a.Z;

        // Endpoint projections (parallel to dir and orthogonal to dir)
        var p0 = ax * dirX + az * dirZ;
        var q0 = ax * normalX + az * normalZ;
        var p1 = (ax + dxs) * dirX + (az + dzs) * dirZ;
        var q1 = (ax + dxs) * normalX + (az + dzs) * normalZ;

        // Inflated bounds
        var front = lenFront + cushion;
        var back = -(lenBack + cushion);
        var left = halfWidth + cushion;
        var right = -(halfWidth + cushion);

        // Quick reject (both outside the same half-plane)
        if (p0 > front && p1 > front || p0 < back && p1 < back || q0 > left && q1 > left || q0 < right && q1 < right)
        {
            return false;
        }

        // Quick accept (both endpoints fully inside)
        if (p0 <= front && p0 >= back && p1 <= front && p1 >= back && q0 <= left && q0 >= right && q1 <= left && q1 >= right)
        {
            return true;
        }

        // Scalar Liang–Barsky on projections
        float tmin = 0f, tmax = 1f;
        var dp = p1 - p0;
        var dq = q1 - q0;

        return ClipUpper(p0, dp, front, ref tmin, ref tmax) // p <= front
        && ClipLower(p0, dp, back, ref tmin, ref tmax) // p >= back
        && ClipUpper(q0, dq, left, ref tmin, ref tmax) // q <= left
        && ClipLower(q0, dq, right, ref tmin, ref tmax) // q >= right
        && tmax > tmin + Epsilon;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ClipUpper(float s0, float ds, float upper, ref float tmin, ref float tmax)
    {
        if (NearlyZero(ds))
        {
            return s0 <= upper + Epsilon;
        }
        var t = (upper - s0) / ds;
        if (ds > 0f)
        {
            tmax = Math.Min(tmax, t);
        }
        else
        {
            tmin = Math.Max(tmin, t);
        }
        return tmax > tmin + Epsilon;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ClipLower(float s0, float ds, float lower, ref float tmin, ref float tmax)
    {
        if (NearlyZero(ds))
        {
            return s0 >= lower - Epsilon;
        }
        var t = (lower - s0) / ds;
        if (ds > 0f)
        {
            tmin = Math.Max(tmin, t);
        }
        else
        {
            tmax = Math.Min(tmax, t);
        }
        return tmax > tmin + Epsilon;
    }
}

[SkipLocalsInit]
public sealed class SDInvertedRect : ShapeDistance
{
    private readonly float originX, originZ, dirX, dirZ, normalX, normalZ, lenFront, lenBack, halfWidth;

    public SDInvertedRect(WPos Origin, Angle Direction, float LenFront, float LenBack, float HalfWidth) : this(Origin, Direction.ToDirection(), LenFront, LenBack, HalfWidth) { }

    public SDInvertedRect(WPos From, WPos To, float HalfWidth)
    {
        var from = From;
        var dir = To - from;
        var l = dir.Length();
        var normalizedDir = dir / l;
        var normal = normalizedDir.OrthoL();

        originX = from.X;
        originZ = from.Z;
        dirX = normalizedDir.X;
        dirZ = normalizedDir.Z;
        normalX = normal.X;
        normalZ = normal.Z;
        lenFront = l;
        lenBack = default;
        halfWidth = HalfWidth;
    }

    public SDInvertedRect(WPos Origin, WDir Dir, float LenFront, float LenBack, float HalfWidth)
    {
        // dir points outside far side
        var dir = Dir;
        var normal = dir.OrthoL(); // points outside left side
        var origin = Origin;
        originX = origin.X;
        originZ = origin.Z;
        dirX = dir.X;
        dirZ = dir.Z;
        normalX = normal.X;
        normalZ = normal.Z;
        lenFront = LenFront;
        lenBack = LenBack;
        halfWidth = HalfWidth;
    }

    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distParr = pXoriginX * dirX + pZoriginZ * dirZ;
        var distOrtho = pXoriginX * normalX + pZoriginZ * normalZ;
        var distFront = distParr - lenFront;
        var distBack = -distParr - lenBack;
        var distLeft = distOrtho - halfWidth;
        var distRight = -distOrtho - halfWidth;

        var maxParr = distFront > distBack ? distFront : distBack;
        var maxOrtho = distLeft > distRight ? distLeft : distRight;

        return maxParr > maxOrtho ? -maxParr : -maxOrtho;
    }

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        return !SegmentFullyInsideOriginalRect(rowStart, rowStart + width * dx, cushion);
    }

    private bool SegmentFullyInsideOriginalRect(WPos a, WPos b, float cushion)
    {
        bool Inside(WPos p)
        {
            var px = p.X - originX;
            var pz = p.Z - originZ;
            var parr = px * dirX + pz * dirZ;
            var ortho = px * normalX + pz * normalZ;
            return parr <= lenFront + cushion + Epsilon && parr >= -(lenBack + cushion) - Epsilon
            && ortho <= halfWidth + cushion + Epsilon && ortho >= -(halfWidth + cushion) - Epsilon;
        }
        return Inside(a) && Inside(b);
    }
}

[SkipLocalsInit]
public sealed class SDCapsule : ShapeDistance
{
    private readonly float originX, originZ, dirX, dirZ, length, radius;
    private readonly WDir direction;
    private readonly WPos origin;

    public SDCapsule(WPos Origin, Angle Direction, float Length, float Radius) : this(Origin, Direction.ToDirection(), Length, Radius) { }

    public SDCapsule(WPos Origin, WDir Direction, float Length, float Radius)
    {
        origin = Origin;
        direction = Direction;
        originX = origin.X;
        originZ = origin.Z;
        dirX = direction.X;
        dirZ = direction.Z;
        length = Length;
        radius = Radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        var pXoriginX = pX - originX;
        var pZoriginZ = pZ - originZ;
        var t = pXoriginX * dirX + pZoriginZ * dirZ;
        t = (t < 0f) ? default : (t > length ? length : t);
        var proj = origin + t * direction;
        var pXprojX = pX - proj.X;
        var pZprojZ = pZ - proj.Z;
        return MathF.Sqrt(pXprojX * pXprojX + pZprojZ * pZprojZ) - radius;
    }

    // Row-cull: segment vs capsule (segment swept disk)
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        var aX = rowStart.X;
        var aZ = rowStart.Z;
        var b = rowStart + width * dx;
        var bX = b.X;
        var bZ = b.Z;

        var q0X = originX;
        var q0Z = originZ;
        var q1X = originX + length * dirX;
        var q1Z = originZ + length * dirZ;

        var r = radius + cushion;

        return SegmentSegmentDistanceSquared(aX, aZ, bX, bZ, q0X, q0Z, q1X, q1Z) <= r * r + Epsilon;
    }

    // Closest distance^2 between 2D segments P=[p0,p1], Q=[q0,q1]
    private static float SegmentSegmentDistanceSquared(float p0x, float p0z, float p1x, float p1z, float q0x, float q0z, float q1x, float q1z)
    {
        // Based on Ericson RTCD 5.1.9 (clamped)
        var ux = p1x - p0x;
        var uz = p1z - p0z;
        var vx = q1x - q0x;
        var vz = q1z - q0z;
        var wx = p0x - q0x;
        var wz = p0z - q0z;
        var a = ux * ux + uz * uz; // ||u||^2
        var b = ux * vx + uz * vz; // u·v
        var c = vx * vx + vz * vz; // ||v||^2
        var d = ux * wx + uz * wz; // u·w
        var e = vx * wx + vz * wz; // v·w
        float s, t;
        var D = a * c - b * b; // denom

        if (a <= Epsilon && c <= Epsilon)
        {
            // both segments degenerate
            return (p0x - q0x) * (p0x - q0x) + (p0z - q0z) * (p0z - q0z);
        }
        if (a <= Epsilon)
        {
            // P degenerate -> point to segment Q
            s = 0f;
            t = Clamp01(e / c);
        }
        else if (c <= Epsilon)
        {
            // Q degenerate -> point to segment P
            t = 0f;
            s = Clamp01(-d / a);
        }
        else
        {
            s = Clamp01((b * e - c * d) / D);
            t = (b * s + e) / c;
            if (t < 0f)
            {
                t = 0f;
                s = Clamp01(-d / a);
            }
            else if (t > 1f)
            {
                t = 1f;
                s = Clamp01((b - d) / a);
            }
        }

        var dx = p0x + s * ux - (q0x + t * vx);
        var dz = p0z + s * uz - (q0z + t * vz);
        return dx * dx + dz * dz;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float Clamp01(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);
    }
}

[SkipLocalsInit]
public sealed class SDInvertedCapsule : ShapeDistance
{
    private readonly float originX, originZ, dirX, dirZ, length, radius;
    private readonly WDir direction;
    private readonly WPos origin;

    public SDInvertedCapsule(WPos Origin, Angle Direction, float Length, float Radius) : this(Origin, Direction.ToDirection(), Length, Radius) { }

    public SDInvertedCapsule(WPos Origin, WDir Direction, float Length, float Radius)
    {
        origin = Origin;
        direction = Direction;
        originX = origin.X;
        originZ = origin.Z;
        dirX = direction.X;
        dirZ = direction.Z;
        length = Length;
        radius = Radius;
    }

    public override float Distance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        var pXoriginX = pX - originX;
        var pZoriginZ = pZ - originZ;
        var t = pXoriginX * dirX + pZoriginZ * dirZ;
        t = (t < 0f) ? default : (t > length ? length : t);
        var proj = origin + t * direction;
        var pXprojX = pX - proj.X;
        var pZprojZ = pZ - proj.Z;
        return radius - MathF.Sqrt(pXprojX * pXprojX + pZprojZ * pZprojZ);
    }

    // inverted capsules very rarely do not intersect a row, so we always return true
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDCross : ShapeDistance
{
    private readonly float length, halfWidth;
    private readonly WDir direction, normal;
    private readonly WPos origin;

    public SDCross(WPos Origin, Angle Direction, float Length, float HalfWidth)
    {
        origin = Origin;
        direction = Direction.ToDirection();
        length = Length;
        halfWidth = HalfWidth;
        normal = direction.OrthoL();
    }

    public override float Distance(WPos p)
    {
        var offset = p - origin;
        var distParr = offset.Dot(direction);
        var distOrtho = offset.Dot(normal);
        var distPFront = distParr - length;
        var distPBack = -distParr - length;
        var distPLeft = distOrtho - halfWidth;
        var distPRight = -distOrtho - halfWidth;
        var distOFront = distOrtho - length;
        var distOBack = -distOrtho - length;
        var distOLeft = distParr - halfWidth;
        var distORight = -distParr - halfWidth;

        var distPMax1 = distPFront > distPBack ? distPFront : distPBack;
        var distPMax2 = distPLeft > distPRight ? distPLeft : distPRight;
        var distP = distPMax1 > distPMax2 ? distPMax1 : distPMax2;

        var distOMax1 = distOFront > distOBack ? distOFront : distOBack;
        var distOMax2 = distOLeft > distORight ? distOLeft : distORight;
        var distO = distOMax1 > distOMax2 ? distOMax1 : distOMax2;

        return distP < distO ? distP : distO;
    }

    // Row intersects union of two rectangles (axis-aligned in {direction,normal} frame)
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        var a = rowStart;
        var b = rowStart + width * dx;
        return SegmentIntersectsRect(a, b, direction, normal, length, halfWidth, origin, cushion)
        || SegmentIntersectsRect(a, b, normal, direction, length, halfWidth, origin, cushion);
    }

    private static bool SegmentIntersectsRect(WPos a, WPos b, WDir dir, WDir nrm, float len, float halfW, WPos origin, float cushion)
    {
        var a_ = a;
        var aX_ = a_.X;
        var aZ_ = a_.Z;
        var ax = aX_ - origin.X;
        var az = aZ_ - origin.Z;
        var dx = b.X - aX_;
        var dz = b.Z - aZ_;
        var dirX = dir.X;
        var dirZ = dir.Z;
        var nX = nrm.X;
        var nZ = nrm.Z;
        float tmin = 0f, tmax = 1f;

        return ClipLE(dirX, dirZ, ax, az, dx, dz, len + cushion, ref tmin, ref tmax)
        && ClipGE(dirX, dirZ, ax, az, dx, dz, -(len + cushion), ref tmin, ref tmax)
        && ClipLE(nX, nZ, ax, az, dx, dz, halfW + cushion, ref tmin, ref tmax)
        && ClipGE(nX, nZ, ax, az, dx, dz, -(halfW + cushion), ref tmin, ref tmax)
        && tmax > tmin + Epsilon;
    }

    private static bool ClipLE(float nX, float nZ, float ax, float az, float dx, float dz, float value, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (Math.Abs(s1) <= Epsilon)
        {
            return s0 <= value + Epsilon;
        }
        var bound = (value - s0) / s1;
        if (s1 > 0f)
        {
            tmax = Math.Min(tmax, bound);
        }
        else
        {
            tmin = Math.Max(tmin, bound);
        }
        return tmax > tmin + Epsilon;
    }

    private static bool ClipGE(float nX, float nZ, float ax, float az, float dx, float dz, float value, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (Math.Abs(s1) <= Epsilon)
        {
            return s0 >= value - Epsilon;
        }
        var bound = (value - s0) / s1;
        if (s1 > 0f)
        {
            tmin = Math.Max(tmin, bound);
        }
        else
        {
            tmax = Math.Min(tmax, bound);
        }
        return tmax > tmin + Epsilon;
    }
}

[SkipLocalsInit]
public sealed class SDInvertedCross : ShapeDistance
{
    private readonly float length, halfWidth;
    private readonly WDir direction, normal;
    private readonly WPos origin;

    public SDInvertedCross(WPos Origin, Angle Direction, float Length, float HalfWidth)
    {
        origin = Origin;
        direction = Direction.ToDirection();
        length = Length;
        halfWidth = HalfWidth;
        normal = direction.OrthoL();
    }

    public override float Distance(WPos p)
    {
        var offset = p - origin;
        var distParr = offset.Dot(direction);
        var distOrtho = offset.Dot(normal);
        var distPFront = distParr - length;
        var distPBack = -distParr - length;
        var distPLeft = distOrtho - halfWidth;
        var distPRight = -distOrtho - halfWidth;
        var distOFront = distOrtho - length;
        var distOBack = -distOrtho - length;
        var distOLeft = distParr - halfWidth;
        var distORight = -distParr - halfWidth;

        var distPMax1 = distPFront > distPBack ? distPFront : distPBack;
        var distPMax2 = distPLeft > distPRight ? distPLeft : distPRight;
        var distP = distPMax1 > distPMax2 ? distPMax1 : distPMax2;

        var distOMax1 = distOFront > distOBack ? distOFront : distOBack;
        var distOMax2 = distOLeft > distORight ? distOLeft : distORight;
        var distO = distOMax1 > distOMax2 ? distOMax1 : distOMax2;

        return distP < distO ? -distP : -distO;
    }

    // inverted: row intersects unless fully inside the union (i.e., fully in arm P or fully in arm O)
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        // conservative: may over-include, never miss
        var a = rowStart;
        var b = rowStart + width * dx;
        return !SegmentFullyInsideRect(a, b, direction, normal, length, halfWidth, origin, cushion)
        && !SegmentFullyInsideRect(a, b, normal, direction, length, halfWidth, origin, cushion);
    }

    private static bool SegmentFullyInsideRect(WPos a, WPos b, WDir dir, WDir nrm, float len, float halfW, WPos origin, float cushion)
    {
        bool Inside(WPos p)
        {
            var rx = p.X - origin.X;
            var rz = p.Z - origin.Z;
            var dirX = dir.X;
            var dirZ = dir.Z;
            var nX = nrm.X;
            var nZ = nrm.Z;
            var parr = dirX * rx + dirZ * rz;
            var ortho = nX * rx + nZ * rz;
            return parr <= len + cushion + Epsilon && parr >= -(len + cushion) - Epsilon
            && ortho <= halfW + cushion + Epsilon && ortho >= -(halfW + cushion) - Epsilon;
        }
        return Inside(a) && Inside(b); // convex ⇒ endpoints suffice
    }
}

[SkipLocalsInit]
public sealed class SDConvexPolygon : ShapeDistance
{
    private readonly bool cw;
    private readonly (WPos a, WPos b)[] edges;

    public SDConvexPolygon((WPos, WPos)[] Edges, bool Cw)
    {
        edges = Edges;
        cw = Cw;
    }

    public SDConvexPolygon(ReadOnlySpan<WPos> Vertices, bool Cw)
    {
        var vertices = Vertices;
        var len = vertices.Length;
        var edgesA = new (WPos, WPos)[len];
        for (var i = 0; i < len; ++i)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % len];
            edgesA[i] = (a, b);
        }
        edges = edgesA;
        cw = Cw;
    }

    public override float Distance(WPos p)
    {
        var minDistance = float.MaxValue;
        var inside = true;
        var len = edges.Length;
        var point = p;
        for (var i = 0; i < len; ++i)
        {
            ref var edge = ref edges[i];
            var a = edge.a;
            var b = edge.b;
            var ab = b - a;
            var ap = point - a;
            var distance = (ab.X * ap.Z - ab.Z * ap.X) / ab.Length();

            if (cw && distance > 0f || !cw && distance < 0f)
            {
                inside = false;
            }
            minDistance = Math.Min(minDistance, Math.Abs(distance));
        }
        return inside ? -minDistance : minDistance;
    }

    // Segment vs convex polygon (inflated by cushion). Rotation-agnostic.
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        var a = rowStart;
        var b = rowStart + width * dx;
        var tmin = 0f;
        var tmax = 1f;

        var dirx = b.X - a.X;
        var dirz = b.Z - a.Z;
        var lenEdges = edges.Length;

        // Degenerate segment → point-in-inflated-polygon test by half-planes
        if (Math.Abs(dirx) + Math.Abs(dirz) <= Epsilon)
        {
            for (var i = 0; i < lenEdges; ++i)
            {
                ref var edge = ref edges[i];
                var eA = edge.a;
                var eB = edge.b;
                var ex = eB.X - eA.X;
                var ez = eB.Z - eA.Z;
                var len = MathF.Sqrt(ex * ex + ez * ez);
                if (len <= Epsilon)
                {
                    continue;
                }
                var off = cushion * len;
                var s0 = ex * (a.Z - eA.Z) - ez * (a.X - eA.X);
                if (cw)
                {
                    if (s0 > off + Epsilon)
                    {
                        return false;
                    }
                }
                else
                {
                    if (s0 < -off - Epsilon)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        for (var i = 0; i < lenEdges; ++i)
        {
            ref var edge = ref edges[i];
            var eA = edge.a;
            var eB = edge.b;
            var ex = eB.X - eA.X;
            var ez = eB.Z - eA.Z;
            var len = MathF.Sqrt(ex * ex + ez * ez);
            if (len <= Epsilon)
            {
                continue; // skip degenerate
            }
            var off = cushion * len; // scale cushion by edge length to avoid normalization

            // s(t) = cross(ex,ez, (a - eA) + t*(b - a)) = s0 + t*s1
            var s0 = ex * (a.Z - eA.Z) - ez * (a.X - eA.X);
            var s1 = ex * dirz - ez * dirx;

            if (cw)
            {
                // s(t) <= off
                if (!ClipLE_Unnormalized(s0, s1, off, ref tmin, ref tmax))
                {
                    return false;
                }
            }
            else
            {
                // s(t) >= -off
                if (!ClipGE_Unnormalized(s0, s1, -off, ref tmin, ref tmax))
                {
                    return false;
                }
            }
        }
        return tmax > tmin + Epsilon;
    }

    private static bool ClipLE_Unnormalized(float s0, float s1, float value, ref float tmin, ref float tmax)
    {
        if (Math.Abs(s1) <= Epsilon)
        {
            return s0 <= value + Epsilon;
        }
        var bound = (value - s0) / s1;
        if (s1 > 0f)
        {
            tmax = Math.Min(tmax, bound);
        }
        else
        {
            tmin = Math.Max(tmin, bound);
        }
        return tmax > tmin + Epsilon;
    }

    private static bool ClipGE_Unnormalized(float s0, float s1, float value, ref float tmin, ref float tmax)
    {
        if (Math.Abs(s1) <= Epsilon)
        {
            return s0 >= value - Epsilon;
        }
        var bound = (value - s0) / s1;
        if (s1 > 0f)
        {
            tmin = Math.Max(tmin, bound);
        }
        else
        {
            tmax = Math.Min(tmax, bound);
        }
        return tmax > tmin + Epsilon;
    }
}
