using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Interface;
using Dalamud.Plugin;
using RebornMaterial;
using System.IO;

namespace BossMod.RebornUI;

internal static class MaterialUI
{
    private static IDalamudPluginInterface? _dalamud;
    private static SettingsWindow? _settings;
    private static readonly List<FoldHost> _folds = [];
    private static bool _frameFailed;

    public static void Start(IDalamudPluginInterface dalamud, WorldState ws, RotationDatabase rotationDB, RotationModuleManager rotation, AIManager ai, DirectoryInfo replayDir)
    {
        try
        {
            StartCore(dalamud, ws, rotationDB, rotation, ai, replayDir);
        }
        catch (Exception ex)
        {
            Service.Logger.Error(ex, "[RebornUI] Failed to start; using the classic interface instead");
            Stop();
        }
    }

    private static void StartCore(IDalamudPluginInterface dalamud, WorldState ws, RotationDatabase rotationDB, RotationModuleManager rotation, AIManager ai, DirectoryInfo replayDir)
    {
        M3.ElementBaseline = 0.75f;
        M3.Initialize(dalamud, Service.Config.Get<InterfaceConfig>());
        _dalamud = dalamud;

        dalamud.UiBuilder.Draw += BeginFrame;

        _settings = new(Service.Config, ws, replayDir, rotationDB);

        var rotationHost = FoldHost.Wrap<UIRotationWindow>("Autorotation", new(400f, 400f), new() { MinimumSize = new(260f, 140f), MaximumSize = new(5000f, 5000f) },
            () => RotationStatus(rotation), ["Edit autorotation presets"]);
        if (rotationHost != null)
        {
            _folds.Add(rotationHost);
        }

        var aiHost = FoldHost.Wrap<AIManagementWindow>("Automovement", new(560f, 340f), new() { MinimumSize = new(300f, 160f), MaximumSize = new(5000f, 5000f) },
            () => AIStatus(ai), null, new FoldHost.HostAction(FontAwesomeIcon.Cog, "Open the automovement settings", () => OpenSettings(nameof(AIConfig))));
        if (aiHost != null)
        {
            _folds.Add(aiHost);
        }
    }

    public static bool OpenSettings(string page = "")
    {
        if (_settings == null)
        {
            return false;
        }

        _settings.Open(SettingsWindow.PageForClassicTab(page));
        return true;
    }

    public static bool OpenEncounterSettings(BossModuleRegistry.Info info, WorldState ws)
    {
        if (_settings == null)
        {
            return false;
        }

        _ = new EncounterSettingsWindow(info, ws);
        return true;
    }

    public static void Stop()
    {
        if (_dalamud == null)
        {
            return;
        }

        _dalamud.UiBuilder.Draw -= BeginFrame;
        foreach (var fold in _folds)
        {
            Try(fold.Dispose);
        }
        _folds.Clear();
        if (_settings != null)
        {
            Try(_settings.Dispose);
            _settings = null;
        }
        Try(M3.Dispose);
        _dalamud = null;
        _frameFailed = false;
    }

    private static void Try(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Service.Logger.Error(ex, "[RebornUI] Cleanup failed");
        }
    }

    private static void BeginFrame()
    {
        if (_frameFailed)
        {
            return;
        }

        try
        {
            M3.BeginFrame();
        }
        catch (Exception ex)
        {
            _frameFailed = true;
            Service.Logger.Error(ex, "[RebornUI] Per-frame update failed; it's off until the plugin reloads");
        }
    }

    private static (string, Vector4?) RotationStatus(RotationModuleManager rotation)
    {
        var preset = rotation.Preset;
        return preset == RotationModuleManager.ForceDisable ? ("Force-disabled", M3.Scheme.Error)
            : preset != null ? ($"Running {preset.Name}", M3.Severity(M3Severity.Success))
            : ("No preset active", null);
    }

    private static (string, Vector4?) AIStatus(AIManager ai)
    {
        if (ai.Beh == null)
        {
            return ("Off", null);
        }

        var master = ai.WorldState.Party[ai.MasterSlot];
        return (master != null ? $"On, following {master.Name} (slot {ai.MasterSlot + 1})" : "On", M3.Severity(M3Severity.Success));
    }
}
