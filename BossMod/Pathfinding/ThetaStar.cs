using System.Buffers;

namespace BossMod.Pathfinding;

[SkipLocalsInit]
public sealed class ThetaStar
{
    public enum Score
    {
        JustBad, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), destination is unsafe and has same or lower max-g than starting cell
        UltimatelyBetter, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), destination is unsafe but has larger max-g than starting cell
        UltimatelySafe, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), however destination is safe
        UnsafeAsStart, // the path is unsafe (there are cells along the path with negative leeway, but no max-g lower than starting cell), destination is unsafe with same max-g as starting cell (starting cell will have this score if its max-g is <= 0)
        SemiSafeAsStart, // the path is semi-safe (no cell along the path has negative leeway or max-g lower than starting cell), destination is unsafe with same max-g as starting cell (starting cell will have this score if its max-g is > 0)
        UnsafeImprove, // the path is unsafe (there are cells along the path with negative leeway, but no max-g lower than starting cell), destination is at least better than start
        SemiSafeImprove, // the path is semi-safe (no cell along the path has negative leeway or max-g lower than starting cell), destination is unsafe but better than start
        Safe, // the path reaches safe cell and is fully safe (no cell along the path has negative leeway) (starting cell will have this score if it's safe)
        SafeBetterPrio, // the path reaches safe cell with a higher goal priority than starting cell (but less than max) and is fully safe (no cell along the path has negative leeway)
        SafeMaxPrio // the path reaches safe cell with max goal priority and is fully safe (no cell along the path has negative leeway)
    }

    public struct Node
    {
        public float GScore;
        public float HScore;
        public float PathLeeway; // min diff along path between node's g-value and cell's g-value
        public float PathMinG; // minimum 'max g' value along path
        public int ParentIndex;
        public int OpenHeapIndex; // -1 if in closed list, 0 if not in any lists, otherwise (index+1)
        public Score Score;

        public readonly float FScore => GScore + HScore;
    }

    private Map _map = new();
    private Node[] _nodes = [];
    private readonly List<int> _openList = [];
    private float _deltaGSide;
    private float _deltaGDiag;

    private float _mapResolution;
    private float _mapHalfResolution;
    public int StartNodeIndex;
    private float _startMaxG;
    private float _startPrio;
    private Score _startScore;

    private int _bestIndex; // node with best score
    private int _fallbackIndex; // best 'fallback' node: node that we don't necessarily want to go to, but might want to move closer to it (to the parent)

    // statistics
    public int NumSteps;
    public int NumReopens;
    private bool _hasTeleporters;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Node NodeByIndex(int index) => ref _nodes[index];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WPos CellCenter(int index) => _map.GridToWorld(index % _map.Width, index / _map.Width, 0.5f, 0.5f);

    // gMultiplier is typically inverse speed, which turns g-values into time
    public void Start(Map map, WPos startPos, float gMultiplier)
    {
        _map = map;
        var numPixels = map.Width * map.Height;
        if (_nodes.Length < numPixels)
        {
            _nodes = new Node[numPixels];
        }
        else
        {
            new Span<Node>(_nodes, 0, numPixels).Clear();
        }
        _openList.Clear();
        _deltaGSide = map.Resolution * gMultiplier;
        _deltaGDiag = _deltaGSide * 1.414214f;
        _mapResolution = map.Resolution;
        _mapHalfResolution = map.Resolution * 0.5f;
        _hasTeleporters = map.HasTeleporters;

        var startFrac = map.WorldToGridFrac(startPos);
        var gridPos = Map.FracToGrid(startFrac);
        var start = map.ClampToGrid(gridPos);
        StartNodeIndex = _bestIndex = _fallbackIndex = _map.GridToIndex(start.x, start.y);
        var gridPosX = gridPos.x;
        var gridPosY = gridPos.y;
        if (gridPosX < 0 || gridPosX >= map.Width || gridPosY < 0 || gridPosY >= map.Height) // hack to make AI go back into arena bounds after failed knockbacks etc
        {
            _map.PixelPriority[StartNodeIndex] = float.MinValue;
        }

        PrefillH();

        _startMaxG = _map.PixelMaxG[StartNodeIndex];
        _startPrio = _map.PixelPriority[StartNodeIndex];
        //if (_startMaxG < 0)
        //    _startMaxG = float.MaxValue; // TODO: this is a hack that allows navigating outside the obstacles, reconsider...
        _startScore = CalculateScore(_startMaxG, _startMaxG, _startMaxG, StartNodeIndex);

        NumSteps = NumReopens = 0;

        startFrac.X -= start.x + 0.5f;
        startFrac.Y -= start.y + 0.5f;
        ref var startNode = ref _nodes[StartNodeIndex];
        _nodes[StartNodeIndex] = new()
        {
            GScore = 0f,
            HScore = startNode.HScore, //HeuristicDistance(start.x, start.y),
            ParentIndex = StartNodeIndex, // start's parent is self
            PathLeeway = _startMaxG,
            PathMinG = _startMaxG,
            Score = _startScore,
        };
        AddToOpen(StartNodeIndex);
    }

    // returns whether search is to be terminated; on success, first node of the open list would contain found goal
    private static readonly int[] _dx = [0, -1, 1, -1, 1, 0, -1, 1];
    private static readonly int[] _dy = [-1, -1, -1, 0, 0, 1, 1, 1];
    private static readonly bool[] _diagonal = [false, true, true, false, false, false, true, true];

    public bool ExecuteStep()
    {
        if (_openList.Count == 0)
        {
            return false;
        }

        ++NumSteps;
        var pIdx = PopMinOpen();
        var width = _map.Width;
        var widthu = (uint)width;
        var height = (uint)_map.Height;
        var px = pIdx % width;
        var py = pIdx / width;

        ref var p = ref _nodes[pIdx];

        // update best & fallback
        if (CompareNodeScores(ref p, ref _nodes[_bestIndex]) < 0)
        {
            _bestIndex = pIdx;
        }
        if (p.Score == Score.UltimatelySafe && (_fallbackIndex == StartNodeIndex || CompareNodeScores(ref p, ref _nodes[_fallbackIndex]) < 0))
        {
            _fallbackIndex = pIdx;
        }

        // neighbor loop with bounds checks and packed cost
        for (var i = 0; i < 8; ++i)
        {
            var dx = _dx[i];
            var dy = _dy[i];
            var nx = px + dx;
            var ny = py + dy;
            if ((uint)nx >= widthu || (uint)ny >= height)
            {
                continue;
            }

            var nIdx = ny * width + nx;
            VisitNeighbour(pIdx, nx, ny, nIdx, !_diagonal[i] ? _deltaGSide : _deltaGDiag, dx, dy);
        }

        if (_hasTeleporters)
        {
            if (!_map.HasTeleEdges(pIdx) && !_map.IsTeleShadow(pIdx) && TryCommitDirectApproachToNearestEntrance(pIdx))
            {
                return true;
            }
            var edges = _map.TeleEdgesForIndex(pIdx);
            var len = edges.Length;
            for (var i = 0; i < len; ++i)
            {
                VisitTeleporterEdge(pIdx, edges[i]);
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Execute()
    {
        var pixelMaxG = _map.PixelMaxG;
        while (_fallbackIndex == StartNodeIndex && ExecuteStep())
        {
            ref var nd = ref _nodes[_bestIndex]; // instead of only looking for the perfect cell we search for a cell that is reasonably better
            if (nd.Score >= Score.SafeBetterPrio && nd.PathLeeway > 0f || nd.Score > _startScore && pixelMaxG[_bestIndex] > pixelMaxG[StartNodeIndex] + 2f || nd.HScore <= 0f)
            {
                break;
            }
        }
        return BestIndex();
    }

    public int BestIndex()
    {
        ref var nd = ref _nodes[_bestIndex];
        if (nd.Score > _startScore)
        {
            return _bestIndex; // we've found something better than start
        }

        if (_fallbackIndex != StartNodeIndex)
        {
            // find first parent of best-among-worst that is at least as good as start
            var destIndex = _fallbackIndex;
            ref var ndp = ref _nodes[destIndex];
            var parentIndex = ndp.ParentIndex;

            ref var current = ref _nodes[parentIndex];
            while (current.Score < _startScore)
            {
                destIndex = parentIndex;
                ref var ndd = ref current;
                parentIndex = current.ParentIndex;
                current = ref _nodes[parentIndex];
            }

            // TODO: this is very similar to LineOfSight, try to unify implementations...
            var (x2, y2) = _map.IndexToGrid(destIndex);
            var (x1, y1) = _map.IndexToGrid(parentIndex);
            var dx = x2 - x1;
            var dy = y2 - y1;
            var sx = dx > 0 ? 1 : -1;
            var sy = dy > 0 ? 1 : -1;
            var hsx = 0.5f * sx;
            var hsy = 0.5f * sy;
            var indexDeltaX = sx;
            var indexDeltaY = sy * _map.Width;

            var ab = new Vector2(dx, dy);
            ab /= ab.Length();
            var invx = ab.X != 0f ? 1f / ab.X : float.MaxValue; // either can be infinite, but not both; we want to avoid actual infinities here, because 0*inf = NaN (and we'd rather have it be 0 in this case)
            var invy = ab.Y != 0f ? 1f / ab.Y : float.MaxValue;

            while (x1 != x2 || y1 != y2)
            {
                var tx = hsx * invx; // if negative, we'll never intersect it
                var ty = hsy * invy;
                if (tx < 0f || x1 == x2)
                {
                    tx = float.MaxValue;
                }
                if (ty < 0f || y1 == y2)
                {
                    ty = float.MaxValue;
                }
                var nextIndex = parentIndex;
                if (tx < ty)
                {
                    x1 += sx;
                    nextIndex += indexDeltaX;
                }
                else
                {
                    y1 += sy;
                    nextIndex += indexDeltaY;
                }

                ref var next = ref _nodes[nextIndex];
                if (next.Score < _startScore)
                {
                    return parentIndex;
                }
                parentIndex = nextIndex;
            }
        }

        return _bestIndex;
    }

    public Score CalculateScore(float pixMaxG, float pathMinG, float pathLeeway, int pixelIndex)
    {
        var destSafe = pixMaxG == float.MaxValue;
        var pathSafe = pathLeeway > 0f;
        var destBetter = pixMaxG > _startMaxG;

        if (destSafe && pathSafe)
        {
            var prio = _map.PixelPriority[pixelIndex];
            return prio == _map.MaxPriority ? Score.SafeMaxPrio : prio > _startPrio ? Score.SafeBetterPrio : Score.Safe;
        }

        if (pathMinG == _startMaxG) // TODO: some small threshold? should be solved by preprocessing...
            return pathSafe
                ? (destBetter ? Score.SemiSafeImprove : Score.SemiSafeAsStart) // note: if pix.MaxG is < _startMaxG, then PathMinG will be < too
                : (destBetter ? Score.UnsafeImprove : Score.UnsafeAsStart);

        return destSafe ? Score.UltimatelySafe : destBetter ? Score.UltimatelyBetter : Score.JustBad;
    }

    // return a 'score' difference: 0 if identical, -1 if left is somewhat better, -2 if left is significantly better, +1/+2 when right is better
    public static int CompareNodeScores(ref Node nodeL, ref Node nodeR)
    {
        if (nodeL.Score != nodeR.Score)
        {
            return nodeL.Score > nodeR.Score ? -2 : +2;
        }

        const float Eps = 1e-5f;
        // TODO: should we use leeway here or distance?..
        //return nodeL.PathLeeway > nodeR.PathLeeway;
        var gl = nodeL.GScore;
        var gr = nodeR.GScore;
        var fl = gl + nodeL.HScore;
        var fr = gr + nodeR.HScore;
        if (fl + Eps < fr)
        {
            return -1;
        }
        if (fr + Eps < fl)
        {
            return +1;
        }
        if (gl != gr)
        {
            return gl > gr ? -1 : 1; // tie-break towards larger g-values
        }
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float BestTeleHeuristicForEntrance(int entranceIdx)
    {
        var edges = _map.TeleEdgesForIndex(entranceIdx);
        ref var entrance = ref _nodes[entranceIdx];
        var best = entrance.HScore; // fallback to prefilled EDT
        var len = edges.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var e = ref edges[i];
            ref var dest = ref _nodes[e.DestIndex];
            var destH = dest.HScore;
            var cand = destH + Math.Max(0f, e.UseTime);
            if (cand < best)
            {
                best = cand;
            }
        }
        return best;
    }

    // LOS to a specific cell that ignores tele-shadow (but respects walls). Used to "commit" to an entrance
    private bool LineOfSightToExactCellAllowShadow(int x0, int y0, int xe, int ye, float parentGScore, out float approachLeeway, out float approachDist, out float approachMinG)
    {
        approachLeeway = float.MaxValue;
        approachMinG = float.MaxValue;

        var dxRaw = xe - x0;
        var dyRaw = ye - y0;
        var shiftdx = dxRaw >> 31;
        var shiftdy = dyRaw >> 31;
        var stepX = dxRaw == 0 ? 0 : (shiftdx | 1);
        var stepY = dyRaw == 0 ? 0 : (shiftdy | 1);
        var dx = (dxRaw ^ shiftdx) - shiftdx;
        var dy = (dyRaw ^ shiftdy) - shiftdy;

        approachDist = MathF.Sqrt(dx * dx + dy * dy);

        var invdx = dx != 0 ? 1f / dx : float.MaxValue;
        var invdy = dy != 0 ? 1f / dy : float.MaxValue;
        var tMaxX = _mapHalfResolution * invdx;
        var tMaxY = _mapHalfResolution * invdy;
        var tDeltaX = _mapResolution * invdx;
        var tDeltaY = _mapResolution * invdy;

        int x = x0, y = y0;
        var w = _map.Width;
        var pixG = _map.PixelMaxG;
        var cumulativeG = parentGScore;
        const float kEps = 0.003f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool CheckCell(int cx, int cy, float gAtEntry, ref float minG, ref float minLeeway)
        {
            var idx = cy * w + cx;
            var maxG = pixG[idx];
            if (maxG < 0f)
            {
                return false; // walls/bounds only (shadow intentionally ignored)
            }
            if (maxG < minG)
            {
                minG = maxG;
            }
            var leeway = maxG - gAtEntry;
            if (leeway < minLeeway)
            {
                minLeeway = leeway;
            }
            return true;
        }

        while (true)
        {
            if (!CheckCell(x, y, cumulativeG, ref approachMinG, ref approachLeeway))
            {
                return false;
            }

            if (x == xe && y == ye)
            {
                return true;
            }

            var diff = tMaxX - tMaxY;
            if (diff < -kEps)
            {
                tMaxX += tDeltaX;
                x += stepX;
                cumulativeG += _deltaGSide;
            }
            else if (diff > kEps)
            {
                tMaxY += tDeltaY;
                y += stepY;
                cumulativeG += _deltaGSide;
            }
            else
            {
                var gAtCorner = cumulativeG + _deltaGSide;
                if (!CheckCell(x + stepX, y, gAtCorner, ref approachMinG, ref approachLeeway))
                {
                    return false;
                }
                if (!CheckCell(x, y + stepY, gAtCorner, ref approachMinG, ref approachLeeway))
                {
                    return false;
                }
                tMaxX += tDeltaX;
                tMaxY += tDeltaY;
                x += stepX;
                y += stepY;
                cumulativeG += _deltaGDiag;
            }
        }
    }

    // scan a small square around 'parent' for entrance cells, pick the best reachable by LOS
    private bool TryCommitDirectApproachToNearestEntrance(int parentIndex, int maxRadiusCells = 14, int maxCandidates = 3)
    {
        var w = _map.Width;
        var h = _map.Height;
        var (px, py) = _map.IndexToGrid(parentIndex);
        ref var parent = ref _nodes[parentIndex];
        var parentG = parent.GScore;

        var x1 = Math.Max(0, px - maxRadiusCells);
        var y1 = Math.Max(0, py - maxRadiusCells);
        var x2 = Math.Min(w - 1, px + maxRadiusCells);
        var y2 = Math.Min(h - 1, py + maxRadiusCells);

        Span<int> idxBuf = stackalloc int[32];
        Span<float> fEstBuf = stackalloc float[32];
        Span<float> distBuf = stackalloc float[32];
        Span<float> leewayBuf = stackalloc float[32];
        Span<float> minGBuf = stackalloc float[32];

        var candCount = 0;
        var bestF = float.MaxValue;

        for (var y = y1; y <= y2; ++y)
        {
            var rowBase = y * w;
            for (var x = x1; x <= x2; ++x)
            {
                var idx = rowBase + x;
                if (!_map.HasTeleEdges(idx))
                {
                    continue;
                }

                if (!LineOfSightToExactCellAllowShadow(px, py, x, y, parentG, out var leeway, out var dist, out var minG))
                {
                    continue;
                }

                var fEst = parentG + _deltaGSide * dist + BestTeleHeuristicForEntrance(idx);

                if (candCount < maxCandidates)
                {
                    idxBuf[candCount] = idx;
                    fEstBuf[candCount] = fEst;
                    distBuf[candCount] = dist;
                    leewayBuf[candCount] = leeway;
                    minGBuf[candCount] = minG;
                    ++candCount;
                    if (fEst < bestF)
                    {
                        bestF = fEst;
                    }
                }
                else
                {
                    var worst = 0;
                    for (var i = 1; i < candCount; ++i)
                    {
                        if (fEstBuf[i] > fEstBuf[worst])
                        {
                            worst = i;
                        }
                    }

                    if (fEst < fEstBuf[worst])
                    {
                        idxBuf[worst] = idx;
                        fEstBuf[worst] = fEst;
                        distBuf[worst] = dist;
                        leewayBuf[worst] = leeway;
                        minGBuf[worst] = minG;
                        if (fEst < bestF)
                        {
                            bestF = fEst;
                        }
                    }
                }
            }
        }

        if (candCount == 0)
        {
            return false;
        }

        var eps = Math.Max(2f * _deltaGSide, 0.05f * bestF);
        var committed = 0;

        for (var i = 0; i < candCount && committed < maxCandidates; ++i)
        {
            if (fEstBuf[i] > bestF + eps)
            {
                continue;
            }

            VisitTeleporterViaEntrance(parentIndex, idxBuf[i], _deltaGSide * distBuf[i], leewayBuf[i], minGBuf[i]);
            ++committed;
        }

        return committed > 0;
    }

    public bool LineOfSight(int x0, int y0, int x1, int y1, float parentGScore, out float lineOfSightLeeway, out float lineOfSightDist, out float lineOfSightMinG)
    {
        lineOfSightLeeway = float.MaxValue;
        lineOfSightMinG = float.MaxValue;

        var dxRaw = x1 - x0;
        var dyRaw = y1 - y0;

        var shiftdx = dxRaw >> 31;
        var shiftdy = dyRaw >> 31;
        var stepX = dxRaw == 0 ? 0 : (shiftdx | 1);
        var stepY = dyRaw == 0 ? 0 : (shiftdy | 1);

        var dx = (dxRaw ^ shiftdx) - shiftdx;
        var dy = (dyRaw ^ shiftdy) - shiftdy;

        lineOfSightDist = MathF.Sqrt(dx * dx + dy * dy);

        var invdx = dx != 0 ? 1f / dx : float.MaxValue;
        var invdy = dy != 0 ? 1f / dy : float.MaxValue;

        // DDA params
        var tMaxX = _mapHalfResolution * invdx;
        var tMaxY = _mapHalfResolution * invdy;
        var tDeltaX = _mapResolution * invdx;
        var tDeltaY = _mapResolution * invdy;

        int x = x0, y = y0;
        var w = _map.Width;
        var pixG = _map.PixelMaxG;
        var cumulativeG = parentGScore;

        // Teleporter awareness
        var hasTeleporters = _hasTeleporters;
        var startedInShadow = false;
        if (hasTeleporters)
        {
            var startIdx = y0 * w + x0;
            var endIdx = y1 * w + x1;
            var startIsEntrance = _map.HasTeleEdges(startIdx);
            var endIsEntrance = _map.HasTeleEdges(endIdx);

            // walking LOS directly into an entrance from outside must not bypass the compound "step-into-entrance + teleport"
            if (!startIsEntrance && endIsEntrance)
            {
                return false;
            }
            startedInShadow = _map.IsTeleShadow(startIdx);
        }

        const float kEps = 0.003f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool CheckCellAndUpdate(int cx, int cy, float gAtEntry, ref float minG, ref float minLeeway)
        {
            var idx = cy * w + cx;

            if (hasTeleporters && !startedInShadow)
            {
                // forbid crossing into any tele shadow cell unless we started inside.
                if (_map.IsTeleShadow(idx))
                {
                    return false;
                }
            }

            var maxG = pixG[idx];
            if (maxG < 0f)
            {
                return false;
            }

            if (maxG < minG)
            {
                minG = maxG;
            }
            var leeway = maxG - gAtEntry;
            if (leeway < minLeeway)
            {
                minLeeway = leeway;
            }
            return true;
        }

        while (true)
        {
            if (!CheckCellAndUpdate(x, y, cumulativeG, ref lineOfSightMinG, ref lineOfSightLeeway))
            {
                return false;
            }

            if (x == x1 && y == y1)
            {
                return true;
            }

            var diff = tMaxX - tMaxY;
            if (diff < -kEps)
            {
                tMaxX += tDeltaX;
                x += stepX;
                cumulativeG += _deltaGSide;
            }
            else if (diff > kEps)
            {
                tMaxY += tDeltaY;
                y += stepY;
                cumulativeG += _deltaGSide;
            }
            else
            {
                // corner: check both side-adjacent cells too (supercover)
                var gAtCorner = cumulativeG + _deltaGSide;

                // peek X side
                if (!CheckCellAndUpdate(x + stepX, y, gAtCorner, ref lineOfSightMinG, ref lineOfSightLeeway))
                {
                    return false;
                }

                // peek Y side
                if (!CheckCellAndUpdate(x, y + stepY, gAtCorner, ref lineOfSightMinG, ref lineOfSightLeeway))
                {
                    return false;
                }

                tMaxX += tDeltaX;
                tMaxY += tDeltaY;
                x += stepX;
                y += stepY;
                cumulativeG += _deltaGDiag;
            }
        }
    }

    private void VisitNeighbour(int parentIndex, int nodeX, int nodeY, int nodeIndex, float deltaGrid, int dx, int dy)
    {
        ref var currentParentNode = ref _nodes[parentIndex];
        ref var destNode = ref _nodes[nodeIndex];

        if (destNode.OpenHeapIndex < 0 && destNode.Score >= Score.SemiSafeAsStart)
        {
            return;
        }

        var pixelMaxG = _map.PixelMaxG;
        var destPixG = pixelMaxG[nodeIndex];
        var parentPixG = pixelMaxG[parentIndex];
        if (destPixG < 0f && parentPixG >= 0f || parentPixG == -1f && destPixG < -1f)
        {
            return; // impassable
        }

        // diagonal corner-cutting check
        if (dx != 0 && dy != 0)
        {
            var sideX = parentIndex + dx; // (px + sign(dx), py)
            var sideY = parentIndex + dy * _map.Width; // (px, py + sign(dy))
            if (!Passable(pixelMaxG, sideX) || !Passable(pixelMaxG, sideY))
            {
                return;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool Passable(float[] g, int idx) => (uint)idx < (uint)g.Length && g[idx] >= 0f;
        }

        // avoid accidental teleports
        // if we are outside and the destination is an entrance → do no allow a plain walk-in
        // treat it as "approach via entrance then teleport" compound transitions
        // Outside → direct entrance neighbor → use compound teleport
        if (_hasTeleporters && !_map.HasTeleEdges(parentIndex))
        {
            // Neighbor is an entrance: do compound step
            if (_map.HasTeleEdges(nodeIndex))
            {
                VisitTeleporterViaEntrance(parentIndex, nodeIndex, deltaGrid);
                return;
            }

            // Otherwise, block stepping into the shadow ring to avoid accidental walk-ins
            if (!_map.IsTeleShadow(parentIndex) && _map.IsTeleShadow(nodeIndex))
            {
                return;
            }
        }
        var stepCost = deltaGrid; // either _deltaGSide or _deltaGDiag
        var candidateG = currentParentNode.GScore + stepCost;

        var candidateLeeway = Math.Min(currentParentNode.PathLeeway, Math.Min(destPixG, parentPixG) - candidateG);
        var candidateMinG = Math.Min(currentParentNode.PathMinG, destPixG);

        var altNode = new Node
        {
            GScore = candidateG,
            HScore = destNode.HScore, // or init if first time
            ParentIndex = parentIndex,
            OpenHeapIndex = destNode.OpenHeapIndex,
            PathLeeway = candidateLeeway,
            PathMinG = candidateMinG,
            Score = CalculateScore(destPixG, candidateMinG, candidateLeeway, nodeIndex)
        };
        ref var altnode = ref altNode;
        if (currentParentNode.Score >= Score.UnsafeAsStart && altnode.PathLeeway < 0f && altnode.Score == Score.JustBad) // don't leave safe cells if it requires going through bad cells
        {
            return;
        }

        var grandParentIndex = currentParentNode.ParentIndex;
        ref var grandparentnode = ref _nodes[grandParentIndex];
        if (grandParentIndex != nodeIndex && grandparentnode.PathMinG >= currentParentNode.PathMinG)
        {
            var (gx, gy) = _map.IndexToGrid(grandParentIndex);
            if (LineOfSight(gx, gy, nodeX, nodeY, grandparentnode.GScore, out var losLeeway, out var losDist, out var losMinG))
            {
                var losScore = CalculateScore(destPixG, losMinG, losLeeway, nodeIndex);
                if (losScore >= altnode.Score)
                {
                    altnode.GScore = _nodes[grandParentIndex].GScore + _deltaGSide * losDist;
                    altnode.ParentIndex = grandParentIndex;
                    altnode.PathLeeway = losLeeway;
                    altnode.PathMinG = losMinG;
                    altnode.Score = losScore;
                }
            }
        }

        var visit = destNode.OpenHeapIndex == 0 || CompareNodeScores(ref altnode, ref destNode) < (destNode.OpenHeapIndex < 0 ? -1 : 0);
        if (visit)
        {
            if (destNode.OpenHeapIndex < 0)
            {
                ++NumReopens;
            }
            destNode = altnode;
            AddToOpen(nodeIndex);
        }
    }

    private void VisitTeleporterEdge(int parentIndex, in Map.TeleEdge edge)
    {
        ref var parent = ref _nodes[parentIndex];

        var pixelMaxG = _map.PixelMaxG;
        var parentPixG = pixelMaxG[parentIndex];
        var destPixG = pixelMaxG[edge.DestIndex];
        if (destPixG < 0f)
        {
            return; // blocked landing
        }

        // wait at entrance if cooldown not elapsed
        var wait = Math.Max(0f, edge.NotBeforeG - parent.GScore);
        var candidateG = parent.GScore + wait + Math.Max(0f, edge.UseTime);

        // ensure waiting at entrance is survivable, then ensure landing is survivable at candidate time
        var leewayAtWait = parentPixG - (parent.GScore + wait);
        var leewayAtDest = destPixG - candidateG;

        var candidateLeeway = Math.Min(parent.PathLeeway, Math.Min(leewayAtWait, leewayAtDest));
        var candidateMinG = Math.Min(parent.PathMinG, destPixG);

        var altnode = new Node
        {
            GScore = candidateG,
            HScore = _nodes[edge.DestIndex].HScore,
            ParentIndex = parentIndex,
            OpenHeapIndex = _nodes[edge.DestIndex].OpenHeapIndex,
            PathLeeway = candidateLeeway,
            PathMinG = candidateMinG,
            Score = CalculateScore(destPixG, candidateMinG, candidateLeeway, edge.DestIndex)
        };

        ref var nalt = ref altnode;
        if (parent.Score >= Score.UnsafeAsStart && nalt.PathLeeway < 0f && nalt.Score == Score.JustBad)
        {
            return;
        }

        ref var destNode = ref _nodes[edge.DestIndex];
        var visit = destNode.OpenHeapIndex == 0 || CompareNodeScores(ref altnode, ref destNode) < (destNode.OpenHeapIndex < 0 ? -1 : 0);
        if (visit)
        {
            if (destNode.OpenHeapIndex < 0)
            {
                ++NumReopens;
            }
            destNode = nalt;
            AddToOpen(edge.DestIndex);
        }
    }

    private void VisitTeleporterViaEntrance(int parentIndex, int viaEntranceIndex, float stepCostIntoEntrance)
    {
        ref var parent = ref _nodes[parentIndex];

        var pixG = _map.PixelMaxG;
        var entrancePixG = pixG[viaEntranceIndex];
        if (entrancePixG < 0f)
        {
            return;
        }

        var gAfterStep = parent.GScore + stepCostIntoEntrance;
        var baseLeeway = Math.Min(parent.PathLeeway, entrancePixG - gAfterStep);
        var baseMinG = Math.Min(parent.PathMinG, entrancePixG);

        // upsert entrance node (so path extraction aims for entrance first)
        ref var eNodeOld = ref _nodes[viaEntranceIndex];
        var eNodeNew = new Node
        {
            GScore = gAfterStep,
            HScore = eNodeOld.HScore,
            ParentIndex = parentIndex,
            OpenHeapIndex = eNodeOld.OpenHeapIndex,
            PathLeeway = baseLeeway,
            PathMinG = baseMinG,
            Score = CalculateScore(entrancePixG, baseMinG, baseLeeway, viaEntranceIndex)
        };
        ref var nnew = ref eNodeNew;
        nnew.HScore = Math.Min(nnew.HScore, BestTeleHeuristicForEntrance(viaEntranceIndex));
        if (parent.Score >= Score.UnsafeAsStart && nnew.PathLeeway < 0f && nnew.Score == Score.JustBad)
        {
            return;
        }

        var visitEntrance = eNodeOld.OpenHeapIndex == 0 || CompareNodeScores(ref eNodeNew, ref eNodeOld) < (eNodeOld.OpenHeapIndex < 0 ? -1 : 0);

        if (visitEntrance)
        {
            if (eNodeOld.OpenHeapIndex < 0)
            {
                ++NumReopens;
            }
            eNodeOld = nnew;
            AddToOpen(viaEntranceIndex);
        }

        // from entrance, consider teleport edges to destinations
        var edges = _map.TeleEdgesForIndex(viaEntranceIndex);
        var len = edges.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var e = ref edges[i];
            var destIdx = e.DestIndex;
            var destPixG = pixG[destIdx];
            if (destPixG < 0f)
            {
                continue;
            }

            var g = gAfterStep;
            var wait = Math.Max(0f, e.NotBeforeG - g);
            g += wait + Math.Max(0f, e.UseTime);

            var candLeeway = Math.Min(baseLeeway, destPixG - g);
            var candMinG = Math.Min(baseMinG, destPixG);

            ref var destNode = ref _nodes[destIdx];
            var newNode = new Node
            {
                GScore = g,
                HScore = destNode.HScore,
                ParentIndex = viaEntranceIndex,   // parent is an entrance
                OpenHeapIndex = destNode.OpenHeapIndex,
                PathLeeway = candLeeway,
                PathMinG = candMinG,
                Score = CalculateScore(destPixG, candMinG, candLeeway, destIdx)
            };

            ref var nnode = ref newNode;
            if (parent.Score >= Score.UnsafeAsStart && nnode.PathLeeway < 0f && nnode.Score == Score.JustBad)
            {
                continue;
            }

            var visit = destNode.OpenHeapIndex == 0 || CompareNodeScores(ref newNode, ref destNode) < (destNode.OpenHeapIndex < 0 ? -1 : 0);

            if (visit)
            {
                if (destNode.OpenHeapIndex < 0)
                {
                    ++NumReopens;
                }
                destNode = newNode;
                AddToOpen(destIdx);
            }
        }
    }

    private void VisitTeleporterViaEntrance(int parentIndex, int viaEntranceIndex, float stepCostIntoEntrance, float approachLeeway, float approachMinG)
    {
        ref var parent = ref _nodes[parentIndex];

        var pixG = _map.PixelMaxG;
        var entrancePixG = pixG[viaEntranceIndex];
        if (entrancePixG < 0f)
        {
            return;
        }

        var gAfterStep = parent.GScore + stepCostIntoEntrance;
        var baseLeeway = Math.Min(parent.PathLeeway, Math.Min(approachLeeway, entrancePixG - gAfterStep));
        var baseMinG = Math.Min(parent.PathMinG, Math.Min(approachMinG, entrancePixG));

        // upsert entrance node, with boosted (reduced) heuristic through the teleporter
        ref var eNodeOld = ref _nodes[viaEntranceIndex];
        var eH = Math.Min(eNodeOld.HScore, BestTeleHeuristicForEntrance(viaEntranceIndex));

        var eNodeNew = new Node
        {
            GScore = gAfterStep,
            HScore = eH, // make the entrance attractive early
            ParentIndex = parentIndex,
            OpenHeapIndex = eNodeOld.OpenHeapIndex,
            PathLeeway = baseLeeway,
            PathMinG = baseMinG,
            Score = CalculateScore(entrancePixG, baseMinG, baseLeeway, viaEntranceIndex)
        };

        var visitEntrance = eNodeOld.OpenHeapIndex == 0 || CompareNodeScores(ref eNodeNew, ref eNodeOld) < (eNodeOld.OpenHeapIndex < 0 ? -1 : 0);
        if (visitEntrance)
        {
            if (eNodeOld.OpenHeapIndex < 0)
            {
                ++NumReopens;
            }
            eNodeOld = eNodeNew;
            AddToOpen(viaEntranceIndex);
        }

        // from entrance, relax teleport landings
        var edges = _map.TeleEdgesForIndex(viaEntranceIndex);
        var len = edges.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var e = ref edges[i];
            var destIdx = e.DestIndex;
            var destPixG = pixG[destIdx];
            if (destPixG < 0f)
            {
                continue;
            }

            var g = gAfterStep;
            var wait = Math.Max(0f, e.NotBeforeG - g);
            g += wait + Math.Max(0f, e.UseTime);

            var candLeeway = Math.Min(baseLeeway, destPixG - g);
            var candMinG = Math.Min(baseMinG, destPixG);

            ref var destNode = ref _nodes[destIdx];
            var newNode = new Node
            {
                GScore = g,
                HScore = destNode.HScore,
                ParentIndex = viaEntranceIndex,
                OpenHeapIndex = destNode.OpenHeapIndex,
                PathLeeway = candLeeway,
                PathMinG = candMinG,
                Score = CalculateScore(destPixG, candMinG, candLeeway, destIdx)
            };

            var visit = destNode.OpenHeapIndex == 0 || CompareNodeScores(ref newNode, ref destNode) < (destNode.OpenHeapIndex < 0 ? -1 : 0);
            if (visit)
            {
                if (destNode.OpenHeapIndex < 0)
                {
                    ++NumReopens;
                }
                destNode = newNode;
                AddToOpen(destIdx);
            }
        }
    }
    private void PrefillH()
    {
        var width = _map.Width;
        var height = _map.Height;
        var maxPriority = _map.MaxPriority;

        var pixelPriority = _map.PixelPriority;
        var nodes = _nodes;

        const float INFf = 1e30f; // large but safe for float ops
        const double INFd = 1e300d; // keep far from finite s intersections

        var deltaGSide = _deltaGSide;

        var colDist = ArrayPool<float>.Shared.Rent(width * height);
        try
        {
            // 1) Column-wise 1D EDT (parallel over columns)
            var partitioner = Partitioner.Create(0, width);
            Parallel.ForEach(partitioner, range =>
            {
                var x1 = range.Item1;
                var x2 = range.Item2;
                var n = height;
                Span<float> f = stackalloc float[n];
                Span<float> d = stackalloc float[n];
                Span<int> v = stackalloc int[n];
                Span<double> z = stackalloc double[n + 1];
                // f: input; d: output; v: parabola sites; z: intersections
                for (var x = x1; x < x2; ++x)
                {
                    // features: 0 at goals, +INF elsewhere
                    for (var y = 0; y < n; ++y)
                    {
                        var idx = y * width + x;
                        f[y] = pixelPriority[idx] == maxPriority ? 0f : INFf;
                    }

                    DistanceTransform1D(f, d, v, z, n, INFd);

                    for (var y = 0; y < n; ++y)
                    {
                        colDist[y * width + x] = d[y];
                    }
                }
            });

            // 2) Row-wise 1D EDT from column results (parallel over rows)
            partitioner = Partitioner.Create(0, height);
            Parallel.ForEach(partitioner, range =>
            {
                var y1 = range.Item1;
                var y2 = range.Item2;
                var n = width;
                Span<float> f = stackalloc float[n];
                Span<float> d = stackalloc float[n];
                Span<int> v = stackalloc int[n];
                Span<double> z = stackalloc double[n + 1];
                for (var y = y1; y < y2; ++y)
                {
                    var rowBase = y * width;
                    for (var x = 0; x < n; ++x)
                    {
                        f[x] = colDist[rowBase + x];
                    }

                    DistanceTransform1D(f, d, v, z, n, INFd);

                    for (var x = 0; x < n; ++x)
                    {
                        var iCell = rowBase + x;
                        if (pixelPriority[iCell] < maxPriority)
                        {
                            ref var nd = ref nodes[iCell];
                            nd.HScore = MathF.Sqrt(d[x]) * deltaGSide;
                        }
                    }
                }
            });
        }
        finally
        {
            ArrayPool<float>.Shared.Return(colDist, false);
        }

        // Felzenszwalb 1D squared distance transform
        // f: 0 at sites, +INF elsewhere. d: output squared distance
        static void DistanceTransform1D(Span<float> f, Span<float> d, Span<int> v, Span<double> z, int n, double INF)
        {
            var k = 0;
            v[0] = 0;
            z[0] = -INF;
            z[1] = +INF;

            for (var q = 1; q < n; ++q)
            {
                // compute intersection with current lower envelope
                var s = Intersection(f, q, v[k]);

                // important invariant: z[0] stays -INF; we never allow k < 0
                while (s <= z[k])
                {
                    --k;
                    if (k < 0)
                    {
                        k = 0; // keep envelope valid
                        break; // will recompute s below with v[0]
                    }
                    s = Intersection(f, q, v[k]);
                }

                ++k;
                v[k] = q;
                z[k] = s;
                z[k + 1] = +INF;
            }

            // evaluate distances
            k = 0;
            for (var q = 0; q < n; ++q)
            {
                while (z[k + 1] < q)
                {
                    ++k;
                }
                var diff = q - v[k];
                d[q] = diff * diff + f[v[k]];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static double Intersection(Span<float> f, int q, int vk)
            {
                // s = ((f[q] + q^2) - (f[vk] + vk^2)) / (2(q - vk))
                // use double for stability
                double fq = f[q];
                double fv = f[vk];
                return (fq + q * q - (fv + vk * vk)) / (2d * (q - vk));
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void AddToOpen(int nodeIndex)
    {
        ref var nd = ref _nodes[nodeIndex];

        if (nd.OpenHeapIndex <= 0)
        {
            _openList.Add(nodeIndex);
            nd.OpenHeapIndex = _openList.Count;
        }
        // update location
        PercolateUp(nd.OpenHeapIndex - 1);
    }

    // remove first (minimal) node from open heap and mark as closed
    private int PopMinOpen()
    {
        var nodeIndex = _openList[0];
        _openList[0] = _openList[^1];
        _nodes[nodeIndex].OpenHeapIndex = -1;
        _openList.RemoveAt(_openList.Count - 1);
        if (_openList.Count > 0)
        {
            _nodes[_openList[0]].OpenHeapIndex = 1;
            PercolateDown(0);
        }
        return nodeIndex;
    }

    private void PercolateUp(int heapIndex)
    {
        var openSpan = CollectionsMarshal.AsSpan(_openList);
        var nodeIndex = openSpan[heapIndex];
        ref var node = ref _nodes[nodeIndex];
        while (heapIndex > 0)
        {
            var parentHeapIndex = (heapIndex - 1) >> 1;
            ref var parentNodeIndex = ref openSpan[parentHeapIndex];
            ref var parent = ref _nodes[parentNodeIndex];
            if (CompareNodeScores(ref node, ref parent) >= 0)
                break; // parent is 'less' (same/better), stop

            openSpan[heapIndex] = parentNodeIndex;
            parent.OpenHeapIndex = heapIndex + 1;
            heapIndex = parentHeapIndex;
        }
        openSpan[heapIndex] = nodeIndex;
        node.OpenHeapIndex = heapIndex + 1;
    }

    private void PercolateDown(int heapIndex)
    {
        var openSpan = CollectionsMarshal.AsSpan(_openList);
        var nodeIndex = openSpan[heapIndex];
        ref var node = ref _nodes[nodeIndex];

        var maxSize = openSpan.Length;
        while (true)
        {
            // find 'better' child
            var childHeapIndex = (heapIndex << 1) + 1;
            if (childHeapIndex >= maxSize)
                break; // node is already a leaf

            var childNodeIndex = openSpan[childHeapIndex];
            ref var child = ref _nodes[childNodeIndex];
            var altChildHeapIndex = childHeapIndex + 1;
            if (altChildHeapIndex < maxSize)
            {
                var altChildNodeIndex = openSpan[altChildHeapIndex];
                ref var altChild = ref _nodes[altChildNodeIndex];
                if (CompareNodeScores(ref altChild, ref child) < 0)
                {
                    childHeapIndex = altChildHeapIndex;
                    childNodeIndex = altChildNodeIndex;
                    child = ref altChild;
                }
            }

            if (CompareNodeScores(ref node, ref child) < 0)
                break; // node is better than best child, so should remain on top

            openSpan[heapIndex] = childNodeIndex;
            child.OpenHeapIndex = heapIndex + 1;
            heapIndex = childHeapIndex;
        }
        openSpan[heapIndex] = nodeIndex;
        node.OpenHeapIndex = heapIndex + 1;
    }
}

public readonly struct Teleporter(WPos entrance, WPos exit, float radius, bool bidirectional, float useTime = 0f, float notBeforeG = 0f)
{
    public readonly WPos Entrance = entrance;
    public readonly WPos Exit = exit;
    public readonly float Radius = radius;
    public readonly bool Bidirectional = bidirectional;
    public readonly float UseTime = useTime;
    public readonly float NotBeforeG = notBeforeG;
}
