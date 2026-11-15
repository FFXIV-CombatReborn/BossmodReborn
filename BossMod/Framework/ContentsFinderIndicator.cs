using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace BossMod;

internal sealed unsafe class ContentsFinderIndicator : IDisposable
{
    private static readonly string[] AddonNames = ["ContentsFinder", "RaidFinder"];
    private const uint LabelNodeId = 100_056_001;
    private const float LabelGap = 6f;
    private const float NameMinGap = 20f;
    private const int MaxTextCacheEntries = 4096;
    private static readonly byte[] LabelText = "BMR ✓\0"u8.ToArray();
    private const sbyte NotSupported = -1;

    private sealed class RowState
    {
        public AtkTextNode* Label;
        public sbyte Maturity = NotSupported;
        public ushort Width;
        public float X = float.NaN, Y = float.NaN;
        public ushort Height;
    }

    private sealed class AddonState
    {
        public readonly Dictionary<nint, RowState> Rows = [];
    }

    private readonly BossModuleConfig _bossConfig = Service.Config.Get<BossModuleConfig>();
    private readonly ZoneModuleConfig _zoneConfig = Service.Config.Get<ZoneModuleConfig>();
    private readonly EventSubscriptions _configSubscriptions;
    private readonly Dictionary<nint, AddonState> _addons = [];
    private readonly HashSet<nint> _visitedComponents = [];
    private readonly Dictionary<ulong, sbyte> _textCache = [];
    private Dictionary<string, BossModuleInfo.Maturity>? _supportedNames;
    private bool _labelsCreated;
    private bool _disposed;

