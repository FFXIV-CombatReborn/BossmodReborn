using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using RebornMaterial;

namespace BossMod.RebornUI;

public abstract class RebornFoldWindow : RebornWindow
{
    private const ImGuiWindowFlags FoldFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    private const string LogoResource = "BossMod.RebornUI.Images.Logo.png";

    private static int _logoFrame = -1;
    private static IDalamudTextureWrap? _logo;

    private readonly M3WindowFold _fold = new();
    private readonly string _brandLabel;
    private readonly Vector2 _defaultSize;
    private readonly WindowSizeConstraints? _defaultConstraints;
    private int _shownActions = int.MaxValue;

    protected RebornFoldWindow(string name, string brandLabel, Vector2 initialSize, WindowSizeConstraints? constraints = null) : base(name, false, initialSize, FoldFlags)
    {
        _brandLabel = brandLabel;
        _defaultSize = initialSize;
        _defaultConstraints = constraints;
        SizeConstraints = constraints;
    }

    internal static IDalamudTextureWrap? LogoTexture
    {
        get
        {
            var frame = ImGui.GetFrameCount();
            if (frame != _logoFrame)
            {
                _logoFrame = frame;
                _logo = Service.Texture?.GetFromManifestResource(typeof(RebornFoldWindow).Assembly, LogoResource).TryGetWrap(out var logo, out _) == true ? logo : null;
            }
            return _logo;
        }
    }

    internal static void DrawLogo(ImDrawListPtr drawList, Vector2 min, float size)
    {
        var max = min + new Vector2(size);
        if (M3ActionIcon.Image(drawList, LogoTexture, min, max, M3.ShapeSmall))
        {
            return;
        }

        var s = M3.Scheme;
        drawList.AddRectFilled(min, max, M3.U32(s.PrimaryContainer), M3.ShapeSmall);
        M3Draw.IconCentered(drawList, FontAwesomeIcon.Dragon, min, max, s.OnPrimaryContainer);
    }

    protected M3WindowBrand Brand => new(LogoTexture, _brandLabel, FontAwesomeIcon.Dragon);

    protected virtual M3WindowAction[] WindowActions => [];

    protected virtual int MinimumActions => 0;

    protected virtual ImGuiWindowFlags ExtraFlags => ImGuiWindowFlags.None;

    public bool IsMinimized => _fold.IsMinimized;

    protected int ShownActions => Math.Min(_shownActions, WindowActions.Length);

    public void Restore() => _fold.Restore();

    public override void PreDraw()
    {
        base.PreDraw();
        Flags = FoldFlags | ExtraFlags;
        if (_fold.Prepare(this, ShownActions, Brand))
        {
            Position = null;
            Size = _defaultSize;
            SizeCondition = ImGuiCond.FirstUseEver;
            SizeConstraints = _defaultConstraints;
        }
    }

    public override void PostDraw()
    {
        _fold.PopStyle();
        base.PostDraw();
    }

    public override void OnClose()
    {
        _fold.Reset();
        base.OnClose();
    }

    public sealed override void Draw()
    {
        _fold.BeginDraw();

        if (_fold.Amount < 1f)
        {
            using var alpha = ImRaii.PushStyle(ImGuiStyleVar.Alpha, _fold.ContentAlpha);
            DrawBackdrop(_fold.Rounding);

            using var content = _fold.BeginContent("##fold_content");
            if (content)
            {
                DrawOpen();
            }
        }

        var actions = WindowActions;
        var first = actions.Length - ShownActions;
        var pressed = _fold.DrawBar("##fold_actions", actions.AsSpan(first), Brand, out var closed, M3.Scheme.SurfaceContainerHigh);
        if (pressed >= 0)
        {
            OnWindowAction(first + pressed);
        }

        if (closed)
        {
            OnCloseRequested();
        }
    }

    protected virtual void OnCloseRequested() => IsOpen = false;

    protected virtual void DrawBackdrop(float rounding) { }

    protected virtual void DrawOpen()
    {
        var (status, statusColor) = Status;
        DrawHeader(_brandLabel, status, statusColor);
        ImGui.Dummy(new(0f, M3.Space1));

        using var body = ImRaii.Child("##fold_body", -Vector2.One, false, BodyFlags);
        if (body)
        {
            DrawContents();
        }
    }

    protected virtual void DrawContents() { }

    protected virtual void OnWindowAction(int index) { }

    protected virtual (string Text, Vector4? Color) Status => ("", null);

    protected virtual ImGuiWindowFlags BodyFlags => ImGuiWindowFlags.None;

    protected void SetActionBar(int shownActions, float barTop)
    {
        _shownActions = shownActions;
        _fold.BarTop = barTop;
    }

    protected void DrawHeader(string title, string status, Vector4? statusColor)
    {
        var s = M3.Scheme;
        var scale = M3.Scale;
        var width = MathF.Max(1f, ImGui.GetContentRegionAvail().X);
        var logoSize = 32f * scale;
        var gap = M3.Space2;
        var lineGap = 2f * M3.PaddingScale;

        var shown = M3Widgets.WindowActionsThatFit(WindowActions.Length, Brand, width - logoSize - gap - 80f * scale, MinimumActions);
        var pill = M3Widgets.WindowActionsSize(shown, Brand, 0f);

        float titleHeight;
        using (ImRaii.PushFont(M3.TitleMedium))
        {
            titleHeight = ImGui.CalcTextSize(title).Y;
        }
        var statusHeight = status.Length > 0 ? ImGui.GetTextLineHeight() : 0f;
        var textHeight = titleHeight + (statusHeight > 0f ? lineGap + statusHeight : 0f);
        var height = MathF.Max(pill.Y, MathF.Max(logoSize, textHeight));

        var origin = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();
        var logoMin = new Vector2(origin.X, origin.Y + (height - logoSize) * 0.5f);
        DrawLogo(drawList, logoMin, logoSize);

        var textX = logoMin.X + logoSize + gap;
        var textWidth = MathF.Max(0f, origin.X + width - pill.X - gap - textX);
        var textY = origin.Y + (height - textHeight) * 0.5f;
        using (ImRaii.PushFont(M3.TitleMedium))
        {
            drawList.AddText(new(textX, textY), M3.U32(s.OnSurface), M3Navigation.Truncate(title, textWidth));
        }
        if (statusHeight > 0f)
        {
            drawList.AddText(new(textX, textY + titleHeight + lineGap), M3.U32(statusColor ?? s.OnSurfaceVariant), M3Navigation.Truncate(status, textWidth));
        }

        SetActionBar(shown, (height - pill.Y) * 0.5f);
        ImGui.Dummy(new(width, height));
    }
}
