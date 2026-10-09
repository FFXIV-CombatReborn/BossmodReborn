using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

[ConfigDisplay(Name = "Action tweaks", Order = 4)]
public sealed class ActionTweaksConfig : ConfigNode
{
    // TODO: consider exposing max-delay to config; 0 would mean 'remove all delay', max-value would mean 'disable'
    [PropertyDisplay("Remove extra lag-induced animation lock delay from instant casts (read tooltip!)", tooltip: "Do NOT use with XivAlexander or NoClippy - this should automatically disable itself if they are detected, but double check first!")]
    public bool RemoveAnimationLockDelay = false;

    [PropertyDisplay("Animation lock max. simulated delay (read tooltip!)", tooltip: "Configures the maximum simulated delay in milliseconds when using animation lock removal - this is required and cannot be reduced to zero. Setting this to 20ms will enable triple-weaving when using autorotation. The minimum setting to remove triple-weaving is 26ms. The minimum of 20ms has been accepted by FFLogs and should not cause issues with your logs.")]
    [PropertySlider(20, 50, Speed = 0.1f)]
    public int AnimationLockDelayMax = 20;

    [PropertyDisplay("Remove extra framerate-induced cooldown delay", tooltip: "Dynamically adjusts cooldown and animation locks to ensure queued actions resolve immediately regardless of framerate limitations")]
    public bool RemoveCooldownDelay = false;

    [PropertyDisplay("Prevent movement while casting", tooltip: "Ignored while Rotation Solver Reborn's autorotation is active", tags: ["slidecast"])]
    public bool PreventMovingWhileCasting = false;

    public enum ModifierKey
    {
        [PropertyDisplay("None")]
        None,
        [PropertyDisplay("Control")]
        Ctrl,
        [PropertyDisplay("Alt")]
        Alt,
        [PropertyDisplay("Shift")]
        Shift,
        [PropertyDisplay("LMB + RMB")]
        M12
    }

    [PropertyDisplay("Hold to allow movement while casting", tooltip: "Requires the above setting checked as well", tags: ["slidecast"])]
    public ModifierKey MoveEscapeHatch = ModifierKey.None;

    [PropertyDisplay("Automatically cancel a cast when target is dead")]
    public bool CancelCastOnDeadTarget = false;

    [PropertyDisplay("Prevent movement and action execution when pyretic-like mechanics are imminent (set to 0 to disable, otherwise increase threshold depending on your ping).")]
    [PropertySlider(0, 10, Speed = 0.01f)]
    public float PyreticThreshold = 1.0f;

    [PropertyDisplay("Auto misdirection: prevent movement under misdirection if angle between normal movement and misdirected is greater than this threshold (set to 180 to disable).")]
    [PropertySlider(0, 180)]
    public float MisdirectionThreshold = 180f;

    [PropertyDisplay("Restore character orientation after action use")]
    public bool RestoreRotation = false;

    [PropertyDisplay("Use actions on mouseover target")]
    public bool PreferMouseover = false;

    public bool SmartTargeting = false;

    [PropertyDisplay("Use custom queueing for manually pressed actions", tooltip: "This setting allows better integration with autorotations and will prevent you from triple-weaving or drifting GCDs if you press a healing ability while autorotation is going on")]
    public bool UseManualQueue = false;

    [PropertyDisplay("Try to prevent dashing into AOEs", tooltip: "Prevent automatic use of targeted dashes (like WAR Onslaught) if they would move you into a dangerous area. May not work as expected in instances that do not have modules.\n\nThis option will also apply to manually pressed dashes if you have \"Use custom queueing for manually pressed actions\" enabled.")]
    public bool DashSafety = true;

    [PropertyDisplay("Apply to all dashes, not just gap closers", tooltip: "Includes backdashes (e.g. SAM Yaten), teleports (e.g. NIN Shukuchi), and fixed-length dashes (e.g. DRG Elusive Jump)")]
    public bool DashSafetyExtra = true;

    [PropertyDisplay("Automatically manage auto attacks", tooltip: "This setting prevents starting autos early during countdown, starts them automatically at pull, when switching targets and when using any actions that don't explicitly cancel autos.")]
    public bool AutoAutos = false;

    [PropertyDisplay("Automatically dismount to execute actions")]
    public bool AutoDismount = true;

    [PropertyDisplay("Allow forbidding targets", tooltip: "Some modules want to prevent attacking certain targets, for example to prevent an early enrage. This setting gets ignored while AI is on.")]
    public bool PreventForbiddenTargets = true;

    public enum GroundTargetingMode
    {
        [PropertyDisplay("Manually select position by extra click (normal game behaviour)")]
        Manual,

        [PropertyDisplay("Cast at current mouse position")]
        AtCursor,

        [PropertyDisplay("Cast at selected target's position")]
        AtTarget
    }
    [PropertyDisplay("Automatic target selection for ground-targeted abilities")]
    public GroundTargetingMode GTMode = GroundTargetingMode.Manual;

    public bool ActivateAnticheat = true;

    private static bool IsRSREnabled()
    {
        try
        {
            const string rsrName = "Rotation Solver Reborn";
            foreach (var p in Service.PluginInterface.InstalledPlugins)
            {
                if ((p.Name.Equals(rsrName, StringComparison.OrdinalIgnoreCase) || p.InternalName.Equals(rsrName, StringComparison.OrdinalIgnoreCase)) && p.IsLoaded)
                {
                    return true;
                }
            }
        }
        catch { }
        return false;
    }

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        ImGui.AlignTextToFramePadding();
        UIMisc.HelpMarker("If the usual (mouseover/primary) target is not valid for an action, select the next best target automatically (e.g. co-tank for Shirk)");
        ImGui.SameLine();
        var rsrEnabled = IsRSREnabled();
        using var color = ImRaii.PushColor(ImGuiCol.Text, 0xFF0000FFu, rsrEnabled);
        if (ImGui.Checkbox("Smart ability targeting (Do not use with RSR)", ref SmartTargeting))
        {
            Modified.Fire();
        }
    }
}
