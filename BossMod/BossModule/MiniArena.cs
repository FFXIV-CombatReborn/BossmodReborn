using Dalamud.Bindings.ImGui;

namespace BossMod;

// note on coordinate systems:
// - world coordinates - X points West to East, Z points North to South - so SE is corner with both maximal coords, NW is corner with both minimal coords
//                       rotation 0 corresponds to South, and increases counterclockwise (so East is +pi/2, North is pi, West is -pi/2)
// - camera azimuth 0 correpsonds to camera looking North and increases counterclockwise
// - screen coordinates - X points left to right, Y points top to bottom
[SkipLocalsInit]
public sealed class MiniArena(WPos center, ArenaBounds bounds)
{
    public static readonly BossModuleConfig Config = Service.Config.Get<BossModuleConfig>();
    private WPos _center = center;
    private readonly TriangulationCache _triCache = new();

    public WPos Center
    {
        get => _center;
        set
        {
            if (_center != value)
            {
                _center = value;
                _triCache.Invalidate();
            }
        }
    }

    private ArenaBounds _bounds = bounds;
    public ArenaBounds Bounds
    {
        get => _bounds;
        set
        {
            if (!ReferenceEquals(_bounds, value))
            {
                _bounds = value;
                _triCache.Invalidate();
            }
        }
    }

    public float ScreenHalfSize => 150f * Config.ArenaScale;
    public float ScreenMarginSize => 20f * Config.ArenaScale;

    // these are set at the beginning of each draw
    public Vector2 ScreenCenter;
    private Angle _cameraAzimuth;
    private float _cameraSinAzimuth;
    private float _cameraCosAzimuth = 1f;

    public bool InBounds(WPos position) => _bounds.Contains(position - _center);
    public WPos ClampToBounds(WPos position) => _center + _bounds.ClampToBounds(position - _center);
    public float IntersectRayBounds(WPos rayOrigin, WDir rayDir) => _bounds.IntersectRay(rayOrigin - _center, rayDir);

    // prepare for drawing - set up internal state, clip rect etc.
    public void Begin(Angle cameraAzimuth)
    {
        var centerOffset = new Vector2(ScreenMarginSize + Config.SlackForRotations * ScreenHalfSize);
        var fullSize = 2f * centerOffset;
        var currentWindowSize = ImGui.GetWindowSize();
        var requiredWindowSize = Vector2.Max(fullSize, currentWindowSize);
        ImGui.SetWindowSize(requiredWindowSize);
        var cursor = ImGui.GetCursorScreenPos();
        ImGui.Dummy(fullSize);

        if (_bounds.ScreenHalfSize != ScreenHalfSize)
        {
            _bounds.ScreenHalfSize = ScreenHalfSize;
            _triCache.Invalidate();
        }
        else
        {
            _triCache.NextFrame();
        }

        ScreenCenter = cursor + centerOffset;

        _cameraAzimuth = cameraAzimuth;
        (_cameraSinAzimuth, _cameraCosAzimuth) = MathF.SinCos(cameraAzimuth.Rad);
        var wmin = ImGui.GetWindowPos();
        var wmax = wmin + ImGui.GetWindowSize();
        ImGui.GetWindowDrawList().PushClipRect(Vector2.Max(cursor, wmin), Vector2.Min(cursor + fullSize, wmax));

        if (Config.OpaqueArenaBackground)
        {
            Zone(_bounds.ShapeTriangulation, Colors.Background);
        }
    }

    // if you are 100% sure your primitive does not need clipping, you can use drawlist api directly
    // this helper allows converting world-space coords to screen-space ones
    public Vector2 WorldPositionToScreenPosition(WPos p)
    {
        return ScreenCenter + WorldOffsetToScreenOffset(p - Center);
        //var viewPos = SharpDX.Vector3.Transform(new SharpDX.Vector3(worldOffset.X, 0, worldOffset.Z), CameraView);
        //return ScreenHalfSize * new Vector2(viewPos.X / viewPos.Z, viewPos.Y / viewPos.Z);
        //return ScreenHalfSize * new Vector2(viewPos.X, viewPos.Y) / WorldHalfSize;
    }

    // this is useful for drawing on margins (TODO better api)
    public Vector2 RotatedCoords(Vector2 coords)
    {
        var cx = coords.X;
        var cy = coords.Y;
        var x = cx * _cameraCosAzimuth - cy * _cameraSinAzimuth;
        var y = cy * _cameraCosAzimuth + cx * _cameraSinAzimuth;
        return new(x, y);
    }

