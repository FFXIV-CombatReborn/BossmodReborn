using System.Threading;
using System.Buffers;

namespace BossMod.Pathfinding;

// utility for selecting player's navigation target
// there are several goals that navigation has to meet, in following rough priority
// 1. stay away from aoes; tricky thing is that sometimes it is ok to temporarily enter aoe, if we're sure we'll exit it in time
// 2. maintain uptime - this is represented by being in specified range of specified target, and not moving to interrupt casts unless needed
// 3. execute positionals - this is strictly less important than points above, we only do that if we can meet other conditions
// 4. be in range of healers - even less important, but still nice to do

[SkipLocalsInit]
public struct NavigationDecision
{
    // context that allows reusing large memory allocations
    public sealed class Context
    {
        public Map Map = new();
        public ThetaStar ThetaStar = new();
    }

    public WPos? Destination;
    public WPos? NextWaypoint;
    public float LeewaySeconds; // can be used for finishing casts / slidecasting etc.
    public float TimeToGoal;

    public TimeSpan RasterizeTime;
    public TimeSpan PathfindTime;

    public const float ActivationTimeCushion = 1f; // reduce time between now and activation by this value in seconds; increase for more conservativeness

    public static NavigationDecision Build(Context ctx, DateTime currentTime, AIHints hints, Actor player, float playerSpeed = 6f, float forbiddenZoneCushion = default)
    {
        var startTime = DateTime.Now;

        hints.InitPathfindMap(ctx.Map);
        var pos = player.Position;
        // make local copies of forbidden zones and goals to ensure no race conditions during async pathfinding
        if (hints.TemporaryObstacles.Count != 0)
        {
            RasterizeVoidzones(ctx.Map, [.. hints.TemporaryObstacles]);
        }
        if (hints.ForbiddenZones.Count != 0)
        {
            RasterizeForbiddenZones(ctx.Map, [.. hints.ForbiddenZones], currentTime);
        }
        if (player.CastInfo == null) // don't rasterize goal zones if casting or if inside a very dangerous pixel
        {
            var index = ctx.Map.GridToIndex(ctx.Map.WorldToGrid(pos));
            var len = ctx.Map.PixelMaxG.Length;
            if (index >= 0 && len > index && ctx.Map.PixelMaxG[index] >= 1f || index < 0 || index >= len) // prioritize safety over uptime
            {
                if (hints.GoalZones.Count != 0)
                {
                    RasterizeGoalZones(ctx.Map, [.. hints.GoalZones]);
                }
                if (forbiddenZoneCushion > 0f)
                {
                    AvoidForbiddenZone(ctx.Map, forbiddenZoneCushion);
                }
            }
        }

        if (hints.Teleporters.Count != 0)
        {
            ctx.Map.BuildTeleporterEdges([.. hints.Teleporters]);
        }

        var rasterFinish = DateTime.Now;

        // execute pathfinding
        ctx.ThetaStar.Start(ctx.Map, pos, 1f / playerSpeed);
        var bestNodeIndex = ctx.ThetaStar.Execute();
        ref var bestNode = ref ctx.ThetaStar.NodeByIndex(bestNodeIndex);
        var waypoints = GetFirstWaypoints(ctx.ThetaStar, ctx.Map, bestNodeIndex, pos);
        var finishTime = DateTime.Now;
        return new NavigationDecision() { Destination = waypoints.first, NextWaypoint = waypoints.second, LeewaySeconds = bestNode.PathLeeway, TimeToGoal = bestNode.GScore, PathfindTime = finishTime - rasterFinish, RasterizeTime = rasterFinish - startTime };
    }

