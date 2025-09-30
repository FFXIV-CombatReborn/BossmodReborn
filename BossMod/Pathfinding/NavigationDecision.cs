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

    public const float ActivationTimeCushion = 1f; // reduce time between now and activation by this value in seconds; increase for more conservativeness

    public static NavigationDecision Build(Context ctx, WorldState ws, AIHints hints, Actor player, float playerSpeed = 6f, float forbiddenZoneCushion = default)
    {
        // build a pathfinding map: rasterize all forbidden zones and goals
        hints.InitPathfindMap(ctx.Map);
        // local copies of forbidden zones and goals to ensure no race conditions during async pathfinding
        (ShapeDistance, DateTime, ulong)[] localForbiddenZones = [.. hints.ForbiddenZones];
        Func<WPos, float>[] localGoalZones = [.. hints.GoalZones];
        ShapeDistance[] localTemporaryObstacles = [.. hints.TemporaryObstacles];
        if (localTemporaryObstacles.Length != 0)
        {
            RasterizeVoidzones(ctx.Map, localTemporaryObstacles);
        }
        if (localForbiddenZones.Length != 0)
        {
            RasterizeForbiddenZones(ctx.Map, localForbiddenZones, ws.CurrentTime);
        }
        if (player.CastInfo == null) // don't rasterize goal zones if casting or if inside a very dangerous pixel
        {
            var index = ctx.Map.GridToIndex(ctx.Map.WorldToGrid(player.Position));
            var len = ctx.Map.PixelMaxG.Length;
            if (index >= 0 && len > index && ctx.Map.PixelMaxG[index] >= 1f || index < 0 || index >= len) // prioritize safety over uptime
            {
                if (localGoalZones.Length != 0)
                {
                    RasterizeGoalZones(ctx.Map, localGoalZones);
                }
                if (forbiddenZoneCushion > 0)
                {
                    AvoidForbiddenZone(ctx.Map, forbiddenZoneCushion);
                }
            }
        }
        // execute pathfinding
        ctx.ThetaStar.Start(ctx.Map, player.Position, 1.0f / playerSpeed);
        var bestNodeIndex = ctx.ThetaStar.Execute();
        ref var bestNode = ref ctx.ThetaStar.NodeByIndex(bestNodeIndex);
        var waypoints = GetFirstWaypoints(ctx.ThetaStar, ctx.Map, bestNodeIndex, player.Position);
        return new() { Destination = waypoints.first, NextWaypoint = waypoints.second, LeewaySeconds = bestNode.PathLeeway, TimeToGoal = bestNode.GScore };
    }

    private static void AvoidForbiddenZone(Map map, float forbiddenZoneCushion)
    {
        var d = (int)(forbiddenZoneCushion / map.Resolution);
        map.MaxPriority = -1f;
        var pixels = map.EnumeratePixels();
        var len = pixels.Length;
        for (var i = 0; i < len; ++i)
        {
            var p = pixels[i];
            var px = p.x;
            var py = p.y;
            var cellIndex = map.GridToIndex(px, py);
            if (map.PixelMaxG[cellIndex] == float.MaxValue)
            {
                for (var ox = -1; ox <= 1; ++ox)
                {
                    for (var oy = -1; oy <= 1; ++oy)
                    {
                        if (ox == 0 && oy == 0)
                        {
                            continue;
                        }
                        var (nx, ny) = map.ClampToGrid((px + ox * d, py + oy * d));
                        if (map.PixelMaxG[map.GridToIndex(nx, ny)] != float.MaxValue)
                        {
                            map.PixelPriority[cellIndex] -= 0.125f;
                            goto next;
                        }
                    }
                }
            }
        next:
            map.MaxPriority = Math.Max(map.MaxPriority, map.PixelPriority[cellIndex]);
        }
    }

    private static void RasterizeForbiddenZones(Map map, (ShapeDistance shapeDistance, DateTime activation, ulong source)[] zones, DateTime current)
    {
        // very slight difference in activation times cause issues for pathfinding - cluster them together
        var lenZones = zones.Length;
        var zonesFixed = new (ShapeDistance shapeDistance, float g)[lenZones];
        DateTime clusterEnd = default, globalStart = current, globalEnd = current.AddSeconds(120d);
        float clusterG = 0;

        for (var i = 0; i < lenZones; ++i)
        {
            ref var zone = ref zones[i];
            var activation = zone.activation.Clamp(globalStart, globalEnd);
            if (activation > clusterEnd)
            {
                clusterG = ActivationToG(activation, current);
                clusterEnd = activation.AddSeconds(0.5d);
            }
            zonesFixed[i] = (zone.shapeDistance, clusterG);
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
            try
            {
                var shapesInRowBuf = new List<(ShapeDistance shapeDistance, float g)>(lenZones);
                // compute top-edge mins for rows ys .. ys+rowsToCompute (if row < height)
                for (var r = 0; r < scratchRows; ++r)
                {
                    var row = ys + r;

                    if (row >= height)
                    {
                        // beyond bottom; treat as outside arena
                        var baseIdx = r * width;
                        for (var x1 = 0; x1 < width; ++x1)
                        {
                            localScratch[baseIdx + x1] = float.MaxValue;
                        }
                        continue;
                    }

                    shapesInRowBuf.Clear();
                    var rowCenter = topLeft + (row + 0.5f) * dy;
                    for (var i = 0; i < lenZones; ++i)
                    {
                        ref var zone = ref zonesFixed[i];
                        if (zone.shapeDistance.RowIntersectsShape(rowCenter, dx, width, cushion))
                        {
                            shapesInRowBuf.Add(zone);
                        }
                    }
                    if (shapesInRowBuf.Count == 0)
                    {
                        // no zones affect this row → scratch float max value
                        var baseIdx = r * width;
                        for (var x1 = 0; x1 < width; ++x1)
                        {
                            localScratch[baseIdx + x1] = float.MaxValue;
                        }
                        continue;
                    }

                    var rowStartPos = topLeft + row * dy;
                    var leftPos = rowStartPos;
                    var leftG = CalculateMaxG(shapesInRowBuf, leftPos);
                    var baseIndex = r * width;
                    var x = 0;
                    while (x < width)
                    {
                        var idx = row * width + x;
                        if (pixelMaxG[idx] < 0f)
                        {
                            // start of a blocked run
                            var runStart = x;
                            do
                            {
                                localScratch[baseIndex + x] = float.MaxValue;
                                leftPos += dx;
                                ++x;
                                idx = row * width + x;
                            }
                            while (x < width && pixelMaxG[idx] < 0f);

                            // compute right corner once at the run boundary
                            if (x < width)
                            {
                                var rightG2 = CalculateMaxG(shapesInRowBuf, leftPos);
                                // advance chain: leftG becomes boundary corner
                                leftG = rightG2;
                            }
                            continue;
                        }

                        // normal cell
                        var rightPos = leftPos + dx;
                        var rightG = CalculateMaxG(shapesInRowBuf, rightPos);
                        localScratch[baseIndex + x] = Math.Min(leftG, rightG);
                        leftPos = rightPos;
                        leftG = rightG;
                        ++x;
                    }
                }

                // Now process rows ys .. ye-1 using top-edge (localScratch[row-ys]) and bottom-edge (localScratch[row+1-ys] or direct calc if bottom-most)
                for (var y = ys; y < ye; ++y)
                {
                    if (y >= height)
                    {
                        break;
                    }

                    shapesInRowBuf.Clear();
                    var rowCenter = topLeft + (y + 0.5f) * dy;

                    for (var i = 0; i < lenZones; ++i)
                    {
                        ref var zone = ref zonesFixed[i];
                        if (zone.shapeDistance.RowIntersectsShape(rowCenter, dx, width, cushion))
                        {
                            shapesInRowBuf.Add(zone);
                        }
                    }
                    if (shapesInRowBuf.Count == 0)
                    {
                        continue; // whole row unaffected
                    }
                    var rowBase = (y - ys) * width;
                    var nextRowBase = rowBase + width; // index for row+1 within localScratch

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
                            // bottom-most row: compute corner at y+1 == height
                            var cornerPos = topLeft + (y + 1) * dy + x * dx;
                            bottomG = CalculateMaxG(shapesInRowBuf, cornerPos);
                        }

                        // merge with existing PixelMaxG
                        var cellEdgeG = Math.Min(Math.Min(topG, bottomG), pixelMaxG[idx]);

                        // center check with cushion, this is needed for shapes that can intersect cells between corners
                        var centerPos = topLeft + (y + 0.5f) * dy + (x + 0.5f) * dx;
                        var centerG = CalculateMaxGCenter(shapesInRowBuf, centerPos, cushion);

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

        static float CalculateMaxGCenter(List<(ShapeDistance shapeDistance, float g)> zones, in WPos p, float cushion = default)
        {
            // assumes signed distance: inside < 0; on boundary == 0; outside > 0.
            // threshold > 0 inflates by that margin (used for center cushion).
            // zones are already sorted by activation time in AIHints.Normalize(), so we can exit early on first match
            var count = zones.Count;
            var threshold = cushion;
            for (var i = 0; i < count; ++i)
            {
                var z = zones[i];
                if (z.shapeDistance.Distance(p) <= threshold)
                {
                    return z.g;
                }
            }
            return float.MaxValue;
        }

        static float CalculateMaxG(List<(ShapeDistance shapeDistance, float g)> zones, in WPos p)
        {
            // pip test for corners
            var count = zones.Count;
            for (var i = 0; i < count; ++i)
            {
                var z = zones[i];
                if (z.shapeDistance.Contains(p))
                {
                    return z.g;
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

            var shapesInRow = new List<ShapeDistance>(len);

            for (var y = ys; y < ye; ++y)
            {
                var rowCenter = topLeft + (y + 0.5f) * dy;
                shapesInRow.Clear();

                for (var j = 0; j < len; ++j)
                {
                    var s = zones[j];
                    if (s.RowIntersectsShape(rowCenter, dx, width, cushion))
                    {
                        shapesInRow.Add(s);
                    }
                }

                var count = shapesInRow.Count;
                if (count == 0)
                {
                    continue;
                }

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
                        var shape = shapesInRow[j];

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
