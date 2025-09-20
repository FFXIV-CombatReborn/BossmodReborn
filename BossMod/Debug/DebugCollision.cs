using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision.Math;
using Dalamud.Bindings.ImGui;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace BossMod;

public sealed unsafe class DebugCollision() : IDisposable
{
    private readonly UITree _tree = new();
    private BitMask _shownLayers = new(1);
    private BitMask _materialMask;
    private BitMask _materialId;
    private bool _showZeroLayer = true;
    private bool _showOnlyFlagRaycast;
    private bool _showOnlyFlagVisit;

    private readonly HashSet<nint> _streamedMeshes = [];
    private BitMask _availableLayers;
    private BitMask _availableMaterials;

    private static readonly (int, int)[] _boxEdges =
    [
        (0, 1), (1, 3), (3, 2), (2, 0),
        (4, 5), (5, 7), (7, 6), (6, 4),
        (0, 4), (1, 5), (2, 6), (3, 7)
    ];

    private static readonly Vector3[] _boxCorners =
    [
        new(-1, -1, -1),
        new(-1, -1,  1),
        new(-1,  1, -1),
        new(-1,  1,  1),
        new( 1, -1, -1),
        new( 1, -1,  1),
        new( 1,  1, -1),
        new( 1,  1,  1),
    ];

    private float _maxDrawDistance = 10;

    public void Dispose()
    {
    }

    public void Draw()
    {
        var module = Framework.Instance()->BGCollisionModule;
        ImGui.TextUnformatted($"Module: {(nint)module:X}->{(nint)module->SceneManager:X} ({module->SceneManager->NumScenes} scenes, {module->LoadInProgressCounter} loads)");
        ImGui.TextUnformatted($"Streaming: {SphereStr(module->ForcedStreamingSphere)} / {SphereStr(module->SceneManager->StreamingSphere)}");
        module->ForcedStreamingSphere.W = _maxDrawDistance;

        GatherInfo();
        DrawSettings();

        var i = 0;
        foreach (var s in module->SceneManager->Scenes)
        {
            DrawSceneColliders(s->Scene, i);
            DrawSceneQuadtree(s->Scene->Quadtree, i);
            ++i;
        }
    }

    public void DrawVisualizers()
    {

    }

    private void GatherInfo()
    {
        _streamedMeshes.Clear();
        _availableLayers.Reset();
        _availableMaterials.Reset();
        foreach (var s in Framework.Instance()->BGCollisionModule->SceneManager->Scenes)
        {
            foreach (var coll in s->Scene->Colliders)
            {
                _availableLayers |= new BitMask(coll->LayerMask);
                _availableMaterials |= new BitMask(coll->ObjectMaterialValue);

                var collType = coll->GetColliderType();
                if (collType == ColliderType.Streamed)
                {
                    var cast = (ColliderStreamed*)coll;
                    if (cast->Header != null && cast->Elements != null)
                    {
                        for (var i = 0; i < cast->Header->NumMeshes; ++i)
                        {
                            var m = cast->Elements[i].Mesh;
                            if (m != null)
                                _streamedMeshes.Add((nint)m);
                        }
                    }
                }
                else if (collType == ColliderType.Mesh)
                {
                    var cast = (ColliderMesh*)coll;
                    if (!cast->MeshIsSimple && cast->Mesh != null)
                    {
                        var mesh = (MeshPCB*)cast->Mesh;
                        var mask = new BitMask(coll->ObjectMaterialMask);
                        GatherMeshNodeMaterials(mesh->RootNode, ~mask);
                    }
                }
            }
        }
    }

    private bool FilterCollider(Collider* coll)
    {
        if (coll->LayerMask == 0 ? !_showZeroLayer : (_shownLayers.Raw & coll->LayerMask) == 0)
            return false;
        if (_showOnlyFlagRaycast && (coll->VisibilityFlags & 1) == 0)
            return false;
        if (_showOnlyFlagVisit && (coll->VisibilityFlags & 2) == 0)
            return false;
        var matFilter = _availableMaterials & _materialMask;
        if (matFilter.Any() && coll->GetColliderType() != ColliderType.Mesh)
            return /*_materialId.None() ? (matFilter.Raw & coll->ObjectMaterialValue) != 0 :*/ (matFilter.Raw & (coll->ObjectMaterialValue ^ _materialId.Raw)) == 0;
        return true;
    }