    private static void AvoidForbiddenZone(Map map, float forbiddenZoneCushion)
    {
        var d = (int)(forbiddenZoneCushion / map.Resolution);

        var width = map.Width;
        var height = map.Height;
        var pixelMaxG = map.PixelMaxG;
        var pixelPriority = map.PixelPriority;

        map.MaxPriority = -1f;

        var partitions = Partitioner.Create(0, height);

        var globalMax = float.NegativeInfinity;

        Parallel.ForEach(partitions, () => float.NegativeInfinity, (range, _, localMax) =>
            {
                var y1 = range.Item1;
                var y2 = range.Item2;
                for (var y = y1; y < y2; ++y)
                {
                    var rowBase = y * width;

                    var topY = y - d;
                    if (topY < 0)
                    {
                        topY = 0;
                    }
                    var botY = y + d;
                    if (botY >= height)
                    {
                        botY = height - 1;
                    }
                    var topBase = topY * width;
                    var curBase = rowBase;
                    var botBase = botY * width;

                    for (var x = 0; x < width; ++x)
                    {
                        var idx = rowBase + x;

                        // only penalize safe cells near danger
                        if (pixelMaxG[idx] == float.MaxValue)
                        {
                            var leftX = x - d;
                            if (leftX < 0)
                            {
                                leftX = 0;
                            }
                            var rightX = x + d;
                            if (rightX >= width)
                            {
                                rightX = width - 1;
                            }

                            // check the 8 clamped neighbors (Chebyshev ring at distance d)
                            if (pixelMaxG[topBase + leftX] != float.MaxValue ||
                                pixelMaxG[topBase + x] != float.MaxValue ||
                                pixelMaxG[topBase + rightX] != float.MaxValue ||
                                pixelMaxG[curBase + leftX] != float.MaxValue ||
                                pixelMaxG[curBase + rightX] != float.MaxValue ||
                                pixelMaxG[botBase + leftX] != float.MaxValue ||
                                pixelMaxG[botBase + x] != float.MaxValue ||
                                pixelMaxG[botBase + rightX] != float.MaxValue)
                            {
                                pixelPriority[idx] -= 0.125f;
                            }
                        }

                        // track local maximum priority
                        var p = pixelPriority[idx];
                        if (p > localMax)
                        {
                            localMax = p;
                        }
                    }
                }
                return localMax;
            },
            localMax =>
            {
                float init, newVal;
                do
                {
                    init = globalMax;
                    newVal = localMax > init ? localMax : init;
                }
                while (init != Interlocked.CompareExchange(ref globalMax, newVal, init));
            });

        map.MaxPriority = globalMax;
    }

