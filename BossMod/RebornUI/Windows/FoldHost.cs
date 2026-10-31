using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using RebornMaterial;

namespace BossMod.RebornUI;

internal sealed class FoldHost : RebornFoldWindow
{
    public readonly record struct HostAction(FontAwesomeIcon Icon, string Tooltip, Action Click);

    private readonly UIWindow _inner;
    private readonly M3WindowAction[] _actions;
    private readonly Action[] _clicks;
    private readonly Func<(string, Vector4?)>? _status;
    private bool _innerShown;

    private FoldHost(UIWindow inner, string title, Vector2 size, WindowSizeConstraints constraints, Func<(string, Vector4?)>? status, string[] tooltips, HostAction[] extra)
        : base(inner.WindowName, title, size, constraints)
    {
        _inner = inner;
        _status = status;
        RespectCloseHotkey = inner.RespectCloseHotkey;

        List<M3WindowAction> actions = [];
        List<Action> clicks = [];
        foreach (var a in extra)
        {
            actions.Add(new($"##host_action{actions.Count}", a.Icon, a.Tooltip));
            clicks.Add(a.Click);
        }

        var buttons = inner.TitleBarButtons;
        for (var i = 0; i < buttons.Count; ++i)
        {
            var button = buttons[i];
            actions.Add(new($"##host_action{actions.Count}", button.Icon, i < tooltips.Length ? tooltips[i] : "Settings"));
            clicks.Add(() => button.Click?.Invoke(ImGuiMouseButton.Left));
        }
        _actions = [.. actions];
        _clicks = [.. clicks];
    }

    public static FoldHost? Wrap<T>(string title, Vector2 size, WindowSizeConstraints constraints, Func<(string, Vector4?)>? status = null, string[]? tooltips = null, params HostAction[] extra) where T : UIWindow
    {
        var system = Service.WindowSystem;
        if (system == null)
        {
            return null;
        }

        T? inner = null;
        foreach (var window in system.Windows)
        {
            if (window is T match)
            {
                inner = match;
                break;
            }
        }

        if (inner == null)
        {
            return null;
        }

        system.RemoveWindow(inner);
        try
        {
            return new FoldHost(inner, title, size, constraints, status, tooltips ?? [], extra);
        }
        catch
        {
            system.AddWindow(inner);
            throw;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        var system = Service.WindowSystem;
        if (system != null && !IsRegistered(system, _inner))
        {
            system.AddWindow(_inner);
        }
    }

    protected override M3WindowAction[] WindowActions => _actions;

    protected override int MinimumActions => _actions.Length;

    protected override ImGuiWindowFlags ExtraFlags => _inner.Flags & (ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus);

    protected override ImGuiWindowFlags BodyFlags => _inner.Flags & (ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

    protected override (string Text, Vector4? Color) Status => _status?.Invoke() ?? ("", null);

    protected override void OnWindowAction(int index) => _clicks[index]();

    public override void PreOpenCheck()
    {
        _inner.PreOpenCheck();
        IsOpen = _inner.IsOpen;
    }

    public override void Update() => _inner.Update();

    public override bool DrawConditions() => _inner.DrawConditions();

    protected override void DrawContents() => _inner.Draw();

    public override void OnOpen()
    {
        base.OnOpen();
        if (!_innerShown)
        {
            _innerShown = true;
            _inner.OnOpen();
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        HideInner();
    }

    protected override void OnCloseRequested()
    {
        _inner.IsOpen = false;
        HideInner();
        base.OnCloseRequested();
    }

    private void HideInner()
    {
        if (_innerShown)
        {
            _innerShown = false;
            _inner.OnClose();
        }
    }

    private static bool IsRegistered(WindowSystem system, Window window)
    {
        foreach (var w in system.Windows)
        {
            if (w == window)
            {
                return true;
            }
        }
        return false;
    }
}
