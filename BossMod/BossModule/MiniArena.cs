using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface;

namespace BossMod;

// note on coordinate systems:
// - world coordinates - X points West to East, Z points North to South - so SE is corner with both maximal coords, NW is corner with both minimal coords
//                       rotation 0 corresponds to South, and increases counterclockwise (so East is +pi/2, North is pi, West is -pi/2)
// - camera azimuth 0 correpsonds to camera looking North and increases counterclockwise
// - screen coordinates - X points left to right, Y points top to bottom
public sealed partial class MiniArena(WPos center, ArenaBounds bounds, Func<Actor, int?>? arenaProjectionLayerResolver = null)
{
    public static readonly BossModuleConfig Config = Service.Config.Get<BossModuleConfig>();
    private const float ActorWorldProjectionHeight = 0.5f;
    private WPos _center = center;
    private Vector2 _currentWindowSize = new(400f, 900f);

    public WPos Center
    {
        get => _center;
        set
        {
            if (_center != value)
            {
                _center = value;
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
                _bounds.ScreenHalfSize = ScreenHalfSize; // ensure arena bounds are fully initialized before doing anything else
                _worldProjectionFloorYInitialized = false;
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

    // Frame-constant rendering state, populated once by Begin().
    private float _scaledCos;
    private float _scaledSin;
    private float _frameArenaScale;
    private float _frameThicknessScale;
    private float _frameActorScale;
    private float _frameScreenHalfSize;
    private float _frameScreenMarginSize;
    private float _frameCardinalsFontSize;
    private float _frameWorldTextFontSize;
    private float _frameWorldIconFontSize;
    private float _frameBillboardYOffset;
    private bool _frameShowWorldTextIconBillboards;
    private bool _frameShowOutlinesAndShadows;
    private bool _frameProjectActorTriangles;
    private bool _frameProjectIntoWorld;
    private bool _frameClipWorldZonesToArena;
    private Camera? _frameWorldCamera;
    private Camera.WorldProjectionLayer[]? _allWorldProjectionLayers;
    private bool _allWorldProjectionLayersInitialized;
    private float _frameWorldProjectionY;
    private float _frameWorldBorderY;
    private float _frameWorldProjectionHeight = ArenaBounds.DefaultWorldProjectionHeight;
    private float _frameWorldProjectionHoleFillRadius;
    private RelSimplifiedComplexPolygon? _frameWorldProjectionArenaClip;
    private float _frameWorldBossY;
    private int? _frameArenaProjectionLayer;
    // The player's complete 2D presentation domain remains stable across mechanic stencil scopes.
    private RelSimplifiedComplexPolygon? _frameArenaProjectionShape;
    private WDir _frameArenaCenterOffset;
    private RelSimplifiedComplexPolygon? _frameArenaStencilShape;
    private bool _frameSuppress2DZoneRendering;
    private float _worldProjectionFloorY;
    private bool _worldProjectionFloorYInitialized;
    private ArenaBoundsCustom? _worldProjectionLayerOwner;
    private int _worldProjectionDefaultLayerIndex = -1;
    private ArenaBoundsCustom? _arenaProjectionLayerOwner;
    private ulong _arenaProjectionLayerActorID;
    private int _arenaProjectionDefaultLayerIndex = -1;
    private bool _frameDraw2D;

    private enum WorldPathCommandKind : byte { Point, Arc }

    private readonly struct WorldPathCommand
    {
        public readonly WorldPathCommandKind Kind;
        public readonly WPos Point;
        public readonly WPos Center;
        public readonly float Radius;
        public readonly float MinAngle;
        public readonly float MaxAngle;

        private WorldPathCommand(WorldPathCommandKind kind, WPos point, WPos center, float radius, float minAngle, float maxAngle)
        {
            Kind = kind;
            Point = point;
            Center = center;
            Radius = radius;
            MinAngle = minAngle;
            MaxAngle = maxAngle;
        }

        public static WorldPathCommand LinePoint(WPos point) => new(WorldPathCommandKind.Point, point, default, 0f, 0f, 0f);
        public static WorldPathCommand Arc(WPos center, float radius, float minAngle, float maxAngle) => new(WorldPathCommandKind.Arc, default, center, radius, minAngle, maxAngle);
    }

    // Dx11ArenaRenderer's path itself is intentionally still the authoritative 2D path. These commands
    // are only a compact semantic mirror used after PathStroke confirms that the 2D path was drawable.
    // Keeping arcs semantic (rather than caching tessellated points) lets the world mirror stay analytic.
    private readonly List<WorldPathCommand> _worldPathCommands = [with(32)];
    private static MiniArena? _worldPathOwner;

    // Actor markers normally sit directly under a known actor.
    // a destination marker is usually not underneath an actor. A zero bounds/layer height overrides
    // the shallow marker band as well so reference-plane projection remains arena-wide.
    private const float WorldProjectionLayerSwitchHysteresis = 0.75f;
    private const float WorldOutlineUnit = 0.08f;
    // 3D arena-rim Dimensions
    private const float WorldArenaRimHeight = 0.35f;
    // Keep the lower rail slightly above the provisional reference plane. This avoids fighting the scene depth of a coplanar floor while
    // still reading visually as the arena's ground contact edge
    private const float WorldArenaRimBaseLift = 0.075f;
    private const float WorldArenaRimSupportSpacing = 2.0f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InBounds(WPos position) => _bounds.Contains(position - _center);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WPos ClampToBounds(WPos position) => _center + _bounds.ClampToBounds(position - _center);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float IntersectRayBounds(WPos rayOrigin, in WDir rayDir) => _bounds.IntersectRay(rayOrigin - _center, rayDir);

    public void SetWindowSize(Vector2 fullSize)
    {
        var resize = new Vector2(400, 900);

        if (Config.RadarResize)
        {
            // Resize the radar arena back to original value also.
            Config.ArenaScale = 1.0f;

            _currentWindowSize = resize;
            Config.RadarResize = false;
        }
        else
        {
            _currentWindowSize = ImGui.GetWindowSize();
        }
        var requiredWindowSize = Vector2.Max(fullSize, _currentWindowSize);
        ImGui.SetWindowSize(requiredWindowSize, ImGuiCond.Always);
    }

    // Right click anywhere on mini radar window to recenter and resize.
    public void DrawContextMenu()
    {
        // Opens context menu on right mouse click of the mini radar
        if (ImGui.BeginPopupContextWindow("MyWindowContext", ImGuiPopupFlags.MouseButtonRight))
        {
            if (ImGui.MenuItem("Recenter and Resize Radar"))
            {
                Service.BossModWindow?.RecenterWindow();
            }
            ImGui.EndPopup();
        }
    }

    // prepare for drawing - set up internal state, clip rect etc.
    public void Begin(Angle cameraAzimuth, Actor primaryActor, Actor player, bool draw2D = true)
    {
        // The renderer owns one global ImDrawList-style path. Mirror that lifetime for world-path semantics as well so an abandoned path can never leak into a later arena/frame
        _worldPathOwner?._worldPathCommands.Clear();
        _worldPathOwner = null;
        _worldPathCommands.Clear();
        _frameDraw2D = draw2D;

        // Snapshot renderer-facing configuration once per arena frame. Most primitive methods are hot
        // and do not need to re-read the config object for values that cannot meaningfully change halfway through one Begin/End pair
        var arenaScale = Config.ArenaScale;
        _frameArenaScale = arenaScale;
        _frameThicknessScale = Config.ThicknessScale;
        _frameActorScale = Config.ActorScale;
        _frameShowOutlinesAndShadows = Config.ShowOutlinesAndShadows;
        _frameCardinalsFontSize = Config.CardinalsFontSize;
        _allWorldProjectionLayersInitialized = false;
        _frameWorldCamera?.ProjectedShapeLayers = null;
        // World clipping is a property of the bounds, not of whether its visible 3D border is enabled.

        _frameSuppress2DZoneRendering = !draw2D;

        ArenaBoundsCustom? layeredBounds = null;
        ArenaProjectionLayer[]? projectionLayers = null;
        if (_bounds is ArenaBoundsCustom { WorldProjectionLayers: { Length: > 0 } layers } customBounds)
        {
            layeredBounds = customBounds;
            projectionLayers = layers;
            var layerActorID = player.InstanceID;
            if (!ReferenceEquals(_arenaProjectionLayerOwner, customBounds) || _arenaProjectionLayerActorID != layerActorID || (uint)_arenaProjectionDefaultLayerIndex >= (uint)layers.Length)
            {
                _arenaProjectionLayerOwner = customBounds;
                _arenaProjectionLayerActorID = layerActorID;
                _arenaProjectionDefaultLayerIndex = -1;
            }
            if (arenaProjectionLayerResolver is { } resolveLayer)
            {
                var selectedLayer = resolveLayer(player);

                _frameArenaProjectionLayer =
                    customBounds.IsValidProjectionLayer(selectedLayer)
                        ? selectedLayer : null;

                _arenaProjectionDefaultLayerIndex = _frameArenaProjectionLayer ?? -1;
            }
            else
            {
                ref var playerPosition = ref player.PosRot;
                _arenaProjectionDefaultLayerIndex = customBounds.ResolveProjectionLayer(new WPos(ref playerPosition) - _center, playerPosition.Y, _arenaProjectionDefaultLayerIndex, WorldProjectionLayerSwitchHysteresis);
                _frameArenaProjectionLayer = _arenaProjectionDefaultLayerIndex;
            }
        }
        else
        {
            _arenaProjectionLayerOwner = null;
            _arenaProjectionLayerActorID = 0u;
            _arenaProjectionDefaultLayerIndex = -1;
            _frameArenaProjectionLayer = null;
        }

        _frameWorldCamera = Config.ProjectRadarInto3DWorld ? Camera.Instance : null;
        _frameProjectIntoWorld = _frameWorldCamera != null;

        if (_frameProjectIntoWorld)
        {
            _frameWorldTextFontSize = Config.TextBillboardFontSize;
            _frameWorldIconFontSize = Config.IconBillboardFontSize;
            _frameProjectActorTriangles = Config.ShowActorTrianglesIn3DWorld;
            _frameShowWorldTextIconBillboards = Config.EnableTextIconBillboards;
            _frameBillboardYOffset = Config.BillboardHeightOffset;
            _frameClipWorldZonesToArena = _frameProjectIntoWorld && _bounds.AllowDrawing3DArenaBounds;
            _frameWorldBossY = primaryActor.PosRot.Y;

            if (layeredBounds != null && projectionLayers != null)
            {
                // Authored vertical arenas have reliable floor heights. Null mechanic layer selection
                // follows the live player's floor; explicit mechanic layer IDs can still override this
                // temporarily. A small hysteresis keeps ordinary jumps from flipping between close floors.
                var layerIndex = SelectDefaultWorldProjectionLayer(layeredBounds, player.Position - _center, _frameWorldBossY);
                ref readonly var layer = ref projectionLayers[layerIndex];
                _frameWorldProjectionY = ResolveWorldProjectionY(layer.Y);
                _frameWorldBorderY = ResolveWorldBorderY(layer.BorderY, _frameWorldProjectionY);
                _frameWorldProjectionHeight = ResolveWorldProjectionHeight(layer);
                _frameWorldProjectionHoleFillRadius = ResolveWorldProjectionHoleFillRadius(layer);
                _frameWorldProjectionArenaClip = layeredBounds.WorldProjectionClip(layerIndex);
            }
            else
            {
                _frameWorldProjectionY = ResolveWorldProjectionY(_bounds.Y);
                _frameWorldBorderY = ResolveWorldBorderY(_bounds.BorderY, _frameWorldProjectionY);
                _frameWorldProjectionHeight = ResolveWorldProjectionHeight();
                _frameWorldProjectionHoleFillRadius = ResolveWorldProjectionHoleFillRadius();
                _frameWorldProjectionArenaClip = (_bounds as ArenaBoundsCustom)?.WorldProjectionClip() ?? _bounds.Shape;
                _worldProjectionLayerOwner = null;
                _worldProjectionDefaultLayerIndex = -1;
            }
        }
        else
        {
            _worldProjectionFloorYInitialized = false;
            _worldProjectionLayerOwner = null;
            _worldProjectionDefaultLayerIndex = -1;
            _frameWorldProjectionY = !float.IsNaN(_bounds.Y) ? _bounds.Y : _frameWorldBossY;
            _frameWorldBorderY = ResolveWorldBorderY(_bounds.BorderY, _frameWorldProjectionY);
            _frameWorldProjectionHeight = ResolveWorldProjectionHeight();
            _frameWorldProjectionHoleFillRadius = ResolveWorldProjectionHoleFillRadius();
            _frameWorldProjectionArenaClip = (_bounds as ArenaBoundsCustom)?.WorldProjectionClip() ?? _bounds.Shape;
        }

        // bounds build Shape lazily from ScreenHalfSize. Vertical custom layers already supplied an explicit clip above; the normal single-floor path needs the now-initialized shape
        _frameWorldProjectionArenaClip ??= _bounds.Shape;

        var arenaInvRadius = _bounds.InvRadius;
        _frameArenaProjectionShape = null;
        _frameArenaCenterOffset = default;
        if (layeredBounds != null)
        {
            var presentation = layeredBounds.ProjectionLayer2DBounds(_frameArenaProjectionLayer);
            _frameArenaProjectionShape = presentation.Shape;
            _frameArenaCenterOffset = presentation.CenterOffset;
            arenaInvRadius = 1f / presentation.Radius;
            if (draw2D)
            {
                presentation.Shape.VerifyPolygonIndexExistance();
            }
        }
        // Select the player's floor/group before submitting the opaque background. World-only
        // stencil exclusions and temporary mechanic scopes do not change this presentation domain.
        var arenaShape = _frameArenaProjectionShape ?? _bounds.Shape;
        _frameArenaStencilShape = arenaShape;

        if (draw2D)
        {
            var screenHalfSize = _frameScreenHalfSize = 150f * arenaScale;
            var screenMarginSize = _frameScreenMarginSize = 20f * arenaScale;

            var centerOffset = new Vector2(screenMarginSize + Config.SlackForRotations * screenHalfSize);
            var fullSize = 2f * centerOffset;

            SetWindowSize(fullSize);
            DrawContextMenu();

            var cursor = ImGui.GetCursorScreenPos();
            ImGui.Dummy(fullSize);

            if (_bounds.ScreenHalfSize != screenHalfSize)
            {
                _bounds.ScreenHalfSize = screenHalfSize;
            }
            var screenCenter = cursor + centerOffset;
            ScreenCenter = screenCenter;

            _cameraAzimuth = cameraAzimuth;
            (_cameraSinAzimuth, _cameraCosAzimuth) = MathF.SinCos(cameraAzimuth.Rad);

            var screenScale = screenHalfSize * arenaInvRadius;
            var scaledCos = _cameraCosAzimuth * screenScale;
            var scaledSin = _cameraSinAzimuth * screenScale;
            _scaledCos = scaledCos;
            _scaledSin = scaledSin;

            // All submitted polygons/positions remain relative to the logical arena center.
            // Move that origin on screen so the active layer/group is centered in the radar.
            var renderCenter = screenCenter - WorldOffsetToScreenOffset(_frameArenaCenterOffset);

            var drawList = ImGui.GetWindowDrawList();

            var wmin = ImGui.GetWindowPos();
            var wmax = wmin + ImGui.GetWindowSize();
            drawList.PushClipRect(Vector2.Max(cursor, wmin), Vector2.Min(cursor + fullSize, wmax));

            Dx11ArenaRenderer.BeginArena(drawList, arenaShape, renderCenter.X, renderCenter.Y, _scaledCos, _scaledSin, screenScale);

            if (Config.OpaqueArenaBackground)
            {
                Dx11ArenaRenderer.AppendArenaBackground(Colors.Background);
            }
        }
        if (_frameClipWorldZonesToArena && _frameWorldProjectionArenaClip != null)
        {
            // Prime the independent world clip at the radar's real screen scale without ever installing it as the live 2D stencil. Camera/world draws later reuse this immutable SDF
            Dx11ArenaRenderer.PrepareArenaSdfForWorldProjection(_frameWorldProjectionArenaClip);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float ResolveWorldProjectionHeight() => _bounds.WorldProjectionHeight >= 0f ? _bounds.WorldProjectionHeight : ArenaBounds.DefaultWorldProjectionHeight;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float ResolveWorldProjectionHeight(in ArenaProjectionLayer layer) => layer.ProjectionHeight >= 0f ? layer.ProjectionHeight : ResolveWorldProjectionHeight();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float ResolveWorldProjectionY(float configuredY)
    {
        if (!float.IsNaN(configuredY))
        {
            return configuredY;
        }
        if (!_worldProjectionFloorYInitialized)
        {
            _worldProjectionFloorY = _frameWorldBossY;
            _worldProjectionFloorYInitialized = true;
        }
        return _worldProjectionFloorY;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float ResolveWorldBorderY(float configuredY, float projectionY) => !float.IsNaN(configuredY) ? configuredY : projectionY;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float ResolveWorldProjectionHoleFillRadius() => Math.Clamp(_bounds.WorldProjectionHoleFillRadius, 0f, 2f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float ResolveWorldProjectionHoleFillRadius(in ArenaProjectionLayer layer)
        => !float.IsNaN(layer.WorldProjectionHoleFillRadius) ? Math.Clamp(layer.WorldProjectionHoleFillRadius, 0f, 2f) : ResolveWorldProjectionHoleFillRadius();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int SelectDefaultWorldProjectionLayer(ArenaBoundsCustom bounds, in WDir positionOffset, float y)
    {
        if (!ReferenceEquals(_worldProjectionLayerOwner, bounds) || bounds.WorldProjectionLayers is not { Length: > 0 } layers || (uint)_worldProjectionDefaultLayerIndex >= (uint)layers.Length)
        {
            _worldProjectionLayerOwner = bounds;
            _worldProjectionDefaultLayerIndex = -1;
        }
        return _worldProjectionDefaultLayerIndex = bounds.ResolveProjectionLayer(positionOffset, y, _worldProjectionDefaultLayerIndex, WorldProjectionLayerSwitchHysteresis);
    }

    // Actor markers resolve their own containing/nearest authored floor from their live PosRot.
    // This keeps direct PosRot callers correct as well as Actor callers, without disturbing the 2D Zone* stencil.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private WorldProjectionLayerScope WorldProjectionLayerForActor(ref Vector4 posRot)
    {
        if (_frameProjectIntoWorld && _bounds is ArenaBoundsCustom { WorldProjectionLayers.Length: > 0 } customBounds)
        {
            return WorldProjectionLayer(customBounds.ResolveProjectionLayer(new WPos(ref posRot) - _center, posRot.Y), false, false);
        }
        return default;
    }

    public int? CurrentArenaProjectionLayer => _frameArenaProjectionLayer;

    // True restricts 2D visibility to the selected floor/group. False preserves unrestricted legacy
    // visibility. Null ignores layerID and applies/draws the mechanic on every authored floor.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WorldProjectionLayerScope WorldProjectionLayer(int? layerID, bool? restrictToArenaProjectionLayer = false) => WorldProjectionLayer(layerID, restrictToArenaProjectionLayer, true);

    private WorldProjectionLayerScope WorldProjectionLayer(int? layerID, bool? restrictToArenaProjectionLayer, bool switch2DStencil)
    {
        if (_bounds is not ArenaBoundsCustom { WorldProjectionLayers: { Length: > 0 } layers } customBounds)
        {
            return default;
        }
        var allLayers = !restrictToArenaProjectionLayer.HasValue;
        var index = layerID.GetValueOrDefault(-1);
        if (!allLayers && (uint)index >= (uint)layers.Length)
        {
            return default;
        }

        var scope = new WorldProjectionLayerScope(this, _frameWorldProjectionY, _frameWorldBorderY, _frameWorldProjectionHeight, _frameWorldProjectionHoleFillRadius,
            _frameWorldProjectionArenaClip, _frameArenaStencilShape, _frameSuppress2DZoneRendering);
        if (allLayers)
        {
            // A wildcard uses the player's whole visible domain, including Shared2DGroup, and
            // retains an outer scope's 2D suppression. It never changes the radar viewport.
            var stencil = _frameArenaProjectionShape ?? _bounds.Shape;
            if (switch2DStencil && !_frameSuppress2DZoneRendering && !ReferenceEquals(_frameArenaStencilShape, stencil))
            {
                Dx11ArenaRenderer.SetArenaStencil(stencil);
                _frameArenaStencilShape = stencil;
            }
            _frameWorldCamera?.ProjectedShapeLayers = AllWorldProjectionLayers(customBounds, layers);
            return scope;
        }

        ref readonly var layer = ref layers[index];
        // Suppression is cumulative for nested scopes: an inner unrestricted scope must not make a mechanic visible again while an outer restricted scope is hiding it
        var suppress2D = _frameSuppress2DZoneRendering || restrictToArenaProjectionLayer == true && !customBounds.ProjectionLayersShare2DGroup(_frameArenaProjectionLayer, index);
        _frameSuppress2DZoneRendering = suppress2D;
        if (switch2DStencil && !suppress2D)
        {
            if (!ReferenceEquals(_frameArenaStencilShape, layer.Shape))
            {
                Dx11ArenaRenderer.SetArenaStencil(layer.Shape);
                _frameArenaStencilShape = layer.Shape;
            }
        }
        if (_frameProjectIntoWorld)
        {
            _frameWorldCamera!.ProjectedShapeLayers = null;
            _frameWorldProjectionY = ResolveWorldProjectionY(layer.Y);
            _frameWorldBorderY = ResolveWorldBorderY(layer.BorderY, _frameWorldProjectionY);
            _frameWorldProjectionHeight = ResolveWorldProjectionHeight(layer);
            _frameWorldProjectionHoleFillRadius = ResolveWorldProjectionHoleFillRadius(layer);
            _frameWorldProjectionArenaClip = customBounds.WorldProjectionClip(index);
        }
        return scope;
    }

    private Camera.WorldProjectionLayer[] AllWorldProjectionLayers(ArenaBoundsCustom bounds, ArenaProjectionLayer[] layers)
    {
        var result = _allWorldProjectionLayers;
        if (result == null || result.Length != layers.Length)
        {
            result = _allWorldProjectionLayers = new Camera.WorldProjectionLayer[layers.Length];
            _allWorldProjectionLayersInitialized = false;
        }
        if (!_allWorldProjectionLayersInitialized)
        {
            for (var i = 0; i < layers.Length; ++i)
            {
                ref readonly var layer = ref layers[i];
                result[i] = new(ResolveWorldProjectionY(layer.Y), ResolveWorldProjectionHeight(layer), ResolveWorldProjectionHoleFillRadius(layer),
                    _frameClipWorldZonesToArena ? bounds.WorldProjectionClip(i) : null, _center);
            }
            _allWorldProjectionLayersInitialized = true;
        }
        return result;
    }

    public readonly struct WorldProjectionLayerScope : IDisposable
    {
        private readonly MiniArena? _arena;
        private readonly float _projectionY;
        private readonly float _borderY;
        private readonly float _projectionHeight;
        private readonly float _holeFillRadius;
        private readonly RelSimplifiedComplexPolygon? _arenaClip;
        private readonly RelSimplifiedComplexPolygon? _stencilShape;
        private readonly bool _suppress2D;
        private readonly Camera.WorldProjectionLayer[]? _worldProjectionLayers;

        internal WorldProjectionLayerScope(MiniArena arena, float projectionY, float borderY, float projectionHeight, float holeFillRadius,
            RelSimplifiedComplexPolygon? arenaClip, RelSimplifiedComplexPolygon? stencilShape, bool suppress2D)
        {
            _arena = arena;
            _projectionY = projectionY;
            _borderY = borderY;
            _projectionHeight = projectionHeight;
            _holeFillRadius = holeFillRadius;
            _arenaClip = arenaClip;
            _stencilShape = stencilShape;
            _suppress2D = suppress2D;
            _worldProjectionLayers = arena._frameWorldCamera?.ProjectedShapeLayers;
        }

        public void Dispose()
        {
            if (_arena != null)
            {
                if (_stencilShape != null && !ReferenceEquals(_arena._frameArenaStencilShape, _stencilShape))
                {
                    Dx11ArenaRenderer.SetArenaStencil(_stencilShape);
                    _arena._frameArenaStencilShape = _stencilShape;
                }
                _arena._frameSuppress2DZoneRendering = _suppress2D;
                _arena._frameWorldProjectionY = _projectionY;
                _arena._frameWorldBorderY = _borderY;
                _arena._frameWorldProjectionHeight = _projectionHeight;
                _arena._frameWorldProjectionHoleFillRadius = _holeFillRadius;
                _arena._frameWorldProjectionArenaClip = _arenaClip;
                _arena._frameWorldCamera?.ProjectedShapeLayers = _worldProjectionLayers;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector3 ProjectedPoint(WPos p) => new(p.X, _frameWorldProjectionY, p.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector3 ProjectedPointBillboard(WPos p) => new(p.X, _frameWorldProjectionY + _frameBillboardYOffset, p.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float ProjectedOutlineWidth(float thickness) => Math.Max(0.02f, thickness * _frameThicknessScale * WorldOutlineUnit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float CurrentActorWorldProjectionHeight() => _frameWorldProjectionHeight == 0f ? 0f : ActorWorldProjectionHeight;

    // Once the actor's own layer is active, align small actor/layer Y discrepancies to that authored floor.
    // Large discrepancies are treated as real vertical movement (jump/fall), so the marker can disappear
    // naturally when the actor leaves the shallow projection band instead of being snapped back to the floor.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float CurrentActorProjectionY(float actorY)
    {
        if (_bounds is not ArenaBoundsCustom { WorldProjectionLayers.Length: > 0 })
        {
            return actorY;
        }

        var delta = _frameWorldProjectionY - actorY;
        return Math.Abs(delta) <= ActorWorldProjectionHeight ? actorY + delta : actorY;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RelSimplifiedComplexPolygon? ProjectedArenaClip() => _frameClipWorldZonesToArena ? _frameWorldProjectionArenaClip : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProjectedLine(WPos a, WPos b, uint color, float thickness, bool arenaClipped = false)
    {
        var camera = _frameWorldCamera;
        if (camera == null)
        {
            return;
        }
        var delta = b - a;
        var len = delta.Length();
        if (len <= 1e-5f)
        {
            return;
        }
        var width = ProjectedOutlineWidth(thickness);
        camera.DrawProjectedCapsule(ProjectedPoint(a), delta / len, 0.5f * width, len, color, _frameWorldProjectionHeight,
            arenaClip: arenaClipped ? ProjectedArenaClip() : null, arenaOrigin: _center, holeFillRadius: _frameWorldProjectionHoleFillRadius);
    }

    private void ProjectedPolyline(ReadOnlySpan<WPos> vertices, uint color, float thickness, bool closed, bool arenaClipped = false)
    {
        var len = vertices.Length;
        if (!_frameProjectIntoWorld || len < 2)
        {
            return;
        }
        for (var i = 1; i < len; ++i)
        {
            ProjectedLine(vertices[i - 1], vertices[i], color, thickness, arenaClipped);
        }
        if (closed && len > 2)
        {
            ProjectedLine(vertices[^1], vertices[0], color, thickness, arenaClipped);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProjectedArenaArgs(out RelSimplifiedComplexPolygon? arenaClip, out WPos arenaOrigin)
    {
        arenaClip = ProjectedArenaClip();
        arenaOrigin = _center;
    }

    public void ArenaOutline(uint color, float thickness = 2f)
    {
        Dx11ArenaRenderer.AppendArenaOutline(color, thickness);
        WorldArenaOutline(color, thickness);
    }

    public void WorldArenaOutline(uint color, float thickness = 2f)
    {
        if (!Config.EnableArenaOutlineIn3DWorld || !_frameClipWorldZonesToArena || _frameWorldCamera == null)
        {
            return;
        }

        // Draw the visible arena border as true world-space geometry rather than as a terrain-projected SDF
        if (_bounds is ArenaBoundsCustom { WorldProjectionLayers: { Length: > 0 } layers })
        {
            // The 2D MiniArena still has one logical boundary, but the world view can expose every authored floor simultaneously. Scene depth naturally occludes rails on hidden floors
            var len = layers.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var layer = ref layers[i];
                var y = layer.Y;
                var projectionY = !float.IsNaN(y) ? y : _frameWorldProjectionY;
                DrawWorldArenaRim(layer.Shape, color, thickness, ResolveWorldBorderY(layer.BorderY, projectionY), ResolveWorldProjectionHeight(layer));
            }
        }
        else
        {
            DrawWorldArenaRim(_bounds.Shape, color, thickness, _frameWorldBorderY, _frameWorldProjectionHeight);
        }
    }

    private void DrawWorldArenaRim(RelSimplifiedComplexPolygon polygon, uint color, float thickness, float referenceY, float projectionHeight)
    {
        var camera = _frameWorldCamera;
        if (camera == null)
        {
            return;
        }

        var lineThickness = Math.Max(1f, thickness * _frameThicknessScale);
        var supportThickness = Math.Max(1f, lineThickness * 0.65f);
        var baseY = referenceY + WorldArenaRimBaseLift;
        var topY = baseY + WorldArenaRimHeight;
        // Preserve the stable-floor jump behavior, but only for the layer the player is actually near.
        // Other authored floors keep a fixed rim instead of stretching toward a player on another level.
        if (Math.Abs(_frameWorldBossY - referenceY) <= projectionHeight + 1f)
        {
            topY = Math.Max(topY, _frameWorldBossY + WorldArenaRimHeight);
        }

        var parts = CollectionsMarshal.AsSpan(polygon.Parts);
        var len = parts.Length;
        for (var i = 0; i < len; ++i)
        {
            var part = parts[i];
            DrawWorldArenaRimContour(camera, part.Exterior, color, lineThickness, supportThickness, baseY, topY);
            var count = part.HoleStarts.Count;
            for (var h = 0; h < count; ++h)
            {
                DrawWorldArenaRimContour(camera, part.Interior(h), color, lineThickness, supportThickness, baseY, topY);
            }
        }
    }

    private void DrawWorldArenaRimContour(Camera camera, ReadOnlySpan<WDir> contour, uint color, float lineThickness, float supportThickness, float baseY, float topY)
    {
        // The top rail is continuous. Vertical supports are distance-spaced rather than emitted for every polygon vertex; this avoids turning highly tessellated circles into a picket fence
        var distanceToNextSupport = 0f;
        var len = contour.Length;
        var centerX = _center.X;
        var centerZ = _center.Z;
        for (var i = 0; i < len; ++i)
        {
            var a = contour[i];
            var b = contour[(i + 1) % len];
            var ax = centerX + a.X;
            var az = centerZ + a.Z;
            var bx = centerX + b.X;
            var bz = centerZ + b.Z;
            var baseA = new Vector3(ax, baseY, az);
            var baseB = new Vector3(bx, baseY, bz);
            var topA = new Vector3(ax, topY, az);
            var topB = new Vector3(bx, topY, bz);
            camera.DrawWorldLine(baseA, baseB, color, lineThickness);
            camera.DrawWorldLine(topA, topB, color, lineThickness);

            var dx = bx - ax;
            var dz = bz - az;
            var edgeLength = MathF.Sqrt(dx * dx + dz * dz);
            var invEdgeLength = 1f / edgeLength;
            var along = distanceToNextSupport;
            while (along < edgeLength)
            {
                var t = along * invEdgeLength;
                var x = ax + dx * t;
                var z = az + dz * t;
                camera.DrawWorldLine(new Vector3(x, baseY, z), new Vector3(x, topY, z), color, supportThickness);
                along += WorldArenaRimSupportSpacing;
            }

            distanceToNextSupport = along - edgeLength;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 WorldPositionToScreenPosition(WPos p) => ScreenCenter + WorldOffsetToScreenOffset(p - _center - _frameArenaCenterOffset);

    // this is useful for drawing on margins (TODO better api)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 RotatedCoords(Vector2 coords)
    {
        var cx = coords.X;
        var cy = coords.Y;
        var x = cx * _cameraCosAzimuth - cy * _cameraSinAzimuth;
        var y = cy * _cameraCosAzimuth + cx * _cameraSinAzimuth;
        return new(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector2 WorldOffsetToScreenOffset(WDir worldOffset)
    {
        var wx = worldOffset.X;
        var wz = worldOffset.Z;
        return new(wx * _scaledCos - wz * _scaledSin, wz * _scaledCos + wx * _scaledSin);
    }

    // Unclipped primitive rendering that accepts world-space positions
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLine(WPos a, WPos b, uint color = default, float thickness = 1f)
    {
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        Span<WDir> points = [a - _center, b - _center];
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendPolyline(points, false, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        ProjectedLine(a, b, lineColor, thickness);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCircleUnfilled(WPos center, float radius, uint color = default, float thickness = 1f)
    {
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendCircleOutlineUnclipped(center - _center, radius, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        _frameWorldCamera?.DrawProjectedCircle(ProjectedPoint(center), radius, lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), holeFillRadius: _frameWorldProjectionHoleFillRadius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddTriangle(WPos p1, WPos p2, WPos p3, uint color = default, float thickness = 1f)
    {
        var actualColor = color != default ? color : Colors.Danger;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendPrimitiveTriangleStroke(p1 - _center, p2 - _center, p3 - _center, actualColor, thickness * _frameThicknessScale);
        }
        _frameWorldCamera?.DrawProjectedTriangle(ProjectedPoint(p1), ProjectedPoint(p2), ProjectedPoint(p3), actualColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), holeFillRadius: _frameWorldProjectionHoleFillRadius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddTriangleFilled(WPos p1, WPos p2, WPos p3, uint color = default)
    {
        var actualColor = color != default ? color : Colors.Danger;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendPrimitiveTriangle(p1 - _center, p2 - _center, p3 - _center, actualColor);
        }
        _frameWorldCamera?.DrawProjectedTriangle(ProjectedPoint(p1), ProjectedPoint(p2), ProjectedPoint(p3), actualColor, _frameWorldProjectionHeight, holeFillRadius: _frameWorldProjectionHoleFillRadius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddQuad(WPos p1, WPos p2, WPos p3, WPos p4, uint color = default, float thickness = 1f)
    {
        var actualColor = color != default ? color : Colors.Danger;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendQuadStroke(p1 - _center, p2 - _center, p3 - _center, p4 - _center, actualColor, thickness * _frameThicknessScale);
        }
        Span<WPos> projected = [p1, p2, p3, p4];
        ProjectedPolyline(projected, actualColor, thickness, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color, float thickness = 1f)
    {
        thickness *= _frameThicknessScale;
        var side = halfWidth * direction.OrthoR();
        var front = origin + lenFront * direction;
        var back = origin - lenBack * direction;
        AddQuad(front + side, front - side, back - side, back + side, color, thickness);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddPolygon(ReadOnlySpan<WPos> vertices, uint color = default, float thickness = 1f)
    {
        var len = vertices.Length;
        Span<WDir> local = stackalloc WDir[len];
        for (var i = 0; i < len; ++i)
        {
            local[i] = vertices[i] - _center;
        }
        var actualColor = color != default ? color : Colors.Danger;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendPolyline(local, true, actualColor, thickness * _frameThicknessScale);
        }
        ProjectedPolyline(vertices, actualColor, thickness, true);
    }

    public void AddComplexPolygon(RelSimplifiedComplexPolygon poly, uint color = default, float thickness = 1f)
    {
        var parts = CollectionsMarshal.AsSpan(poly.Parts);
        var len = parts.Length;
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        if (!_frameSuppress2DZoneRendering)
        {
            for (var i = 0; i < len; ++i)
            {
                var part = parts[i];
                DrawContour(part.Exterior);
                var countH = part.HoleStarts.Count;
                for (var h = 0; h < countH; ++h)
                {
                    DrawContour(part.Interior(h));
                }
            }
        }
        _frameWorldCamera?.DrawProjectedPolygon(ProjectedPoint(_center), poly, _center, lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), holeFillRadius: _frameWorldProjectionHoleFillRadius);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DrawContour(ReadOnlySpan<WDir> contour) => Dx11ArenaRenderer.AppendPolyline(contour, true, lineColor, lineThickness, shadowColor, shadowThickness);
    }

    // WPos follows the active projection floor; the Vector3 overload for callers that author an exact floating height independently of arena projection layers
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawEye(WPos eyeCenter, bool danger, bool inverted) => DrawEye(ProjectedPointBillboard(eyeCenter), danger, inverted);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawEye(Vector3 eyeCenter, bool danger, bool inverted) => DrawEye(new WPos(eyeCenter.X, eyeCenter.Z), eyeCenter, danger, inverted);

    private void DrawEye(WPos eyeCenter, Vector3 worldEyeCenter, bool danger, bool inverted)
    {
        var bodyColor = danger ? Colors.Enemy : Colors.PC;
        var centerOffset = eyeCenter - _center;

        const float _eyeOuterH = 10f;
        const float _eyeOuterV = 6f;
        const float _eyeBorder = 1.15f;
        const float _eyeIrisR = 3.25f;
        const float _eyePupilR = 1.65f;
        const float _eyeHighlightR = 0.72f;
        const float _eyeShadowOffsetY = 1.15f;
        const uint _eyePupil = 0xFF101010;
        const uint _eyeHighlight = 0xF8FFFFFF;
        const uint _eyeShadow = 0x50000000;

        var border = Colors.Border;
        if (!_frameSuppress2DZoneRendering)
        {
            // All pieces are analytic screen-space instances and consecutive ScreenAnalytic segments merge into one instanced draw
            Dx11ArenaRenderer.AppendArenaScreenEye(centerOffset, new Vector2(0f, _eyeShadowOffsetY), _eyeOuterH, _eyeOuterV, _eyeShadow);
            Dx11ArenaRenderer.AppendArenaScreenEye(centerOffset, _eyeOuterH, _eyeOuterV, border);
            Dx11ArenaRenderer.AppendArenaScreenEye(centerOffset, _eyeOuterH - _eyeBorder, _eyeOuterV - _eyeBorder, bodyColor);

            // the whole eye body is red/green. The inner circles only add depth/readability
            Dx11ArenaRenderer.AppendArenaScreenCircle(centerOffset, _eyeIrisR, border);
            Dx11ArenaRenderer.AppendArenaScreenCircle(centerOffset, _eyePupilR, _eyePupil);
            Dx11ArenaRenderer.AppendArenaScreenCircle(centerOffset, new Vector2(-0.9f, -0.9f), _eyeHighlightR, _eyeHighlight);
        }

        // Mirror eye into the 3D world. A restricted projection-layer scope can suppress the 2D copy on unrelated floors while retaining the authored 3D indicator
        var worldCamera = _frameWorldCamera;
        if (worldCamera == null)
        {
            return;
        }

        const float worldEyeHalfWidth = 1.75f;
        const float worldEyeHalfHeight = 1.05f;
        const float worldEyeHalfDepth = 0.55f;
        const float worldEyeMistRadius = 0.85f;

        worldCamera.DrawWorldEye(worldEyeCenter, worldEyeHalfWidth, worldEyeHalfHeight, worldEyeHalfDepth, worldEyeMistRadius, bodyColor, border, inverted);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PathLineTo(WPos p)
    {
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.PathLineTo(p - _center);
        }
        RecordWorldPathCommand(WorldPathCommand.LinePoint(p));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PathArcTo(WPos center, float radius, float amin, float amax)
    {
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.PathArcTo(center - _center, radius, amin, amax);
        }
        if (radius > 0f)
        {
            RecordWorldPathCommand(WorldPathCommand.Arc(center, radius, amin, amax));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PathStroke(bool closed, uint color = default, float thickness = 1f)
    {
        var actualColor = color != default ? color : Colors.Danger;
        var drew2D = false;
        if (!_frameSuppress2DZoneRendering)
        {
            drew2D = Dx11ArenaRenderer.PathStrokeWithResult(closed, actualColor, thickness * Config.ThicknessScale);
        }
        var owner = _worldPathOwner;
        _worldPathOwner = null;
        if (owner == null)
        {
            return;
        }

        try
        {
            if (drew2D || !owner._frameDraw2D)
            {
                owner.StrokeWorldPath(closed, actualColor, thickness);
            }
        }
        finally
        {
            owner._worldPathCommands.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RecordWorldPathCommand(in WorldPathCommand command)
    {
        if (_frameWorldCamera == null)
        {
            return;
        }

        if (!ReferenceEquals(_worldPathOwner, this))
        {
            _worldPathOwner?._worldPathCommands.Clear();
            _worldPathOwner = this;
            _worldPathCommands.Clear();
        }
        _worldPathCommands.Add(command);
    }

    private void StrokeWorldPath(bool closed, uint color, float thickness)
    {
        var camera = _frameWorldCamera;
        if (camera == null || _worldPathCommands.Count == 0)
        {
            return;
        }

        var strokeRadius = 0.5f * ProjectedOutlineWidth(thickness);
        WPos first = default;
        WPos previous = default;
        var havePrevious = false;

        void AppendPoint(WPos point)
        {
            if (!havePrevious)
            {
                first = previous = point;
                havePrevious = true;
                return;
            }

            DrawPathLine(previous, point);
            previous = point;
        }

        void DrawPathLine(WPos from, WPos to)
        {
            var delta = to - from;
            var length = delta.Length();
            if (length <= 1e-5f)
            {
                return;
            }
            camera.DrawProjectedCapsule(ProjectedPoint(from), delta / length, strokeRadius, length, color, _frameWorldProjectionHeight, suppressZoneWave: true, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }

        var commands = CollectionsMarshal.AsSpan(_worldPathCommands);
        var len = commands.Length;

        for (var i = 0; i < len; ++i)
        {
            ref readonly var command = ref commands[i];
            if (command.Kind == WorldPathCommandKind.Point)
            {
                AppendPoint(command.Point);
                continue;
            }

            var min = command.MinAngle;
            var max = command.MaxAngle;
            var (sinStart, cosStart) = MathF.SinCos(min);
            var (sinEnd, cosEnd) = MathF.SinCos(max);
            var center = command.Center;
            var centerX = center.X;
            var centerZ = center.Z;
            var radius = command.Radius;
            var start = new WPos(centerX + radius * sinStart, centerZ + radius * cosStart);
            var end = new WPos(centerX + radius * sinEnd, centerZ + radius * cosEnd);
            AppendPoint(start);

            var angularLength = max - min;
            if (Math.Abs(angularLength) > 1e-7f)
            {
                camera.DrawProjectedArcCapsule(ProjectedPoint(start), ProjectedPoint(center), angularLength, strokeRadius, color, _frameWorldProjectionHeight, suppressZoneWave: true, holeFillRadius: _frameWorldProjectionHoleFillRadius);
            }
            previous = end;
        }

        if (closed && havePrevious)
        {
            DrawPathLine(previous, first);
        }
    }

    // Filled zones:
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color)
    {
        var actualColor = color != default ? color : Colors.AOE;
        var direction = centerDirection.ToDirection();
        var rad = halfAngle.Rad;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendCone(center - _center, innerRadius, outerRadius, direction, rad, actualColor);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedCone(ProjectedPoint(center), innerRadius, outerRadius, direction, rad, actualColor, _frameWorldProjectionHeight, arenaClip: clip, arenaOrigin: clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneCircle(WPos center, float radius, uint color)
    {
        var actualColor = color != default ? color : Colors.AOE;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendCircle(center - _center, radius, actualColor);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedCircle(ProjectedPoint(center), radius, actualColor, _frameWorldProjectionHeight, arenaClip: clip, arenaOrigin: clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneDonut(WPos center, float innerRadius, float outerRadius, uint color)
    {
        var actualColor = color != default ? color : Colors.AOE;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendDonut(center - _center, innerRadius, outerRadius, actualColor);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedCircle(ProjectedPoint(center), outerRadius, actualColor, _frameWorldProjectionHeight, arenaClip: clip, arenaOrigin: clipOrigin, innerRadius: innerRadius, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneTri(WPos a, WPos b, WPos c, uint color)
    {
        var actualColor = color != default ? color : Colors.AOE;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendTriangle(a - _center, b - _center, c - _center, actualColor);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedTriangle(ProjectedPoint(a), ProjectedPoint(b), ProjectedPoint(c), actualColor, _frameWorldProjectionHeight, arenaClip: clip, arenaOrigin: clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneIsoscelesTri(WPos apex, WDir height, WDir halfBase, uint color)
    {
        var a = apex - _center;
        var actualColor = color != default ? color : Colors.AOE;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendTriangle(a, a + height + halfBase, a + height - halfBase, actualColor);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedTriangle(ProjectedPoint(apex), ProjectedPoint(apex + height + halfBase), ProjectedPoint(apex + height - halfBase), actualColor, _frameWorldProjectionHeight, arenaClip: clip, arenaOrigin: clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneIsoscelesTri(WPos apex, Angle direction, Angle halfAngle, float height, uint color)
    {
        var a = apex - _center;
        var dir = direction.ToDirection();
        var h = height * dir;
        var halfBase = height * halfAngle.Tan() * dir.OrthoL();
        var actualColor = color != default ? color : Colors.AOE;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendTriangle(a, a + h + halfBase, a + h - halfBase, actualColor);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedTriangle(ProjectedPoint(apex), ProjectedPoint(apex + h + halfBase), ProjectedPoint(apex + h - halfBase), actualColor, _frameWorldProjectionHeight, arenaClip: clip, arenaOrigin: clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color)
    {
        var actualColor = color != default ? color : Colors.AOE;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendRect(origin - _center, direction, lenFront, lenBack, halfWidth, actualColor);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedRect(ProjectedPoint(origin), direction, lenFront, lenBack, halfWidth, actualColor, _frameWorldProjectionHeight, arenaClip: clip, arenaOrigin: clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth, uint color)
        => ZoneRect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneRect(WPos start, WPos end, float halfWidth, uint color)
    {
        var dir = end - start;
        var len = dir.Length();
        if (len > 0f)
        {
            var actualColor = color != default ? color : Colors.AOE;
            var direction = dir / len;
            if (!_frameSuppress2DZoneRendering)
            {
                Dx11ArenaRenderer.AppendRect(start - _center, direction, len, 0f, halfWidth, actualColor);
            }
            if (_frameWorldCamera != null)
            {
                ProjectedArenaArgs(out var clip, out var clipOrigin);
                _frameWorldCamera.DrawProjectedRect(ProjectedPoint(start), direction, len, 0f, halfWidth, actualColor, _frameWorldProjectionHeight, arenaClip: clip, arenaOrigin: clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneCross(WPos origin, Angle rotation, float range, float halfWidth, uint color)
    {
        var actualColor = color != default ? color : Colors.AOE;
        var direction = rotation.ToDirection();
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendCross(origin - _center, direction, range, halfWidth, actualColor);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedCross(ProjectedPoint(origin), direction, range, halfWidth, actualColor, _frameWorldProjectionHeight, arenaClip: clip, arenaOrigin: clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneRelPoly(RelSimplifiedComplexPolygon poly, uint color)
    {
        var actualColor = color != default ? color : Colors.AOE;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendRelPoly(poly, actualColor);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedPolygon(new Vector3(_center.X, _frameWorldProjectionY, _center.Z), poly, _center, actualColor, _frameWorldProjectionHeight, arenaClip: clip, arenaOrigin: clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneCapsule(WPos start, WDir direction, float radius, float length, uint color)
    {
        var actualColor = color != default ? color : Colors.AOE;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendCapsule(start - _center, direction, radius, length, actualColor);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedCapsule(ProjectedPoint(start), direction, radius, length, actualColor, _frameWorldProjectionHeight, arenaClip: clip, arenaOrigin: clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneArcCapsule(WPos start, WPos orbitCenter, Angle angularLength, float radius, uint color)
    {
        var actualColor = color != default ? color : Colors.AOE;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendArcCapsule(start - _center, orbitCenter - start, angularLength.Rad, radius, actualColor);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedArcCapsule(ProjectedPoint(start), ProjectedPoint(orbitCenter), angularLength.Rad, radius, actualColor, _frameWorldProjectionHeight, arenaClip: clip, arenaOrigin: clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PrepareOutlineStyle(uint color, float thickness, out uint lineColor, out float lineThickness, out uint shadowColor, out float shadowThickness)
    {
        lineColor = color != default ? color : Colors.Danger;
        lineThickness = thickness * _frameThicknessScale;
        if (_frameShowOutlinesAndShadows)
        {
            shadowColor = Colors.Shadows;
            shadowThickness = (thickness + 1f) * _frameThicknessScale;
        }
        else
        {
            shadowColor = 0u;
            shadowThickness = lineThickness;
        }
    }

    // draw zone outlines
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneConeOutline(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color = default, float thickness = 1f)
    {
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        var direction = centerDirection.ToDirection();
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendConeOutline(center - _center, innerRadius, outerRadius, direction, halfAngle.Rad, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedCone(ProjectedPoint(center), innerRadius, outerRadius, direction, halfAngle.Rad, lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), clip, clipOrigin, _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneCircleOutline(WPos center, float radius, uint color = default, float thickness = 1f)
    {
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendCircleOutline(center - _center, radius, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedCircle(ProjectedPoint(center), radius, lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), clip, clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneDonutOutline(WPos center, float innerRadius, float outerRadius, uint color = default, float thickness = 1f)
    {
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendDonutOutline(center - _center, innerRadius, outerRadius, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedCircle(ProjectedPoint(center), outerRadius, lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), clip, clipOrigin, innerRadius, _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneTriOutline(WPos a, WPos b, WPos c, uint color = default, float thickness = 1f)
    {
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendTriangleOutline(a - _center, b - _center, c - _center, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedTriangle(ProjectedPoint(a), ProjectedPoint(b), ProjectedPoint(c), lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), clip, clipOrigin, _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneIsoscelesTriOutline(WPos apex, WDir height, WDir halfBase, uint color = default, float thickness = 1f)
    {
        var a = apex - _center;
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendTriangleOutline(a, a + height + halfBase, a + height - halfBase, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedTriangle(ProjectedPoint(apex), ProjectedPoint(apex + height + halfBase), ProjectedPoint(apex + height - halfBase), lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), clip, clipOrigin, _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneRectOutline(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color = default, float thickness = 1f)
    {
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendRectOutline(origin - _center, direction, lenFront, lenBack, halfWidth, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedRect(ProjectedPoint(origin), direction, lenFront, lenBack, halfWidth, lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), clip, clipOrigin, _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneRectOutline(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth, uint color = default, float thickness = 1f)
    {
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        var dir = direction.ToDirection();
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendRectOutline(origin - _center, dir, lenFront, lenBack, halfWidth, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedRect(ProjectedPoint(origin), dir, lenFront, lenBack, halfWidth, lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), clip, clipOrigin, _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneRectOutline(WPos start, WPos end, float halfWidth, uint color = default, float thickness = 1f)
    {
        var dir = end - start;
        var len = dir.Length();
        if (len <= 0f)
        {
            return;
        }
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        var direction = dir / len;
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendRectOutline(start - _center, direction, len, 0f, halfWidth, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedRect(ProjectedPoint(start), direction, len, 0f, halfWidth, lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), clip, clipOrigin, _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneCrossOutline(WPos origin, Angle rotation, float range, float halfWidth, uint color = default, float thickness = 1f)
    {
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        var direction = rotation.ToDirection();
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendCrossOutline(origin - _center, direction, range, halfWidth, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedCross(ProjectedPoint(origin), direction, range, halfWidth, lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), clip, clipOrigin, _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneRelPolyOutline(RelSimplifiedComplexPolygon poly, uint color = default, float thickness = 1f)
    {
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendCustomOutline(poly, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedPolygon(new Vector3(_center.X, _frameWorldProjectionY, _center.Z), poly, _center, lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), clip, clipOrigin, _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneCapsuleOutline(WPos start, WDir direction, float radius, float length, uint color = default, float thickness = 1f)
    {
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendCapsuleOutline(start - _center, direction, radius, length, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedCapsule(ProjectedPoint(start), direction, radius, length, lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), clip, clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZoneArcCapsuleOutline(WPos start, WPos orbitCenter, Angle angularLength, float radius, uint color = default, float thickness = 1f)
    {
        PrepareOutlineStyle(color, thickness, out var lineColor, out var lineThickness, out var shadowColor, out var shadowThickness);
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendArcCapsuleOutline(start - _center, orbitCenter - start, angularLength.Rad, radius, lineColor, lineThickness, shadowColor, shadowThickness);
        }
        if (_frameWorldCamera != null)
        {
            ProjectedArenaArgs(out var clip, out var clipOrigin);
            _frameWorldCamera.DrawProjectedArcCapsule(ProjectedPoint(start), ProjectedPoint(orbitCenter), angularLength.Rad, radius, lineColor, _frameWorldProjectionHeight, ProjectedOutlineWidth(thickness), clip, clipOrigin, holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SpriteScreen(Vector2 min, Vector2 max, IDalamudTextureWrap texture, uint color = 0xFFFFFFFFu)
    {
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendSpriteScreen(min, max, texture, color);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TextScreen(Vector2 center, string text, uint color, float fontSize = 17f, uint outlineColor = 0u, float outlineWidth = 0f)
    {
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendTextScreen(center, text, fontSize * _frameArenaScale, color, outlineColor, outlineWidth * _frameArenaScale);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TextWorld(WPos center, string text, uint color, float fontSize = 17f, uint outlineColor = 0u, float outlineWidth = 0f)
        => TextScreen(WorldPositionToScreenPosition(center), text, color, fontSize, outlineColor, outlineWidth);

    // Text/Icon drawing
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TextWorldBillboard(Vector3 center, string text, uint color, uint outlineColor = 0u, float outlineWidth = 0f)
    {
        if (_frameShowWorldTextIconBillboards)
        {
            _frameWorldCamera?.DrawWorldTextBillboard(center, text, color, _frameWorldTextFontSize, outlineColor, outlineWidth);
        }
    }

    // WPos callers intentionally opt into the currently selected projection floor because WPos has no vertical component
    // use the Vector3 overload whenever the label's Y is authoritative
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TextWorldBillboard(WPos center, string text, uint color, uint outlineColor = 0u, float outlineWidth = 0f)
    {
        if (_frameShowWorldTextIconBillboards && _frameWorldCamera?.ProjectedShapeLayers is { Length: > 0 } layers)
        {
            for (var i = 0; i < layers.Length; ++i)
            {
                TextWorldBillboard(new Vector3(center.X, layers[i].Y + _frameBillboardYOffset, center.Z), text, color, outlineColor, outlineWidth);
            }
        }
        else
        {
            TextWorldBillboard(ProjectedPointBillboard(center), text, color, outlineColor, outlineWidth);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IconScreen(Vector2 center, FontAwesomeIcon icon, uint color, float fontSize = 17f)
    {
        if (!_frameSuppress2DZoneRendering)
        {
            var text = icon.ToIconString();
            Dx11ArenaRenderer.AppendIconScreen(center, text, fontSize, color);
        }
    }

    // WPos has no Y, so its world copy follows the current arena projection floor.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IconWorld(WPos center, FontAwesomeIcon icon, uint color, float fontSize = 17f)
    {
        var text = icon.ToIconString();
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendIconScreen(WorldPositionToScreenPosition(center), text, fontSize, color);
        }
        if (_frameShowWorldTextIconBillboards)
        {
            if (_frameWorldCamera?.ProjectedShapeLayers is { Length: > 0 } layers)
            {
                for (var i = 0; i < layers.Length; ++i)
                {
                    _frameWorldCamera.DrawWorldIconBillboard(new Vector3(center.X, layers[i].Y + _frameBillboardYOffset, center.Z), text, color, _frameWorldIconFontSize);
                }
            }
            else
            {
                _frameWorldCamera?.DrawWorldIconBillboard(ProjectedPointBillboard(center), text, color, _frameWorldIconFontSize);
            }
        }
    }

    // Exact-Y variant for floating/head-height/etc. icons. Explicit X/Z conversion keeps the radar
    // side independent of Vector3->WPos constructor conventions (same bug class as the gaze fix).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IconWorld(Vector3 center, FontAwesomeIcon icon, uint color, float fontSize = 17f)
    {
        var text = icon.ToIconString();
        if (!_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendIconScreen(WorldPositionToScreenPosition(new WPos(center)), text, fontSize, color);
        }
        if (_frameShowWorldTextIconBillboards)
        {
            _frameWorldCamera?.DrawWorldIconBillboard(center, text, color, _frameWorldIconFontSize);
        }
    }

    public void CardinalNames()
    {
        var center = ScreenCenter;
        var fontSetting = _frameCardinalsFontSize;
        var offCenterSizeOffset = (_frameScreenHalfSize + _frameScreenMarginSize * 0.5f) * _bounds.ScaleFactor + fontSetting - 17f;
        var offS = RotatedCoords(new(default, offCenterSizeOffset));
        var offE = RotatedCoords(new(offCenterSizeOffset, default));
        TextScreen(center - offS, "N", Colors.CardinalN, fontSetting);
        TextScreen(center + offS, "S", Colors.CardinalS, fontSetting);
        TextScreen(center + offE, "E", Colors.CardinalE, fontSetting);
        TextScreen(center - offE * 1.02f, "W", Colors.CardinalW, fontSetting); // w is slightly wider, so we are putting it 2% farther away than the E
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ActorInsideBounds(WPos position, Angle rotation, uint color)
        => ActorInsideBounds(position, rotation, color, _frameWorldProjectionHeight);

    // Exact-world actor path. Keep PosRot intact for the 3D marker so the actor's real Y becomes
    // the projection origin; only the 2D radar copy needs the X/Z + rotation decomposition
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ActorInsideBounds(ref Vector4 posRot, uint color)
        => ActorInsideBounds(ref posRot, color, true, true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ActorInsideBounds(ref Vector4 posRot, uint color, bool draw2D, bool drawWorld)
    {
        var position = new WPos(ref posRot);
        var rotation = new Angle(ref posRot);
        var scale = _frameActorScale * _frameThicknessScale;
        var dir = rotation.ToDirection();
        var scale07 = scale * 0.7f * dir;
        var scale035 = scale * 0.35f * dir;
        var scale0433 = scale * 0.433f * dir.OrthoR();
        var positionscale07 = position + scale07;
        var positionscale035 = position - scale035;
        var positionscale035pscale0433 = positionscale035 + scale0433;
        var positionscale035mscale0433 = positionscale035 - scale0433;

        if (draw2D && !_frameSuppress2DZoneRendering)
        {
            if (_frameShowOutlinesAndShadows)
            {
                Dx11ArenaRenderer.AppendPrimitiveTriangleStroke(positionscale07 - _center, positionscale035pscale0433 - _center, positionscale035mscale0433 - _center, Colors.Shadows, 2f * _frameThicknessScale);
            }
            Dx11ArenaRenderer.AppendPrimitiveTriangle(positionscale07 - _center, positionscale035pscale0433 - _center, positionscale035mscale0433 - _center, color);
        }

        if (drawWorld && _frameWorldCamera != null)
        {
            var outlineWidth = _frameShowOutlinesAndShadows ? ProjectedOutlineWidth(2f) : 0f;
            var outlineColor = _frameShowOutlinesAndShadows ? Colors.Shadows : 0u;
            var worldPosRot = posRot;
            worldPosRot.Y = CurrentActorProjectionY(posRot.Y);
            _frameWorldCamera.DrawProjectedActorTriangle(ref worldPosRot, scale, color, outlineColor, CurrentActorWorldProjectionHeight(), outlineWidth,
                holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ActorInsideBounds(WPos position, Angle rotation, uint color, float worldProjectionHeight, bool draw2D = true, bool drawWorld = true)
    {
        var scale = _frameActorScale * _frameThicknessScale;
        var dir = rotation.ToDirection();
        var scale07 = scale * 0.7f * dir;
        var scale035 = scale * 0.35f * dir;
        var scale0433 = scale * 0.433f * dir.OrthoR();
        var positionscale07 = position + scale07;
        var positionscale035 = position - scale035;
        var positionscale035pscale0433 = positionscale035 + scale0433;
        var positionscale035mscale0433 = positionscale035 - scale0433;

        if (draw2D && !_frameSuppress2DZoneRendering)
        {
            if (_frameShowOutlinesAndShadows)
            {
                Dx11ArenaRenderer.AppendPrimitiveTriangleStroke(positionscale07 - _center, positionscale035pscale0433 - _center, positionscale035mscale0433 - _center, Colors.Shadows, 2f * _frameThicknessScale);
            }
            Dx11ArenaRenderer.AppendPrimitiveTriangle(positionscale07 - _center, positionscale035pscale0433 - _center, positionscale035mscale0433 - _center, color);
        }

        // World actor marker: one projected triangle instance carries both fill and optional outline
        if (drawWorld && _frameWorldCamera != null)
        {
            var outlineWidth = _frameShowOutlinesAndShadows ? ProjectedOutlineWidth(2f) : 0f;
            var outlineColor = _frameShowOutlinesAndShadows ? Colors.Shadows : 0u;
            _frameWorldCamera.DrawProjectedTriangleFilledOutlined(ProjectedPoint(positionscale07), ProjectedPoint(positionscale035pscale0433),
                ProjectedPoint(positionscale035mscale0433), color, outlineColor, worldProjectionHeight, outlineWidth, _frameWorldProjectionHeight,
                holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ActorOutsideBounds(WPos position, Angle rotation, uint color)
        => ActorOutsideBounds(position, rotation, color, true, true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ActorOutsideBounds(WPos position, Angle rotation, float worldY, uint color, bool draw2D, bool drawWorld)
    {
        var scale = _frameActorScale;
        var dir = rotation.ToDirection();
        var scale07 = scale * 0.7f * dir;
        var scale035 = scale * 0.35f * dir;
        var scale0433 = scale * 0.433f * dir.OrthoR();
        var positionscale035 = position - scale035;
        var a = position + scale07;
        var b = positionscale035 + scale0433;
        var c = positionscale035 - scale0433;
        var actualColor = color != default ? color : Colors.Danger;
        if (draw2D && !_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendPrimitiveTriangleStroke(a - _center, b - _center, c - _center, actualColor, _frameThicknessScale);
        }
        if (drawWorld)
        {
            worldY = CurrentActorProjectionY(worldY);
            _frameWorldCamera?.DrawProjectedTriangle(a.ToVec3(worldY), b.ToVec3(worldY), c.ToVec3(worldY), actualColor, CurrentActorWorldProjectionHeight(),
                ProjectedOutlineWidth(1f), holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ActorOutsideBounds(WPos position, Angle rotation, uint color, bool draw2D, bool drawWorld)
    {
        var scale = _frameActorScale;
        var dir = rotation.ToDirection();
        var scale07 = scale * 0.7f * dir;
        var scale035 = scale * 0.35f * dir;
        var scale0433 = scale * 0.433f * dir.OrthoR();
        var positionscale035 = position - scale035;
        var a = position + scale07;
        var b = positionscale035 + scale0433;
        var c = positionscale035 - scale0433;
        var actualColor = color != default ? color : Colors.Danger;
        if (draw2D && !_frameSuppress2DZoneRendering)
        {
            Dx11ArenaRenderer.AppendPrimitiveTriangleStroke(a - _center, b - _center, c - _center, actualColor, _frameThicknessScale);
        }
        if (drawWorld)
        {
            _frameWorldCamera?.DrawProjectedTriangle(ProjectedPoint(a), ProjectedPoint(b), ProjectedPoint(c), actualColor, _frameWorldProjectionHeight,
                ProjectedOutlineWidth(1f), holeFillRadius: _frameWorldProjectionHoleFillRadius);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ActorProjected(WPos from, WPos to, Angle rotation, uint color)
    {
        var shape = _frameArenaProjectionShape;
        if (shape != null && !_frameSuppress2DZoneRendering && shape.Parts.Count > 0)
        {
            ActorProjected(from, to, rotation, color, shape, true, false);
        }
        if (shape == null || _frameProjectIntoWorld)
        {
            ActorProjected(from, to, rotation, color, null, shape == null, true);
        }
    }

    private void ActorProjected(WPos from, WPos to, Angle rotation, uint color, RelSimplifiedComplexPolygon? shape, bool draw2D, bool drawWorld)
    {
        if (shape?.Contains(to - _center) ?? InBounds(to))
        {
            // Use the same presentation polygon for the destination test and the boundary ray.
            ActorInsideBounds(to, rotation, color, _frameWorldProjectionHeight, draw2D, drawWorld);
            return;
        }

        var dir = to - from;
        var l = dir.Length();

        if (l == default)
        {
            return; // can't determine projection direction
        }

        dir /= l;
        var t = shape != null ? Intersect.RayPolygon(from - _center, dir, shape) : IntersectRayBounds(from, dir);
        if (t <= l)
        {
            ActorOutsideBounds(from + t * dir, rotation, color, draw2D, drawWorld);
        }
    }

    // Preferred actor-marker overload for callers that have the native game PosRot. The real Y is
    // preserved for world projection; Actor(WPos, Angle, ...) remains the synthetic/legacy path.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Actor(ref Vector4 posRot, uint color, bool drawWorld = true)
    {
        using (WorldProjectionLayerForActor(ref posRot))
        {
            var shape = _frameArenaProjectionShape;
            if (shape != null && !_frameSuppress2DZoneRendering && shape.Parts.Count > 0)
            {
                Actor(ref posRot, color, shape, true, false);
            }
            if (shape == null || drawWorld && _frameProjectIntoWorld)
            {
                Actor(ref posRot, color, null, shape == null, drawWorld);
            }
        }
    }

    private void Actor(ref Vector4 posRot, uint color, RelSimplifiedComplexPolygon? shape, bool draw2D, bool drawWorld)
    {
        var position = new WPos(ref posRot);
        var offset = position - _center;
        if (shape?.Contains(offset) ?? InBounds(position))
        {
            ActorInsideBounds(ref posRot, color, draw2D, drawWorld);
        }
        else
        {
            // Keep the actor's exact Y even when its 2D marker is clamped to the presentation boundary.
            var rot = new Angle(ref posRot);
            if (offset.AlmostEqual(default, 1f) || Math.Abs(offset.X) < 0.1f) // if actor is almost in the center of the arena, do nothing (eg donut arena or wall boss)
            {
                ActorOutsideBounds(position, rot, color, draw2D, drawWorld);
            }
            else
            {
                var clamped = shape != null ? _center + shape.ClosestPointOnBoundary(offset) : ClampToBounds(position);
                ActorOutsideBounds(clamped, rot, posRot.Y, color, draw2D, drawWorld);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Actor(WPos position, Angle rotation, uint color, bool drawWorld = true)
    {
        var shape = _frameArenaProjectionShape;
        if (shape != null && !_frameSuppress2DZoneRendering && shape.Parts.Count > 0)
        {
            Actor(position, rotation, color, shape, true, false);
        }
        // World markers keep the global logical bounds and their existing clamping exceptions.
        if (shape == null || drawWorld && _frameProjectIntoWorld)
        {
            Actor(position, rotation, color, null, shape == null, drawWorld);
        }
    }

    private void Actor(WPos position, Angle rotation, uint color, RelSimplifiedComplexPolygon? shape, bool draw2D, bool drawWorld)
    {
        var offset = position - _center;
        if (shape?.Contains(offset) ?? InBounds(position))
        {
            ActorInsideBounds(position, rotation, color, _frameWorldProjectionHeight, draw2D, drawWorld);
        }
        else
        {
            // Do not apply the global custom-arena center/axis exceptions to a layer: those points
            // can lie in a hole or an inactive island and must still clamp to the visible boundary.
            if (offset.AlmostEqual(default, 1f) || Math.Abs(offset.X) < 0.1f) // if actor is almost in the center of the arena, do nothing (eg donut arena or wall boss)
            {
                ActorOutsideBounds(position, rotation, color, draw2D, drawWorld);
            }
            else
            {
                var clamped = shape != null ? _center + shape.ClosestPointOnBoundary(offset) : ClampToBounds(position);
                ActorOutsideBounds(clamped, rotation, color, draw2D, drawWorld);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Actor(Actor? actor, uint color = default, bool allowDeadAndUntargetable = false, bool? drawWorld = null)
    {
        if (actor != null && !actor.IsDestroyed && (allowDeadAndUntargetable || actor.IsTargetable && !actor.IsDead))
        {
            var showWorld = drawWorld ?? _frameProjectActorTriangles;
            Actor(ref actor.PosRot, color == default ? Colors.Enemy : color, showWorld);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Actors(IEnumerable<Actor> actors, uint color = default, bool allowDeadAndUntargetable = false, bool? drawWorld = null)
    {
        foreach (var a in actors)
        {
            Actor(a, color == default ? Colors.Enemy : color, allowDeadAndUntargetable, drawWorld);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Actors(List<Actor> actors, uint color = default, bool allowDeadAndUntargetable = false, bool? drawWorld = null)
    {
        var count = actors.Count;
        for (var i = 0; i < count; ++i)
        {
            Actor(actors[i], color == default ? Colors.Enemy : color, allowDeadAndUntargetable, drawWorld);
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
                    Actor(enemy, color_, allowDeadAndUntargetable);
                }
            }
        }
    }

    public void ActorsInBounds(BossModule module, uint[] actors, uint color = default, bool allowDeadAndUntargetable = false)
    {
        var actors_ = actors;
        var len = actors_.Length;
        var center = _center;
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
                    Actor(enemy, color_, allowDeadAndUntargetable);
                }
            }
        }
    }

    public void End()
    {
        if (!_frameDraw2D)
        {
            return;
        }
        // Flush the final contiguous run while the arena clip rect is still active
        Dx11ArenaRenderer.EndArena();
        ImGui.GetWindowDrawList().PopClipRect();
    }
}