    private static void RasterizeForbiddenZones(Map map, (ShapeDistance shapeDistance, DateTime activation, ulong source)[] zones, DateTime current)
    {
        // very slight difference in activation times cause issues for pathfinding - cluster them together
        var lenZones = zones.Length;
        var zonesFixed = new ShapeDistance[lenZones];
        var gFixed = new float[lenZones];
        DateTime clusterEnd = default, globalStart = current, globalEnd = current.AddSeconds(120d);
        float clusterG = default;

        for (var i = 0; i < lenZones; ++i)
        {
            ref var zone = ref zones[i];
            var activation = zone.activation.Clamp(globalStart, globalEnd);
            if (activation > clusterEnd)
            {
                clusterG = ActivationToG(activation, current);
                clusterEnd = activation.AddSeconds(0.5d);
            }
            zonesFixed[i] = zone.shapeDistance;
            gFixed[i] = clusterG;
        }

        var width = map.Width;
        var pixelMaxG = map.PixelMaxG;
        var pixelPriority = map.PixelPriority;
        var height = map.Height;
        var lenPixelMaxG = pixelMaxG.Length;

        var resolution = map.Resolution;
        var cushion = resolution * 0.5f;
        map.MaxG = clusterG;

        var dy = map.LocalZDivRes * resolution * resolution;
        var dx = dy.OrthoL();
        var topLeft = map.Center - (width >> 1) * dx - (height >> 1) * dy;

        // Partition rows so each worker computes its own local 'top-edge' scratch for rows [ys..ye]
        var rangePartitioner = Partitioner.Create(0, height);

        Parallel.ForEach(rangePartitioner, range =>
        {
            var ys = range.Item1;
            var ye = range.Item2;
            var rowsToCompute = Math.Min(ye, height) - ys;
            // allocate local scratch for rowsToCompute + 1 (extra row if available)
            var scratchRows = rowsToCompute + 1; // the +1 may correspond to row ys+rowsToCompute (i.e., ye)
            var localScratch = ArrayPool<float>.Shared.Rent(scratchRows * width);
            Span<int> idxBuf0 = stackalloc int[lenZones];
            Span<int> idxBuf1 = stackalloc int[lenZones];
            var prevIdx = idxBuf0;
            var curIdx = idxBuf1;
            var prevCount = 0;
            var curCount = 0;

            try
            {
                // for r in [0..scratchRows), compute row r's scratch + active set,
                // then immediately process row (r-1) using prev active set and row r as bottom edge.
                for (var r = 0; r < scratchRows; ++r)
                {
                    var row = ys + r;

                    // build active set for current row (unless beyond bottom)
                    if (row < height)
                    {
                        var rowCenter = topLeft + (row + 0.5f) * dy;
                        curCount = 0;
                        for (var i = 0; i < lenZones; ++i)
                        {
                            if (zonesFixed[i].RowIntersectsShape(rowCenter, dx, width, cushion))
                            {
                                curIdx[curCount++] = i;
                            }
                        }
                    }
                    else
                    {
                        // past the arena bottom; no active shapes for this pseudo-row
                        curCount = 0;
                    }

                    // fill top-edge scratch for row r
                    {
                        var baseIndex = r * width;

                        if (row >= height || curCount == 0)
                        {
                            // either out of bounds or no zones affect this row
                            for (var x = 0; x < width; ++x)
                            {
                                localScratch[baseIndex + x] = float.MaxValue;
                            }
                        }
                        else
                        {
                            var slice = curIdx[..curCount];
                            var rowStartPos = topLeft + row * dy;
                            var leftPos = rowStartPos;
                            var leftG = CalculateMaxG(slice, zonesFixed, gFixed, leftPos);
                            var x = 0;

                            while (x < width)
                            {
                                var idx = row * width + x;
                                if (pixelMaxG[idx] < 0f)
                                {
                                    // blocked run
                                    do
                                    {
                                        localScratch[baseIndex + x] = float.MaxValue;
                                        leftPos += dx;
                                        ++x;
                                        if (x >= width)
                                        {
                                            break;
                                        }
                                        idx = row * width + x;
                                    } while (pixelMaxG[idx] < 0f);

                                    if (x < width)
                                    {
                                        var rightG2 = CalculateMaxG(slice, zonesFixed, gFixed, leftPos);
                                        leftG = rightG2; // advance boundary corner
                                    }
                                    continue;
                                }

                                // normal cell
                                var rightPos = leftPos + dx;
                                var rightG = CalculateMaxG(slice, zonesFixed, gFixed, rightPos);
                                localScratch[baseIndex + x] = Math.Min(leftG, rightG);
                                leftPos = rightPos;
                                leftG = rightG;
                                ++x;
                            }
                        }
                    }

                    // process previous real row (y = ys + r - 1) as soon as bottom edge (row r) is ready
                    if (r > 0)
                    {
                        var y = ys + r - 1;
                        if (y < height)
                        {
                            var rowBase = (r - 1) * width; // top edge for row y
                            var nextRowBase = r * width; // bottom edge for row y
                            var slicePrev = prevIdx[..prevCount];

                            for (var x = 0; x < width; ++x)
                            {
                                var idx = y * width + x;
                                if (pixelMaxG[idx] < 0f)
                                {
                                    continue;
                                }

                                var topG = localScratch[rowBase + x];

                                float bottomG;
                                if (y + 1 < height)
                                {
                                    bottomG = localScratch[nextRowBase + x];
                                }
                                else
                                {
                                    // bottom-most real row: compute corner directly
                                    var cornerPos = topLeft + (y + 1) * dy + x * dx;
                                    bottomG = CalculateMaxG(slicePrev, zonesFixed, gFixed, cornerPos);
                                }

                                var cellEdgeG = Math.Min(Math.Min(topG, bottomG), pixelMaxG[idx]);

                                // center check (needed for shapes like cones that might not intersect a corner
                                var centerPos = topLeft + (y + 0.5f) * dy + (x + 0.5f) * dx;
                                var centerG = CalculateMaxGCenter(slicePrev, zonesFixed, gFixed, centerPos, cushion);

                                var finalG = Math.Min(cellEdgeG, centerG);

                                var oldVal = pixelMaxG[idx];
                                if (finalG < oldVal)
                                {
                                    pixelMaxG[idx] = finalG;
                                    if (oldVal == float.MaxValue)
                                    {
                                        pixelPriority[idx] = float.MinValue;
                                    }
                                }
                            }
                        }
                    }

                    // swap ring slots
                    var tmpIdx = prevIdx;
                    prevIdx = curIdx;
                    curIdx = tmpIdx;
                    (prevCount, curCount) = (curCount, prevCount);
                }
            }
            finally
            {
                ArrayPool<float>.Shared.Return(localScratch, clearArray: false);
            }
        });

        for (var i = 0; i < lenPixelMaxG; ++i)
        {
            if (pixelMaxG[i] == float.MaxValue)
            {
                return;
            }
        }
        // everything is dangerous, clear least dangerous so that pathfinding works reasonably
        // note that max value could be smaller than MaxG, if more dangerous stuff overlaps it
        var realMaxG = 0f;
        for (var iCell = 0; iCell < lenPixelMaxG; ++iCell)
        {
            realMaxG = Math.Max(realMaxG, pixelMaxG[iCell]);
        }
        for (var iCell = 0; iCell < lenPixelMaxG; ++iCell)
        {
            if (pixelMaxG[iCell] == realMaxG)
            {
                pixelMaxG[iCell] = float.MaxValue;
                pixelPriority[iCell] = default;
            }
        }

        static float CalculateMaxGCenter(ReadOnlySpan<int> idx, ShapeDistance[] shapeDistance, float[] g, in WPos p, float cushion = default)
        {
            // assumes signed distance: inside < 0; on boundary == 0; outside > 0.
            // threshold > 0 inflates by that margin (used for center cushion).
            // zones are already sorted by activation time in AIHints.Normalize(), so we can exit early on first match
            var count = idx.Length;
            var threshold = cushion;
            for (var i = 0; i < count; ++i)
            {
                var id = idx[i];
                if (shapeDistance[id].Distance(p) <= threshold)
                {
                    return g[id];
                }
            }
            return float.MaxValue;
        }

        static float CalculateMaxG(ReadOnlySpan<int> idx, ShapeDistance[] shapeDistance, float[] g, in WPos p)
        {
            // pip test for corners
            var count = idx.Length;
            for (var i = 0; i < count; ++i)
            {
                var id = idx[i];
                if (shapeDistance[id].Contains(p))
                {
                    return g[id];
                }
            }
            return float.MaxValue;
        }
        static float ActivationToG(DateTime activation, DateTime current) => Math.Max(0f, (float)(activation - current).TotalSeconds - ActivationTimeCushion);
    }

