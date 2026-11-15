using Dalamud.Bindings.ImGui;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BossMod;

[ConfigDisplay(Name = "Boss modules and radar", Order = 1)]
public sealed class BossModuleConfig : ConfigNode
{
    public bool RadarResize;

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        if (ImGui.Button("Recenter Window"))
        {
            Service.BossModWindow?.RecenterWindow();
        }
    }

    // boss module settings
    [PropertyDisplay("Minimal maturity for the module to be loaded", tooltip: "Some modules will have the \"WIP\" status and will not automatically load unless you change this")]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.Contributed;

    // Modules explicitly disabled from Supported fights. Primary actor OIDs are used as unique module IDs.
    public uint[] DisabledModuleOIDs = [];
    [JsonIgnore]
    internal HashSet<uint>? _disabledModuleOIDs;

    [PropertyDisplay("Allow modules to automatically use actions", tooltip: "Example: modules can automatically use anti-knockback abilities before a knockback happens")]
    public bool AllowAutomaticActions = true;

    [PropertyDisplay("Mark supported duties in the Duty Finder", tooltip: "Shows a \"BMR\" label next to the level requirement of duties that have module support (Regular and High-end Duty).\nColor shows the lowest maturity among the duty's loadable boss modules: red = WIP, blue = Contributed, green = Verified, gold = AI support.")]
    public bool ShowDutyFinderIndicator = true;

    [PropertyDisplay("Show testing radar and hint window", tooltip: "Useful for configuring your radar and hint windows without being inside of a boss encounter", separator: true, depends: nameof(EnableRadar))]
    public bool ShowDemo = false;

    // radar window settings
    [PropertyDisplay("Enable radar", separator: true)]
    public bool EnableRadar = true;

    [PropertyDisplay("Enable projecting radar into the 3D world")]
    public bool ProjectRadarInto3DWorld = false;

    [PropertyDisplay("Show actor triangles in the 3D world", tooltip: "Show ordinary actor triangles. Mechanic markers, including knockback destinations, remain visible when this is disabled.", depends: nameof(ProjectRadarInto3DWorld))]
    public bool ShowActorTrianglesIn3DWorld = true;

    [PropertyDisplay("Include drawing arena outline into the 3D world", tooltip: "If projecting the radar into the 3D world is enabled, the outline can also be drawn", depends: nameof(ProjectRadarInto3DWorld))]
    public bool EnableArenaOutlineIn3DWorld = true;

    [PropertyDisplay("Allow drawing text and icon billboards into the 3D world", tooltip: "If projecting the radar into the 3D world is enabled, the outline can also be drawn", depends: nameof(ProjectRadarInto3DWorld))]
    public bool EnableTextIconBillboards = true;

    [PropertyDisplay("Billboard height offset", tooltip: "How many yalms billboards should appear above ground. Includes gazes, text and icons.", depends: nameof(ProjectRadarInto3DWorld))]
    [PropertySlider(0f, 20f, Speed = 0.1f, Logarithmic = true)]
    public float BillboardHeightOffset = 5f;

    [PropertyDisplay("Text billboard font size", tooltip: "Change text size of 3D world billboards", depends: nameof(ProjectRadarInto3DWorld))]
    [PropertySlider(17f, 250f, Speed = 0.5f, Logarithmic = true)]
    public float TextBillboardFontSize = 110f;

    [PropertyDisplay("Icon billboard font size", tooltip: "Change icon size of 3D world billboards", separator: true, depends: nameof(ProjectRadarInto3DWorld))]
    [PropertySlider(17f, 250f, Speed = 0.5f, Logarithmic = true)]
    public float IconBillboardFontSize = 110f;

    [PropertyDisplay("Lock radar and hint window movement and mouse interaction")]
    public bool Lock = false;

    [PropertyDisplay("Transparent radar window background", tooltip: "Removes the black window around the radar; this will not work if you move the radar to a different monitor")]
    public bool TrishaMode = true;

    [PropertyDisplay("Add opaque background to the arena in the radar")]
    public bool OpaqueArenaBackground = true;

    [PropertyDisplay("Show outlines and shadows on various radar markings")]
    public bool ShowOutlinesAndShadows = true;

    [PropertyDisplay("Radar arena scale factor", tooltip: "Scale of the arena inside of the radar window")]
    [PropertySlider(0.1f, 10f, Speed = 0.1f, Logarithmic = true)]
    public float ArenaScale = 1f;

    [PropertyDisplay("Radar element thickness scale factor", tooltip: "Globally scales the outline thickness of radar elements")]
    [PropertySlider(0.1f, 10f, Speed = 0.1f, Logarithmic = true)]
    public float ThicknessScale = 1f;

    [PropertyDisplay("Rotate radar to match camera orientation")]
    public bool RotateArena = true;

    [PropertyDisplay("Rotate map by 180° if rotating map is off")]
    public bool FlipArena = false;

    [PropertyDisplay("Give radar extra space for rotations", tooltip: "If you are using the above setting, you can give the radar extra space on the sides before the edges are clipped in order to account for rotating your camera during an encounter or to give the cardinal directions space.")]
    [PropertySlider(1f, 2f, Speed = 0.1f, Logarithmic = true)]
    public float SlackForRotations = 1.5f;

    [PropertyDisplay("Show arena border in radar")]
    public bool ShowBorder = true;

    [PropertyDisplay("Change arena border color if player is at risk", tooltip: "Changes the white border to red when you are standing somewhere you are likely to be hit by a mechanic")]
    public bool ShowBorderRisk = true;

    [PropertyDisplay("Pulse screen edges when player is at risk", tooltip: "A glow pulses in the risky arena border color (Enemy color) while a player warning is active. Works independently of the radar and 3D projection settings.")]
    public bool ShowScreenRiskBorder = false;

    [PropertyDisplay("Screen danger pulse intensity", depends: nameof(ShowScreenRiskBorder))]
    [PropertySlider(0f, 10f, Speed = 0.1f)]
    public float ScreenRiskBorderIntensity = 2.5f;

    [PropertyDisplay("Show cardinal direction names on radar")]
    public bool ShowCardinals = false;

    [PropertyDisplay("Cardinal direction font size", depends: nameof(ShowCardinals))]
    [PropertySlider(0.1f, 100f, Speed = 1f)]
    public float CardinalsFontSize = 17f;

    [PropertyDisplay("Show waymarks on radar")]
    public bool ShowWaymarks = false;

    [PropertyDisplay("Waymark font size", depends: nameof(ShowWaymarks))]
    [PropertySlider(0.1f, 100f, Speed = 1f)]
    public float WaymarkFontSize = 22f;

    [PropertyDisplay("Show signs on radar ('attack', 'bind', 'ignore', and shape markers)")]
    public bool ShowSigns = false;

    [PropertyDisplay("Always show all alive party members")]
    public bool ShowIrrelevantPlayers = false;

    [PropertyDisplay("Show role-based colors on otherwise uncolored players in the radar")]
    public bool ColorPlayersBasedOnRole = false;

    [PropertyDisplay("Always show focus targeted party member", separator: true)]
    public bool ShowFocusTargetPlayer = false;

    [PropertyDisplay("Actor triangle scale factor")]
    [PropertySlider(0.1f, 10f, Speed = 0.1f)]
    public float ActorScale = 1f;

    // hint window settings
    [PropertyDisplay("Show pre-fight encounter hint popup", tooltip: "Shows encounter-specific notes before a pull. Individual encounters can be hidden permanently from the popup itself and re-enabled from that encounter's config window.")]
    public bool ShowPrePullHints = true;

    // Persisted separately from module-specific config so every encounter can support "Never show again". Primary actor OIDs are used as unique module IDs.
    public uint[] SuppressedPrePullHintOIDs = [];

    [JsonIgnore]
    internal HashSet<uint>? _suppressedPrePullHintOIDs;

    [PropertyDisplay("Show text hints in separate window", tooltip: "Separates the radar window from the hints window, allowing you to reposition the hints window")]
    public bool HintsInSeparateWindow = false;

    [PropertyDisplay("Make separate hints window transparent")]
    public bool HintsInSeparateWindowTransparent = false;

    [PropertyDisplay("Show mechanic sequence and timer hints")]
    public bool ShowMechanicTimers = true;

    [PropertyDisplay("Show raidwide hints")]
    public bool ShowGlobalHints = true;

    [PropertyDisplay("Show player hints and warnings", separator: true)]
    public bool ShowPlayerHints = true;

    // misc. settings
    [PropertyDisplay("Show movement hints in world", tooltip: "Not used very much, but can show you arrows in the game world to indicate where to move for certain mechanics")]
    public bool ShowWorldArrows = false;

    [PropertyDisplay("Show melee range indicator")]
    public bool ShowMeleeRangeIndicator = false;

    [PropertyDisplay("Maximum load distance", tooltip: "Maximum load distance in yalms (clamped to 100yalms for safety). If the boss is farther away than this, then a module will not be loaded or unloaded if already active.")]
    [PropertySlider(100f, 500f, Speed = 0.1f, Logarithmic = true)]
    public float MaxLoadDistance = 500f;

    public override void Deserialize(JsonElement j, JsonSerializerOptions ser)
    {
        base.Deserialize(j, ser);
        _disabledModuleOIDs = null;
        _suppressedPrePullHintOIDs = null;
    }

    public bool IsModuleEnabled(uint primaryActorOID) => !DisabledModuleOIDSet().Contains(primaryActorOID);

    public bool IncludeInSupportedFightControls(BossModuleRegistry.Info info)
        => info.Maturity != BossModuleInfo.Maturity.Dummy || MinMaturity == BossModuleInfo.Maturity.Dummy;

    public void SetModuleEnabled(uint primaryActorOID, bool enabled)
    {
        var set = DisabledModuleOIDSet();
        var disabled = set.Contains(primaryActorOID);
        if (enabled == !disabled)
        {
            return;
        }

        if (enabled)
        {
            set.Remove(primaryActorOID);
        }
        else
        {
            set.Add(primaryActorOID);
        }

        PersistDisabledModuleOIDs(set);
        Modified.Fire();
    }

    public void SetModulesEnabled(List<uint> primaryActorOIDs, bool enabled)
    {
        var set = DisabledModuleOIDSet();
        var changed = false;
        var count = primaryActorOIDs.Count;
        for (var i = 0; i < count; ++i)
        {
            changed |= enabled ? set.Remove(primaryActorOIDs[i]) : set.Add(primaryActorOIDs[i]);
        }

        if (changed)
        {
            PersistDisabledModuleOIDs(set);
            Modified.Fire();
        }
    }

    public (bool anyEnabled, bool allEnabled) ModulesEnabledState(List<uint> primaryActorOIDs)
    {
        var set = DisabledModuleOIDSet();
        var anyEnabled = false;
        var allEnabled = true;
        var count = primaryActorOIDs.Count;
        for (var i = 0; i < count; ++i)
        {
            var enabled = !set.Contains(primaryActorOIDs[i]);
            anyEnabled |= enabled;
            allEnabled &= enabled;
        }
        return (anyEnabled, count > 0 && allEnabled);
    }

    public void SetExpansionEnabled(BossModuleInfo.Expansion expansion, bool enabled)
    {
        var set = DisabledModuleOIDSet();
        var changed = false;
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.Expansion != expansion || !IncludeInSupportedFightControls(info))
            {
                continue;
            }
            changed |= enabled ? set.Remove(info.PrimaryActorOID) : set.Add(info.PrimaryActorOID);
        }

        if (changed)
        {
            PersistDisabledModuleOIDs(set);
            Modified.Fire();
        }
    }

    public void SetCategoryEnabled(BossModuleInfo.Category category, bool enabled)
    {
        var set = DisabledModuleOIDSet();
        var changed = false;
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.Category != category || !IncludeInSupportedFightControls(info))
            {
                continue;
            }
            changed |= enabled ? set.Remove(info.PrimaryActorOID) : set.Add(info.PrimaryActorOID);
        }

        if (changed)
        {
            PersistDisabledModuleOIDs(set);
            Modified.Fire();
        }
    }

    public (bool anyEnabled, bool allEnabled) ExpansionEnabledState(BossModuleInfo.Expansion expansion)
    {
        var set = DisabledModuleOIDSet();
        var anyEnabled = false;
        var allEnabled = true;
        var any = false;
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.Expansion != expansion || !IncludeInSupportedFightControls(info))
            {
                continue;
            }
            any = true;
            var enabled = !set.Contains(info.PrimaryActorOID);
            anyEnabled |= enabled;
            allEnabled &= enabled;
        }
        return (anyEnabled, any && allEnabled);
    }

    public (bool anyEnabled, bool allEnabled) CategoryEnabledState(BossModuleInfo.Category category)
    {
        var set = DisabledModuleOIDSet();
        var anyEnabled = false;
        var allEnabled = true;
        var any = false;
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.Category != category || !IncludeInSupportedFightControls(info))
            {
                continue;
            }
            any = true;
            var enabled = !set.Contains(info.PrimaryActorOID);
            anyEnabled |= enabled;
            allEnabled &= enabled;
        }
        return (anyEnabled, any && allEnabled);
    }

    private HashSet<uint> DisabledModuleOIDSet()
    {
        if (_disabledModuleOIDs != null)
        {
            return _disabledModuleOIDs;
        }

        var len = DisabledModuleOIDs.Length;
        var set = new HashSet<uint>(len);
        for (var i = 0; i < len; ++i)
        {
            set.Add(DisabledModuleOIDs[i]);
        }
        return _disabledModuleOIDs = set;
    }

    private void PersistDisabledModuleOIDs(HashSet<uint> set)
    {
        var persisted = new uint[set.Count];
        set.CopyTo(persisted);
        Array.Sort(persisted);
        DisabledModuleOIDs = persisted;
    }

    public bool ShowPrePullHintsFor(uint primaryActorOID) => !SuppressedPrePullHintOIDSet().Contains(primaryActorOID);

    public void SetShowPrePullHintsFor(uint primaryActorOID, bool show)
    {
        var set = SuppressedPrePullHintOIDSet();
        var suppressed = set.Contains(primaryActorOID);
        if (show == !suppressed)
        {
            return;
        }

        if (show)
        {
            set.Remove(primaryActorOID);
        }
        else
        {
            set.Add(primaryActorOID);
        }

        var persisted = new uint[set.Count];
        set.CopyTo(persisted);
        Array.Sort(persisted);
        SuppressedPrePullHintOIDs = persisted;
        Modified.Fire();
    }

    private HashSet<uint> SuppressedPrePullHintOIDSet()
    {
        if (_suppressedPrePullHintOIDs != null)
        {
            return _suppressedPrePullHintOIDs;
        }

        var len = SuppressedPrePullHintOIDs.Length;
        var set = new HashSet<uint>(len);
        for (var i = 0; i < len; ++i)
        {
            set.Add(SuppressedPrePullHintOIDs[i]);
        }
        return _suppressedPrePullHintOIDs = set;
    }
}