    public ContentsFinderIndicator()
    {
        _configSubscriptions = new(
            _bossConfig.Modified.Subscribe(InvalidateSupport),
            _zoneConfig.Modified.Subscribe(InvalidateSupport));
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, AddonNames, OnPostUpdate);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, AddonNames, OnPreFinalize);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        Service.AddonLifecycle.UnregisterListener(OnPostUpdate, OnPreFinalize);
        _configSubscriptions.Dispose();
        if (!Service.Framework.IsFrameworkUnloading)
        {
            foreach (var (addon, state) in _addons)
            {
                DestroyLabels((AtkUnitBase*)addon, state);
            }
        }
        _addons.Clear();
    }

    private void InvalidateSupport()
    {
        _supportedNames = null;
        _textCache.Clear();
    }

    private void OnPreFinalize(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon.Address;
        if (_addons.Remove((nint)addon, out var state))
        {
            DestroyLabels(addon, state);
        }
    }

    private void OnPostUpdate(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon.Address;
        if (addon == null || !addon->IsVisible)
        {
            return;
        }

        _addons.TryGetValue((nint)addon, out var state);
        if (!_bossConfig.ShowDutyFinderIndicator)
        {
            if (state != null)
            {
                DestroyLabels(addon, state);
                _addons.Remove((nint)addon);
            }
            return;
        }

        if (state == null)
        {
            _addons[(nint)addon] = state = new();
        }

        _supportedNames ??= BuildSupportedNames();
        _visitedComponents.Clear();
        _labelsCreated = false;
        VisitManager(state, &addon->UldManager, 0);
        if (_labelsCreated)
        {
            addon->UldManager.UpdateDrawNodeList();
        }
    }

    private void VisitManager(AddonState state, AtkUldManager* manager, int depth)
    {
        if (depth > 4)
        {
            return;
        }

        if (manager->NodeList != null)
        {
            for (var i = 0; i < manager->NodeListCount; ++i)
            {
                VisitNode(state, manager->NodeList[i], depth);
            }
        }
        VisitChain(state, manager->RootNode, depth, 0);
    }

    private void VisitChain(AddonState state, AtkResNode* first, int depth, int treeDepth)
    {
        if (treeDepth > 8)
        {
            return;
        }
        for (var node = first; node != null; node = node->PrevSiblingNode)
        {
            VisitNode(state, node, depth);
            if ((int)node->Type < 1000 && node->ChildNode != null)
            {
                VisitChain(state, node->ChildNode, depth, treeDepth + 1);
            }
        }
    }

    private void VisitNode(AddonState state, AtkResNode* node, int depth)
    {
        if (node == null || (int)node->Type < 1000)
        {
            return;
        }

        var componentNode = (AtkComponentNode*)node;
        var component = componentNode->Component;
        if (component == null || !_visitedComponents.Add((nint)componentNode))
        {
            return;
        }

        var info = (AtkUldComponentInfo*)component->UldManager.Objects;
        if (info == null)
        {
            return;
        }

        if (info->ComponentType == ComponentType.ListItemRenderer)
        {
            UpdateRow(state, componentNode);
        }
        else
        {
            VisitManager(state, &component->UldManager, depth + 1);
        }
    }

    private void UpdateRow(AddonState state, AtkComponentNode* rowNode)
    {
        var rowRes = (AtkResNode*)rowNode;
        var firstChild = rowNode->Component->UldManager.RootNode;
        state.Rows.TryGetValue((nint)rowNode, out var row);
        var label = row != null ? row.Label : null;

        if (firstChild == null || !rowRes->IsVisible())
        {
            Hide(label);
            return;
        }

        AtkTextNode* nameNode = null;
        var maturity = NotSupported;
        FindName(firstChild, (AtkResNode*)label, ref nameNode, ref maturity, 0);
        if (nameNode == null)
        {
            Hide(label);
            return;
        }

        var maxLeafWidth = rowRes->Width * 0.5f;
        var nameX = RowRelativeX((AtkResNode*)nameNode, rowNode);
        var anchor = float.MaxValue;
        AtkTextNode* styleSource = null;
        var styleSourceX = float.MaxValue;
        for (var child = firstChild; child != null; child = child->PrevSiblingNode)
        {
            AccumulateAnchor(child, rowNode, (AtkResNode*)label, nameX + NameMinGap, maxLeafWidth, ref anchor, ref styleSource, ref styleSourceX, 0);
        }

        if (anchor == float.MaxValue || styleSource == null)
        {
            Hide(label);
            return;
        }

        if (row == null)
        {
            state.Rows[(nint)rowNode] = row = new();
        }
        if (label == null)
        {
            label = row.Label = CreateLabel(rowNode, styleSource);
            if (label == null)
            {
                return;
            }
            _labelsCreated = true;
        }

        var resNode = (AtkResNode*)label;
        if (row.Maturity != maturity)
        {
            row.Maturity = maturity;
            label->TextColor = MaturityColor((BossModuleInfo.Maturity)maturity);
            resNode->DrawFlags |= 1u;
        }

        if (row.Width == 0)
        {
            ushort w = 0, h = 0;
            label->GetTextDrawSize(&w, &h);
            row.Width = w;
        }
        var width = row.Width != 0 ? row.Width : (ushort)48;

        var parent = resNode->ParentNode;
        var parentIsRow = parent == null || parent == rowRes;
        var x = anchor - LabelGap - width - (parentIsRow ? 0f : RowRelativeX(parent, rowNode));
        var y = RowRelativeY((AtkResNode*)styleSource, rowNode) - (parentIsRow ? 0f : RowRelativeY(parent, rowNode));
        var height = styleSource->AtkResNode.Height;
        if (x != row.X || y != row.Y || width != resNode->Width || height != row.Height)
        {
            row.X = x;
            row.Y = y;
            row.Height = height;
            resNode->SetWidth(width);
            resNode->SetHeight(height);
            resNode->SetPositionFloat(x, y);
        }
        if (!resNode->IsVisible())
        {
            resNode->ToggleVisibility(true);
        }
    }

    private void FindName(AtkResNode* first, AtkResNode* label, ref AtkTextNode* result, ref sbyte maturity, int depth)
    {
        if (depth > 6)
        {
            return;
        }

        for (var node = first; node != null && result == null; node = node->PrevSiblingNode)
        {
            if (node == label || !node->IsVisible())
            {
                continue;
            }

            if (node->Type == NodeType.Text)
            {
                var m = LookupText((AtkTextNode*)node);
                if (m != NotSupported)
                {
                    result = (AtkTextNode*)node;
                    maturity = m;
                }
            }
            else if ((int)node->Type >= 1000)
            {
                var component = ((AtkComponentNode*)node)->Component;
                if (component != null && component->UldManager.RootNode != null)
                {
                    FindName(component->UldManager.RootNode, label, ref result, ref maturity, depth + 1);
                }
            }
            else if (node->ChildNode != null)
            {
                FindName(node->ChildNode, label, ref result, ref maturity, depth + 1);
            }
        }
    }

    private sbyte LookupText(AtkTextNode* text)
    {
        var bytes = text->NodeText.AsSpan();
        if (bytes.IsEmpty)
        {
            return NotSupported;
        }

        var hash = Fnv1a64(bytes);
        if (_textCache.TryGetValue(hash, out var cached))
        {
            return cached;
        }

        if (_textCache.Count >= MaxTextCacheEntries)
        {
            _textCache.Clear();
        }
        var name = Normalize(new ReadOnlySeStringSpan(bytes).ExtractText());
        var result = _supportedNames!.TryGetValue(name, out var m) ? (sbyte)m : NotSupported;
        _textCache[hash] = result;
        return result;
    }

    private static ulong Fnv1a64(ReadOnlySpan<byte> bytes)
    {
        var hash = 14695981039346656037ul;
        foreach (var b in bytes)
        {
            hash = (hash ^ b) * 1099511628211ul;
        }
        return hash;
    }

    private static void AccumulateAnchor(AtkResNode* node, AtkComponentNode* rowNode, AtkResNode* label, float minX, float maxLeafWidth, ref float anchor, ref AtkTextNode* styleSource, ref float styleSourceX, int depth)
    {
        if (node == label || !node->IsVisible() || node->Type is NodeType.Collision or NodeType.NineGrid || (int)node->Type >= 1000)
        {
            return;
        }

        if (node->Type == NodeType.Res)
        {
            if (depth < 4)
            {
                for (var child = node->ChildNode; child != null; child = child->PrevSiblingNode)
                {
                    AccumulateAnchor(child, rowNode, label, minX, maxLeafWidth, ref anchor, ref styleSource, ref styleSourceX, depth + 1);
                }
            }
            return;
        }

        if (node->Width == 0 || node->Width > maxLeafWidth)
        {
            return;
        }

        var x = RowRelativeX(node, rowNode);
        if (x <= minX)
        {
            return;
        }

        var left = x;
        if (node->Type == NodeType.Text)
        {
            var text = (AtkTextNode*)node;
            if (text->NodeText.Length == 0)
            {
                return;
            }
            left = x + VisualLeftOffset(text);
            if (x < styleSourceX)
            {
                styleSource = text;
                styleSourceX = x;
            }
        }
        anchor = Math.Min(anchor, left);
    }

    private static float VisualLeftOffset(AtkTextNode* text)
    {
        var align = text->AlignmentType;
        if (align is AlignmentType.TopLeft or AlignmentType.Left or AlignmentType.BottomLeft)
        {
            return 0;
        }

        ushort width = 0, height = 0;
        text->GetTextDrawSize(&width, &height);
        var nodeWidth = text->AtkResNode.Width;
        return align is AlignmentType.TopRight or AlignmentType.Right or AlignmentType.BottomRight ? nodeWidth - width : (nodeWidth - width) * 0.5f;
    }

    private static float RowRelativeX(AtkResNode* node, AtkComponentNode* rowNode)
    {
        var x = node->X;
        for (var p = node->ParentNode; p != null && p != (AtkResNode*)rowNode; p = p->ParentNode)
        {
            x += p->X;
        }
        return x;
    }

    private static float RowRelativeY(AtkResNode* node, AtkComponentNode* rowNode)
    {
        var y = node->Y;
        for (var p = node->ParentNode; p != null && p != (AtkResNode*)rowNode; p = p->ParentNode)
        {
            y += p->Y;
        }
        return y;
    }

    private static void Hide(AtkTextNode* label)
    {
        if (label != null && ((AtkResNode*)label)->IsVisible())
        {
            ((AtkResNode*)label)->ToggleVisibility(false);
        }
    }

    private static ByteColor MaturityColor(BossModuleInfo.Maturity maturity) => maturity switch
    {
        BossModuleInfo.Maturity.Dummy => new() { R = 0x9A, G = 0x9A, B = 0x9A, A = 0xFF },
        BossModuleInfo.Maturity.WIP => new() { R = 0xF0, G = 0x70, B = 0x70, A = 0xFF },
        BossModuleInfo.Maturity.Contributed => new() { R = 0x8C, G = 0xC8, B = 0xFF, A = 0xFF },
        BossModuleInfo.Maturity.Verified => new() { R = 0x8C, G = 0xE6, B = 0x8C, A = 0xFF },
        _ => new() { R = 0xF5, G = 0xC8, B = 0x50, A = 0xFF }, // AISupport
    };

    private static string Normalize(string s)
    {
        var sb = new StringBuilder(s.Length);
        var pendingSpace = false;
        foreach (var c in s)
        {
            if (c is '­' or '​')
            {
                continue;
            }
            if (char.IsWhiteSpace(c))
            {
                pendingSpace = sb.Length > 0;
                continue;
            }
            if (pendingSpace)
            {
                sb.Append(' ');
                pendingSpace = false;
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    private AtkTextNode* CreateLabel(AtkComponentNode* rowNode, AtkTextNode* style)
    {
        var label = IMemorySpace.GetUISpace()->Create<AtkTextNode>();
        if (label == null)
        {
            return null;
        }

        var node = (AtkResNode*)label;
        node->Type = NodeType.Text;
        node->NodeId = LabelNodeId;
        node->NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Enabled;
        node->DrawFlags = 0;
        node->ScaleX = node->ScaleY = 1f;
        node->Color = new() { R = 0xFF, G = 0xFF, B = 0xFF, A = 0xFF };
        node->MultiplyRed = node->MultiplyGreen = node->MultiplyBlue = 100;
        node->AddRed = node->AddGreen = node->AddBlue = 0;

        label->AlignmentFontType = style->AlignmentFontType;
        label->FontSize = style->FontSize;
        label->TextFlags = style->TextFlags;
        label->LineSpacing = style->LineSpacing;
        label->CharSpacing = style->CharSpacing;
        label->SetAlignment(AlignmentType.Right);
        label->EdgeColor = style->EdgeColor;
        fixed (byte* p = LabelText)
        {
            label->SetText(p);
        }

        var anchorNode = (AtkResNode*)style;
        var parent = anchorNode->ParentNode;
        node->ParentNode = parent;
        node->NextSiblingNode = anchorNode;
        node->PrevSiblingNode = anchorNode->PrevSiblingNode;
        if (anchorNode->PrevSiblingNode != null)
        {
            anchorNode->PrevSiblingNode->NextSiblingNode = node;
        }
        anchorNode->PrevSiblingNode = node;
        if (parent != null && (int)parent->Type < 1000)
        {
            parent->ChildCount++;
        }

        node->DrawFlags |= 1u; // dirty
        rowNode->Component->UldManager.UpdateDrawNodeList();
        return label;
    }

    private static void DestroyLabels(AtkUnitBase* addon, AddonState state)
    {
        foreach (var (rowPtr, row) in state.Rows)
        {
            if (row.Label == null)
            {
                continue;
            }

            var rowNode = (AtkComponentNode*)rowPtr;
            var node = (AtkResNode*)row.Label;
            var parent = node->ParentNode;
            if (parent != null && parent->ChildNode == node)
            {
                parent->ChildNode = node->PrevSiblingNode;
            }
            if (node->PrevSiblingNode != null)
            {
                node->PrevSiblingNode->NextSiblingNode = node->NextSiblingNode;
            }
            if (node->NextSiblingNode != null)
            {
                node->NextSiblingNode->PrevSiblingNode = node->PrevSiblingNode;
            }
            if (parent != null && (int)parent->Type < 1000 && parent->ChildCount > 0)
            {
                parent->ChildCount--;
            }
            node->ParentNode = node->PrevSiblingNode = node->NextSiblingNode = null;
            if (rowNode->Component != null)
            {
                rowNode->Component->UldManager.UpdateDrawNodeList();
            }
            node->Destroy(true);
            row.Label = null;
        }
        state.Rows.Clear();
        if (addon != null)
        {
            addon->UldManager.UpdateDrawNodeList();
        }
    }

    private Dictionary<string, BossModuleInfo.Maturity> BuildSupportedNames()
    {
        var bossMin = _bossConfig.MinMaturity;
        var zoneMin = _zoneConfig.MinMaturity;
        var byCFC = new Dictionary<uint, BossModuleInfo.Maturity>();
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.GroupID == 0 || info.Maturity < bossMin || !_bossConfig.IsModuleEnabled(info.PrimaryActorOID)
                || info.GroupType is not (BossModuleInfo.GroupType.CFC or BossModuleInfo.GroupType.MaskedCarnivale or BossModuleInfo.GroupType.CrucibleOfTheUnbroken))
            {
                continue;
            }
            byCFC[info.GroupID] = byCFC.TryGetValue(info.GroupID, out var existing) && existing < info.Maturity ? existing : info.Maturity;
        }
        foreach (var (cfcId, maturity) in ZoneModuleRegistry.RegisteredCFCs())
        {
            if (maturity >= zoneMin)
            {
                byCFC.TryAdd(cfcId, maturity);
            }
        }

        var names = new Dictionary<string, BossModuleInfo.Maturity>(StringComparer.OrdinalIgnoreCase);
        foreach (var (id, maturity) in byCFC)
        {
            if (Service.LuminaRow<ContentFinderCondition>(id) is { } row)
            {
                var name = Normalize(row.Name.ExtractText());
                if (name.Length > 0)
                {
                    names[name] = names.TryGetValue(name, out var existing) && existing > maturity ? existing : maturity;
                }
            }
        }
        return names;
    }
}