    public static void RasterizeGoalZones(Map map, Func<WPos, float>[] goals)
    {
        var resolution = map.Resolution;
        var width = map.Width;
        var height = map.Height;
        var dy = map.LocalZDivRes * resolution * resolution;
        var dx = dy.OrthoL();
        var topLeft = map.Center - (width >> 1) * dx - (height >> 1) * dy;
        var len = goals.Length;

        var globalMaxPriority = float.MinValue;
        var rangePartitioner = Partitioner.Create(0, height);
        var pixelMaxG = map.PixelMaxG;
        var pixelPriority = map.PixelPriority;

        Parallel.ForEach(rangePartitioner, () => float.MinValue, (range, loopState, localMax) =>
            {
                var ys = range.Item1;
                var ye = range.Item2;

                var rows = Math.Max(0, Math.Min(ye, height) - ys);
                var scratchRows = rows + 1; // extra row for bottom corners
                var scratchLen = scratchRows * width;

                var localScratch = ArrayPool<float>.Shared.Rent(scratchLen);
                try
                {
                    // Fill corner mins for each scratch row
                    for (var r = 0; r < scratchRows; ++r)
                    {
                        var row = ys + r;
                        var baseIdx = r * width;

                        var rowCorner = topLeft + row * dy;
                        var leftPos = rowCorner;

                        // compute left corner sum
                        var leftP = 0f;
                        for (var i = 0; i < len; ++i)
                        {
                            leftP += goals[i](leftPos);
                        }

                        for (var x = 0; x < width; ++x)
                        {
                            var rightPos = leftPos + dx;
                            var rightP = 0f;
                            for (var i = 0; i < len; ++i)
                            {
                                rightP += goals[i](rightPos);
                            }

                            localScratch[baseIdx + x] = Math.Min(leftP, rightP);

                            leftPos = rightPos;
                            leftP = rightP;
                        }
                    }

                    // produce final cell priorities
                    for (var y = ys; y < ye; ++y)
                    {
                        var rowBase = (y - ys) * width;
                        var nextRowBase = rowBase + width;

                        var idx = y * width;
                        for (var x = 0; x < width; ++x, ++idx)
                        {
                            var topP = localScratch[rowBase + x];
                            var bottomP = localScratch[nextRowBase + x];

                            var cellP = (pixelMaxG[idx] == float.MaxValue) ? Math.Min(topP, bottomP) : float.MinValue;
                            if (cellP != float.MinValue)
                            {
                                pixelPriority[idx] = cellP;
                            }

                            if (cellP > localMax)
                            {
                                localMax = cellP;
                            }
                        }
                    }
                    return localMax;
                }
                finally
                {
                    ArrayPool<float>.Shared.Return(localScratch, clearArray: false);
                }
            },
            localMax =>
            {
                float initVal, newVal;
                do
                {
                    initVal = globalMaxPriority;
                    newVal = Math.Max(initVal, localMax);
                }
                while (initVal != Interlocked.CompareExchange(ref globalMaxPriority, newVal, initVal));
            });

        map.MaxPriority = globalMaxPriority;
    }

