using Dalamud.Bindings.ImGui;

namespace BossMod;

[SkipLocalsInit]
public sealed class UITree
{
    private uint _selectedId;

    public record struct NodeProperties(string Text, bool Leaf = false, uint Color = default)
    {
        public uint Colors = Color == default ? BossMod.Colors.TextColor1 : Color;
    }

    public struct NodeRaii(bool selected, bool opened, bool hovered, bool realOpened) : IDisposable
    {
        public bool Selected = selected;
        public bool Opened = opened;
        public bool Hovered = hovered;
        private bool _disposed;
        private readonly bool _realOpened = realOpened;

        public readonly bool SelectedOrHovered => Selected || Hovered;

        public void Dispose()
        {
            if (_disposed)
                return;
            if (_realOpened)
                ImGui.TreePop();
            ImGui.PopID();
            _disposed = true;
        }
    }

    // contains 0 elements (if node is closed) or single null (if node is opened)
    // expected usage is 'foreach (_ in Node(...)) { draw subnodes... }'
    public IEnumerable<object?> Node(string text, bool leaf = false, uint color = default, Action? contextMenu = null, Action? doubleClick = null, Action? select = null)
    {
        if (RawNode(text, leaf, color == default ? Colors.TextColor1 : color, contextMenu, doubleClick, select))
        {
            yield return null;
            ImGui.TreePop();
        }
        ImGui.PopID();
    }

    // draw a node for each element in collection
    public IEnumerable<T> Nodes<T>(IEnumerable<T> collection, Func<T, NodeProperties> map, Action<T>? contextMenu = null, Action<T>? doubleClick = null, Action<T>? select = null)
    {
        foreach (var t in collection)
        {
            var props = map(t);
            if (RawNode(props.Text, props.Leaf, props.Colors, contextMenu != null ? () => contextMenu(t) : null, doubleClick != null ? () => doubleClick(t) : null, select != null ? () => select(t) : null))
            {
                yield return t;
                ImGui.TreePop();
            }
            ImGui.PopID();
        }
    }

    public void LeafNode(string text, uint color = default, Action? contextMenu = null, Action? doubleClick = null, Action? select = null)
    {
        if (RawNode(text, true, color == default ? Colors.TextColor1 : color, contextMenu, doubleClick, select))
            ImGui.TreePop();
        ImGui.PopID();
    }

    // draw leaf nodes for each element in collection
    public void LeafNodes<T>(IEnumerable<T> collection, Func<T, string> map, Action<T>? contextMenu = null, Action<T>? doubleClick = null, Action<T>? select = null)
    {
        foreach (var t in collection)
        {
            if (RawNode(map(t), true, Colors.TextColor1, contextMenu != null ? () => contextMenu(t) : null, doubleClick != null ? () => doubleClick(t) : null, select != null ? () => select(t) : null))
                ImGui.TreePop();
            ImGui.PopID();
        }
    }

    public void LeafNodes<T>(ReadOnlySpan<T> collection, Func<T, string> map, Action<T>? contextMenu = null, Action<T>? doubleClick = null, Action<T>? select = null)
    {
        var col = collection;
        var len = collection.Length;
        for (var i = 0; i < len; ++i)
        {
            var t = col[i];
            if (RawNode(map(t), true, Colors.TextColor1, contextMenu != null ? () => contextMenu(t) : null, doubleClick != null ? () => doubleClick(t) : null, select != null ? () => select(t) : null))
                ImGui.TreePop();
            ImGui.PopID();
        }
    }

    // handle selection & id scopes
    private bool RawNode(string text, bool leaf, uint color, Action? contextMenu, Action? doubleClick, Action? select)
    {
        var id = ImGui.GetID(text);
        var flags = ImGuiTreeNodeFlags.None;
        if (id == _selectedId)
            flags |= ImGuiTreeNodeFlags.Selected;
        if (leaf)
            flags |= ImGuiTreeNodeFlags.Leaf;

        ImGui.PushID((int)id);
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        var open = ImGui.TreeNodeEx(text, flags);
        ImGui.PopStyleColor();
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            _selectedId = id;
            select?.Invoke();
        }
        if (doubleClick != null && ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            doubleClick();
        if (contextMenu != null && ImGui.BeginPopupContextItem($"###{text}popup"))
        {
            contextMenu();
            ImGui.EndPopup();
        }
        return open;
    }

    public NodeRaii Node2(string text, bool leaf = false, uint color = default)
    {
        var id = ImGui.GetID(text);
        var flags = ImGuiTreeNodeFlags.None;
        if (id == _selectedId)
            flags |= ImGuiTreeNodeFlags.Selected;
        if (leaf)
            flags |= ImGuiTreeNodeFlags.Leaf;

        ImGui.PushID((int)id);
        ImGui.PushStyleColor(ImGuiCol.Text, color == default ? Colors.TextColor1 : color);
        bool open = ImGui.TreeNodeEx(text, flags);
        ImGui.PopStyleColor();
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            _selectedId = id;
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.SetClipboardText(text);
        return new(id == _selectedId, open && !leaf, ImGui.IsItemHovered(), open);
    }

    // returned node is auto disposed
    public NodeRaii LeafNode2(string text, uint color = default)
    {
        var n = Node2(text, true, color == default ? Colors.TextColor1 : color);
        n.Dispose();
        return n;
    }
}