    private void DrawSettings()
    {
        using var n = _tree.Node2("Settings");
        if (!n.Opened)
            return;

        ImGui.Checkbox("Show objects with zero layer", ref _showZeroLayer);
        {
            var shownLayers = _availableLayers & _shownLayers;
            using var layers = ImRaii.Combo("Shown layers", shownLayers == _availableLayers ? "All" : shownLayers.None() ? "None" : string.Join(", ", shownLayers.SetBits()));
            if (layers)
            {
                foreach (var i in _availableLayers.SetBits())
                {
                    var shown = _shownLayers[i];
                    if (ImGui.Checkbox($"Layer {i}", ref shown))
                        _shownLayers[i] = shown;
                }
            }
        }

        {
            var matMask = _materialMask & _availableMaterials;
            using var materials = ImRaii.Combo("Material mask", matMask.None() ? "None" : matMask.Raw.ToString("X"));
            if (materials)
            {
                foreach (var i in _availableMaterials.SetBits())
                {
                    var filter = _materialMask[i];
                    if (ImGui.Checkbox($"Material {1u << i:X16}", ref filter))
                        _materialMask[i] = filter;
                }
            }
        }

        {
            var matId = _materialId & _availableMaterials;
            using var materials = ImRaii.Combo("Material id", matId.None() ? "None" : matId.Raw.ToString("X"));
            if (materials)
            {
                foreach (var i in _availableMaterials.SetBits())
                {
                    var filter = _materialId[i];
                    if (ImGui.Checkbox($"Material {1u << i:X16}", ref filter))
                        _materialId[i] = filter;
                }
            }
        }

        {
            using var flags = ImRaii.Combo("Flag filter", _showOnlyFlagRaycast ? _showOnlyFlagVisit ? "Only when both flags are set" : "Only if raycast flag is set" : _showOnlyFlagVisit ? "Only if global visit flag is set" : "Show everything");
            if (flags)
            {
                ImGui.Checkbox("Hide objects without raycast flag (0x1)", ref _showOnlyFlagRaycast);
                ImGui.Checkbox("Hide objects without global viist flag (0x2)", ref _showOnlyFlagVisit);
            }
        }

        if (ImGui.SliderFloat("Max Draw Distance", ref _maxDrawDistance, 10, 1000, "%.0f"))
        { }
    }

    private void DrawSceneColliders(Scene* s, int index)
    {
        using var n = _tree.Node2($"Scene {index}: {s->NumColliders} colliders, {s->NumLoading} loading, streaming={SphereStr(s->StreamingSphere)}###scene_{index}");
        if (n.SelectedOrHovered)
            foreach (var coll in s->Colliders)
                if (FilterCollider(coll))
                    VisualizeCollider(coll, _materialId, _materialMask);
        if (n.Opened)
            foreach (var coll in s->Colliders)
                DrawCollider(coll);
    }