    public static void RasterizeVoidzones(Map map, ShapeDistance[] zones)
    {
        var len = zones.Length;
        var width = map.Width;
        var height = map.Height;

        var resolution = map.Resolution;
        var cushion = resolution * 0.5f;

        var dy = map.LocalZDivRes * resolution * resolution;
        var dx = dy.OrthoL();
        var topLeft = map.Center - (width >> 1) * dx - (height >> 1) * dy;

        var partitioner = Partitioner.Create(0, height);
        var pixelMaxG = map.PixelMaxG;
        var pixelPriority = map.PixelPriority;

        Parallel.ForEach(partitioner, range =>
        {
            var ys = range.Item1;
            var ye = range.Item2;

            Span<int> activeIdx = stackalloc int[len];
            var count = 0;
            var slice = activeIdx;

            for (var y = ys; y < ye; ++y)
            {
                var rowCenter = topLeft + (y + 0.5f) * dy;
                count = 0;
                for (var i = 0; i < len; ++i)
                {
                    ref var z = ref zones[i];
                    if (z.RowIntersectsShape(rowCenter, dx, width, cushion))
                    {
                        activeIdx[count++] = i;
                    }
                }

                if (count == 0)
                {
                    continue;
                }
                slice = activeIdx[..count];
                var rowBaseIndex = y * width;
                var rowTopLeft = topLeft + y * dy;

                for (var x = 0; x < width; ++x)
                {
                    var idx = rowBaseIndex + x;

                    if (pixelMaxG[idx] < 0f)
                    {
                        continue; // arena bounds already blocked
                    }

                    var cellTopLeft = rowTopLeft + x * dx;

                    var tl = cellTopLeft;
                    var tr = cellTopLeft + dx;
                    var bl = cellTopLeft + dy;
                    var br = cellTopLeft + dx + dy;
                    var center = cellTopLeft + (dx * 0.5f + dy * 0.5f);

                    for (var j = 0; j < count; ++j)
                    {
                        ref var shape = ref zones[slice[j]];

                        // center with cushion, corners without
                        if (shape.Distance(center) <= cushion || shape.Contains(tl) || shape.Contains(br) || shape.Contains(tr) || shape.Contains(bl))
                        {
                            pixelMaxG[idx] = -1f;
                            pixelPriority[idx] = float.MinValue;
                            break;
                        }
                    }
                }
            }
        });
    }

    private static (WPos? first, WPos? second) GetFirstWaypoints(ThetaStar pf, Map map, int cell, WPos startingPos)
    {
        ref var startingNode = ref pf.NodeByIndex(cell);

        if (startingNode.GScore == 0f && startingNode.PathMinG == float.MaxValue)
        {
            return default; // we're already in safe zone
        }

        var nextCell = cell;
        var iterations = 0; // iteration counter to prevent rare cases of infinite loops
        var maxIterations = map.Width * map.Height;
        do
        {
            ref var node = ref pf.NodeByIndex(cell);
            if (pf.NodeByIndex(node.ParentIndex).GScore == 0f || ++iterations == maxIterations)
            {
                var destCoord = map.IndexToGrid(cell);
                var playerCoordFrac = map.WorldToGridFrac(startingPos);
                var playerCoord = Map.FracToGrid(playerCoordFrac);
                if (destCoord == playerCoord)
                {
                    return default;
                }
                return (pf.CellCenter(cell), pf.CellCenter(nextCell));
            }
            nextCell = cell;
            cell = node.ParentIndex;
        }
        while (true);
    }
}
