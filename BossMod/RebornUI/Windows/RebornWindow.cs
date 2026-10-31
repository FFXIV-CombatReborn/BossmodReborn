using Dalamud.Bindings.ImGui;
using RebornMaterial;

namespace BossMod.RebornUI;

public abstract class RebornWindow(string name, bool detached, Vector2 initialSize, ImGuiWindowFlags flags = ImGuiWindowFlags.None) : UIWindow(name, detached, initialSize, flags)
{
    private static readonly InterfaceConfig _interfaceConfig = Service.Config.Get<InterfaceConfig>();
    private M3Style.Scope _theme;

    public static M3Density SettingsDensity => _interfaceConfig.CompactSettings ? M3Density.Tight : M3Density.Comfortable;

    protected virtual M3Density Density => M3Density.Compact;

    public override void PreDraw()
    {
        _theme = M3.IsInitialized ? M3Style.Push(Density) : default;
        base.PreDraw();
    }

    public override void PostDraw()
    {
        base.PostDraw();
        _theme.Dispose();
        _theme = default;
    }
}