    private Vector2 WorldOffsetToScreenOffset(WDir worldOffset)
    {
        return ScreenHalfSize * RotatedCoords(new(worldOffset.X, worldOffset.Z)) / _bounds.Radius;
    }

    // unclipped primitive rendering that accept world-space positions; thin convenience wrappers around drawlist api
    public void AddLine(WPos a, WPos b, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        if (Config.ShowOutlinesAndShadows)
            ImGui.GetWindowDrawList().AddLine(WorldPositionToScreenPosition(a), WorldPositionToScreenPosition(b), Colors.Shadows, thickness + 1f);
        ImGui.GetWindowDrawList().AddLine(WorldPositionToScreenPosition(a), WorldPositionToScreenPosition(b), color != default ? color : Colors.Danger, thickness);
    }

    public void AddTriangle(WPos p1, WPos p2, WPos p3, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        ImGui.GetWindowDrawList().AddTriangle(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), color != default ? color : Colors.Danger, thickness);
    }

    public void AddTriangleFilled(WPos p1, WPos p2, WPos p3, uint color = default)
    {
        ImGui.GetWindowDrawList().AddTriangleFilled(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), color != default ? color : Colors.Danger);
    }

    public void AddQuad(WPos p1, WPos p2, WPos p3, WPos p4, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        ImGui.GetWindowDrawList().AddQuad(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), WorldPositionToScreenPosition(p4), color != default ? color : Colors.Danger, thickness);
    }

    public void AddQuadFilled(WPos p1, WPos p2, WPos p3, WPos p4, uint color = default)
    {
        ImGui.GetWindowDrawList().AddQuadFilled(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), WorldPositionToScreenPosition(p4), color != default ? color : Colors.Danger);
    }

    public void AddRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        var side = halfWidth * direction.OrthoR();
        var front = origin + lenFront * direction;
        var back = origin - lenBack * direction;
        AddQuad(front + side, front - side, back - side, back + side, color, thickness);
    }

    public void AddCircle(WPos center, float radius, uint color = default, float thickness = 1f)
    {
        var radiusscreenhalfsize = radius / _bounds.Radius * ScreenHalfSize;
        thickness *= Config.ThicknessScale;
        if (Config.ShowOutlinesAndShadows)
            ImGui.GetWindowDrawList().AddCircle(WorldPositionToScreenPosition(center), radiusscreenhalfsize, Colors.Shadows, default, thickness + 1f);
        ImGui.GetWindowDrawList().AddCircle(WorldPositionToScreenPosition(center), radiusscreenhalfsize, color != default ? color : Colors.Danger, default, thickness);
    }

    public void AddCircleFilled(WPos center, float radius, uint color = default)
    {
        ImGui.GetWindowDrawList().AddCircleFilled(WorldPositionToScreenPosition(center), radius / _bounds.Radius * ScreenHalfSize, color != default ? color : Colors.Danger);
    }

    public void AddCone(WPos center, float radius, Angle centerDirection, Angle halfAngle, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        var sCenter = WorldPositionToScreenPosition(center);
        var sDir = Angle.HalfPi - centerDirection.Rad + _cameraAzimuth.Rad;
        var drawlist = ImGui.GetWindowDrawList();
        drawlist.PathLineTo(sCenter);
        drawlist.PathArcTo(sCenter, radius / _bounds.Radius * ScreenHalfSize, sDir - halfAngle.Rad, sDir + halfAngle.Rad);
        drawlist.PathStroke(color != default ? color : Colors.Danger, ImDrawFlags.Closed, thickness);
    }

    public void AddDonutCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        var sCenter = WorldPositionToScreenPosition(center);
        var sDir = Angle.HalfPi - centerDirection.Rad + _cameraAzimuth.Rad;
        var drawlist = ImGui.GetWindowDrawList();
        var sDirP = sDir + halfAngle.Rad;
        var sDirN = sDir - halfAngle.Rad;
        var radius = _bounds.Radius;
        var screenHalfSize = ScreenHalfSize;
        drawlist.PathArcTo(sCenter, innerRadius / radius * screenHalfSize, sDirP, sDirN);
        drawlist.PathArcTo(sCenter, outerRadius / radius * screenHalfSize, sDirN, sDirP);
        drawlist.PathStroke(color != default ? color : Colors.Danger, ImDrawFlags.Closed, thickness);
    }

    public void AddCapsule(WPos start, WDir direction, float radius, float length, uint color = default, float thickness = 1f)
    {
        var dirNorm = direction.Normalized();
        var halfLengthdirNorm = dirNorm * length * 0.5f;
        var capsuleStart = start - halfLengthdirNorm;
        var capsuleEnd = start + halfLengthdirNorm;
        var orthoDir = dirNorm.OrthoR();

        var drawList = ImGui.GetWindowDrawList();

        var screenRadius = radius / _bounds.Radius * ScreenHalfSize;
        var screenCapsuleStart = WorldPositionToScreenPosition(capsuleStart);
        var screenCapsuleEnd = WorldPositionToScreenPosition(capsuleEnd);

        var dirAngle = MathF.Atan2(dirNorm.Z, dirNorm.X);
        var sDirAngle = Angle.HalfPi - dirAngle + _cameraAzimuth.Rad;
        var dirMHalfPI = sDirAngle - Angle.HalfPi;
        var dirPHalfPI = sDirAngle + Angle.HalfPi;
        var orthoDirRadius = orthoDir * radius;

        // Start path at capsuleStart + orthoDir * radius
        drawList.PathLineTo(WorldPositionToScreenPosition(capsuleStart + orthoDirRadius));

        // Line to capsuleEnd + orthoDir * radius
        drawList.PathLineTo(WorldPositionToScreenPosition(capsuleEnd + orthoDirRadius));

        // Arc around capsuleEnd from sDirAngle - π/2 to sDirAngle + π/2
        drawList.PathArcTo(screenCapsuleEnd, screenRadius, dirMHalfPI, dirPHalfPI);

        // Line back to capsuleStart - orthoDir * radius
        drawList.PathLineTo(WorldPositionToScreenPosition(capsuleStart - orthoDirRadius));

        // Arc around capsuleStart from sDirAngle + π/2 to sDirAngle - π/2
        drawList.PathArcTo(screenCapsuleStart, screenRadius, dirPHalfPI, dirMHalfPI);

        drawList.PathStroke(color != default ? color : Colors.Danger, ImDrawFlags.Closed, thickness);
    }

    public void AddPolygon(ReadOnlySpan<WPos> vertices, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        var len = vertices.Length;
        for (var i = 0; i < len; ++i)
            PathLineTo(vertices[i]);
        PathStroke(true, color != default ? color : Colors.Danger, thickness);
    }

    public void AddPolygon(IEnumerable<WPos> vertices, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        foreach (var p in vertices)
            PathLineTo(p);
        PathStroke(true, color != default ? color : Colors.Danger, thickness);
    }

    public void AddPolygonTransformed(WPos center, WDir rotation, ReadOnlySpan<WDir> vertices, uint color, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        foreach (var p in vertices)
            PathLineTo(center + p.Rotate(rotation));
        PathStroke(true, color != default ? color : Colors.Danger, thickness);
    }

    public void AddComplexPolygon(WPos center, WDir rotation, RelSimplifiedComplexPolygon poly, uint color, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;

        foreach (var part in poly.Parts)
        {
            AddPolygonTransformed(center, rotation, part.Exterior, color, thickness);
            foreach (var h in part.Holes)
                AddPolygonTransformed(center, rotation, part.Interior(h), color, thickness);
        }
    }

    // path api: add new point to path; this adds new edge from last added point, or defines first vertex if path is empty
    public void PathLineTo(WPos p)
    {
        ImGui.GetWindowDrawList().PathLineToMergeDuplicate(WorldPositionToScreenPosition(p));
    }

    // adds a bunch of points corresponding to arc - if path is non empty, this adds an edge from last point to first arc point
    public void PathArcTo(WPos center, float radius, float amin, float amax)
    {
        ImGui.GetWindowDrawList().PathArcTo(WorldPositionToScreenPosition(center), radius / _bounds.Radius * ScreenHalfSize, Angle.HalfPi - amin + _cameraAzimuth.Rad, Angle.HalfPi - amax + _cameraAzimuth.Rad);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PathStroke(bool closed, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        ImGui.GetWindowDrawList().PathStroke(color != default ? color : Colors.Danger, closed ? ImDrawFlags.Closed : ImDrawFlags.None, thickness);
    }

    public static void PathFillConvex(uint color = default)
    {
        ImGui.GetWindowDrawList().PathFillConvex(color != default ? color : Colors.Danger);
    }

    // draw clipped & triangulated zone
    public void Zone(List<RelTriangle> triangulation, uint color = default)
    {
        var drawlist = ImGui.GetWindowDrawList();
        var restoreFlags = drawlist.Flags;
        drawlist.Flags &= ~ImDrawListFlags.AntiAliasedFill;
        var triangles = CollectionsMarshal.AsSpan(triangulation);
        var len = triangles.Length;
        var col = color != default ? color : Colors.AOE;
        var center = ScreenCenter;

        var cosAzimuth = _cameraCosAzimuth;
        var sinAzimuth = _cameraSinAzimuth;
        var screenHalfSize = ScreenHalfSize;
        var invRadius = 1f / _bounds.Radius;

        for (var i = 0; i < len; ++i)
        {
            ref readonly var tri = ref triangles[i];
            var a = TransformCoords(tri.A);
            var b = TransformCoords(tri.B);
            var c = TransformCoords(tri.C);
            drawlist.AddTriangleFilled(center + a, center + b, center + c, col);
        }

        drawlist.Flags = restoreFlags;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Vector2 TransformCoords(WDir worldOffset)
        {
            var x0 = worldOffset.X;
            var z0 = worldOffset.Z;
            var x = x0 * cosAzimuth - z0 * sinAzimuth;
            var z = z0 * cosAzimuth + x0 * sinAzimuth;
            return screenHalfSize * new Vector2(x, z) * invRadius;
        }
    }

    // draw zones - these are filled primitives clipped to arena border; note that triangulation is cached
    public void ZoneCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color)
        => Zone(_triCache[TriangulationCache.GetKeyHash(1, center, innerRadius, outerRadius, centerDirection, halfAngle)] ??= _bounds.ClipAndTriangulateCone(center - Center, innerRadius, outerRadius, centerDirection, halfAngle), color);
    public void ZoneCircle(WPos center, float radius, uint color)
        => Zone(_triCache[TriangulationCache.GetKeyHash(2, center, radius)] ??= _bounds.ClipAndTriangulateCircle(center - Center, radius), color);
    public void ZoneDonut(WPos center, float innerRadius, float outerRadius, uint color)
        => Zone(_triCache[TriangulationCache.GetKeyHash(3, center, innerRadius, outerRadius)] ??= _bounds.ClipAndTriangulateDonut(center - Center, innerRadius, outerRadius), color);
    public void ZoneTri(WPos a, WPos b, WPos c, uint color)
        => Zone(_triCache[TriangulationCache.GetKeyHash(4, a, b, c)] ??= _bounds.ClipAndTriangulateTri(a - Center, b - Center, c - Center), color);
    public void ZoneIsoscelesTri(WPos apex, WDir height, WDir halfBase, uint color)
        => Zone(_triCache[TriangulationCache.GetKeyHash(5, apex, height, halfBase)] ??= _bounds.ClipAndTriangulateIsoscelesTri(apex - Center, height, halfBase), color);
    public void ZoneIsoscelesTri(WPos apex, Angle direction, Angle halfAngle, float height, uint color)
        => Zone(_triCache[TriangulationCache.GetKeyHash(6, apex, direction, halfAngle, height)] ??= _bounds.ClipAndTriangulateIsoscelesTri(apex - Center, direction, halfAngle, height), color);
    public void ZoneRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color)
        => Zone(_triCache[TriangulationCache.GetKeyHash(7, origin, direction, lenFront, lenBack, halfWidth)] ??= _bounds.ClipAndTriangulateRect(origin - Center, direction, lenFront, lenBack, halfWidth), color);
    public void ZoneRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth, uint color)
        => Zone(_triCache[TriangulationCache.GetKeyHash(8, origin, direction, lenFront, lenBack, halfWidth)] ??= _bounds.ClipAndTriangulateRect(origin - Center, direction, lenFront, lenBack, halfWidth), color);
    public void ZoneRect(WPos start, WPos end, float halfWidth, uint color)
        => Zone(_triCache[TriangulationCache.GetKeyHash(9, start, end, halfWidth)] ??= _bounds.ClipAndTriangulateRect(start - Center, end - Center, halfWidth), color);
    public void ZoneComplex(WPos origin, Angle direction, RelSimplifiedComplexPolygon poly, uint color)
        => Zone(_triCache[TriangulationCache.GetKeyHash(10, origin, direction, poly)] ?? _bounds.ClipAndTriangulate(poly.Transform(origin - Center, direction.ToDirection())), color);
    public void ZonePoly(object key, WPos[] contour, uint color)
    {
        var hash = TriangulationCache.GetKeyHash(11, key);
        var triangulation = _triCache[hash];
        if (triangulation == null)
        {
            var len = contour.Length;
            var adjustedContour = new WDir[len];
            for (var i = 0; i < len; ++i)
            {
                adjustedContour[i] = contour[i] - Center;
            }
            triangulation = _bounds.ClipAndTriangulate(adjustedContour);
            _triCache[hash] = triangulation;
        }
        Zone(triangulation, color);
    }
    public void ZoneRelPoly(object key, WDir[] relContour, uint color)
        => Zone(_triCache[TriangulationCache.GetKeyHash(12, key)] ??= _bounds.ClipAndTriangulate(relContour), color);
    public void ZoneRelPoly(int key, RelSimplifiedComplexPolygon poly, uint color)
        => Zone(_triCache[key] ??= _bounds.ClipAndTriangulate(poly), color);
    public void ZoneCapsule(WPos start, WDir direction, float radius, float length, uint color)
        => Zone(_triCache[TriangulationCache.GetKeyHash(12, start, direction, radius, length)] ??= _bounds.ClipAndTriangulateCapsule(start - Center, direction, radius, length), color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TextScreen(Vector2 center, string text, uint color, float fontSize = 17f)
    {
        var size = ImGui.CalcTextSize(text) * Config.ArenaScale;
        ImGui.GetWindowDrawList().AddText(ImGui.GetFont(), fontSize * Config.ArenaScale, center - size * 0.5f, color, text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TextWorld(WPos center, string text, uint color, float fontSize = 17f)
    {
        TextScreen(WorldPositionToScreenPosition(center), text, color, fontSize);
    }

    // high level utilities
    // draw arena border
    public void Border(uint color)
    {
        var dl = ImGui.GetWindowDrawList();
        var parts = _bounds.ShapeSimplified.Parts;
        var count = parts.Count;
        for (var i = 0; i < count; ++i)
        {
            var part = parts[i];
            Vector2? lastPoint = null;
            var partExt = part.Exterior;
            var exteriorLen = partExt.Length;
            for (var j = 0; j < exteriorLen; ++j)
            {
                var offset = partExt[j];
                var currentPoint = ScreenCenter + WorldOffsetToScreenOffset(offset);
                if (lastPoint != currentPoint)
                    dl.PathLineTo(currentPoint);
                lastPoint = currentPoint;
            }

            dl.PathStroke(color, ImDrawFlags.Closed, 2f);
            var holes = part.Holes;
            var lenHoles = holes.Length;
            for (var l = 0; l < lenHoles; ++l)
            {
                lastPoint = null;

                var holeInteriorPoints = part.Interior(holes[l]);
                var interiorLen = holeInteriorPoints.Length;
                for (var k = 0; k < interiorLen; ++k)
                {
                    var offset = holeInteriorPoints[k];
                    var currentPoint = ScreenCenter + WorldOffsetToScreenOffset(offset);
                    if (lastPoint != currentPoint)
                        dl.PathLineTo(currentPoint);
                    lastPoint = currentPoint;
                }

                dl.PathStroke(color, ImDrawFlags.Closed, 2f);
            }
        }
    }

    public void CardinalNames()
    {
        var center = ScreenCenter;
        var fontSetting = Config.CardinalsFontSize;
        var offCenterSizeOffset = (ScreenHalfSize + ScreenMarginSize * 0.5f) * _bounds.ScaleFactor + fontSetting - 17f;
        var offS = RotatedCoords(new(default, offCenterSizeOffset));
        var offE = RotatedCoords(new(offCenterSizeOffset, default));
        TextScreen(center - offS, "N", Colors.CardinalN, fontSetting);
        TextScreen(center + offS, "S", Colors.CardinalS, fontSetting);
        TextScreen(center + offE, "E", Colors.CardinalE, fontSetting);
        TextScreen(center - offE, "W", Colors.CardinalW, fontSetting);
    }

    public void ActorInsideBounds(WPos position, Angle rotation, uint color)
    {
        var scale = Config.ActorScale;
        var dir = rotation.ToDirection();
        var scale07 = scale * 0.7f * dir;
        var scale035 = scale * 0.35f * dir;
        var scale0433 = scale * 0.433f * dir.OrthoR();
        var positionscale07 = position + scale07;
        var positionscale035 = position - scale035;
        var positionscale035pscale0433 = positionscale035 + scale0433;
        var positionscale035mscale0433 = positionscale035 - scale0433;
        if (Config.ShowOutlinesAndShadows)
            AddTriangle(positionscale07, positionscale035pscale0433, positionscale035mscale0433, Colors.Shadows, 2f);
        AddTriangleFilled(positionscale07, positionscale035pscale0433, positionscale035mscale0433, color);
    }

    public void ActorOutsideBounds(WPos position, Angle rotation, uint color)
    {
        var scale = Config.ActorScale;
        var dir = rotation.ToDirection();
        var scale07 = scale * 0.7f * dir;
        var scale035 = scale * 0.35f * dir;
        var scale0433 = scale * 0.433f * dir.OrthoR();
        var positionscale035 = position - scale035;
        AddTriangle(position + scale07, positionscale035 + scale0433, positionscale035 - scale0433, color);
    }

    public void ActorProjected(WPos from, WPos to, Angle rotation, uint color)
    {
        if (InBounds(to))
        {
            // projected position is inside bounds
            ActorInsideBounds(to, rotation, color);
            return;
        }

        var dir = to - from;
        var l = dir.Length();

        if (l == default)
            return; // can't determine projection direction

        dir /= l;
        var t = IntersectRayBounds(from, dir);
        if (t <= l)
        {
            ActorOutsideBounds(from + t * dir, rotation, color);
        }
    }

    public void Actor(WPos position, Angle rotation, uint color)
    {
        if (InBounds(position))
            ActorInsideBounds(position, rotation, color);
        else
            ActorOutsideBounds(ClampToBounds(position), rotation, color);
    }

    public void Actor(Actor? actor, uint color = default, bool allowDeadAndUntargetable = false)
    {
        if (actor != null && !actor.IsDestroyed && (allowDeadAndUntargetable || actor.IsTargetable && !actor.IsDead))
            Actor(actor.Position, actor.Rotation, color == default ? Colors.Enemy : color);
    }

    public void Actors(IEnumerable<Actor> actors, uint color = default, bool allowDeadAndUntargetable = false)
    {
        foreach (var a in actors)
            Actor(a, color == default ? Colors.Enemy : color, allowDeadAndUntargetable);
    }

    public void Actors(List<Actor> actors, uint color = default, bool allowDeadAndUntargetable = false)
    {
        var count = actors.Count;
        for (var i = 0; i < count; ++i)
        {
            Actor(actors[i], color == default ? Colors.Enemy : color, allowDeadAndUntargetable);
        }
    }

    public void Actors(BossModule module, uint[] actors, uint color = default, bool allowDeadAndUntargetable = false)
    {
        var actors_ = actors;
        var len = actors_.Length;
        var color_ = color == default ? Colors.Enemy : color;
        for (var i = 0; i < len; ++i)
        {
            var enemies = module.Enemies(actors[i]);
            var count = enemies.Count;
            for (var j = 0; j < count; ++j)
            {
                var enemy = enemies[j];
                if (!enemy.IsDestroyed && (allowDeadAndUntargetable || enemy.IsTargetable && !enemy.IsDead))
                {
                    Actor(enemy.Position, enemy.Rotation, color_);
                }
            }
        }
    }

    public void ActorsInBounds(BossModule module, uint[] actors, uint color = default, bool allowDeadAndUntargetable = false)
    {
        var actors_ = actors;
        var len = actors_.Length;
        var center = Center;
        var radius = Bounds.Radius;
        var color_ = color == default ? Colors.Enemy : color;
        for (var i = 0; i < len; ++i)
        {
            var enemies = module.Enemies(actors[i]);
            var count = enemies.Count;
            for (var j = 0; j < count; ++j)
            {
                var enemy = enemies[j];
                if (!enemy.IsDestroyed && enemy.Position.AlmostEqual(center, radius) && (allowDeadAndUntargetable || enemy.IsTargetable && !enemy.IsDead))
                {
                    Actor(enemy.Position, enemy.Rotation, color_);
                }
            }
        }
    }

    public static void End()
    {
        ImGui.GetWindowDrawList().PopClipRect();
    }
}