    private void DrawSceneQuadtree(Quadtree* tree, int index)
    {
        using var n = _tree.Node2($"Quadtree {index}: {tree->NumLevels} levels ([{tree->MinX}, {tree->MaxX}]x[{tree->MinZ}, {tree->MaxZ}], leaf {tree->LeafSizeX}x{tree->LeafSizeZ}), {tree->NumNodes} nodes###tree_{index}");
        if (!n.Opened)
            return;

        for (int level = 0; level < tree->NumLevels; ++level)
        {
            var cellSizeX = (tree->MaxX - tree->MinX + 1) / (1 << level);
            var cellSizeZ = (tree->MaxZ - tree->MinZ + 1) / (1 << level);
            using var ln = _tree.Node2($"Level {level}, {cellSizeX}x{cellSizeZ} cells ({Quadtree.NumNodesAtLevel(level)} nodes starting at {Quadtree.StartingNodeForLevel(level)})");
            if (!ln.Opened)
                continue;

            var nodes = tree->NodesAtLevel(level);
            for (int i = 0; i < nodes.Length; ++i)
            {
                ref var node = ref nodes[i];
                if (node.Node.NodeLink.Next == null)
                    continue;

                var coord = Quadtree.CellCoords((uint)i);
                var cellX = tree->MinX + coord.x * cellSizeX;
                var cellZ = tree->MinZ + coord.z * cellSizeZ;
                using var cn = _tree.Node2($"[{coord.x}, {coord.z}] ([{cellX}x{cellZ}]-[{cellX + cellSizeX}x{cellZ + cellSizeZ}])###node_{level}_{i}", node.Node.NodeLink.Next == null);

                if (cn.Opened)
                    foreach (var coll in node.Colliders)
                        DrawCollider(coll);

                if (cn.SelectedOrHovered)
                {
                    // TODO: visualize cell bounds?
                    foreach (var coll in node.Colliders)
                        VisualizeCollider(coll, _materialId, _materialMask);
                }
            }
        }
    }

