using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BossMod;

[SkipLocalsInit]
internal sealed class PolygonBoundaryIndex2D
{
    private const float Eps = 1e-7f;

    private readonly struct E
    {
        public readonly float y0, y1; // inclusive bottom, exclusive top
        public readonly float x0; // x at y0
        public readonly float k; // (x1 - x0) / (y1 - y0)
        public readonly float minX, maxX; // for boundary hit

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public E(float ax, float ay, float bx, float by)
        {
            if (ay <= by)
            {
                y0 = ay;
                y1 = by;
                x0 = ax;
                k = (bx - ax) / Math.Max(by - ay, Eps);
            }
            else
            {
                y0 = by;
                y1 = ay;
                x0 = bx;
                k = (ax - bx) / Math.Max(ay - by, Eps);
            }

            minX = Math.Min(ax, bx);
            maxX = Math.Max(ax, bx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float XAt(float y) => x0 + k * (y - y0);
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly struct H(float ax, float ay, float bx)
    {
        public readonly float y = ay, minX = Math.Min(ax, bx), maxX = Math.Max(ax, bx);
    }

    // structure-of-arrays over all row-membership
    // all *_Row arrays are aligned to rows via _rowOffsets
    private readonly float[] _e_y0_Row;
    private readonly float[] _e_y1_Row;
    private readonly float[] _e_x0_Row;
    private readonly float[] _e_k_Row;
    private readonly float[] _e_minX_Row;
    private readonly float[] _e_maxX_Row;

    // horizontal edges per row
    private readonly H[] _hEdges;
    private readonly int[] _hRowOffsets; // rows+1
    private readonly int[] _hRowIdx; // flattened indices into _hEdges

    // row → [start,end) indices into *_Row arrays
    private readonly int[] _rowOffsets;  // rows+1

    // per-row conservative X bounds (for early-out)
    private readonly float[] _rowMinX;
    private readonly float[] _rowMaxX;

    private readonly int _rows;
    private readonly float _minY, _cellH, _invCellH;
    private readonly float _bbMinX, _bbMinY, _bbMaxX, _bbMaxY;

    private PolygonBoundaryIndex2D(float[] e_y0_Row, float[] e_y1_Row, float[] e_x0_Row, float[] e_k_Row, float[] e_minX_Row, float[] e_maxX_Row,
        int[] rowOffsets, H[] hEdges, int[] hRowOffsets, int[] hRowIdx, int rows, float minY, float cellH, float invCellH,
        float bbMinX, float bbMinY, float bbMaxX, float bbMaxY, float[] rowMinX, float[] rowMaxX)
    {
        _e_y0_Row = e_y0_Row;
        _e_y1_Row = e_y1_Row;
        _e_x0_Row = e_x0_Row;
        _e_k_Row = e_k_Row;
        _e_minX_Row = e_minX_Row;
        _e_maxX_Row = e_maxX_Row;

        _rowOffsets = rowOffsets;

        _hEdges = hEdges;
        _hRowOffsets = hRowOffsets;
        _hRowIdx = hRowIdx;

        _rows = rows;
        _minY = minY;
        _cellH = cellH;
        _invCellH = invCellH;

        _bbMinX = bbMinX;
        _bbMinY = bbMinY;
        _bbMaxX = bbMaxX;
        _bbMaxY = bbMaxY;

        _rowMinX = rowMinX;
        _rowMaxX = rowMaxX;
    }

    public static PolygonBoundaryIndex2D Build(RelSimplifiedComplexPolygon complex, int minRows = 32, int maxRows = 512)
    {
        // Collect edges and global bbox
        var parts = complex.Parts;
        var countP = parts.Count;

        float bbMinX = float.MaxValue, bbMinY = float.MaxValue;
        float bbMaxX = float.MinValue, bbMaxY = float.MinValue;

        var vertsCount = 0;
        for (var i = 0; i < countP; ++i)
        {
            vertsCount += parts[i].VerticesCount;
        }

        var eList = new List<E>(vertsCount);
        var hList = new List<H>(Math.Max(8, vertsCount / 8));

        for (var i = 0; i < countP; ++i)
        {
            var part = parts[i];

            var ext = part.ExteriorEdges;
            var lenExt = ext.Length;
            for (int j = 0, n = lenExt; j < n; ++j)
            {
                var (a, b) = ext[j];
                AccumBB(a, b, ref bbMinX, ref bbMinY, ref bbMaxX, ref bbMaxY);
                float ax = a.X, ay = a.Z, bx = b.X, by = b.Z;
                if (Math.Abs(ay - by) <= Eps)
                {
                    hList.Add(new H(ax, ay, bx));
                }
                else
                {
                    eList.Add(new E(ax, ay, bx, by));
                }
            }

            var holes = part.Holes;
            var lenHoles = holes.Length;
            for (int h = 0, nh = lenHoles; h < nh; ++h)
            {
                var ie = part.InteriorEdges(holes[h]);
                var lenIE = ie.Length;
                for (int j = 0, n = lenIE; j < n; ++j)
                {
                    var (a, b) = ie[j];
                    AccumBB(a, b, ref bbMinX, ref bbMinY, ref bbMaxX, ref bbMaxY);
                    float ax = a.X, ay = a.Z, bx = b.X, by = b.Z;
                    if (Math.Abs(ay - by) <= Eps)
                    {
                        hList.Add(new H(ax, ay, bx));
                    }
                    else
                    {
                        eList.Add(new E(ax, ay, bx, by));
                    }
                }
            }
        }

        var edges = CollectionsMarshal.AsSpan(eList);
        H[] hEdgesArr = [.. hList];

        // Rowing
        var lenEdges = edges.Length;
        var nEdges = Math.Max(lenEdges, 1);
        var rows = Math.Clamp((int)MathF.Round(MathF.Sqrt(nEdges) * 0.9f) + 8, minRows, maxRows);

        var height = Math.Max(bbMaxY - bbMinY, Eps);
        var cellH = height / rows;
        var invCellH = 1f / cellH;

        var counts = new int[rows];
        var hCounts = new int[rows];

        // non-horiz counts
        for (var idx = 0; idx < lenEdges; ++idx)
        {
            ref readonly var e = ref edges[idx];
            var y0 = e.y0;
            var y1 = MathF.BitDecrement(e.y1); // top-exclusive
            var r0 = (int)MathF.Floor((y0 - bbMinY) * invCellH);
            var r1 = (int)MathF.Floor((y1 - bbMinY) * invCellH);
            if (r0 < 0)
            {
                r0 = 0;
            }
            if (r1 >= rows)
            {
                r1 = rows - 1;
            }
            for (var r = r0; r <= r1; ++r)
            {
                ++counts[r];
            }
        }

        // horizontal counts
        var hEdges = hEdgesArr;
        var lenH = hEdges.Length;
        for (int idx = 0, hN = lenH; idx < hN; ++idx)
        {
            ref readonly var hEdge = ref hEdges[idx];
            var y = hEdge.y;
            var r = (int)MathF.Floor((y - bbMinY) * invCellH);
            if (r < 0)
            {
                r = 0;
            }
            else if (r >= rows)
            {
                r = rows - 1;
            }
            ++hCounts[r];
        }
        // decide SIMD pad width
        var padWidth = (Vector512.IsHardwareAccelerated && Avx512F.IsSupported) ? 16 : (Vector256.IsHardwareAccelerated && Avx.IsSupported) ? 8 : 1;

        // prefix sums with padding
        var rowOffsets = new int[rows + 1]; // padded offsets
        var paddedCounts = new int[rows];
        var total = 0;
        for (var r = 0; r < rows; ++r)
        {
            rowOffsets[r] = total;
            var c = counts[r];
            var pc = padWidth == 1 ? c : RoundUp(c, padWidth);
            paddedCounts[r] = pc;
            total += pc;
        }
        rowOffsets[rows] = total;

        // horizontals offsets (no padding needed)
        var hRowOffsets = new int[rows + 1];
        var hTotal = 0;
        for (var r = 0; r < rows; ++r)
        {
            hRowOffsets[r] = hTotal;
            hTotal += hCounts[r];
        }
        hRowOffsets[rows] = hTotal;

        // allocate SoA
        var e_y0_Row = new float[total];
        var e_y1_Row = new float[total];
        var e_x0_Row = new float[total];
        var e_k_Row = new float[total];
        var e_minX_Row = new float[total];
        var e_maxX_Row = new float[total];

        var hRowIdx = new int[hTotal];

        // fill rows
        var wpos = new int[rows];
        Array.Copy(rowOffsets, wpos, rows);

        for (var idx = 0; idx < lenEdges; ++idx)
        {
            ref readonly var e = ref edges[idx];
            var y0 = e.y0;
            var y1 = MathF.BitDecrement(e.y1);
            var r0 = (int)MathF.Floor((y0 - bbMinY) * invCellH);
            var r1 = (int)MathF.Floor((y1 - bbMinY) * invCellH);
            if (r0 < 0)
            {
                r0 = 0;
            }
            if (r1 >= rows)
            {
                r1 = rows - 1;
            }

            for (var r = r0; r <= r1; ++r)
            {
                var w = wpos[r]++;
                e_y0_Row[w] = e.y0;
                e_y1_Row[w] = e.y1;
                e_x0_Row[w] = e.x0;
                e_k_Row[w] = e.k;
                e_minX_Row[w] = e.minX;
                e_maxX_Row[w] = e.maxX;
            }
        }

        // fill rows (padding as NaN sentinels)
        for (var r = 0; r < rows; ++r)
        {
            var start = rowOffsets[r];
            var endActual = start + counts[r];
            var endPad = start + paddedCounts[r];
            for (var i = endActual; i < endPad; ++i)
            {
                e_y0_Row[i] = float.NaN; // makes span/den comparisons false
                e_y1_Row[i] = float.NaN;
                e_x0_Row[i] = float.NaN;
                e_k_Row[i] = 0f;
                e_minX_Row[i] = float.PositiveInfinity;
                e_maxX_Row[i] = float.NegativeInfinity;
            }
        }

        var hwpos = new int[rows];
        Array.Copy(hRowOffsets, hwpos, rows);
        for (int idx = 0, hN = lenH; idx < hN; ++idx)
        {
            ref readonly var hEdge = ref hEdges[idx];
            var y = hEdge.y;
            var r = (int)MathF.Floor((y - bbMinY) * invCellH);
            if (r < 0)
            {
                r = 0;
            }
            else if (r >= rows)
            {
                r = rows - 1;
            }
            hRowIdx[hwpos[r]++] = idx;
        }

        // per-row conservative X bounds (use actual counts only)
        var rowMinX = new float[rows];
        var rowMaxX = new float[rows];

        for (var r = 0; r < rows; ++r)
        {
            var rMinY = bbMinY + r * cellH;
            var rMaxY = rMinY + cellH;

            var rMinX = float.PositiveInfinity;
            var rMaxX = float.NegativeInfinity;

            var start = rowOffsets[r];
            var endActual = start + counts[r];
            for (var i = start; i < endActual; ++i)
            {
                var y0 = e_y0_Row[i];
                var y1 = e_y1_Row[i];
                var x0 = e_x0_Row[i];
                var k = e_k_Row[i];

                var ys = Math.Max(y0, rMinY);
                var ye = Math.Min(y1, rMaxY);
                ye = MathF.BitDecrement(ye);

                var xs = x0 + k * (ys - y0);
                var xe = x0 + k * (ye - y0);

                var lo = xs < xe ? xs : xe;
                var hi = xs > xe ? xs : xe;

                if (lo < rMinX)
                {
                    rMinX = lo;
                }
                if (hi > rMaxX)
                {
                    rMaxX = hi;
                }
            }

            int hs = hRowOffsets[r], he = hRowOffsets[r + 1];
            for (var i = hs; i < he; ++i)
            {
                ref readonly var h = ref hEdges[hRowIdx[i]];
                if (h.minX < rMinX)
                {
                    rMinX = h.minX;
                }
                if (h.maxX > rMaxX)
                {
                    rMaxX = h.maxX;
                }
            }

            rowMinX[r] = rMinX;
            rowMaxX[r] = rMaxX;
        }

        return new PolygonBoundaryIndex2D(e_y0_Row, e_y1_Row, e_x0_Row, e_k_Row, e_minX_Row, e_maxX_Row,
            rowOffsets, hEdges, hRowOffsets, hRowIdx, rows, bbMinY, cellH, invCellH, bbMinX, bbMinY, bbMaxX, bbMaxY, rowMinX, rowMaxX);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int RoundUp(int v, int m) => (v + (m - 1)) / m * m;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void AccumBB(in WDir a, in WDir b, ref float minX, ref float minY, ref float maxX, ref float maxY)
        {
            float ax = a.X, ay = a.Z, bx = b.X, by = b.Z;
            if (ax < minX)
            {
                minX = ax;
            }
            if (ay < minY)
            {
                minY = ay;
            }
            if (ax > maxX)
            {
                maxX = ax;
            }
            if (ay > maxY)
            {
                maxY = ay;
            }
            if (bx < minX)
            {
                minX = bx;
            }
            if (by < minY)
            {
                minY = by;
            }
            if (bx > maxX)
            {
                maxX = bx;
            }
            if (by > maxY)
            {
                maxY = by;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ClampRow(float y)
    {
        var r = (int)MathF.Floor((y - _minY) * _invCellH);
        return r < 0 ? 0 : r >= _rows ? _rows - 1 : r;
    }

    public bool Contains(in WDir p)
    {
        var px = p.X;
        var py = p.Z;

        if (px < _bbMinX - Eps || px > _bbMaxX + Eps || py < _bbMinY - Eps || py > _bbMaxY + Eps)
        {
            return false;
        }

        var row = ClampRow(py);

        int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
        for (var i = hs; i < he; ++i)
        {
            ref readonly var h = ref _hEdges[_hRowIdx[i]];
            if (Math.Abs(py - h.y) <= Eps && px >= h.minX - Eps && px <= h.maxX + Eps)
            {
                return true;
            }
        }

        if (px < _rowMinX[row] - Eps || px > _rowMaxX[row] + Eps)
        {
            return false;
        }

        int es = _rowOffsets[row], ee = _rowOffsets[row + 1];
        if (ee - es == 0)
        {
            return false;
        }

        var parity = 0;

        // AVX512F path: unrolled ×2 (32 lanes/iter)
        if (Vector512.IsHardwareAccelerated && Avx512F.IsSupported)
        {
            var v_py = Vector512.Create(py);
            var v_px = Vector512.Create(px);
            var v_eps2 = Vector512.Create(Eps * Eps);

            var i0 = es;

            for (; i0 + 32 <= ee; i0 += 32)
            {
                // block 0
                var y0_0 = Load512(_e_y0_Row, i0);
                var y1_0 = Load512(_e_y1_Row, i0);
                var x0_0 = Load512(_e_x0_Row, i0);
                var k_0 = Load512(_e_k_Row, i0);
                var mn_0 = Load512(_e_minX_Row, i0);
                var mx_0 = Load512(_e_maxX_Row, i0);

                var span0 = Vector512.GreaterThanOrEqual(v_py, y0_0) & Vector512.LessThan(v_py, y1_0);
                var x0v = x0_0 + k_0 * (v_py - y0_0);
                var dx0 = v_px - x0v;
                var near0 = Vector512.LessThanOrEqual(dx0 * dx0, v_eps2);
                var onB0 = span0 & near0 & Vector512.GreaterThanOrEqual(v_px, mn_0) & Vector512.LessThanOrEqual(v_px, mx_0);

                // block 1
                var b = i0 + 16;
                var y0_1 = Load512(_e_y0_Row, b);
                var y1_1 = Load512(_e_y1_Row, b);
                var x0_1 = Load512(_e_x0_Row, b);
                var k_1 = Load512(_e_k_Row, b);
                var mn_1 = Load512(_e_minX_Row, b);
                var mx_1 = Load512(_e_maxX_Row, b);

                var span1 = Vector512.GreaterThanOrEqual(v_py, y0_1) & Vector512.LessThan(v_py, y1_1);
                var x1v = x0_1 + k_1 * (v_py - y0_1);
                var dx1 = v_px - x1v;
                var near1 = Vector512.LessThanOrEqual(dx1 * dx1, v_eps2);
                var onB1 = span1 & near1 & Vector512.GreaterThanOrEqual(v_px, mn_1) & Vector512.LessThanOrEqual(v_px, mx_1);

                // boundary early-out
                if ((onB0.ExtractMostSignificantBits() | onB1.ExtractMostSignificantBits()) != default)
                {
                    return true;
                }

                // parity from both blocks
                var gt0 = (Vector512.GreaterThan(x0v, v_px) & span0).ExtractMostSignificantBits();
                var gt1 = (Vector512.GreaterThan(x1v, v_px) & span1).ExtractMostSignificantBits();
                parity ^= (BitOperations.PopCount((uint)gt0) + BitOperations.PopCount((uint)gt1)) & 1;
            }

            // final 16 if any (rows are padded to 16; this runs when length % 32 == 16)
            for (; i0 + 16 <= ee; i0 += 16)
            {
                var y0 = Load512(_e_y0_Row, i0);
                var y1 = Load512(_e_y1_Row, i0);
                var x0 = Load512(_e_x0_Row, i0);
                var k = Load512(_e_k_Row, i0);
                var mn = Load512(_e_minX_Row, i0);
                var mx = Load512(_e_maxX_Row, i0);

                var span = Vector512.GreaterThanOrEqual(v_py, y0) & Vector512.LessThan(v_py, y1);
                var x = x0 + k * (v_py - y0);
                var dx = v_px - x;
                var near = Vector512.LessThanOrEqual(dx * dx, v_eps2);
                var onB = span & near & Vector512.GreaterThanOrEqual(v_px, mn) & Vector512.LessThanOrEqual(v_px, mx);
                if (onB.ExtractMostSignificantBits() != default)
                {
                    return true;
                }
                var gt = Vector512.GreaterThan(x, v_px) & span;
                parity ^= BitOperations.PopCount(gt.ExtractMostSignificantBits()) & 1;
            }

            return (parity & 1) != 0;
            static Vector512<float> Load512(float[] arr, int index)
            {
                ref var r0 = ref MemoryMarshal.GetArrayDataReference(arr);
                return Vector512.LoadUnsafe(ref r0, (nuint)index);
            }
        }

        // AVX2 path (2×8 unrolled, fully padded)
        if (Vector256.IsHardwareAccelerated && Avx.IsSupported)
        {
            var v_py = Vector256.Create(py);
            var v_px = Vector256.Create(px);
            var v_eps2 = Vector256.Create(Eps * Eps);

            var i0 = es;
            for (; i0 + 16 <= ee; i0 += 16)
            {
                // block 0
                {
                    var y0 = Load256(_e_y0_Row, i0);
                    var y1 = Load256(_e_y1_Row, i0);
                    var x0 = Load256(_e_x0_Row, i0);
                    var k = Load256(_e_k_Row, i0);
                    var mn = Load256(_e_minX_Row, i0);
                    var mx = Load256(_e_maxX_Row, i0);

                    var span = Vector256.BitwiseAnd(Vector256.GreaterThanOrEqual(v_py, y0), Vector256.LessThan(v_py, y1));

                    var x = Avx.Add(x0, Avx.Multiply(k, Avx.Subtract(v_py, y0)));
                    var dx = Avx.Subtract(v_px, x);
                    var near = Vector256.LessThanOrEqual(Avx.Multiply(dx, dx), v_eps2);
                    var onB = Vector256.BitwiseAnd(Vector256.BitwiseAnd(span, near),
                               Vector256.BitwiseAnd(Vector256.GreaterThanOrEqual(v_px, mn), Vector256.LessThanOrEqual(v_px, mx)));
                    if (onB.ExtractMostSignificantBits() != default)
                    {
                        return true;
                    }

                    var gt = Vector256.BitwiseAnd(Vector256.GreaterThan(x, v_px), span);
                    parity ^= BitOperations.PopCount(gt.ExtractMostSignificantBits()) & 1;
                }
                // block 1
                {
                    var b = i0 + 8;
                    var y0 = Load256(_e_y0_Row, b);
                    var y1 = Load256(_e_y1_Row, b);
                    var x0 = Load256(_e_x0_Row, b);
                    var k = Load256(_e_k_Row, b);
                    var mn = Load256(_e_minX_Row, b);
                    var mx = Load256(_e_maxX_Row, b);

                    var span = Vector256.BitwiseAnd(Vector256.GreaterThanOrEqual(v_py, y0), Vector256.LessThan(v_py, y1));

                    var x = Avx.Add(x0, Avx.Multiply(k, Avx.Subtract(v_py, y0)));
                    var dx = Avx.Subtract(v_px, x);
                    var near = Vector256.LessThanOrEqual(Avx.Multiply(dx, dx), v_eps2);
                    var onB = Vector256.BitwiseAnd(Vector256.BitwiseAnd(span, near),
                               Vector256.BitwiseAnd(Vector256.GreaterThanOrEqual(v_px, mn), Vector256.LessThanOrEqual(v_px, mx)));
                    if (onB.ExtractMostSignificantBits() != default)
                    {
                        return true;
                    }

                    var gt = Vector256.BitwiseAnd(Vector256.GreaterThan(x, v_px), span);
                    parity ^= BitOperations.PopCount(gt.ExtractMostSignificantBits()) & 1;
                }
            }
            // final 8 if any (rows are padded to 16; this runs when length % 16 == 8)
            for (; i0 + 8 <= ee; i0 += 8)
            {
                var y0 = Load256(_e_y0_Row, i0);
                var y1 = Load256(_e_y1_Row, i0);
                var x0 = Load256(_e_x0_Row, i0);
                var k = Load256(_e_k_Row, i0);
                var mn = Load256(_e_minX_Row, i0);
                var mx = Load256(_e_maxX_Row, i0);

                var span = Vector256.BitwiseAnd(Vector256.GreaterThanOrEqual(v_py, y0), Vector256.LessThan(v_py, y1));

                var x = Avx.Add(x0, Avx.Multiply(k, Avx.Subtract(v_py, y0)));
                var dx = Avx.Subtract(v_px, x);
                var near = Vector256.LessThanOrEqual(Avx.Multiply(dx, dx), v_eps2);
                var onB = Vector256.BitwiseAnd(Vector256.BitwiseAnd(span, near),
                           Vector256.BitwiseAnd(Vector256.GreaterThanOrEqual(v_px, mn), Vector256.LessThanOrEqual(v_px, mx)));
                if (onB.ExtractMostSignificantBits() != default)
                {
                    return true;
                }

                var gt = Vector256.BitwiseAnd(Vector256.GreaterThan(x, v_px), span);
                parity ^= BitOperations.PopCount(gt.ExtractMostSignificantBits()) & 1;
            }
            return (parity & 1) != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Vector256<float> Load256(float[] arr, int index)
            {
                ref var r0 = ref MemoryMarshal.GetArrayDataReference(arr);
                return Vector256.LoadUnsafe(ref r0, (nuint)index);
            }
        }

        // Vector<T>
        {
            var i0 = es;
            var lanes = Vector<float>.Count;
            var v_py = new Vector<float>(py);
            var v_px = new Vector<float>(px);
            var v_eps = new Vector<float>(Eps);

            for (; i0 + lanes <= ee; i0 += lanes)
            {
                var y0 = LoadVec(_e_y0_Row, i0);
                var y1 = LoadVec(_e_y1_Row, i0);
                var x0 = LoadVec(_e_x0_Row, i0);
                var k = LoadVec(_e_k_Row, i0);
                var mn = LoadVec(_e_minX_Row, i0);
                var mx = LoadVec(_e_maxX_Row, i0);

                var ge_y0 = Vector.GreaterThanOrEqual(v_py, y0);
                var lt_y1 = Vector.LessThan(v_py, y1);
                var span = ge_y0 & lt_y1;

                var x = x0 + k * (v_py - y0);
                var dx = Vector.Abs(v_px - x);
                var near = Vector.LessThanOrEqual(dx, v_eps);
                var ge_mn = Vector.GreaterThanOrEqual(v_px, mn);
                var le_mx = Vector.LessThanOrEqual(v_px, mx);
                var onB = span & near & ge_mn & le_mx;

                if (!Vector.EqualsAll(onB, default))
                {
                    return true;
                }

                var gt = Vector.GreaterThan(x, v_px) & span;
                parity ^= Parity(gt);
            }

            for (; i0 < ee; ++i0)
            {
                var y0s = _e_y0_Row[i0];
                if (py < y0s - Eps)
                {
                    continue;
                }
                var y1s = _e_y1_Row[i0];
                if (py >= y1s - Eps)
                {
                    continue;
                }
                var x = _e_x0_Row[i0] + _e_k_Row[i0] * (py - y0s);

                if (Math.Abs(px - x) <= Eps && px >= _e_minX_Row[i0] - Eps && px <= _e_maxX_Row[i0] + Eps)
                {
                    return true;
                }

                if (x > px)
                {
                    parity ^= 1;
                }
            }

            return (parity & 1) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector<float> LoadVec(float[] src, int index) => new(src, index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int Parity(Vector<int> mask)
        {
            // XOR signbits for parity
            switch (Vector<int>.Count)
            {
                case 4: return ((mask[0] >> 31) ^ (mask[1] >> 31) ^ (mask[2] >> 31) ^ (mask[3] >> 31)) & 1;
                case 8:
                    return ((mask[0] >> 31) ^ (mask[1] >> 31) ^ (mask[2] >> 31) ^ (mask[3] >> 31)
                          ^ (mask[4] >> 31) ^ (mask[5] >> 31) ^ (mask[6] >> 31) ^ (mask[7] >> 31)) & 1;
                case 16:
                    return ((mask[0] >> 31) ^ (mask[1] >> 31) ^ (mask[2] >> 31) ^ (mask[3] >> 31)
                          ^ (mask[4] >> 31) ^ (mask[5] >> 31) ^ (mask[6] >> 31) ^ (mask[7] >> 31)
                          ^ (mask[8] >> 31) ^ (mask[9] >> 31) ^ (mask[10] >> 31) ^ (mask[11] >> 31)
                          ^ (mask[12] >> 31) ^ (mask[13] >> 31) ^ (mask[14] >> 31) ^ (mask[15] >> 31)) & 1;
                default:
                    var p = 0;
                    for (var i = 0; i < Vector<int>.Count; ++i)
                    {
                        p ^= (mask[i] >> 31) & 1;
                    }
                    return p;
            }
        }
    }

    public float Raycast(in WDir o, in WDir d)
    {
        float ox = o.X, oz = o.Z;
        float dx = d.X, dz = d.Z;

        // AABB slab prune
        float tmin = -float.Epsilon, tmax = float.MaxValue;
        if (Math.Abs(dx) > Eps)
        {
            var inv = 1f / dx;
            var tx1 = (_bbMinX - ox) * inv;
            var tx2 = (_bbMaxX - ox) * inv;
            if (tx1 > tx2)
            {
                (tx1, tx2) = (tx2, tx1);
            }
            tmin = Math.Max(tmin, tx1);
            tmax = Math.Min(tmax, tx2);
        }
        else if (ox < _bbMinX - Eps || ox > _bbMaxX + Eps)
        {
            return float.MaxValue;
        }

        if (Math.Abs(dz) > Eps)
        {
            var inv = 1f / dz;
            var ty1 = (_bbMinY - oz) * inv;
            var ty2 = (_bbMaxY - oz) * inv;
            if (ty1 > ty2)
            {
                (ty1, ty2) = (ty2, ty1);
            }
            tmin = Math.Max(tmin, ty1);
            tmax = Math.Min(tmax, ty2);
        }
        else if (oz < _bbMinY - Eps || oz > _bbMaxY + Eps)
        {
            return float.MaxValue;
        }

        if (tmax < 0f || tmin > tmax)
        {
            return float.MaxValue;
        }

        // Horizontal ray fast path
        if (Math.Abs(dz) <= Eps)
        {
            if (Math.Abs(dx) <= Eps)
            {
                return float.MaxValue;
            }

            var row = ClampRow(oz);
            var best = float.MaxValue;

            int es = _rowOffsets[row], ee = _rowOffsets[row + 1];
            for (var i = es; i < ee; ++i)
            {
                float y0 = _e_y0_Row[i], y1 = _e_y1_Row[i];
                if (oz < y0 - Eps || oz >= y1 - Eps)
                {
                    continue;
                }

                var x = _e_x0_Row[i] + _e_k_Row[i] * (oz - y0);
                var t = (x - ox) / dx;
                if (t >= 0f && t < best && x >= _e_minX_Row[i] - Eps && x <= _e_maxX_Row[i] + Eps)
                {
                    best = t;
                }
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            for (var k = hs; k < he; ++k)
            {
                ref readonly var h = ref _hEdges[_hRowIdx[k]];
                if (Math.Abs(oz - h.y) > Eps)
                {
                    continue;
                }

                if (dx > 0f)
                {
                    var x0 = ox <= h.minX ? h.minX : (ox <= h.maxX ? ox : float.PositiveInfinity);
                    var t = (x0 - ox) / dx;
                    if (t >= 0f && t < best)
                    {
                        best = t;
                    }
                }
                else
                {
                    var x0 = ox >= h.maxX ? h.maxX : (ox >= h.minX ? ox : float.NegativeInfinity);
                    var t = (x0 - ox) / dx;
                    if (t >= 0f && t < best)
                    {
                        best = t;
                    }
                }
            }
            return best;
        }

        var bestT = float.MaxValue;
        var rowCur = ClampRow(oz);
        var step = dz > 0f ? +1 : -1;
        var nextY = (dz > 0f) ? (_minY + (rowCur + 1) * _cellH) : (_minY + rowCur * _cellH);

        // SIMD kernels for non-horizontal segments in a row
        // AVX512 kernel (unrolled ×2)
        static void KernelSIMD512(float[] y0Arr, float[] y1Arr, float[] x0Arr, float[] kArr,
            int es, int ee, float ox, float oz, float dx, float dz, ref float bestT)
        {
            var v_dx = Vector512.Create(dx);
            var v_dz = Vector512.Create(dz);
            var v_ox = Vector512.Create(ox);
            var v_oz = Vector512.Create(oz);
            var v_inf = Vector512.Create(float.PositiveInfinity);
            var v_tiny = Vector512.Create(1e-9f);
            var v_one = Vector512.Create(1f);
            Vector512<float> v_zero = default;

            Span<float> buf = stackalloc float[16];

            var i = es;
            for (; i + 32 <= ee; i += 32)
            {
                // block 0
                {
                    var y0 = Load512(y0Arr, i);
                    var y1 = Load512(y1Arr, i);
                    var x0 = Load512(x0Arr, i);
                    var k = Load512(kArr, i);

                    var dy = y1 - y0;
                    var dxE = k * dy;
                    var wox = x0 - v_ox;
                    var woz = y0 - v_oz;

                    var den = v_dx * dy - v_dz * dxE;
                    var validDen = Vector512.GreaterThan(Vector512.Abs(den), v_tiny);

                    var t = (wox * dy - woz * dxE) / den;
                    var u = (wox * v_dz - woz * v_dx) / den;

                    var valid = validDen
                             & Vector512.GreaterThanOrEqual(t, v_zero)
                             & Vector512.GreaterThanOrEqual(u, v_zero)
                             & Vector512.LessThan(u, v_one);

                    var tCand = Vector512.ConditionalSelect(valid, t, v_inf);

                    var v_best = Vector512.Create(bestT);
                    var anyBetter = Vector512.LessThan(tCand, v_best).ExtractMostSignificantBits();
                    if (anyBetter != default)
                    {
                        tCand.CopyTo(buf);
                        for (var m = 0; m < 16; ++m)
                        {
                            var tv = buf[m];
                            if (tv < bestT)
                            {
                                bestT = tv;
                            }
                        }
                    }
                }

                // block 1
                {
                    var b = i + 16;
                    var y0 = Load512(y0Arr, b);
                    var y1 = Load512(y1Arr, b);
                    var x0 = Load512(x0Arr, b);
                    var k = Load512(kArr, b);

                    var dy = y1 - y0;
                    var dxE = k * dy;
                    var wox = x0 - v_ox;
                    var woz = y0 - v_oz;

                    var den = v_dx * dy - v_dz * dxE;
                    var validDen = Vector512.GreaterThan(Vector512.Abs(den), v_tiny);

                    var t = (wox * dy - woz * dxE) / den;
                    var u = (wox * v_dz - woz * v_dx) / den;

                    var valid = validDen
                             & Vector512.GreaterThanOrEqual(t, v_zero)
                             & Vector512.GreaterThanOrEqual(u, v_zero)
                             & Vector512.LessThan(u, v_one);

                    var tCand = Vector512.ConditionalSelect(valid, t, v_inf);

                    var v_best = Vector512.Create(bestT);
                    var anyBetter = Vector512.LessThan(tCand, v_best).ExtractMostSignificantBits();
                    if (anyBetter != default)
                    {
                        tCand.CopyTo(buf);
                        for (var m = 0; m < 16; ++m)
                        {
                            var tv = buf[m];
                            if (tv < bestT)
                            {
                                bestT = tv;
                            }
                        }
                    }
                }
            }

            // final 16 if any
            for (; i + 16 <= ee; i += 16)
            {
                var y0 = Load512(y0Arr, i);
                var y1 = Load512(y1Arr, i);
                var x0 = Load512(x0Arr, i);
                var k = Load512(kArr, i);

                var dy = y1 - y0;
                var dxE = k * dy;
                var wox = x0 - v_ox;
                var woz = y0 - v_oz;

                var den = v_dx * dy - v_dz * dxE;
                var validDen = Vector512.GreaterThan(Vector512.Abs(den), v_tiny);

                var t = (wox * dy - woz * dxE) / den;
                var u = (wox * v_dz - woz * v_dx) / den;

                Vector512<float> v_zero2 = default;
                var valid = validDen
                         & Vector512.GreaterThanOrEqual(t, v_zero2)
                         & Vector512.GreaterThanOrEqual(u, v_zero2)
                         & Vector512.LessThan(u, v_one);

                var tCand = Vector512.ConditionalSelect(valid, t, v_inf);

                var v_best = Vector512.Create(bestT);
                var anyBetter = Vector512.LessThan(tCand, v_best).ExtractMostSignificantBits();
                if (anyBetter != default)
                {
                    tCand.CopyTo(buf);
                    for (var m = 0; m < 16; ++m)
                    {
                        var tv = buf[m];
                        if (tv < bestT)
                        {
                            bestT = tv;
                        }
                    }
                }
            }
            static Vector512<float> Load512(float[] arr, int index)
            {
                ref var r0 = ref MemoryMarshal.GetArrayDataReference(arr);
                return Vector512.LoadUnsafe(ref r0, (nuint)index);
            }
        }

        // AVX2 kernel (unrolled ×2)
        static void KernelSIMD256(float[] y0Arr, float[] y1Arr, float[] x0Arr, float[] kArr,
            int es, int ee, float ox, float oz, float dx, float dz, ref float bestT)
        {
            var v_dx = Vector256.Create(dx);
            var v_dz = Vector256.Create(dz);
            var v_ox = Vector256.Create(ox);
            var v_oz = Vector256.Create(oz);
            var v_inf = Vector256.Create(float.PositiveInfinity);
            var v_tiny = Vector256.Create(1e-9f);
            var v_one = Vector256.Create(1f);
            Vector256<float> v_zero = default;

            Span<float> buf = stackalloc float[8];

            var i = es;
            for (; i + 16 <= ee; i += 16)
            {
                // block 0
                {
                    var y0 = Load256(y0Arr, i);
                    var y1 = Load256(y1Arr, i);
                    var x0 = Load256(x0Arr, i);
                    var k = Load256(kArr, i);

                    var dy = y1 - y0;
                    var dxE = k * dy;
                    var wox = x0 - v_ox;
                    var woz = y0 - v_oz;

                    var den = v_dx * dy - v_dz * dxE;
                    var validDen = Vector256.GreaterThan(Vector256.Abs(den), v_tiny);

                    var t = (wox * dy - woz * dxE) / den;
                    var u = (wox * v_dz - woz * v_dx) / den;

                    var valid = validDen
                             & Vector256.GreaterThanOrEqual(t, v_zero)
                             & Vector256.GreaterThanOrEqual(u, v_zero)
                             & Vector256.LessThan(u, v_one);

                    var tCand = Vector256.ConditionalSelect(valid, t, v_inf);

                    var v_best = Vector256.Create(bestT);
                    var anyBetter = Vector256.LessThan(tCand, v_best).ExtractMostSignificantBits();
                    if (anyBetter != default)
                    {
                        tCand.CopyTo(buf);
                        for (var m = 0; m < 8; ++m)
                        {
                            var tv = buf[m];
                            if (tv < bestT)
                            {
                                bestT = tv;
                            }
                        }
                    }
                }
                // block 1
                {
                    var b = i + 8;
                    var y0 = Load256(y0Arr, b);
                    var y1 = Load256(y1Arr, b);
                    var x0 = Load256(x0Arr, b);
                    var k = Load256(kArr, b);

                    var dy = y1 - y0;
                    var dxE = k * dy;
                    var wox = x0 - v_ox;
                    var woz = y0 - v_oz;

                    var den = v_dx * dy - v_dz * dxE;
                    var validDen = Vector256.GreaterThan(Vector256.Abs(den), v_tiny);

                    var t = (wox * dy - woz * dxE) / den;
                    var u = (wox * v_dz - woz * v_dx) / den;

                    var valid = validDen
                             & Vector256.GreaterThanOrEqual(t, v_zero)
                             & Vector256.GreaterThanOrEqual(u, v_zero)
                             & Vector256.LessThan(u, v_one);

                    var tCand = Vector256.ConditionalSelect(valid, t, v_inf);

                    var v_best = Vector256.Create(bestT);
                    var anyBetter = Vector256.LessThan(tCand, v_best).ExtractMostSignificantBits();
                    if (anyBetter != default)
                    {
                        tCand.CopyTo(buf);
                        for (var m = 0; m < 8; ++m)
                        {
                            var tv = buf[m];
                            if (tv < bestT)
                            {
                                bestT = tv;
                            }
                        }
                    }
                }
            }

            for (; i + 8 <= ee; i += 8)
            {
                var y0 = Load256(y0Arr, i);
                var y1 = Load256(y1Arr, i);
                var x0 = Load256(x0Arr, i);
                var k = Load256(kArr, i);

                var dy = y1 - y0;
                var dxE = k * dy;
                var wox = x0 - v_ox;
                var woz = y0 - v_oz;

                var den = v_dx * dy - v_dz * dxE;
                var validDen = Vector256.GreaterThan(Vector256.Abs(den), v_tiny);

                var t = (wox * dy - woz * dxE) / den;
                var u = (wox * v_dz - woz * v_dx) / den;

                var valid = validDen
                         & Vector256.GreaterThanOrEqual(t, v_zero)
                         & Vector256.GreaterThanOrEqual(u, v_zero)
                         & Vector256.LessThan(u, v_one);

                var tCand = Vector256.ConditionalSelect(valid, t, v_inf);

                var v_best = Vector256.Create(bestT);
                var anyBetter = Vector256.LessThan(tCand, v_best).ExtractMostSignificantBits();
                if (anyBetter != default)
                {
                    tCand.CopyTo(buf);
                    for (var m = 0; m < 8; ++m)
                    {
                        var tv = buf[m];
                        if (tv < bestT)
                        {
                            bestT = tv;
                        }
                    }
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Vector256<float> Load256(float[] arr, int index)
            {
                ref var r0 = ref MemoryMarshal.GetArrayDataReference(arr);
                return Vector256.LoadUnsafe(ref r0, (nuint)index);
            }
        }

        // Scalar kernel (fallback when no AVX available)
        static void KernelScalar(float[] y0Arr, float[] y1Arr, float[] x0Arr, float[] kArr, int es, int ee,
            float ox, float oz, float dx, float dz, ref float bestT)
        {
            for (var i = es; i < ee; ++i)
            {
                float y0s = y0Arr[i], y1s = y1Arr[i], x0s = x0Arr[i];
                float eys = y1s - y0s, exs = kArr[i] * eys;

                float woxs = x0s - ox, wozs = y0s - oz;
                var den = dx * eys - dz * exs;

                if (Math.Abs(den) > 1e-9f)
                {
                    var t = (woxs * eys - wozs * exs) / den;
                    if (t < 0f || t >= bestT)
                    {
                        continue;
                    }

                    var u = (woxs * dz - wozs * dx) / den;
                    if (u is < 0f or >= (1f - 1e-6f))
                    {
                        continue;
                    }

                    bestT = t;
                }
                else
                {
                    var col = woxs * dz - wozs * dx;
                    if (Math.Abs(col) <= Eps)
                    {
                        var iddd = 1f / (dx * dx + dz * dz + 1e-20f);
                        var tA = (woxs * dx + wozs * dz) * iddd;
                        var tB = ((x0s + exs - ox) * dx + (y0s + eys - oz) * dz) * iddd;

                        if (tA > tB)
                        {
                            (tA, tB) = (tB, tA);
                        }
                        var cand = tA >= 0f ? tA : (tB >= 0f ? tB : float.MaxValue);
                        if (cand < bestT)
                        {
                            bestT = cand;
                        }
                    }
                }
            }
        }

        // DDA over rows; SIMD handles full width, thanks to padding
        while ((uint)rowCur < (uint)_rows)
        {
            var tBoundary = (nextY - oz) / dz;
            if (bestT <= tBoundary + 1e-6f)
            {
                break; // small bias avoids row-oscillation
            }

            int es = _rowOffsets[rowCur], ee = _rowOffsets[rowCur + 1];

            if (Vector512.IsHardwareAccelerated && Avx512F.IsSupported)
            {
                KernelSIMD512(_e_y0_Row, _e_y1_Row, _e_x0_Row, _e_k_Row, es, ee, ox, oz, dx, dz, ref bestT);
            }
            else if (Vector256.IsHardwareAccelerated && Avx.IsSupported)
            {
                KernelSIMD256(_e_y0_Row, _e_y1_Row, _e_x0_Row, _e_k_Row, es, ee, ox, oz, dx, dz, ref bestT);
            }
            else
            {
                KernelScalar(_e_y0_Row, _e_y1_Row, _e_x0_Row, _e_k_Row, es, ee, ox, oz, dx, dz, ref bestT);
            }

            // no scalar tail needed; rows are padded to SIMD width

            // horizontals (scalar)
            int hs = _hRowOffsets[rowCur], he = _hRowOffsets[rowCur + 1];
            for (var k = hs; k < he; ++k)
            {
                ref readonly var h = ref _hEdges[_hRowIdx[k]];
                var eHx = h.maxX - h.minX;

                var denH = -dz * eHx;
                float woxH = h.minX - ox, wozH = h.y - oz;

                if (Math.Abs(denH) > 1e-9f)
                {
                    var t = (0f - wozH * eHx) / denH;
                    if (t >= 0f && t < bestT)
                    {
                        var u = (woxH * dz - wozH * dx) / denH;
                        if (u is >= (-1e-6f) and <= (1f - 1e-6f))
                        {
                            bestT = t;
                        }
                    }
                }
                else
                {
                    if (Math.Abs(wozH * dx - woxH * dz) <= Eps)
                    {
                        var iddd = 1f / (dx * dx + dz * dz + 1e-20f);
                        var tA = (woxH * dx + wozH * dz) * iddd;
                        var tB = ((h.maxX - ox) * dx + (h.y - oz) * dz) * iddd;
                        if (tA > tB)
                        {
                            (tA, tB) = (tB, tA);
                        }
                        var cand = tA >= 0f ? tA : (tB >= 0f ? tB : float.MaxValue);
                        if (cand < bestT)
                        {
                            bestT = cand;
                        }
                    }
                }
            }

            rowCur += step;
            nextY += step * _cellH;
        }

        return bestT;
    }

    public WDir ClosestPointOnBoundary(in WDir p)
    {
        float px = p.X, py = p.Z;

        var row0 = ClampRow(py);
        int rNeg = row0, rPos = row0 + 1;

        var bestSq = float.PositiveInfinity;
        float bestX = px, bestY = py;

        var cellH = 1f / _invCellH;

        // SIMD kernels for projection onto segments in a row
        static void KernelClosest512(float[] y0Arr, float[] y1Arr, float[] x0Arr, float[] kArr,
            int es, int ee, float px, float py, ref float bestSq, ref float bestX, ref float bestY)
        {
            var v_px = Vector512.Create(px);
            var v_py = Vector512.Create(py);
            var v_one = Vector512.Create(1f);
            var v_tiny = Vector512.Create(1e-12f);

            Span<float> bD = stackalloc float[16];
            Span<float> bX = stackalloc float[16];
            Span<float> bY = stackalloc float[16];

            var i = es;
            for (; i + 32 <= ee; i += 32)
            {
                // block 0
                {
                    var y0 = Load512(y0Arr, i);
                    var y1 = Load512(y1Arr, i);
                    var x0 = Load512(x0Arr, i);
                    var k = Load512(kArr, i);

                    var dy = y1 - y0;
                    var dx = k * dy;
                    var relx = v_px - x0;
                    var rely = v_py - y0;

                    var len2 = dx * dx + dy * dy;
                    var valid = Vector512.GreaterThan(len2, v_tiny); // also false for NaN

                    var t = (relx * dx + rely * dy) / len2;
                    Vector512<float> vz = default;
                    t = Vector512.Min(Vector512.Max(t, vz), v_one);

                    var nx = x0 + t * dx;
                    var ny = y0 + t * dy;

                    var dxp = nx - v_px;
                    var dyp = ny - v_py;
                    var dist2 = dxp * dxp + dyp * dyp;

                    // only if any lane beats best
                    var anyBetter = Vector512.LessThan(dist2, Vector512.Create(bestSq)).ExtractMostSignificantBits();
                    if (anyBetter != default)
                    {
                        dist2.CopyTo(bD);
                        nx.CopyTo(bX);
                        ny.CopyTo(bY);
                        for (var m = 0; m < 16; ++m)
                        {
                            var d2 = bD[m];
                            if (d2 < bestSq)
                            {
                                bestSq = d2;
                                bestX = bX[m];
                                bestY = bY[m];
                            }
                        }
                    }
                }
                // block 1
                {
                    var b = i + 16;
                    var y0 = Load512(y0Arr, b);
                    var y1 = Load512(y1Arr, b);
                    var x0 = Load512(x0Arr, b);
                    var k = Load512(kArr, b);

                    var dy = y1 - y0;
                    var dx = k * dy;
                    var relx = v_px - x0;
                    var rely = v_py - y0;

                    var len2 = dx * dx + dy * dy;
                    var valid = Vector512.GreaterThan(len2, v_tiny);

                    var t = (relx * dx + rely * dy) / len2;
                    Vector512<float> vz = default;
                    t = Vector512.Min(Vector512.Max(t, vz), v_one);

                    var nx = x0 + t * dx;
                    var ny = y0 + t * dy;

                    var dxp = nx - v_px;
                    var dyp = ny - v_py;
                    var dist2 = dxp * dxp + dyp * dyp;

                    var anyBetter = Vector512.LessThan(dist2, Vector512.Create(bestSq)).ExtractMostSignificantBits();
                    if (anyBetter != default)
                    {
                        dist2.CopyTo(bD);
                        nx.CopyTo(bX);
                        ny.CopyTo(bY);
                        for (var m = 0; m < 16; ++m)
                        {
                            var d2 = bD[m];
                            if (d2 < bestSq)
                            {
                                bestSq = d2;
                                bestX = bX[m];
                                bestY = bY[m];
                            }
                        }
                    }
                }
            }

            for (; i + 16 <= ee; i += 16)
            {
                var y0 = Load512(y0Arr, i);
                var y1 = Load512(y1Arr, i);
                var x0 = Load512(x0Arr, i);
                var k = Load512(kArr, i);

                var dy = y1 - y0;
                var dx = k * dy;
                var relx = v_px - x0;
                var rely = v_py - y0;

                var len2 = dx * dx + dy * dy;
                var valid = Vector512.GreaterThan(len2, v_tiny);

                var t = (relx * dx + rely * dy) / len2;
                Vector512<float> vz = default;
                t = Vector512.Min(Vector512.Max(t, vz), v_one);

                var nx = x0 + t * dx;
                var ny = y0 + t * dy;

                var dxp = nx - v_px;
                var dyp = ny - v_py;
                var dist2 = dxp * dxp + dyp * dyp;

                var anyBetter = Vector512.LessThan(dist2, Vector512.Create(bestSq)).ExtractMostSignificantBits();
                if (anyBetter != 0)
                {
                    dist2.CopyTo(bD);
                    nx.CopyTo(bX);
                    ny.CopyTo(bY);
                    for (var m = 0; m < 16; ++m)
                    {
                        var d2 = bD[m];
                        if (d2 < bestSq)
                        {
                            bestSq = d2;
                            bestX = bX[m];
                            bestY = bY[m];
                        }
                    }
                }
            }
            static Vector512<float> Load512(float[] arr, int index)
            {
                ref var r0 = ref MemoryMarshal.GetArrayDataReference(arr);
                return Vector512.LoadUnsafe(ref r0, (nuint)index);
            }
        }

        static void KernelClosest256(float[] y0Arr, float[] y1Arr, float[] x0Arr, float[] kArr, int es, int ee,
            float px, float py, ref float bestSq, ref float bestX, ref float bestY)
        {
            var v_px = Vector256.Create(px);
            var v_py = Vector256.Create(py);
            var v_one = Vector256.Create(1f);
            var v_tiny = Vector256.Create(1e-12f);

            Span<float> bD = stackalloc float[8];
            Span<float> bX = stackalloc float[8];
            Span<float> bY = stackalloc float[8];

            var i = es;
            for (; i + 16 <= ee; i += 16)
            {
                // block 0
                {
                    var y0 = Load256(y0Arr, i);
                    var y1 = Load256(y1Arr, i);
                    var x0 = Load256(x0Arr, i);
                    var k = Load256(kArr, i);

                    var dy = y1 - y0;
                    var dx = k * dy;
                    var relx = v_px - x0;
                    var rely = v_py - y0;

                    var len2 = dx * dx + dy * dy;
                    var t = (relx * dx + rely * dy) / len2;
                    Vector256<float> vz = default;
                    t = Vector256.Min(Vector256.Max(t, vz), v_one);

                    var nx = x0 + t * dx;
                    var ny = y0 + t * dy;

                    var dxp = nx - v_px;
                    var dyp = ny - v_py;
                    var dist2 = dxp * dxp + dyp * dyp;

                    var anyBetter = Vector256.LessThan(dist2, Vector256.Create(bestSq)).ExtractMostSignificantBits();
                    if (anyBetter != default)
                    {
                        dist2.CopyTo(bD);
                        nx.CopyTo(bX);
                        ny.CopyTo(bY);
                        for (var m = 0; m < 8; ++m)
                        {
                            var d2 = bD[m];
                            if (d2 < bestSq)
                            {
                                bestSq = d2;
                                bestX = bX[m];
                                bestY = bY[m];
                            }
                        }
                    }
                }
                // block 1
                {
                    var b = i + 8;
                    var y0 = Load256(y0Arr, b);
                    var y1 = Load256(y1Arr, b);
                    var x0 = Load256(x0Arr, b);
                    var k = Load256(kArr, b);

                    var dy = y1 - y0;
                    var dx = k * dy;
                    var relx = v_px - x0;
                    var rely = v_py - y0;

                    var len2 = dx * dx + dy * dy;
                    var t = (relx * dx + rely * dy) / len2;
                    Vector256<float> vz = default;
                    t = Vector256.Min(Vector256.Max(t, vz), v_one);

                    var nx = x0 + t * dx;
                    var ny = y0 + t * dy;

                    var dxp = nx - v_px;
                    var dyp = ny - v_py;
                    var dist2 = dxp * dxp + dyp * dyp;

                    var anyBetter = Vector256.LessThan(dist2, Vector256.Create(bestSq)).ExtractMostSignificantBits();
                    if (anyBetter != default)
                    {
                        dist2.CopyTo(bD);
                        nx.CopyTo(bX);
                        ny.CopyTo(bY);
                        for (var m = 0; m < 8; ++m)
                        {
                            var d2 = bD[m];
                            if (d2 < bestSq)
                            {
                                bestSq = d2;
                                bestX = bX[m];
                                bestY = bY[m];
                            }
                        }
                    }
                }
            }

            for (; i + 8 <= ee; i += 8)
            {
                var y0 = Load256(y0Arr, i);
                var y1 = Load256(y1Arr, i);
                var x0 = Load256(x0Arr, i);
                var k = Load256(kArr, i);

                var dy = y1 - y0;
                var dx = k * dy;
                var relx = v_px - x0;
                var rely = v_py - y0;

                var len2 = dx * dx + dy * dy;
                var t = (relx * dx + rely * dy) / len2;
                Vector256<float> vz = default;
                t = Vector256.Min(Vector256.Max(t, vz), v_one);

                var nx = x0 + t * dx;
                var ny = y0 + t * dy;

                var dxp = nx - v_px;
                var dyp = ny - v_py;
                var dist2 = dxp * dxp + dyp * dyp;

                var anyBetter = Vector256.LessThan(dist2, Vector256.Create(bestSq)).ExtractMostSignificantBits();
                if (anyBetter != default)
                {
                    dist2.CopyTo(bD);
                    nx.CopyTo(bX);
                    ny.CopyTo(bY);
                    for (var m = 0; m < 8; ++m)
                    {
                        var d2 = bD[m];
                        if (d2 < bestSq)
                        {
                            bestSq = d2;
                            bestX = bX[m];
                            bestY = bY[m];
                        }
                    }
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Vector256<float> Load256(float[] arr, int index)
            {
                ref var r0 = ref MemoryMarshal.GetArrayDataReference(arr);
                return Vector256.LoadUnsafe(ref r0, (nuint)index);
            }
        }

        // Process one row (with SIMD where available)
        void ProcessRow(int row)
        {
            int es = _rowOffsets[row], ee = _rowOffsets[row + 1];
            if (es == ee && _hRowOffsets[row] == _hRowOffsets[row + 1])
            {
                return;
            }

            var rMinY = _minY + row * cellH;
            var rMaxY = rMinY + cellH;

            var vDist = py < rMinY ? (rMinY - py) : (py > rMaxY ? (py - rMaxY) : 0f);

            var rMinX = _rowMinX[row];
            var rMaxX = _rowMaxX[row];

            var hDist = 0f;
            if (px < rMinX)
            {
                hDist = rMinX - px;
            }
            else if (px > rMaxX)
            {
                hDist = px - rMaxX;
            }

            var bandBestSq = vDist * vDist + hDist * hDist;
            if (bandBestSq < bestSq)
            {
                if (Vector512.IsHardwareAccelerated && Avx512F.IsSupported)
                {
                    KernelClosest512(_e_y0_Row, _e_y1_Row, _e_x0_Row, _e_k_Row, es, ee, px, py, ref bestSq, ref bestX, ref bestY);
                }
                else if (Vector256.IsHardwareAccelerated && Avx.IsSupported)
                {
                    KernelClosest256(_e_y0_Row, _e_y1_Row, _e_x0_Row, _e_k_Row, es, ee, px, py, ref bestSq, ref bestX, ref bestY);
                }
                else
                {
                    // Vector<T> fallback
                    var lanes = Vector<float>.Count;
                    var v_px = new Vector<float>(px);
                    var v_py = new Vector<float>(py);
                    var v_one = new Vector<float>(1f);
                    var v_tiny = new Vector<float>(1e-12f);
                    Span<float> bufD = stackalloc float[lanes];
                    Span<float> bufX = stackalloc float[lanes];
                    Span<float> bufY = stackalloc float[lanes];
                    var i = es;
                    for (; i + lanes <= ee; i += lanes)
                    {
                        var y0 = new Vector<float>(_e_y0_Row, i);
                        var y1 = new Vector<float>(_e_y1_Row, i);
                        var x0 = new Vector<float>(_e_x0_Row, i);
                        var k = new Vector<float>(_e_k_Row, i);

                        var dy = y1 - y0;
                        var dx = k * dy;
                        var relx = v_px - x0;
                        var rely = v_py - y0;

                        var len2 = dx * dx + dy * dy;
                        len2 = Vector.Max(len2, v_tiny);
                        var t = (relx * dx + rely * dy) / len2;
                        t = Vector.Min(Vector.Max(t, default), v_one);

                        var nx = x0 + t * dx;
                        var ny = y0 + t * dy;

                        var dxp = nx - v_px;
                        var dyp = ny - v_py;
                        var dist2 = dxp * dxp + dyp * dyp;

                        var anyBetter = Vector.LessThan(dist2, new Vector<float>(bestSq));
                        if (!Vector.EqualsAll(anyBetter, default))
                        {
                            dist2.CopyTo(bufD);
                            nx.CopyTo(bufX);
                            ny.CopyTo(bufY);
                            for (var m = 0; m < lanes; ++m)
                            {
                                var d2 = bufD[m];
                                if (d2 < bestSq)
                                {
                                    bestSq = d2;
                                    bestX = bufX[m];
                                    bestY = bufY[m];
                                }
                            }
                        }
                    }

                    for (; i < ee; ++i)
                    {
                        var y0s = _e_y0_Row[i];
                        var y1s = _e_y1_Row[i];
                        var x0s = _e_x0_Row[i];
                        var ks = _e_k_Row[i];

                        var dys = y1s - y0s;
                        var dxs = ks * dys;
                        var rx = px - x0s;
                        var ry = py - y0s;

                        var len2s = dxs * dxs + dys * dys + 1e-12f;
                        var t = (rx * dxs + ry * dys) / len2s;
                        if (t < 0f)
                        {
                            t = 0f;
                        }
                        else if (t > 1f)
                        {
                            t = 1f;
                        }

                        var nx = x0s + t * dxs;
                        var ny = y0s + t * dys;

                        var dxp = nx - px;
                        var dyp = ny - py;
                        var d2 = dxp * dxp + dyp * dyp;

                        if (d2 < bestSq)
                        {
                            bestSq = d2;
                            bestX = nx;
                            bestY = ny;
                        }
                    }
                }
            }

            // horizontals
            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            for (var h = hs; h < he; ++h)
            {
                ref readonly var e = ref _hEdges[_hRowIdx[h]];
                var clampedX = px < e.minX ? e.minX : (px > e.maxX ? e.maxX : px);
                var dxp = clampedX - px;
                var dyp = e.y - py;
                var d2 = dxp * dxp + dyp * dyp;
                if (d2 < bestSq)
                {
                    bestSq = d2;
                    bestX = clampedX;
                    bestY = e.y;
                }
            }
        }

        if ((uint)row0 < (uint)_rows)
        {
            ProcessRow(row0);
        }

        var maxRow = _rows - 1;
        while (true)
        {
            var progressed = false;

            if (rNeg - 1 >= 0)
            {
                var rn = --rNeg;
                var rMinY = _minY + rn * cellH;
                var rMaxY = rMinY + cellH;
                var vDist = py < rMinY ? (rMinY - py) : (py > rMaxY ? (py - rMaxY) : 0f);
                if (vDist * vDist < bestSq)
                {
                    ProcessRow(rn);
                    progressed = true;
                }
            }

            if (rPos <= maxRow)
            {
                var rp = rPos++;
                var rMinY = _minY + rp * cellH;
                var rMaxY = rMinY + cellH;
                var vDist = py < rMinY ? (rMinY - py) : (py > rMaxY ? (py - rMaxY) : 0f);
                if (vDist * vDist < bestSq)
                {
                    ProcessRow(rp);
                    progressed = true;
                }
            }

            if (!progressed || bestSq == 0f || rNeg <= 0 && rPos > maxRow)
            {
                break;
            }
        }

        return new(bestX, bestY);
    }

    public WDir[] VisibilityFrom(in WDir origin, RelSimplifiedComplexPolygon polygon)
    {
        // collect unique vertices (exterior + holes)
        var unique = new HashSet<WDir>();
        var parts = polygon.Parts;
        var countP = parts.Count;
        for (var i = 0; i < countP; ++i)
        {
            var part = parts[i];
            var vs = part.Vertices;
            var countVs = vs.Count;
            for (var j = 0; j < countVs; ++j)
            {
                unique.Add(vs[j]);
            }
        }

        // raycast toward each vertex with small angular jitter to straddle edges
        const float EpsAngle = 1e-4f;
        Span<float> jitter = [-EpsAngle, 0f, EpsAngle];

        var ox = origin.X;
        var oz = origin.Z;

        var hits = new List<(float angle, WDir pt)>(unique.Count * 2);

        foreach (var v in unique)
        {
            var baseAngle = MathF.Atan2(v.Z - oz, v.X - ox);

            for (var k = 0; k < 3; ++k)
            {
                var ang = baseAngle + jitter[k];
                var (sd, cd) = ((float, float))Math.SinCos(ang);
                var dir = new WDir(cd, sd);

                var t = Raycast(origin, dir);
                if (t != float.MaxValue)
                {
                    var hit = origin + t * dir;
                    hits.Add((ang, hit));
                }
            }
        }

        // sort CCW and emit polygon
        hits.Sort(static (a, b) => a.angle.CompareTo(b.angle));

        var n = hits.Count;
        var result = new WDir[n];
        for (var i = 0; i < n; ++i)
        {
            result[i] = hits[i].pt;
        }
        return result;
    }
}
