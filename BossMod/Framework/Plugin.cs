using BossMod.Autorotation;
using Dalamud.Common;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using System.IO;
using System.Reflection;

namespace BossMod;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "BossMod Reborn";

    private readonly ICommandManager CommandManager;

    private readonly RotationDatabase _rotationDB;
    private readonly WorldState _ws;
    private readonly AIHints _hints;
    private readonly BossModuleManager _bossmod;
    private readonly ZoneModuleManager _zonemod;
    private readonly AIHintsBuilder _hintsBuilder;
    private readonly MovementOverride _movementOverride;
    private readonly ActionManagerEx _amex;
    private readonly WorldStateGameSync _wsSync;
    private readonly RotationModuleManager _rotation;
    private readonly AI.AIManager _ai;
    private readonly AI.Broadcast _broadcast;
    private readonly IPCProvider _ipc;
    private readonly DTRProvider _dtr;
    private TimeSpan _prevUpdateTime;
    private DateTime _throttleJump;
    private DateTime _throttleInteract;

    // windows
    private readonly ConfigUI _configUI; // TODO: should be a proper window!
    private readonly BossModuleMainWindow _wndBossmod;
    private readonly BossModuleHintsWindow _wndBossmodHints;
    private readonly ZoneModuleWindow _wndZone;
    private readonly ReplayManagementWindow _wndReplay;
    private readonly UIRotationWindow _wndRotation;
    private readonly MainDebugWindow _wndDebug;
    private readonly RotationSolverRebornModule _rsr;

    public unsafe Plugin(IDalamudPluginInterface dalamud, ICommandManager commandManager, ISigScanner sigScanner, IDataManager dataManager)
    {
        ECommonsMain.Init(dalamud, this, ECommons.Module.DalamudReflector, ECommons.Module.ObjectFunctions);
        if (!dalamud.ConfigDirectory.Exists)
            dalamud.ConfigDirectory.Create();
        var dalamudRoot = dalamud.GetType().Assembly.
                GetType("Dalamud.Service`1", true)!.MakeGenericType(dalamud.GetType().Assembly.GetType("Dalamud.Dalamud", true)!).
                GetMethod("Get")!.Invoke(null, BindingFlags.Default, null, [], null);
        var dalamudStartInfo = dalamudRoot?.GetType().GetProperty("StartInfo", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dalamudRoot) as DalamudStartInfo;
        var gameVersion = dalamudStartInfo?.GameVersion?.ToString() ?? "unknown";
        InteropGenerator.Runtime.Resolver.GetInstance.Setup(sigScanner.SearchBase, gameVersion, new(dalamud.ConfigDirectory.FullName + "/cs.json"));
        FFXIVClientStructs.Interop.Generated.Addresses.Register();
        InteropGenerator.Runtime.Resolver.GetInstance.Resolve();

        dalamud.Create<Service>();
        Service.LogHandlerDebug = msg => Service.Logger.Debug(msg);
        Service.LogHandlerVerbose = msg => Service.Logger.Verbose(msg);
        Service.LuminaGameData = dataManager.GameData;
        Service.WindowSystem = new("bmr");
        //Service.Device = pluginInterface.UiBuilder.Device;
        Service.Condition.ConditionChange += OnConditionChanged;
        MultiboxUnlock.Exec();
        Camera.Instance = new();

        Service.Config.Initialize();
        Service.Config.LoadFromFile(dalamud.ConfigFile);
        Service.Config.Modified.Subscribe(() => Task.Run(() => Service.Config.SaveToFile(dalamud.ConfigFile)));

        CommandManager = commandManager;
        CommandManager.AddHandler("/bmr", new CommandInfo(OnCommand) { HelpMessage = "Show boss mod settings UI" });

        ActionDefinitions.Instance.UnlockCheck = QuestUnlocked; // ensure action definitions are initialized and set unlock check functor (we don't really store the quest progress in clientstate, for now at least)

        var qpf = (ulong)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->PerformanceCounterFrequency;
        _rotationDB = new(new(dalamud.ConfigDirectory.FullName + "/autorot"), new(dalamud.AssemblyLocation.DirectoryName! + "/DefaultRotationPresets.json"));
        _ws = new(qpf, gameVersion);
        _rsr = new(dalamud);
        _hints = new();
        _bossmod = new(_ws);
        _zonemod = new(_ws);
        _hintsBuilder = new(_ws, _bossmod, _zonemod, _rsr);
        _movementOverride = new(dalamud);
        _amex = new(_ws, _hints, _movementOverride);
        _wsSync = new(_ws, _amex);
        _rotation = new(_rotationDB, _bossmod, _hints);
        _ai = new(_rotation, _amex, _movementOverride);
        _broadcast = new();
        _ipc = new(_rotation, _amex, _movementOverride, _ai);
        _dtr = new(_rotation, _ai);
        _wndBossmod = new(_bossmod, _zonemod);
        _wndBossmodHints = new(_bossmod, _zonemod);
        _wndZone = new(_zonemod);
        var config = Service.Config.Get<ReplayManagementConfig>();
        var replayDir = string.IsNullOrEmpty(config.ReplayFolder) ? dalamud.ConfigDirectory.FullName + "/replays" : config.ReplayFolder;
        _wndReplay = new ReplayManagementWindow(_ws, _bossmod, _rotationDB, new DirectoryInfo(replayDir));
        _configUI = new(Service.Config, _ws, new DirectoryInfo(replayDir), _rotationDB);
        config.Modified.ExecuteAndSubscribe(() => _wndReplay.UpdateLogDirectory());
        _wndRotation = new(_rotation, _amex, () => OpenConfigUI("Autorotation presets"));
        _wndDebug = new(_ws, _rotation, _zonemod, _amex, _movementOverride, _hintsBuilder, dalamud);

        dalamud.UiBuilder.DisableAutomaticUiHide = true;
        dalamud.UiBuilder.Draw += DrawUI;
        dalamud.UiBuilder.OpenMainUi += () => OpenConfigUI();
        dalamud.UiBuilder.OpenConfigUi += () => OpenConfigUI();
    }

    public void Dispose()
    {
        Service.Condition.ConditionChange -= OnConditionChanged;
        _wndDebug.Dispose();
        _wndRotation.Dispose();
        _wndReplay.Dispose();
        _wndZone.Dispose();
        _wndBossmodHints.Dispose();
        _wndBossmod.Dispose();
        _configUI.Dispose();
        _dtr.Dispose();
        _ipc.Dispose();
        _ai.Dispose();
        _rotation.Dispose();
        _wsSync.Dispose();
        _amex.Dispose();
        _movementOverride.Dispose();
        _hintsBuilder.Dispose();
        _zonemod.Dispose();
        _bossmod.Dispose();
        ActionDefinitions.Instance.Dispose();
        CommandManager.RemoveHandler("/bmr");
        GarbageCollection();
        ECommonsMain.Dispose();
    }

    private void OnCommand(string cmd, string args)
    {
        Service.Log($"OnCommand: {cmd} {args}");
        var split = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0)
        {
            OpenConfigUI();
            return;
        }

        switch (split[0].ToUpperInvariant())
        {
            case "D":
                _wndDebug.IsOpen = true;
                _wndDebug.BringToFront();
                break;
            case "CFG":
                var output = Service.Config.ConsoleCommand(new ArraySegment<string>(split, 1, split.Length - 1));
                foreach (var msg in output)
                    Service.ChatGui.Print(msg);
                break;
            case "GC":
                GarbageCollection();
                break;
            case "R":
                HandleReplayCommand(split);
                break;
            case "AR":
                ParseAutorotationCommands(split);
                break;
            case "RESETCOLORS":
                ResetColors();
                break;
            case "RESTOREROTATION":
                ToggleRestoreRotation();
                break;
            case "TOGGLEANTICHEAT":
                ToggleAnticheat();
                break;
        }
    }

    private bool HandleReplayCommand(string[] messageData)
    {
        if (messageData.Length == 1)
            _wndReplay.SetVisible(!_wndReplay.IsOpen);
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _wndReplay.StartRecording("");
                    break;
                case "OFF":
                    _wndReplay.StopRecording();
                    break;
                default:
                    Service.ChatGui.Print($"[BMR] Unknown replay command: {messageData[1]}");
                    break;
            }
        }
        return false;
    }

    private static void ResetColors()
    {
        var defaultConfig = ColorConfig.DefaultConfig;
        var currentConfig = Service.Config.Get<ColorConfig>();
        var fields = typeof(ColorConfig).GetFields(BindingFlags.Public | BindingFlags.Instance);

        for (var i = 0; i < fields.Length; ++i)
        {
            ref var field = ref fields[i];
            var value = field.GetValue(defaultConfig);
            if (value is Color or Color[])
                field.SetValue(currentConfig, value);
        }

        currentConfig.Modified.Fire();
        Service.Log("Colors have been reset to default values.");
    }

    private static bool ToggleAnticheat()
    {
        var config = Service.Config.Get<ActionTweaksConfig>();
        config.ActivateAnticheat = !config.ActivateAnticheat;
        config.Modified.Fire();
        Service.Log($"The animation lock anticheat is now {(config.ActivateAnticheat ? "enabled" : "disabled")}");
        return true;
    }

    private static bool ToggleRestoreRotation()
    {
        var config = Service.Config.Get<ActionTweaksConfig>();
        config.RestoreRotation = !config.RestoreRotation;
        config.Modified.Fire();
        Service.Log($"Restore character orientation after action use is now {(config.RestoreRotation ? "enabled" : "disabled")}");
        return true;
    }

    private void OpenConfigUI(string showTab = "")
    {
        _configUI.ShowTab(showTab);
        _ = new UISimpleWindow("BossModReborn", _configUI.Draw, true, new(300, 300));
    }

    private void DrawUI()
    {
        var tsStart = DateTime.Now;
        var moveImminent = _movementOverride.IsMoveRequested() && (!ActionManagerEx.Config.PreventMovingWhileCasting || _movementOverride.IsForceUnblocked());

        _dtr.Update();
        Camera.Instance?.Update();
        _wsSync.Update(_prevUpdateTime);
        _bossmod.Update();
        _zonemod.ActiveModule?.Update();
        _hintsBuilder.Update(_hints, PartyState.PlayerSlot, moveImminent);
        _amex.QueueManualActions();
        _rotation.Update(_amex.AnimationLockDelayEstimate, _movementOverride.IsMoving(), Service.Condition[ConditionFlag.DutyRecorderPlayback]);
        _ai.Update();
        _broadcast.Update();
        _amex.FinishActionGather();

        var uiHidden = Service.GameGui.GameUiHidden || Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.WatchingCutscene];
        if (!uiHidden)
        {
            Service.WindowSystem?.Draw();
        }

        ExecuteHints();

        Camera.Instance?.DrawWorldPrimitives();
        _prevUpdateTime = DateTime.Now - tsStart;
    }

    private unsafe bool QuestUnlocked(uint link)
    {
        // see ActionManager.IsActionUnlocked
        var gameMain = FFXIVClientStructs.FFXIV.Client.Game.GameMain.Instance();
        return link == 0
            || Service.LuminaRow<Lumina.Excel.Sheets.TerritoryType>(gameMain->CurrentTerritoryTypeId)?.TerritoryIntendedUse.RowId == 31 // deep dungeons check is hardcoded in game
            || FFXIVClientStructs.FFXIV.Client.Game.UI.UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(link);
    }

    private unsafe void ExecuteHints()
    {
        _movementOverride.DesiredDirection = _hints.ForcedMovement;
        _movementOverride.MisdirectionThreshold = _hints.MisdirectionThreshold;
        _movementOverride.DesiredSpinDirection = _hints.SpinDirection;
        // update forced target, if needed (TODO: move outside maybe?)
        if (_hints.ForcedTarget != null && _hints.ForcedTarget.IsTargetable)
        {
            var obj = _hints.ForcedTarget.SpawnIndex >= 0 ? FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager.Instance()->Objects.IndexSorted[_hints.ForcedTarget.SpawnIndex].Value : null;
            if (obj != null && obj->EntityId != _hints.ForcedTarget.InstanceID)
                Service.Log($"[ExecHints] Unexpected new target: expected {_hints.ForcedTarget.InstanceID:X} at #{_hints.ForcedTarget.SpawnIndex}, but found {obj->EntityId:X}");
            FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance()->Target = obj;
        }
        foreach (var s in _hints.StatusesToCancel)
        {
            var res = FFXIVClientStructs.FFXIV.Client.Game.StatusManager.ExecuteStatusOff(s.statusId, s.sourceId != 0 ? (uint)s.sourceId : 0xE0000000);
            Service.Log($"[ExecHints] Canceling status {s.statusId} from {s.sourceId:X} -> {res}");
        }
        if (_hints.WantJump && _ws.CurrentTime > _throttleJump)
        {
            //Service.Log($"[ExecHints] Jumping...");
            FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->UseAction(FFXIVClientStructs.FFXIV.Client.Game.ActionType.GeneralAction, 2);
            _throttleJump = _ws.FutureTime(0.1d);
        }

        if ((AI.AIManager.Instance?.Beh != null || Autorotation.MiscAI.NormalMovement.Instance != null) && CheckInteractRange(_ws.Party.Player(), _hints.InteractWithTarget))
        {
            // many eventobj interactions "immediately" start some cast animation (delayed by server roundtrip), and if we keep trying to move toward the target after sending the interact request, it will be canceled and force us to start over
            _movementOverride.DesiredDirection = default;

            if (_amex.EffectiveAnimationLock == 0 && _ws.CurrentTime >= _throttleInteract)
            {
                FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance()->InteractWithObject(GetActorObject(_hints.InteractWithTarget), false);
                _throttleInteract = _ws.FutureTime(1.1d);
            }
        }
    }

    private unsafe bool CheckInteractRange(Actor? player, Actor? target)
    {
        var playerObj = GetActorObject(player);
        var targetObj = GetActorObject(target);
        if (playerObj == null || targetObj == null)
            return false;

        // treasure chests have no client-side interact range check at all; just assume they use the standard "small" range, seems to be accurate from testing
        if (targetObj->ObjectKind is FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Treasure)
            return player?.DistanceToHitbox(target) <= 2.09f;

        return EventFramework.Instance()->CheckInteractRange(playerObj, targetObj, 1, false);
    }

    private unsafe FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* GetActorObject(Actor? actor)
    {
        if (actor == null)
            return null;

        var obj = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager.Instance()->Objects.IndexSorted[actor.SpawnIndex].Value;
        if (obj == null || obj->GetGameObjectId() != actor.InstanceID)
            return null;

        return obj;
    }

    private void ParseAutorotationCommands(string[] cmd)
    {
        switch (cmd.Length > 1 ? cmd[1].ToUpperInvariant() : "")
        {
            case "CLEAR":
                Service.Log($"Console: clearing autorotation preset '{_rotation.Preset?.Name ?? "<n/a>"}'");
                _rotation.Preset = null;
                break;
            case "DISABLE":
                Service.Log($"Console: force-disabling from preset '{_rotation.Preset?.Name ?? "<n/a>"}'");
                _rotation.Preset = RotationModuleManager.ForceDisable;
                break;
            case "SET":
                if (cmd.Length <= 2)
                    Service.Log("Specify an autorotation preset name.");
                else
                    ParseAutorotationSetCommand([.. cmd.Skip(1)], false);
                break;
            case "TOGGLE":
                ParseAutorotationSetCommand(cmd.Length > 2 ? [.. cmd.Skip(1)] : [""], true);
                break;
            case "UI":
                _wndRotation.SetVisible(!_wndRotation.IsOpen);
                break;
        }
    }

    private void ParseAutorotationSetCommand(string[] presetName, bool toggle)
    {
        if (presetName.Length < 2)
        {
            Service.Log("No valid preset name provided.");
            return;
        }

        var userInput = string.Join(" ", presetName.Skip(1)).Trim();
        if (userInput == "null" || string.IsNullOrWhiteSpace(userInput))
        {
            _rotation.Preset = null;
            Service.Log("Disabled AI autorotation preset.");
            return;
        }
        var normalizedInput = userInput.ToUpperInvariant();
        var preset = _rotation.Database.Presets.VisiblePresets
            .FirstOrDefault(p => p.Name.Trim().Equals(normalizedInput, StringComparison.OrdinalIgnoreCase))
            ?? RotationModuleManager.ForceDisable;
        if (preset != null)
        {
            var newPreset = toggle && _rotation.Preset == preset ? null : preset;
            Service.Log($"Console: {(toggle ? "toggle" : "set")} changes preset from '{_rotation.Preset?.Name ?? "<n/a>"}' to '{newPreset?.Name ?? "<n/a>"}'");
            _rotation.Preset = newPreset;
        }
        else
        {
            Service.ChatGui.PrintError($"Failed to find preset '{presetName}'");
        }
    }

    private static void OnConditionChanged(ConditionFlag flag, bool value)
    {
        Service.Log($"Condition change: {flag}={value}");
    }

    public static void GarbageCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