    private void DrawCollider(Collider* coll)
    {
        if (!FilterCollider(coll))
            return;

        var raycastFlag = (coll->VisibilityFlags & 1) != 0;
        var globalVisitFlag = (coll->VisibilityFlags & 2) != 0;
        var flagsText = raycastFlag ? globalVisitFlag ? "raycast, global visit" : "raycast" : globalVisitFlag ? "global visit" : "none";

        var type = coll->GetColliderType();
        var color = Colors.TextColor1;
        if (type == ColliderType.Mesh)
        {
            var collMesh = (ColliderMesh*)coll;
            if (_streamedMeshes.Contains((nint)coll))
                color = Colors.TextColor4;
            else if (collMesh->MeshIsSimple)
                color = Colors.TextColor3;
        }
        using var n = _tree.Node2($"{type} {(nint)coll:X}, layers={coll->LayerMask:X8}, layout-id={coll->LayoutObjectId:X16}, refs={coll->NumRefs}, material={coll->ObjectMaterialValue:X}/{coll->ObjectMaterialMask:X}, flags={flagsText}###{(nint)coll:X}", false, color);
        if (ImGui.BeginPopupContextItem($"###{(nint)coll:X}"))
        {
            ContextCollider(coll);
            ImGui.EndPopup();
        }
        if (n.SelectedOrHovered)
            VisualizeCollider(coll, _materialId, _materialMask);
        if (!n.Opened)
            return;

        _tree.LeafNode2($"Raw flags: {coll->VisibilityFlags:X}");
        switch (type)
        {
            case ColliderType.Streamed:
                {
                    var cast = (ColliderStreamed*)coll;
                    DrawResource(cast->Resource);
                    var path = cast->PathBaseString;
                    _tree.LeafNode2($"Path: {path}/{Encoding.UTF8.GetString(cast->PathBase[(path.Length + 1)..])}");
                    _tree.LeafNode2($"Streamed: [{cast->StreamedMinX:f3}x{cast->StreamedMinZ:f3}] - [{cast->StreamedMaxX:f3}x{cast->StreamedMaxZ:f3}]");
                    _tree.LeafNode2($"Loaded: {cast->Loaded} ({cast->NumMeshesLoading} meshes load in progress)");
                    if (cast->Header != null && cast->Entries != null && cast->Elements != null)
                    {
                        var headerRaw = (float*)cast->Header;
                        _tree.LeafNode2($"Header: meshes={cast->Header->NumMeshes}, u={headerRaw[1]:f3} {headerRaw[2]:f3} {headerRaw[3]:f3} {headerRaw[4]:f3} {headerRaw[5]:f3} {headerRaw[6]:f3} {headerRaw[7]:f3}");
                        for (var i = 0; i < cast->Header->NumMeshes; ++i)
                        {
                            var entry = cast->Entries + i;
                            var elem = cast->Elements + i;
                            var entryRaw = (uint*)entry;
                            using var mn = _tree.Node2($"Mesh {i}: file=tr{entry->MeshId:d4}.pcb, bounds={AABBStr(entry->Bounds)} == {(nint)elem->Mesh:X}###mesh_{i}", elem->Mesh == null);
                            if (mn.SelectedOrHovered && elem->Mesh != null)
                                VisualizeCollider(&elem->Mesh->Collider, _materialId, _materialMask);
                            if (mn.Opened)
                                DrawColliderMesh(elem->Mesh);
                        }
                    }
                }
                break;
            case ColliderType.Mesh:
                DrawColliderMesh((ColliderMesh*)coll);
                break;
            case ColliderType.Box:
                {
                    var cast = (ColliderBox*)coll;
                    _tree.LeafNode2($"Translation: {Vec3Str(cast->Translation)}");
                    var rotation = cast->Rotation;
                    _tree.LeafNode2($"Rotation: {Vec3Str(rotation)} (Yaw: {HeadingDegFromWorld(ref cast->World)}°)");
                    _tree.LeafNode2($"Scale: {Vec3Str(cast->Scale)}");
                    DrawMat4x3("World", ref cast->World);
                    DrawMat4x3("InvWorld", ref cast->InvWorld);
                }
                break;
            case ColliderType.Cylinder:
                {
                    var cast = (ColliderCylinder*)coll;
                    _tree.LeafNode2($"Translation: {Vec3Str(cast->Translation)}");
                    var rotation = cast->Rotation;
                    _tree.LeafNode2($"Rotation: {Vec3Str(rotation)} (Yaw: {HeadingDegFromWorld(ref cast->World)}°)");
                    _tree.LeafNode2($"Scale: {Vec3Str(cast->Scale)}");
                    _tree.LeafNode2($"Radius: {cast->Radius:f3}");
                    DrawMat4x3("World", ref cast->World);
                    DrawMat4x3("InvWorld", ref cast->InvWorld);
                }
                break;
            case ColliderType.Sphere:
                {
                    var cast = (ColliderSphere*)coll;
                    _tree.LeafNode2($"Translation: {Vec3Str(cast->Translation)}");
                    var rotation = cast->Rotation;
                    _tree.LeafNode2($"Rotation: {Vec3Str(rotation)} (Yaw: {HeadingDegFromWorld(ref cast->World)}°)");
                    _tree.LeafNode2($"Scale: {Vec3Str(cast->Scale)}");
                    DrawMat4x3("World", ref cast->World);
                    DrawMat4x3("InvWorld", ref cast->InvWorld);
                }
                break;
            case ColliderType.Plane:
            case ColliderType.PlaneTwoSided:
                {
                    var cast = (ColliderPlane*)coll;
                    _tree.LeafNode2($"Normal: {cast->World.Row2 / cast->Scale.Z:f3}");
                    _tree.LeafNode2($"Translation: {Vec3Str(cast->Translation)}");
                    var rotation = cast->Rotation;
                    _tree.LeafNode2($"Rotation: {Vec3Str(rotation)} (Yaw: {HeadingDegFromWorld(ref cast->World)}°)");
                    _tree.LeafNode2($"Scale: {Vec3Str(cast->Scale)}");
                    DrawMat4x3("World", ref cast->World);
                    DrawMat4x3("InvWorld", ref cast->InvWorld);
                }
                break;
        }
    }

