namespace BossMod.Autorotation;

[ConfigDisplay(Name = "Autorotation (Unsupported by Combat Reborn)", Order = 5)]
public sealed class AutorotationConfig : ConfigNode
{
    [PropertyDisplay("Show in-game UI")]
    public bool ShowUI = false;

    public enum DtrStatus
    {
        [PropertyDisplay("Disabled")]
        None,
        [PropertyDisplay("Text only")]
        TextOnly,
        [PropertyDisplay("With icon")]
        Icon
    }

    [PropertyDisplay("Show active presets in server bar")]
    [PropertyRadio]
    public DtrStatus ShowDTR = DtrStatus.None;

    [PropertyDisplay("Hide VBM default presets", tooltip: "If you don't need the included VBM Default, VBM AI and VBM Multibox presets, this option will prevent them from being shown in the Autorotation, AI and Preset Editor windows.")]
    public bool HideDefaultPresets = true;

    public bool SuggestHealerAI = true;

    [PropertyDisplay("Show positional hints in world", tooltip: "Show tips for positional abilities, indicating to move to the flank or rear of your target")]
    public bool ShowPositionals = false;

    [PropertyDisplay("Follow RotationSolverReborn's desired positional", tooltip: "When enabled, the 'Misc AI: Goes to specified positional' rotation module will override its Positional track setting and instead use the positional currently requested by RotationSolverReborn over IPC (Does not apply to Target Dummies)")]
    public bool FollowRSRDesiredPositional = true;

    [PropertyDisplay("Automatically disable autorotation on death")]
    public bool ClearPresetOnDeath = true;

    [PropertyDisplay("Automatically disable autorotation when exiting combat")]
    public bool ClearPresetOnCombatEnd = false;

    [PropertyDisplay("Automatically disable autorotation if a Luring Trap is triggered", tooltip: "Only applicable in Deep Dungeons")]
    public bool ClearPresetOnLuring = false;

    [PropertyDisplay("Automatically reenable force-disabled autorotation when exiting combat")]
    public bool ClearForceDisableOnCombatEnd = true;

    [PropertyDisplay("Early pull threshold", tooltip: "If someone enters combat with a boss when the countdown is longer than this value, it's consider a ninja-pull and autorotation is force disabled")]
    [PropertySlider(0, 30, Speed = 1)]
    public float EarlyPullThreshold = 1.5f;

    [PropertyDisplay("Disable autorotation if the boss is pulled without a countdown", tooltip: "Only applies if you have a cooldown plan active.")]
    public bool PlannedPullSafety = true;
}
