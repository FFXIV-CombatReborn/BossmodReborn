namespace BossMod;

public static class BitmapPathfindExtensions
{
    public static bool TryWorldToBitmapCell(this Bitmap map, Bitmap.Rect rect, WPos mapCenter, WPos pos, out int x, out int y)
    {
        var centerCellX = (rect.Left + rect.Right) * 0.5f;
        var centerCellY = (rect.Top + rect.Bottom) * 0.5f;
        var invRes = 1.0f / map.PixelSize;
        var delta = (pos - mapCenter) * invRes;
        x = (int)MathF.Floor(centerCellX + delta.X);
        y = (int)MathF.Floor(centerCellY + delta.Z);
        return (uint)x < map.Width && (uint)y < map.Height;
    }

    public static WPos CellCenterToWorld(this Bitmap map, Bitmap.Rect rect, WPos mapCenter, int x, int y)
    {
        var centerCellX = (rect.Left + rect.Right) * 0.5f;
        var centerCellY = (rect.Top + rect.Bottom) * 0.5f;
        var ps = map.PixelSize;
        return new WPos(
            mapCenter.X + (x + 0.5f - centerCellX) * ps,
            mapCenter.Z + (y + 0.5f - centerCellY) * ps);
    }

    public static bool HasObstacleMapLineOfSight(this Bitmap map, Bitmap.Rect rect, WPos mapCenter, WPos from, WPos to)
        => HasObstacleMapLineOfSight(new Bitmap.Region(map, rect), mapCenter, from, to);

    public static WPos GetWorldOrigin(this Bitmap.Region region, WPos mapCenter)
        => region.WorldOrigin ?? mapCenter - region.Bitmap.PixelSize * new WDir((region.Rect.Left + region.Rect.Right) * 0.5f, (region.Rect.Top + region.Rect.Bottom) * 0.5f);

    public static bool TryWorldToBitmapCell(this Bitmap.Region region, WPos mapCenter, WPos pos, out int x, out int y)
    {
        var cell = (pos - region.GetWorldOrigin(mapCenter)) / region.Bitmap.PixelSize;
        x = (int)MathF.Floor(cell.X);
        y = (int)MathF.Floor(cell.Z);
        return (uint)x < region.Bitmap.Width && (uint)y < region.Bitmap.Height;
    }

    public static WPos CellCenterToWorld(this Bitmap.Region region, WPos mapCenter, int x, int y)
        => region.GetWorldOrigin(mapCenter) + region.Bitmap.PixelSize * new WDir(x + 0.5f, y + 0.5f);