    private void DrawColliderMesh(ColliderMesh* coll)
    {
        DrawResource(coll->Resource);
        _tree.LeafNode2($"Translation: {Vec3Str(coll->Translation)}");
        var rotation = coll->Rotation;
        _tree.LeafNode2($"Rotation: {Vec3Str(rotation)} (Yaw: {HeadingDegFromWorld(ref coll->World)}°)");
        _tree.LeafNode2($"Scale: {Vec3Str(coll->Scale)}");
        DrawMat4x3("World", ref coll->World);
        DrawMat4x3("InvWorld", ref coll->InvWorld);
        if (_tree.LeafNode2($"Bounding sphere: {SphereStr(coll->BoundingSphere)}").SelectedOrHovered)
            VisualizeSphere(coll->BoundingSphere, Colors.CollisionColor1);
        if (_tree.LeafNode2($"Bounding box: {AABBStr(coll->WorldBoundingBox)}").SelectedOrHovered)
            VisualizeOBB(ref coll->WorldBoundingBox, ref Matrix4x3.Identity, Colors.CollisionColor1);
        _tree.LeafNode2($"Total size: {coll->TotalPrimitives} prims, {coll->TotalChildren} nodes");
        _tree.LeafNode2($"Mesh type: {(coll->MeshIsSimple ? "simple" : coll->MemoryData != null ? "PCB in-memory" : "PCB from file")} {(coll->Loaded ? "" : "(loading)")}");
        if (coll->Mesh == null || coll->MeshIsSimple)
            return;

        var mesh = (MeshPCB*)coll->Mesh;
        DrawColliderMeshPCBNode("Root", mesh->RootNode, ref coll->World, coll->Collider.ObjectMaterialValue & coll->Collider.ObjectMaterialMask, ~coll->Collider.ObjectMaterialMask, coll);
    }

    private void DrawColliderMeshPCBNode(string tag, MeshPCB.FileNode* node, ref Matrix4x3 world, ulong objMatId, ulong objMatInvMask, ColliderMesh* coll)
    {
        if (node == null)
            return;

        using var n = _tree.Node2(tag);
        if (n.SelectedOrHovered)
            VisualizeColliderMeshPCBNode(node, ref world, Colors.CollisionColor1, objMatId, objMatId, _materialId, _materialMask);
        if (!n.Opened)
            return;

        _tree.LeafNode2($"Header: {node->Header:X16}");

        if (_tree.LeafNode2($"AABB: {AABBStr(node->LocalBounds)}").SelectedOrHovered)
            VisualizeOBB(ref node->LocalBounds, ref world, Colors.CollisionColor1);

        using var nv = _tree.Node2($"Vertices: {node->NumVertsRaw}+{node->NumVertsCompressed}", node->NumVertsRaw + node->NumVertsCompressed == 0);
        if (nv.Opened)
        {
            // Collect all vertices
            Vector3 translation = coll->Translation;
            Vector3 rotation = coll->Rotation;

            List<(Vector3 vertex, int index, char type)> vertices = [];

            for (var i = 0; i < node->NumVertsRaw + node->NumVertsCompressed; ++i)
            {
                var v = node->Vertex(i);
                var transformedVertex = ApplyTransformation(v, translation, rotation);
                vertices.Add((transformedVertex, i, i < node->NumVertsRaw ? 'r' : 'c'));
            }

            var playerPos = Service.ClientState.LocalPlayer!.Position;
            // Sort vertices by distance to player position, ignore height

            vertices.Sort((a, b) =>
            {
                var distA = (playerPos.X - a.vertex.X) * (playerPos.X - a.vertex.X) +
                              (playerPos.Z - a.vertex.Z) * (playerPos.Z - a.vertex.Z);

                var distB = (playerPos.X - b.vertex.X) * (playerPos.X - b.vertex.X) +
                              (playerPos.Z - b.vertex.Z) * (playerPos.Z - b.vertex.Z);

                return distA.CompareTo(distB);
            });

            // Render vertices in sorted order
            foreach (var (vertex, index, type) in vertices)
            {
                var vertexStr = $"new({vertex.X.ToString("F5", System.Globalization.CultureInfo.InvariantCulture)}f, {vertex.Z.ToString("F5", System.Globalization.CultureInfo.InvariantCulture)}f)";
                using var node2 = _tree.Node2($"[{index}] ({type}): {Vec3Str(vertex)}");
                if (node2.SelectedOrHovered)
                {
                    VisualizeVertex(vertex, Colors.CollisionColor2);
                }

                if (ImGui.BeginPopupContextItem($"##popup_vertex_{index}"))
                {
                    if (ImGui.MenuItem("Copy to Clipboard"))
                    {
                        ImGui.SetClipboardText(vertexStr);
                    }
                    ImGui.EndPopup();
                }
            }
        }
        {
            using var np = _tree.Node2($"Primitives: {node->NumPrims}", node->NumPrims == 0);
            if (np.Opened)
            {
                int i = 0;
                foreach (ref var prim in node->Primitives)
                    if (_tree.LeafNode2($"[{++i}]: {prim.V1}x{prim.V2}x{prim.V3}, material={prim.Material:X8}").SelectedOrHovered)
                        VisualizeTriangle(node, ref prim, ref world, Colors.CollisionColor2);
            }
        }
        DrawColliderMeshPCBNode($"Child 1 (+{node->Child1Offset})", node->Child1, ref world, objMatId, objMatId, coll);
        DrawColliderMeshPCBNode($"Child 2 (+{node->Child2Offset})", node->Child2, ref world, objMatId, objMatId, coll);
    }

