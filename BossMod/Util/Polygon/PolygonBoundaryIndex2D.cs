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
        List<(WDir A, WDir B)> all = [];
        var parts = complex.Parts;
        float bbMinX = float.MaxValue, bbMinY = float.MaxValue;
        float bbMaxX = float.MinValue, bbMaxY = float.MinValue;

        var countP = parts.Count;
        for (var i = 0; i < countP; ++i)
        {
            var part = parts[i];
            var ext = part.ExteriorEdges;
            var lenExt = ext.Length;
            for (var j = 0; j < lenExt; ++j)
            {
                var (a, b) = ext[j];
                all.Add((a, b));
                AccumBB(a, b, ref bbMinX, ref bbMinY, ref bbMaxX, ref bbMaxY);
            }
            var holes = part.Holes;
            var lenHoles = holes.Length;
            for (var h = 0; h < lenHoles; ++h)
            {
                var ie = part.InteriorEdges(holes[h]);
                var lenIE = ie.Length;
                for (var j = 0; j < lenIE; ++j)
                {
                    var (a, b) = ie[j];
                    all.Add((a, b));
                    AccumBB(a, b, ref bbMinX, ref bbMinY, ref bbMaxX, ref bbMaxY);
                }
            }
        }

        // Split horizontals/non-horizontals
        var count = all.Count;
        List<E> eList = new(count);
        List<H> hList = new(Math.Max(8, count / 8));

        for (var i = 0; i < count; ++i)
        {
            var (A, B) = all[i];
            float ax = A.X, ay = A.Z, bx = B.X, by = B.Z;
            if (Math.Abs(ay - by) <= Eps)
            {
                hList.Add(new H(ax, ay, bx));
            }
            else
            {
                eList.Add(new E(ax, ay, bx, by));
            }
        }

        E[] edges = [.. eList];
        H[] hEdges = [.. hList];

        // Rowing
        var lenEdges = edges.Length;

        var nEdges = Math.Max(lenEdges, 1);
        var rows = Math.Clamp((int)MathF.Round(MathF.Sqrt(nEdges) * 0.9f) + 8, minRows, maxRows);

        var height = MathF.Max(bbMaxY - bbMinY, Eps);
        var cellH = height / rows;
        var invCellH = 1f / cellH;

        // Bucket non-horizontals to rows
        var rowLists = new List<int>[rows];
        for (var r = 0; r < rows; ++r)
        {
            rowLists[r] = new(8);
        }

        for (var idx = 0; idx < lenEdges; ++idx)
        {
            ref readonly var e = ref edges[idx];

            // Clip edge's span to grid; top-exclusive trick
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
                rowLists[r].Add(idx);
            }
        }

        // Bucket horizontals to single row
        var hRowLists = new List<int>[rows];
        for (var r = 0; r < rows; ++r)
        {
            hRowLists[r] = new(2);
        }
        var lenH = hEdges.Length;
        for (var idx = 0; idx < lenH; ++idx)
        {
            ref readonly var y = ref hEdges[idx].y;
            var r = (int)MathF.Floor((y - bbMinY) * invCellH);
            if (r < 0)
            {
                r = 0;
            }
            else if (r >= rows)
            {
                r = rows - 1;
            }
            hRowLists[r].Add(idx);
        }

        // Flatten rows to SoA order
        var rowOffsets = new int[rows + 1];
        var total = 0;
        for (var r = 0; r < rows; ++r)
        {
            rowOffsets[r] = total;
            total += rowLists[r].Count;
        }
        rowOffsets[rows] = total;

        var e_y0_Row = new float[total];
        var e_y1_Row = new float[total];
        var e_x0_Row = new float[total];
        var e_k_Row = new float[total];
        var e_minX_Row = new float[total];
        var e_maxX_Row = new float[total];

        var rowMinX = new float[rows];
        var rowMaxX = new float[rows];

        for (var r = 0; r < rows; ++r)
        {
            var start = rowOffsets[r];
            var lst = rowLists[r];
            var cnt = lst.Count;

            // Conservative per-row x bounds
            var rMinY = bbMinY + r * cellH;
            var rMaxY = rMinY + cellH;

            var rMinX = float.PositiveInfinity;
            var rMaxX = float.NegativeInfinity;

            for (var i = 0; i < cnt; ++i)
            {
                ref readonly var e = ref edges[lst[i]];
                var w = start + i;

                e_y0_Row[w] = e.y0;
                e_y1_Row[w] = e.y1;
                e_x0_Row[w] = e.x0;
                e_k_Row[w] = e.k;
                e_minX_Row[w] = e.minX;
                e_maxX_Row[w] = e.maxX;

                // Update conservative x-range for this slab: use overlap segment endpoints
                var ys = Math.Max(e.y0, rMinY);
                var ye = Math.Min(e.y1, rMaxY);
                // top-exclusive, nudge down
                ye = MathF.BitDecrement(ye);

                var xs = e.XAt(ys);
                var xe = e.XAt(ye);
                var lo = Math.Min(xs, xe);
                var hi = Math.Max(xs, xe);

                if (lo < rMinX)
                {
                    rMinX = lo;
                }
                if (hi > rMaxX)
                {
                    rMaxX = hi;
                }
            }

            // expand by horizontals
            var hStart = hRowLists[r];
            var hCount = hStart.Count;
            for (var i = 0; i < hCount; ++i)
            {
                ref readonly var h = ref hEdges[hStart[i]];
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

        // Flatten horizontals
        var hRowOffsets = new int[rows + 1];
        total = 0;
        for (var r = 0; r < rows; ++r)
        {
            hRowOffsets[r] = total;
            total += hRowLists[r].Count;
        }
        hRowOffsets[rows] = total;

        var hRowIdx = new int[total];
        for (int r = 0, off = 0; r < rows; ++r)
        {
            var lst = hRowLists[r];
            var countlst = lst.Count;
            for (var i = 0; i < countlst; ++i)
            {
                hRowIdx[off++] = lst[i];
            }
        }

        return new PolygonBoundaryIndex2D(e_y0_Row, e_y1_Row, e_x0_Row, e_k_Row, e_minX_Row, e_maxX_Row,
            rowOffsets, hEdges, hRowOffsets, hRowIdx, rows, bbMinY, cellH, invCellH, bbMinX, bbMinY, bbMaxX, bbMaxY, rowMinX, rowMaxX);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AccumBB(in WDir a, in WDir b, ref float minX, ref float minY, ref float maxX, ref float maxY)
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

        // Global bbox reject
        if (px < _bbMinX - Eps || px > _bbMaxX + Eps || py < _bbMinY - Eps || py > _bbMaxY + Eps)
        {
            return false;
        }

        var row = ClampRow(py);

        // Horizontal boundary hits (row-local)
        var hs = _hRowOffsets[row];
        var he = _hRowOffsets[row + 1];
        for (var i = hs; i < he; ++i)
        {
            ref readonly var h = ref _hEdges[_hRowIdx[i]];
            if (Math.Abs(py - h.y) <= Eps && px >= h.minX - Eps && px <= h.maxX + Eps)
            {
                return true; // on boundary
            }
        }

        // Row X-range early-out (conservative)
        if (px < _rowMinX[row] - Eps || px > _rowMaxX[row] + Eps)
        {
            return false;
        }

        // Non-horizontals: SIMD parity + boundary
        int es = _rowOffsets[row], ee = _rowOffsets[row + 1];
        var n = ee - es;
        if (n == 0)
        {
            return false;
        }

        var i0 = es;
        var parity = 0;

        // Vectorized loop
        var lanes = Vector<float>.Count;
        var v_py = new Vector<float>(py);
        var v_px = new Vector<float>(px);
        var v_eps = new Vector<float>(Eps);

        // loop over vector blocks
        for (; i0 + lanes <= ee; i0 += lanes)
        {
            // Load SoA lanes
            var y0 = LoadVec(_e_y0_Row, i0);
            var y1 = LoadVec(_e_y1_Row, i0);
            var x0 = LoadVec(_e_x0_Row, i0);
            var k = LoadVec(_e_k_Row, i0);
            var mn = LoadVec(_e_minX_Row, i0);
            var mx = LoadVec(_e_maxX_Row, i0);

            var ge_y0 = Vector.GreaterThanOrEqual(v_py, y0);
            var lt_y1 = Vector.LessThan(v_py, y1);
            var span = ge_y0 & lt_y1;

            // xAt(py) = x0 + k*(py - y0)
            var x = x0 + k * (v_py - y0);

            // Boundary: |px - x| <= eps && px within [minX,maxX]
            var dx = Vector.Abs(v_px - x);
            var near = Vector.LessThanOrEqual(dx, v_eps);
            var ge_mn = Vector.GreaterThanOrEqual(v_px, mn);
            var le_mx = Vector.LessThanOrEqual(v_px, mx);
            var onBoundary = span & near & ge_mn & le_mx;

            if (!Vector.EqualsAll(onBoundary, Vector<int>.Zero))
            {
                return true;
            }

            // Crossings: x > px under span
            var gt = Vector.GreaterThan(x, v_px) & span;

            // Count lane bits (popcount over mask)
            parity ^= PopCount(gt);
        }

        // Scalar tail
        for (; i0 < ee; ++i0)
        {
            var y0 = _e_y0_Row[i0];
            if (py < y0 - Eps)
            {
                continue;
            }
            var y1 = _e_y1_Row[i0];
            if (py >= y1 - Eps)
            {
                continue;
            }

            var x = _e_x0_Row[i0] + _e_k_Row[i0] * (py - y0);

            // boundary check (use MathF.Abs to avoid double)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector<float> LoadVec(float[] src, int index) => new(src, index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int PopCount(Vector<int> mask)
        {
            // Each lane is either 0 or -1; convert to 0/1 and sum lanes.
            // (-1 >> 31) == -1; multiply by -1 to get 1; simpler: (mask >> 31) & 1 not available; use equals to AllOnes.
            var count = 0;
            var countc = Vector<int>.Count;
            for (var i = 0; i < countc; ++i)
            {
                count += mask[i] < 0 ? 1 : 0;
            }
            return count & 1; // we only need parity; reduce immediately
        }
    }

    public float Raycast(in WDir o, in WDir d)
    {
        float ox = o.X, oz = o.Z;
        float dx = d.X, dz = d.Z;

        // AABB slab reject (why: fast coarse pruning)
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

        // Horizontal ray fast path (why: hot case, cheaper than general solve)
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

        // DDA rows + robust segment-ray solve
        var bestT = float.MaxValue;
        var rowCur = ClampRow(oz);
        var step = dz > 0f ? +1 : -1;
        var nextY = (dz > 0f) ? (_minY + (rowCur + 1) * _cellH) : (_minY + rowCur * _cellH);

        var v_dx = new Vector<float>(dx);
        var v_dz = new Vector<float>(dz);
        var v_ox = new Vector<float>(ox);
        var v_oz = new Vector<float>(oz);
        var v_zero = Vector<float>.Zero;
        var v_inf = new Vector<float>(float.PositiveInfinity);
        var v_tiny = new Vector<float>(1e-9f); // stricter than Eps for denom
        var v_one = new Vector<float>(1f);

        while ((uint)rowCur < (uint)_rows)
        {
            var tBoundary = (nextY - oz) / dz;
            if (bestT <= tBoundary + 1e-6f)
            {
                break; // small bias to avoid row-transition jitter
            }

            int es = _rowOffsets[rowCur], ee = _rowOffsets[rowCur + 1];

            // SIMD blocks
            var i = es;
            var lanes = Vector<float>.Count;
            for (; i + lanes <= ee; i += lanes)
            {
                // Edge anchor A and vector e
                var y0 = LoadVec(_e_y0_Row, i);
                var y1 = LoadVec(_e_y1_Row, i);
                var x0 = LoadVec(_e_x0_Row, i);
                var k = LoadVec(_e_k_Row, i);

                var ey = y1 - y0; // dy
                var ex = k * ey; // dx
                var wox = x0 - v_ox; // A - o
                var woz = y0 - v_oz;

                // den = cross(d, e) = dx*ey - dz*ex
                var den = v_dx * ey - v_dz * ex;
                var absDen = Vector.Abs(den);
                var nonParallel = Vector.GreaterThan(absDen, v_tiny);

                // t = cross(w, e)/den = (w.x*ey - w.y*ex)/den
                var t = (wox * ey - woz * ex) / den;

                // u = cross(w, d)/den = (w.x*dz - w.y*dx)/den
                var u = (wox * v_dz - woz * v_dx) / den;

                var tNonNeg = Vector.GreaterThanOrEqual(t, v_zero);
                // half-open on u to avoid counting far vertex twice
                var ge0 = Vector.GreaterThanOrEqual(u, v_zero);
                var lt1 = Vector.LessThan(u, v_one);
                var inSeg = ge0 & lt1;

                var valid = nonParallel & tNonNeg & inSeg;

                var tCand = Vector.ConditionalSelect(valid, t, v_inf);
                var v_best = new Vector<float>(bestT);
                tCand = Vector.Min(tCand, v_best);

                var laneMin = ReduceMin(tCand);
                if (laneMin < bestT)
                {
                    bestT = laneMin;
                }
            }

            // Scalar tail (with collinear handling)
            for (; i < ee; ++i)
            {
                float y0s = _e_y0_Row[i], y1s = _e_y1_Row[i], x0s = _e_x0_Row[i];
                float eys = y1s - y0s, exs = _e_k_Row[i] * eys;

                float woxs = x0s - ox, wozs = y0s - oz;
                var den = dx * eys - dz * exs;

                if (Math.Abs(den) > 1e-9f)
                {
                    var t = (woxs * eys - wozs * exs) / den;
                    if (t < 0f || t >= bestT)
                    {
                        goto NEXT_SCALAR;
                    }

                    var u = (woxs * dz - wozs * dx) / den;
                    if (u is < 0f or >= (1f - 1e-6f))
                    {
                        goto NEXT_SCALAR; // half-open
                    }

                    bestT = t;
                }
                else
                {
                    // Possible parallel; if also collinear, project segment onto ray.
                    var col = woxs * dz - wozs * dx; // cross(w, d)
                    if (MathF.Abs(col) <= 1e-7f)
                    {
                        // param along ray for edge endpoints
                        var iddd = 1f / (dx * dx + dz * dz + 1e-20f);
                        var tA = (woxs * dx + wozs * dz) * iddd;
                        var tB = ((x0s + exs - ox) * dx + (y0s + eys - oz) * dz) * iddd;

                        if (tA > tB)
                        {
                            (tA, tB) = (tB, tA);
                        }
                        // nearest non-negative overlap
                        var cand = tA >= 0f ? tA : (tB >= 0f ? tB : float.MaxValue);
                        if (cand < bestT)
                        {
                            bestT = cand;
                        }
                    }
                }
            NEXT_SCALAR:
                ;
            }

            // horizontals in this row
            int hs = _hRowOffsets[rowCur], he = _hRowOffsets[rowCur + 1];
            for (var k = hs; k < he; ++k)
            {
                ref readonly var h = ref _hEdges[_hRowIdx[k]];
                // general ray vs horizontal segment, using same cross-based rule
                var eHx = h.maxX - h.minX;
                var eHy = 0f;

                // den = cross(d, eH) = dx*0 - dz*eHx = -dz*eHx
                var denH = -dz * eHx;
                float woxH = h.minX - ox, wozH = h.y - oz;

                if (Math.Abs(denH) > 1e-9f)
                {
                    var t = (woxH * eHy - wozH * eHx) / denH; // simplifies to -wozH*eHx/denH
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
                    // collinear with horizontal: project
                    if (Math.Abs(wozH * dx - woxH * dz) <= 1e-7f)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector<float> LoadVec(float[] src, int index) => new(src, index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float ReduceMin(in Vector<float> v)
        {
            Span<float> buf = stackalloc float[Vector<float>.Count];
            v.CopyTo(buf);
            var m = buf[0];
            var len = buf.Length;
            for (var i = 1; i < len; ++i)
            {
                if (buf[i] < m)
                {
                    m = buf[i];
                }
            }
            return m;
        }
    }

    public WDir ClampToBounds(in WDir p)
    {
        float px = p.X, py = p.Z;

        // setup outward row scan starting from the row containing py
        var row0 = ClampRow(py);
        int rNeg = row0, rPos = row0 + 1;

        var bestSq = float.PositiveInfinity;
        float bestX = px, bestY = py;

        var cellH = 1f / _invCellH;

        // process one row range [start, end).
        void ProcessRow(int row)
        {
            int es = _rowOffsets[row], ee = _rowOffsets[row + 1];
            if (es == ee && _hRowOffsets[row] == _hRowOffsets[row + 1])
            {
                return;
            }

            // Row band prune using circle test against [rowMinX,rowMaxX] × [rowMinY,rowMaxY]
            var rMinY = _minY + row * cellH;
            var rMaxY = rMinY + cellH;

            // vertical distance from py to row band
            var vDist = py < rMinY ? (rMinY - py) : (py > rMaxY ? (py - rMaxY) : 0f);

            // horizontal distance from px to row's x-range (conservative)
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

            // if even the best possible distance within this row band can't beat bestSq, skip.
            var bandBestSq = vDist * vDist + hDist * hDist;
            if (bandBestSq >= bestSq)
            {
                goto HORIZ_ONLY_IF_ANY; // still let horizontals run if rowMinX/Max were unset
            }

            // SIMD over non-horizontal segments
            var lanes = Vector<float>.Count;
            var v_px = new Vector<float>(px);
            var v_py = new Vector<float>(py);
            var v_zero = Vector<float>.Zero;
            var v_one = new Vector<float>(1f);
            var v_tiny = new Vector<float>(1e-12f);
            Span<float> bufD = stackalloc float[lanes];
            Span<float> bufX = stackalloc float[lanes];
            Span<float> bufY = stackalloc float[lanes];
            var i = es;
            for (; i + lanes <= ee; i += lanes)
            {
                // Segment A = (x0,y0), D = (dx,dy); dx = k*(y1 - y0), dy = (y1 - y0)
                var y0 = LoadVec(_e_y0_Row, i);
                var y1 = LoadVec(_e_y1_Row, i);
                var x0 = LoadVec(_e_x0_Row, i);
                var k = LoadVec(_e_k_Row, i);

                var dy = y1 - y0;
                var dx = k * dy;
                var relx = v_px - x0;
                var rely = v_py - y0;

                // t = clamp( (rel · D) / |D|^2 , [0,1] )
                var len2 = dx * dx + dy * dy;
                len2 = Vector.Max(len2, v_tiny);
                var t = (relx * dx + rely * dy) / len2;
                t = Vector.Min(Vector.Max(t, v_zero), v_one);

                var nx = x0 + t * dx;
                var ny = y0 + t * dy;

                var dxp = nx - v_px;
                var dyp = ny - v_py;
                var dist2 = dxp * dxp + dyp * dyp;

                // Reduce lane min and update best point
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

            // Scalar tail
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

                var len2s = dxs * dxs + dys * dys + 1e-12f; // avoid div-by-zero
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

        HORIZ_ONLY_IF_ANY:
            // ---- Horizontal edges (scalar; cheap & few) ----
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

        // Process the seed row, then expand outwards while rows can still beat current best.
        if ((uint)row0 < (uint)_rows)
        {
            ProcessRow(row0);
        }

        var maxRow = _rows - 1;
        while (true)
        {
            var progressed = false;

            // step down
            if (rNeg - 1 >= 0)
            {
                var rn = --rNeg;
                // vertical distance to that row band
                var rMinY = _minY + rn * cellH;
                var rMaxY = rMinY + cellH;
                var vDist = py < rMinY ? (rMinY - py) : (py > rMaxY ? (py - rMaxY) : 0f);
                if (vDist * vDist < bestSq)
                {
                    ProcessRow(rn);
                    progressed = true;
                }
            }

            // step up
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

            if (!progressed)
            {
                break; // both directions can no longer beat best
            }
            if (bestSq == 0f)
            {
                break; // exact hit
            }
            if (rNeg <= 0 && rPos > maxRow)
            {
                break;
            }
        }

        return new WDir(bestX, bestY);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector<float> LoadVec(float[] src, int index) => new(src, index);
    }

    public WDir[] VisibilityFrom(WDir origin, RelSimplifiedComplexPolygon polygon)
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