    public static bool HasObstacleMapLineOfSight(this Bitmap.Region region, WPos mapCenter, WPos from, WPos to)
    {
        var map = region.Bitmap;
        if (!region.TryWorldToBitmapCell(mapCenter, from, out var x0, out var y0) || !region.TryWorldToBitmapCell(mapCenter, to, out var x1, out var y1))
            return true;

        var dx = Math.Abs(x1 - x0);
        var sx = x0 < x1 ? 1 : -1;
        var dy = -Math.Abs(y1 - y0);
        var sy = y0 < y1 ? 1 : -1;
        var err = dx + dy;
        var x = x0;
        var y = y0;

        while (true)
        {
            if ((uint)x < map.Width && (uint)y < map.Height && map[x, y])
                return false;
            if (x == x1 && y == y1)
                return true;
            var e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    public static void BlockMapPixels(this Bitmap.Region region, Pathfinding.Map map, WPos mapCenter)
    {
        var bitmap = region.Bitmap;
        var clip = region.Rect.Clamped(bitmap.FullRect);
        var origin = region.GetWorldOrigin(mapCenter);
        var first = (map.GridToWorld(0, 0, 0.5f, 0.5f) - origin) / bitmap.PixelSize;
        var stepZ = map.LocalZDivRes * (map.Resolution * map.Resolution / bitmap.PixelSize);
        var stepX = stepZ.OrthoL();
        var halfX = (Math.Abs(stepX.X) + Math.Abs(stepZ.X)) * 0.5f;
        var halfZ = (Math.Abs(stepX.Z) + Math.Abs(stepZ.Z)) * 0.5f;
        const float epsilon = 1e-4f; // avoid including a neighboring pixel merely because of roundoff at a shared edge
        var axisAligned = Math.Abs(stepX.Z) < epsilon && Math.Abs(stepZ.X) < epsilon
            || Math.Abs(stepX.X) < epsilon && Math.Abs(stepZ.Z) < epsilon;
        var sampleCenters = map.Resolution <= bitmap.PixelSize && axisAligned;

        // A cropped/rotated/coarser navigation grid does not share the bitmap's indices.
        // At equal/finer resolution, preserve walkability at each node's world position:
        // unioning neighboring pixels would close narrow passages when origins differ.
        // Coarser or rotated grids still use conservative coverage to retain thin obstacles.
        for (var y = 0; y < map.Height; ++y)
        {
            var rowStart = first + y * stepZ;
            for (var x = 0; x < map.Width; ++x)
            {
                var index = map.GridToIndex(x, y);
                if (map.PixelMaxG[index] < 0f)
                    continue;
                var blocked = false;
                if (sampleCenters)
                {
                    // Use the same world coordinate as navigation/LOS queries. Accumulating
                    // transformed offsets can round to the other side of a pixel boundary.
                    var sample = (map.GridToWorld(x, y, 0.5f, 0.5f) - origin) / bitmap.PixelSize;
                    var bx = (int)MathF.Floor(sample.X);
                    var by = (int)MathF.Floor(sample.Z);
                    blocked = bx >= clip.Left && bx < clip.Right && by >= clip.Top && by < clip.Bottom && bitmap[bx, by];
                }
                else
                {
                    var cell = rowStart + x * stepX;
                    var left = Math.Max(clip.Left, (int)MathF.Floor(cell.X - halfX + epsilon));
                    var right = Math.Min(clip.Right, (int)MathF.Ceiling(cell.X + halfX - epsilon));
                    var top = Math.Max(clip.Top, (int)MathF.Floor(cell.Z - halfZ + epsilon));
                    var bottom = Math.Min(clip.Bottom, (int)MathF.Ceiling(cell.Z + halfZ - epsilon));
                    for (var by = top; by < bottom && !blocked; ++by)
                        for (var bx = left; bx < right; ++bx)
                            if (bitmap[bx, by])
                            {
                                blocked = true;
                                break;
                            }
                }
                if (blocked)
                {
                    map.PixelMaxG[index] = -1000f;
                    map.PixelPriority[index] = float.MinValue;
                }
            }
        }
    }
}

// Scratch-allocation-free symmetric shadowcasting for obstacle bitmaps.
// Uses the exact-rational model described at https://www.albertford.com/shadowcasting/ (CC0).
// Input bits are opaque cells; output bits are visible, non-opaque cells.
public static class BitmapShadowcasting
{
    private readonly struct Slope(int numerator, int denominator)
    {
        public readonly int Numerator = numerator;
        public readonly int Denominator = denominator;
    }

    // Build a circular field of view centered on an obstacle-map cell.
    // Radius follows follwing LOS convention: cells at exactly radius are excluded
    public static Bitmap BuildFieldOfView(Bitmap obstacles, int originX, int originY, int radius)
    {
        var visible = new Bitmap(obstacles.Width, obstacles.Height, obstacles.Color0, obstacles.Color1, obstacles.Resolution);
        if (radius <= 0 || (uint)originX >= (uint)obstacles.Width || (uint)originY >= (uint)obstacles.Height)
        {
            return visible;
        }

        // current behaviour: it is visible even if bad map metadata happens to place the caster in a bit marked as an obstacle
        visible[originX, originY] = true;

        // No in-bounds cell is farther away than its Manhattan distance. Capping oversized ranges here preserves every possible result while bounding scan depth
        var maxDeltaX = Math.Max(originX, obstacles.Width - 1 - originX);
        var maxDeltaY = Math.Max(originY, obstacles.Height - 1 - originY);
        var maxUsefulRadius = (int)Math.Min(int.MaxValue, (long)maxDeltaX + maxDeltaY + 1);
        if (radius > maxUsefulRadius)
        {
            radius = maxUsefulRadius;
        }

        if (radius == 1)
        {
            return visible;
        }

        var radiusSquared = (long)radius * radius;
        var initialStart = new Slope(-1, 1);
        var initialEnd = new Slope(1, 1);

        // Four 90-degree quadrants cover the field. Their diagonal edges overlap deliberately;
        // setting an existing output bit again is cheaper than maintaining edge special cases.
        ScanQuadrant(obstacles, visible, originX, originY, radius, radiusSquared, 0, 1, initialStart, initialEnd); // north
        ScanQuadrant(obstacles, visible, originX, originY, radius, radiusSquared, 1, 1, initialStart, initialEnd); // east
        ScanQuadrant(obstacles, visible, originX, originY, radius, radiusSquared, 2, 1, initialStart, initialEnd); // south
        ScanQuadrant(obstacles, visible, originX, originY, radius, radiusSquared, 3, 1, initialStart, initialEnd); // west

        return visible;
    }

    // Scan one visible slope interval in a quadrant. Exact rational comparisons give reciprocal
    // floor-to-floor visibility and avoid floating-point cracks along long wall edges.
    private static void ScanQuadrant(Bitmap obstacles, Bitmap visible, int originX, int originY, int radius, long radiusSquared,
        int quadrant, int depth, Slope startSlope, Slope endSlope)
    {
        if (depth >= radius)
        {
            return;
        }

        var minColumn = RoundTiesUp(depth, startSlope);
        var maxColumn = RoundTiesDown(depth, endSlope);
        var havePrevious = false;
        var previousOpaque = false;

        for (var column = minColumn; column <= maxColumn; ++column)
        {
            Transform(quadrant, originX, originY, depth, column, out var x, out var y);
            var inBounds = (uint)x < (uint)obstacles.Width && (uint)y < (uint)obstacles.Height;

            // The map boundary is transparent to the angular sweep but can never be revealed. This avoids boundary cells casting artificial shadows back into the rectangular map
            var opaque = inBounds && obstacles[x, y];

            if (inBounds && !opaque && IsSymmetric(depth, column, startSlope, endSlope)
                && (long)depth * depth + (long)column * column < radiusSquared)
            {
                visible[x, y] = true;
            }

            if (havePrevious)
            {
                if (previousOpaque && !opaque)
                {
                    // Leaving a wall: trim the beginning of this row's remaining interval
                    startSlope = SlopeAt(depth, column);
                }
                else if (!previousOpaque && opaque)
                {
                    // Entering a wall: the floor interval before it continues into the next row
                    ScanQuadrant(obstacles, visible, originX, originY, radius, radiusSquared, quadrant, depth + 1, startSlope, SlopeAt(depth, column));
                }
            }

            havePrevious = true;
            previousOpaque = opaque;
        }

        // If this row ends on floor, its remaining slope interval continues outward
        if (havePrevious && !previousOpaque)
        {
            ScanQuadrant(obstacles, visible, originX, originY, radius, radiusSquared, quadrant, depth + 1, startSlope, endSlope);
        }
    }

    private static void Transform(int quadrant, int originX, int originY, int depth, int column, out int x, out int y)
    {
        switch (quadrant)
        {
            case 0:
                x = originX + column;
                y = originY - depth;
                break;
            case 1:
                x = originX + depth;
                y = originY + column;
                break;
            case 2:
                x = originX + column;
                y = originY + depth;
                break;
            default:
                x = originX - depth;
                y = originY + column;
                break;
        }
    }

    private static Slope SlopeAt(int depth, int column) => new(2 * column - 1, 2 * depth);

    private static bool IsSymmetric(int depth, int column, Slope startSlope, Slope endSlope)
        => (long)column * startSlope.Denominator >= (long)depth * startSlope.Numerator
            && (long)column * endSlope.Denominator <= (long)depth * endSlope.Numerator;

    // floor(n + 0.5), with ties toward positive infinity
    private static int RoundTiesUp(int depth, Slope slope)
        => (int)FloorDiv(2L * depth * slope.Numerator + slope.Denominator, 2L * slope.Denominator);

    // ceil(n - 0.5), with ties toward negative infinity
    private static int RoundTiesDown(int depth, Slope slope)
        => (int)CeilingDiv(2L * depth * slope.Numerator - slope.Denominator, 2L * slope.Denominator);

    private static long FloorDiv(long numerator, long denominator)
    {
        var quotient = numerator / denominator;
        return numerator < 0 && numerator % denominator != 0 ? quotient - 1 : quotient;
    }

    private static long CeilingDiv(long numerator, long denominator)
    {
        var quotient = numerator / denominator;
        return numerator > 0 && numerator % denominator != 0 ? quotient + 1 : quotient;
    }
}