    private void DrawResource(Resource* res)
    {
        if (res != null)
        {
            _tree.LeafNode2($"Resource: {(nint)res:X} '{res->PathString}'");
        }
        else
        {
            _tree.LeafNode2($"Resource: null");
        }
    }

    public void VisualizeCollider(Collider* coll, BitMask filterId, BitMask filterMask)
    {
        switch (coll->GetColliderType())
        {
            case ColliderType.Streamed:
                {
                    var cast = (ColliderStreamed*)coll;
                    if (cast->Header != null && cast->Elements != null)
                    {
                        for (int i = 0; i < cast->Header->NumMeshes; ++i)
                        {
                            var elem = cast->Elements + i;
                            VisualizeColliderMesh(elem->Mesh, Colors.CollisionColor1, _materialId, _materialMask);
                        }
                    }
                }
                break;
            case ColliderType.Mesh:
                VisualizeColliderMesh((ColliderMesh*)coll, _streamedMeshes.Contains((nint)coll) ? Colors.CollisionColor1 : Colors.CollisionColor2, _materialId, _materialMask);
                break;
            case ColliderType.Box:
                {
                    var cast = (ColliderBox*)coll;
                    Span<Vector3> corners = stackalloc Vector3[8];
                    for (var i = 0; i < 8; ++i)
                        corners[i] = cast->World.TransformCoordinate(_boxCorners[i]);

                    foreach (var (start, end) in _boxEdges)
                        Camera.Instance?.DrawWorldLine(corners[start], corners[end], Colors.CollisionColor3);
                }
                break;
            case ColliderType.Cylinder:
                {
                    var cast = (ColliderCylinder*)coll;
                    VisualizeCylinder(ref cast->World, Colors.CollisionColor3);
                }
                break;
            case ColliderType.Sphere:
                {
                    var cast = (ColliderSphere*)coll;
                    Camera.Instance?.DrawWorldSphere(cast->Translation, cast->Scale.X, Colors.CollisionColor3);
                }
                break;
            case ColliderType.Plane:
            case ColliderType.PlaneTwoSided:
                {
                    var cast = (ColliderPlane*)coll;
                    var a = cast->World.TransformCoordinate(new(-1, +1, 0));
                    var b = cast->World.TransformCoordinate(new(-1, -1, 0));
                    var c = cast->World.TransformCoordinate(new(+1, -1, 0));
                    var d = cast->World.TransformCoordinate(new(+1, +1, 0));
                    Camera.Instance?.DrawWorldLine(a, b, Colors.CollisionColor3);
                    Camera.Instance?.DrawWorldLine(b, c, Colors.CollisionColor3);
                    Camera.Instance?.DrawWorldLine(c, d, Colors.CollisionColor3);
                    Camera.Instance?.DrawWorldLine(d, a, Colors.CollisionColor3);
                }
                break;
        }
    }

    private void VisualizeColliderMesh(ColliderMesh* coll, uint color, BitMask filterId, BitMask filterMask)
    {
        if (coll != null && !coll->MeshIsSimple && coll->Mesh != null)
        {
            var mesh = (MeshPCB*)coll->Mesh;
            VisualizeColliderMeshPCBNode(mesh->RootNode, ref coll->World, color, coll->Collider.ObjectMaterialValue & coll->Collider.ObjectMaterialMask, ~coll->Collider.ObjectMaterialMask, filterId, filterMask);
        }
    }

