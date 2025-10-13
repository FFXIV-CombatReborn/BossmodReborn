using System.Buffers;
namespace BossMod.Pathfinding;

// 'map' used for running pathfinding algorithms
// this is essentially a square grid representing an arena (or immediate neighbourhood of the player) where we rasterize forbidden/desired zones
// area covered by each pixel can be in one of the following states:
// - default: safe to traverse but non-goal
// - danger: unsafe to traverse after X seconds (X >= 0); instead of X, we store max 'g' value (distance travelled assuming constant speed) for which pixel is still considered unblocked
// - goal: destination with X priority (X > 0); 'default' is considered a goal with priority 0
// - goal and danger are mutually exclusive, 'danger' overriding 'goal' state
// typically we try to find a path to goal with highest priority; if that fails, try lower priorities; if no paths can be found (e.g. we're currently inside an imminent aoe) we find direct path to closest safe pixel

[SkipLocalsInit]
public sealed class Map
{
    public readonly struct TeleEdge(int destIndex, float useTime, float notBeforeG)
    {
        public readonly int DestIndex = destIndex;
        public readonly float UseTime = useTime;
        public readonly float NotBeforeG = notBeforeG;
    }

    private int[] _teleEdgeOffsets = [];
    private TeleEdge[] _teleEdges = [];
    private bool[] _teleShadow = []; // cells that partially intersect a teleporter
    public bool HasTeleporters;

    public float Resolution; // pixel size, in world units
    public int Width; // always even
    public int Height; // always even
    public float[] PixelMaxG = []; // == MaxValue if not dangerous (TODO: consider changing to a byte per pixel?), < 0 if impassable
    public float[] PixelPriority = [];

    public WPos Center; // position of map center in world units
    public Angle Rotation; // rotation relative to world space (=> ToDirection() is equal to direction of local 'height' axis in world space)
    public WDir LocalZDivRes;

    public float MaxG; // maximal 'maxG' value of all blocked pixels
    public float MaxPriority; // maximal 'priority' value of all goal pixels

    // min-max bounds of 'interesting' area, default to (0,0) to (width-1,height-1)
    public int MinX;
    public int MinY;
    public int MaxX;
    public int MaxY;

    public Map() { }
    public Map(float resolution, WPos center, float worldHalfWidth, float worldHalfHeight, Angle rotation = default) => Init(resolution, center, worldHalfWidth, worldHalfHeight, rotation);

    public void Init(float resolution, WPos center, float worldHalfWidth, float worldHalfHeight, Angle rotation = default)
    {
        Resolution = resolution;
        Width = 2 * (int)MathF.Ceiling(worldHalfWidth / resolution);
        Height = 2 * (int)MathF.Ceiling(worldHalfHeight / resolution);

        var numPixels = Width * Height;
        if (PixelMaxG.Length < numPixels)
        {
            PixelMaxG = new float[numPixels];
        }
        new Span<float>(PixelMaxG, 0, numPixels).Fill(float.MaxValue); // fill is unconditional, can we avoid it by changing storage?..
        if (PixelPriority.Length < numPixels)
        {
            PixelPriority = new float[numPixels];
        }
        else
        {
            new Span<float>(PixelPriority, 0, numPixels).Clear();
        }

        Center = center;
        Rotation = rotation;
        LocalZDivRes = rotation.ToDirection() / Resolution;

        MaxG = 0f;
        MaxPriority = 0f;

        MinX = MinY = 0;
        MaxX = Width - 1;
        MaxY = Height - 1;

        HasTeleporters = false;
    }