    private void VisualizeColliderMeshPCBNode(MeshPCB.FileNode* node, ref Matrix4x3 world, uint color, ulong objMatId, ulong objMatInvMask, BitMask filterId, BitMask filterMask)
    {
        if (node == null)
            return;

        if (node->NumVertsRaw + node->NumVertsCompressed != 0)
        {
            for (var i = 0; i < node->NumPrims; ++i)
            {
                var prim = node->Primitives[i];

                var v1 = world.TransformCoordinate(node->Vertex(prim.V1));
                var v2 = world.TransformCoordinate(node->Vertex(prim.V2));
                var v3 = world.TransformCoordinate(node->Vertex(prim.V3));

                Camera.Instance?.DrawWorldLine(v1, v2, color);
                Camera.Instance?.DrawWorldLine(v2, v3, color);
                Camera.Instance?.DrawWorldLine(v3, v1, color);
            }
        }

        VisualizeColliderMeshPCBNode(node->Child1, ref world, color, objMatId, objMatInvMask, filterId, filterMask);
        VisualizeColliderMeshPCBNode(node->Child2, ref world, color, objMatId, objMatInvMask, filterId, filterMask);
    }

    private void VisualizeOBB(ref AABB localBB, ref Matrix4x3 world, uint color)
    {
        var aaa = world.TransformCoordinate(new(localBB.Min.X, localBB.Min.Y, localBB.Min.Z));
        var aab = world.TransformCoordinate(new(localBB.Min.X, localBB.Min.Y, localBB.Max.Z));
        var aba = world.TransformCoordinate(new(localBB.Min.X, localBB.Max.Y, localBB.Min.Z));
        var abb = world.TransformCoordinate(new(localBB.Min.X, localBB.Max.Y, localBB.Max.Z));
        var baa = world.TransformCoordinate(new(localBB.Max.X, localBB.Min.Y, localBB.Min.Z));
        var bab = world.TransformCoordinate(new(localBB.Max.X, localBB.Min.Y, localBB.Max.Z));
        var bba = world.TransformCoordinate(new(localBB.Max.X, localBB.Max.Y, localBB.Min.Z));
        var bbb = world.TransformCoordinate(new(localBB.Max.X, localBB.Max.Y, localBB.Max.Z));
        Camera.Instance?.DrawWorldLine(aaa, aab, color);
        Camera.Instance?.DrawWorldLine(aab, bab, color);
        Camera.Instance?.DrawWorldLine(bab, baa, color);
        Camera.Instance?.DrawWorldLine(baa, aaa, color);
        Camera.Instance?.DrawWorldLine(aba, abb, color);
        Camera.Instance?.DrawWorldLine(abb, bbb, color);
        Camera.Instance?.DrawWorldLine(bbb, bba, color);
        Camera.Instance?.DrawWorldLine(bba, aba, color);
        Camera.Instance?.DrawWorldLine(aaa, aba, color);
        Camera.Instance?.DrawWorldLine(aab, abb, color);
        Camera.Instance?.DrawWorldLine(baa, bba, color);
        Camera.Instance?.DrawWorldLine(bab, bbb, color);
    }

    private void VisualizeCylinder(ref Matrix4x3 world, uint color)
    {
        int numSegments = CurveApprox.CalculateCircleSegments(world.Row0.Length(), 360.Degrees(), 0.1f);
        var prev1 = world.TransformCoordinate(new(0, +1, 1));
        var prev2 = world.TransformCoordinate(new(0, -1, 1));
        for (int i = 1; i <= numSegments; ++i)
        {
            var dir = (i * 360.0f / numSegments).Degrees().ToDirection().ToVec2();
            var curr1 = world.TransformCoordinate(new(dir.X, +1, dir.Y));
            var curr2 = world.TransformCoordinate(new(dir.X, -1, dir.Y));
            Camera.Instance?.DrawWorldLine(curr1, prev1, color);
            Camera.Instance?.DrawWorldLine(curr2, prev2, color);
            Camera.Instance?.DrawWorldLine(curr1, curr2, color);
            prev1 = curr1;
            prev2 = curr2;
        }
    }