    public void Init(Map source, WPos center)
    {
        Resolution = source.Resolution;
        Width = source.Width;
        Height = source.Height;

        var numPixels = Width * Height;
        if (PixelMaxG.Length < numPixels)
        {
            PixelMaxG = new float[numPixels];
        }
        Array.Copy(source.PixelMaxG, PixelMaxG, numPixels);
        if (PixelPriority.Length < numPixels)
        {
            PixelPriority = new float[numPixels];
        }
        Array.Copy(source.PixelPriority, PixelPriority, numPixels);

        Center = center;
        Rotation = source.Rotation;
        LocalZDivRes = source.LocalZDivRes;

        MaxG = source.MaxG;
        MaxPriority = source.MaxPriority;

        MinX = source.MinX;
        MinY = source.MinY;
        MaxX = source.MaxX;
        MaxY = source.MaxY;

        HasTeleporters = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 WorldToGridFrac(WPos world)
    {
        var offset = world - Center;
        var x = offset.Dot(LocalZDivRes.OrthoL());
        var y = offset.Dot(LocalZDivRes);
        return new((Width >> 1) + x, (Height >> 1) + y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GridToIndex(int x, int y) => y * Width + x;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GridToIndex((int x, int y) p) => GridToIndex(p.x, p.y);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int x, int y) IndexToGrid(int index) => (index % Width, index / Width);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int x, int y) FracToGrid(Vector2 frac) => ((int)MathF.Floor(frac.X), (int)MathF.Floor(frac.Y));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int x, int y) WorldToGrid(WPos world) => FracToGrid(WorldToGridFrac(world));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int x, int y) ClampToGrid((int x, int y) pos) => (Math.Clamp(pos.x, 0, Width - 1), Math.Clamp(pos.y, 0, Height - 1));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WPos GridToWorld(int gx, int gy, float fx, float fy)
    {
        var rsq = Resolution * Resolution; // since we then multiply by _localZDivRes, end result is same as * res * rotation.ToDir()
        var ax = (gx - (Width >> 1) + fx) * rsq;
        var az = (gy - (Height >> 1) + fy) * rsq;
        return Center + ax * LocalZDivRes.OrthoL() + az * LocalZDivRes;
    }

    // block all pixels for which function returns value smaller than threshold ('inside' shape + extra cushion)
    public void BlockPixelsInside(ShapeDistance shape, float maxG, float threshold)
    {
        MaxG = Math.Max(MaxG, maxG);
        var width = Width;
        var height = Height;
        var resolution = Resolution;
        var dir = Rotation.ToDirection();
        var dx = dir.OrthoL() * resolution;
        var dy = dir * resolution;
        var startPos = Center - ((width >> 1) - 0.5f) * dx - ((height >> 1) - 0.5f) * dy;
        var maxG_ = maxG;
        var threshold_ = threshold;
        var shape_ = shape;

        for (var y = 0; y < height; ++y)
        {
            var posY = startPos + y * dy;
            var rowBaseIndex = y * width;
            for (var x = 0; x < width; ++x)
            {
                var pos = posY + x * dx;
                if (shape_.Distance(pos) <= threshold_)
                {
                    PixelMaxG[rowBaseIndex + x] = maxG_;
                }
            }
        }
    }

    // enumerate pixels along line starting from (x1, y1) to (x2, y2); first is not returned, last is returned
    public (int x, int y)[] EnumeratePixelsInLine(int x1, int y1, int x2, int y2)
    {
        var absDx = Math.Abs(x2 - x1);
        var absDy = Math.Abs(y2 - y1);
        var estimatedLength = Math.Max(absDx, absDy);

        var result = new (int x, int y)[estimatedLength];

        int dx = absDx, sx = x1 < x2 ? 1 : -1;
        int dy = -absDy, sy = y1 < y2 ? 1 : -1;
        int err = dx + dy, e2;

        for (var i = 0; i < estimatedLength; ++i)
        {
            e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x1 += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y1 += sy;
            }

            result[i] = (x1, y1);
        }

        return result;
    }

    public void BuildTeleporterEdges(Teleporter[] teleporters)
    {
        var w = Width;
        var h = Height;
        var cellCount = w * h;

        _teleEdgeOffsets = cellCount > 0 ? new int[cellCount + 1] : [];
        _teleEdges = [];
        if (_teleShadow.Length < cellCount)
        {
            _teleShadow = new bool[cellCount];
        }
        else
        {
            Array.Clear(_teleShadow, 0, cellCount);
        }

        var pxMaxG = PixelMaxG;
        var len = teleporters.Length;
        var resolution = Resolution;

        var radToPix = 1f / resolution;
        var dy = LocalZDivRes * resolution * resolution;
        var dx = dy.OrthoL();
        var topLeft = Center - (w >> 1) * dx - (h >> 1) * dy;

        var counts = ArrayPool<int>.Shared.Rent(cellCount);
        var cursors = ArrayPool<int>.Shared.Rent(cellCount);
        Array.Clear(counts, 0, cellCount);

        var exitIdx = ArrayPool<int>.Shared.Rent(len);
        var backIdx = ArrayPool<int>.Shared.Rent(len);
        var radPixArr = ArrayPool<int>.Shared.Rent(len);

        try
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void CellCorners(int x, int y, out WPos tl, out WPos tr, out WPos bl, out WPos br)
            {
                var cellTopLeft = topLeft + y * dy + x * dx;
                tl = cellTopLeft;
                tr = cellTopLeft + dx;
                bl = cellTopLeft + dy;
                br = cellTopLeft + dx + dy;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float Dist2ToSegment(in WPos p, in WPos a, in WPos b)
            {
                var ab = b - a;
                var ap = p - a;
                var denom = ab.Dot(ab);
                var t = denom > 1e-12f ? Math.Max(0f, Math.Min(1f, ap.Dot(ab) / denom)) : 0f;
                var q = a + t * ab;
                var d = p - q;
                return d.LengthSq();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void ClassifyCell(int x, int y, in WPos center, float r2, out bool fullyInside, out bool intersects)
            {
                CellCorners(x, y, out var tl, out var tr, out var bl, out var br);

                var itl = (tl - center).LengthSq() <= r2;
                var itr = (tr - center).LengthSq() <= r2;
                var ibl = (bl - center).LengthSq() <= r2;
                var ibr = (br - center).LengthSq() <= r2;

                fullyInside = itl && itr && ibl && ibr;
                if (fullyInside)
                {
                    intersects = true;
                    return;
                }

                var edgeHit =
                    Dist2ToSegment(center, tl, tr) <= r2 ||
                    Dist2ToSegment(center, tr, br) <= r2 ||
                    Dist2ToSegment(center, br, bl) <= r2 ||
                    Dist2ToSegment(center, bl, tl) <= r2;

                intersects = itl || itr || ibl || ibr || edgeHit;
            }

            int SnapToPassableIndex(WPos p, int searchRadPix)
            {
                var (gx0, gy0) = ClampToGrid(WorldToGrid(p));
                var bestIdx = GridToIndex(gx0, gy0);
                if (pxMaxG[bestIdx] >= 0f)
                {
                    return bestIdx;
                }
                for (var r = 1; r <= searchRadPix; ++r)
                {
                    var x1 = Math.Max(0, gx0 - r);
                    var y1 = Math.Max(0, gy0 - r);
                    var x2 = Math.Min(w - 1, gx0 + r);
                    var y2 = Math.Min(h - 1, gy0 + r);

                    for (var x = x1; x <= x2; ++x)
                    {
                        var ti = GridToIndex(x, y1);
                        if (pxMaxG[ti] >= 0f)
                        {
                            return ti;
                        }
                        ti = GridToIndex(x, y2);
                        if (pxMaxG[ti] >= 0f)
                        {
                            return ti;
                        }
                    }
                    for (var y = y1 + 1; y <= y2 - 1; ++y)
                    {
                        var ti = GridToIndex(x1, y);
                        if (pxMaxG[ti] >= 0f)
                        {
                            return ti;
                        }
                        ti = GridToIndex(x2, y);
                        if (pxMaxG[ti] >= 0f)
                        {
                            return ti;
                        }
                    }
                }
                return -1;
            }

            // Count + shadow for true entrance disks.
            void CountAndShadowForEntrance(WPos entrance, float radius)
            {
                // Use fractional grid center to avoid missing rows/cols when center is off the lattice.
                var cFrac = WorldToGridFrac(entrance);
                var radG = radius / Resolution;

                // Conservative AABB in grid space (+1 margin avoids FP/tangency drops)
                var xMin = Math.Max(0, (int)MathF.Floor(cFrac.X - radG) - 1);
                var xMax = Math.Min(w - 1, (int)MathF.Ceiling(cFrac.X + radG) + 1);
                var yMin = Math.Max(0, (int)MathF.Floor(cFrac.Y - radG) - 1);
                var yMax = Math.Min(h - 1, (int)MathF.Ceiling(cFrac.Y + radG) + 1);

                var r2 = radius * radius;

                for (var y = yMin; y <= yMax; ++y)
                {
                    var rowBase = y * w;
                    for (var x = xMin; x <= xMax; ++x)
                    {
                        var idx = rowBase + x;
                        if (pxMaxG[idx] < 0f) // blocked terrain → skip
                        {
                            continue;
                        }

                        // Exact world-space classification of the rotated cell.
                        ClassifyCell(x, y, entrance, r2, out var fullyInside, out var intersects);

                        if (intersects && !fullyInside)
                        {
                            _teleShadow[idx] = true;
                        }
                        if (fullyInside)
                        {
                            ++counts[idx]; // actual entrance seats
                        }
                    }
                }
            }

            void EmitEdgesForEntrance(WPos entrance, int destIndex, float useTime, float notBeforeG, float radius)
            {
                var cFrac = WorldToGridFrac(entrance);
                var radG = radius / Resolution;

                var xMin = Math.Max(0, (int)MathF.Floor(cFrac.X - radG) - 1);
                var xMax = Math.Min(w - 1, (int)MathF.Ceiling(cFrac.X + radG) + 1);
                var yMin = Math.Max(0, (int)MathF.Floor(cFrac.Y - radG) - 1);
                var yMax = Math.Min(h - 1, (int)MathF.Ceiling(cFrac.Y + radG) + 1);

                var r2 = radius * radius;

                for (var y = yMin; y <= yMax; ++y)
                {
                    var rowBase = y * w;
                    for (var x = xMin; x <= xMax; ++x)
                    {
                        var idx = rowBase + x;
                        if (pxMaxG[idx] < 0f)
                        {
                            continue;
                        }

                        ClassifyCell(x, y, entrance, r2, out var fullyInside, out _);
                        if (!fullyInside)
                        {
                            continue;
                        }

                        _teleEdges[cursors[idx]++] = new TeleEdge(destIndex, useTime, notBeforeG);
                    }
                }
            }

            // PASS 0: precompute per teleporter (counts + shadow marking)
            for (var i = 0; i < len; ++i)
            {
                ref readonly var tp = ref teleporters[i];
                var rp = Math.Max(1, (int)MathF.Ceiling(tp.Radius * radToPix));
                radPixArr[i] = rp;

                exitIdx[i] = SnapToPassableIndex(tp.Exit, rp);
                backIdx[i] = tp.Bidirectional ? SnapToPassableIndex(tp.Entrance, rp) : -1;

                // Always shadow entrance + exit
                CountAndShadowForEntrance(tp.Entrance, tp.Radius);
                if (tp.Bidirectional)
                {
                    CountAndShadowForEntrance(tp.Exit, tp.Radius); // counts + shadow (exit acts as entrance)
                }
            }

            // CSR prefix sums
            var total = 0;
            var offs = _teleEdgeOffsets;
            for (var i = 0; i < cellCount; ++i)
            {
                offs[i] = total;
                total += counts[i];
            }
            offs[cellCount] = total;

            _teleEdges = total > 0 ? new TeleEdge[total] : [];
            HasTeleporters = total > 0;
            if (total == 0)
            {
                return;
            }

            Array.Copy(offs, 0, cursors, 0, cellCount);

            // PASS 1: emit edges for true entrances
            for (var i = 0; i < len; ++i)
            {
                ref readonly var tp = ref teleporters[i];
                var rp = radPixArr[i];

                var ex = exitIdx[i];
                if (ex >= 0)
                {
                    EmitEdgesForEntrance(tp.Entrance, ex, tp.UseTime, tp.NotBeforeG, tp.Radius);
                }

                if (tp.Bidirectional)
                {
                    var bi = backIdx[i];
                    if (bi >= 0)
                    {
                        EmitEdgesForEntrance(tp.Exit, bi, tp.UseTime, tp.NotBeforeG, tp.Radius);
                    }
                }
            }
        }
        finally
        {
            ArrayPool<int>.Shared.Return(counts, false);
            ArrayPool<int>.Shared.Return(cursors, false);
            ArrayPool<int>.Shared.Return(exitIdx, false);
            ArrayPool<int>.Shared.Return(backIdx, false);
            ArrayPool<int>.Shared.Return(radPixArr, false);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasTeleEdges(int index)
    {
        // fast "is entrance cell?" check, avoids per-step range work
        var offs = _teleEdgeOffsets;
        // offs length is cellCount+1; entrance if range size > 0
        return offs.Length > 1 && (uint)index < (uint)(offs.Length - 1) && offs[index] != offs[index + 1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<TeleEdge> TeleEdgesForIndex(int index)
    {
        var off = _teleEdgeOffsets;
        if ((uint)index >= (uint)off.Length - 1)
        {
            return [];
        }
        var start = off[index];
        var count = off[index + 1] - start;
        return new(_teleEdges, start, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsTeleShadow(int index)
    {
        var s = _teleShadow;
        return (uint)index < (uint)s.Length && s[index];
    }
}