    private void VisualizeSphere(Vector4 sphere, uint color) => Camera.Instance?.DrawWorldSphere(new(sphere.X, sphere.Y, sphere.Z), sphere.W, color);

    private void VisualizeVertex(Vector3 worldPos, uint color) => Camera.Instance?.DrawWorldSphere(worldPos, 0.1f, color);

    private void VisualizeTriangle(MeshPCB.FileNode* node, ref Mesh.Primitive prim, ref Matrix4x3 world, uint color)
    {
        var v1 = world.TransformCoordinate(node->Vertex(prim.V1));
        var v2 = world.TransformCoordinate(node->Vertex(prim.V2));
        var v3 = world.TransformCoordinate(node->Vertex(prim.V3));
        Camera.Instance?.DrawWorldLine(v1, v2, color);
        Camera.Instance?.DrawWorldLine(v2, v3, color);
        Camera.Instance?.DrawWorldLine(v3, v1, color);
    }

    private void GatherMeshNodeMaterials(MeshPCB.FileNode* node, BitMask invMask)
    {
        if (node == null)
            return;
        foreach (ref var prim in node->Primitives)
            _availableMaterials |= invMask & new BitMask(prim.Material);
        GatherMeshNodeMaterials(node->Child1, invMask);
        GatherMeshNodeMaterials(node->Child2, invMask);
    }

    private string SphereStr(Vector4 s) => $"[{s.X:f3}, {s.Y:f3}, {s.Z:f3}] R{s.W:f3}";
    private string Vec3Str(Vector3 v) => $"[{v.X:f5}, {v.Y:f3}, {v.Z:f5}]";
    private string AABBStr(AABB bb) => $"{Vec3Str(bb.Min)} - {Vec3Str(bb.Max)}";

    private void DrawMat4x3(string tag, ref Matrix4x3 mat)
    {
        _tree.LeafNode2($"{tag} R0: {Vec3Str(mat.Row0)}");
        _tree.LeafNode2($"{tag} R1: {Vec3Str(mat.Row1)}");
        _tree.LeafNode2($"{tag} R2: {Vec3Str(mat.Row2)}");
        _tree.LeafNode2($"{tag} R3: {Vec3Str(mat.Row3)}");
    }

    private void ContextCollider(Collider* coll)
    {
        var activeLayers = new BitMask(coll->LayerMask);
        foreach (var i in _availableLayers.SetBits())
        {
            var active = activeLayers[i];
            if (ImGui.Checkbox($"Layer {i}", ref active))
            {
                activeLayers[i] = active;
                coll->LayerMask = activeLayers.Raw;
            }
        }

        var raycast = (coll->VisibilityFlags & 1) != 0;
        if (ImGui.Checkbox("Flag: raycast", ref raycast))
            coll->VisibilityFlags ^= 1;

        var globalVisit = (coll->VisibilityFlags & 2) != 0;
        if (ImGui.Checkbox("Flag: global visit", ref globalVisit))
            coll->VisibilityFlags ^= 2;
    }

    private static Vector3 ApplyTransformation(Vector3 vertex, Vector3 translation, Vector3 rotation)
    {
        var rotX = rotation.X;
        var rotY = rotation.Y;
        var rotZ = rotation.Z;
        var rotMatrix = Matrix4x4.CreateRotationX(rotX) * Matrix4x4.CreateRotationY(rotY) * Matrix4x4.CreateRotationZ(rotZ);
        var rotatedVertex = Vector3.Transform(vertex, rotMatrix);
        return rotatedVertex + translation;
    }

    private static Angle HeadingDegFromWorld(ref Matrix4x3 world)
    {
        var f = world.Row2;
        var x = f.X;
        var z = f.Z;
        if (x == 0f && z == 0f)
        {
            return default; // degenerate/zero scale
        }

        var rad = MathF.Atan2(x, z);
        var deg = rad * (180f / MathF.PI);
        return new Angle(rad).Normalized();
    }
}