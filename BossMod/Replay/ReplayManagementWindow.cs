using BossMod.Autorotation;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;
using System.Diagnostics;
using System.IO;

namespace BossMod;

public sealed class ReplayManagementWindow : UIWindow
{
    private readonly WorldState _ws;
    private DirectoryInfo _logDir;
    private static readonly ReplayManagementConfig _config = Service.Config.Get<ReplayManagementConfig>();
    private readonly ReplayManager _manager;
    private readonly EventSubscriptions _subscriptions;
    private readonly BossModuleManager _bmm;
    private ReplayRecorder? _recorder;
    private string _message = "";
    private bool _recordingManual; // recording was started manually, and so should not be stopped automatically
    private bool _recordingDuty; // recording was started automatically because we've entered duty
    private int _recordingActiveModules; // recording was started automatically, because we've activated N modules
    private FileDialog? _folderDialog;
    private string _lastErrorMessage = "";
    private DalamudLinkPayload? _startLinkPayload;
    private DalamudLinkPayload? _uploadLinkPayload;
    private DalamudLinkPayload? _disableAlertLinkPayload;

    private const string _windowID = "###Replay recorder";

    public ReplayManagementWindow(WorldState ws, BossModuleManager bmm, RotationDatabase rotationDB, DirectoryInfo logDir) : base(_windowID, false, new(300f, 200f))
    {
        _ws = ws;
        _logDir = logDir;
        _manager = new(rotationDB, logDir.FullName);
        _bmm = bmm;
        _subscriptions = new
        (
            _config.Modified.ExecuteAndSubscribe(() =>
            {
                IsOpen = _config.ShowUI;
                UpdateLogDirectory();
            }),
            _ws.CurrentZoneChanged.Subscribe(op => OnZoneChange(op.CFCID)),
            _bmm.ModuleActivated.Subscribe(OnModuleActivation),
            _bmm.ModuleDeactivated.Subscribe(OnModuleDeactivation)
        );
        if (!OnZoneChange(_ws.CurrentCFCID))
            UpdateTitle();

        RespectCloseHotkey = false;
    }

    protected override void Dispose(bool disposing)
    {
        _recorder?.Dispose();
        _subscriptions.Dispose();
        _manager.Dispose();
        base.Dispose(disposing);
    }

    public void SetVisible(bool vis)
    {
        if (_config.ShowUI != vis)
        {
            _config.ShowUI = vis;
            _config.Modified.Fire();
        }
    }

    public override void PreOpenCheck()
    {
        _manager.Update();
    }

    public override void Draw()
    {
        if (ImGui.Button(!IsRecording() ? "Start recording" : "Stop recording"))
        {
            if (!IsRecording())
            {
                _recordingManual = true;
                StartRecording("");
            }
            else
            {
                StopRecording();
            }
        }
        ImGui.SameLine();
        if (ImGui.Button("Select replay folder"))
        {
            _folderDialog ??= new FileDialog("select_replay_folder", "Select replay folder", "", _config.ReplayFolder, "", "", 1, false, ImGuiFileDialogFlags.SelectOnly);
            _folderDialog.Show();
        }

        if (_folderDialog?.Draw() ?? false)
        {
            if (_folderDialog.GetIsOk())
            {
                _config.ReplayFolder = _folderDialog.GetResults().FirstOrDefault() ?? "";
                _config.Modified.Fire();
            }
            _folderDialog.Hide();
            _folderDialog = null;
        }
        if (_recorder != null)
        {
            ImGui.InputText("###msg", ref _message, 1024);
            ImGui.SameLine();
            if (ImGui.Button("Add log marker") && _message.Length > 0)
            {
                _ws.Execute(new WorldState.OpUserMarker(_message));
                _message = "";
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Open replay folder") && _logDir != null)
            _lastErrorMessage = OpenDirectory(_logDir);

        if (_lastErrorMessage.Length > 0)
        {
            ImGui.SameLine();
            using var color = ImRaii.PushColor(ImGuiCol.Text, Colors.TextColor3);
            ImGui.TextUnformatted(_lastErrorMessage);
        }

        ImGui.Separator();
        _manager.Draw();
    }

    public bool IsRecording() => _recorder != null;

    public override void OnClose()
    {
        SetVisible(false);
    }

    private void UpdateTitle() => WindowName = $"Replay recording: {(_recorder != null ? "in progress..." : "idle")}{_windowID}";

    public bool ShouldAutoRecord => _config.AutoRecord && (_config.AutoARR || !Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.DutyRecorderPlayback]);

    private bool OnZoneChange(uint cfcId)
    {
        if (_config.ImportantDutyAlert && IsImportantDuty(cfcId) && !ShouldAutoRecord)
        {
            _startLinkPayload ??= Service.ChatGui.AddChatLinkHandler(default, (id, str) =>
            {
                if (id == default)
                {
                    StartRecording("");
                    Service.ChatGui.Print("[BMR] Replay recording started");
                }
            });
            _disableAlertLinkPayload ??= Service.ChatGui.AddChatLinkHandler(2u, (id, str) =>
            {
                if (id == 2u)
                {
                    _config.ImportantDutyAlert = false;
                    _config.Modified.Fire();
                    Service.ChatGui.Print("[BMR] Important duty alert disabled");
                }
            });
            var alertPayload =
                new TextPayload("[BMR] This duty does not yet have a complete module. Recording and uploading a replay will help enable module creation. ");
            var linkTextPayload = new TextPayload("[Start replay recording]");
            var disableTextPayload = new TextPayload("[Permanently disable these alerts]");

            var seString = new SeStringBuilder()
                .Add(alertPayload)
                .Add(_startLinkPayload)
                .Add(linkTextPayload)
                .Add(RawPayload.LinkTerminator)
                .AddText(" ")
                .Add(_disableAlertLinkPayload)
                .Add(disableTextPayload)
                .Add(RawPayload.LinkTerminator)
                .Build();
            Service.ChatGui.Print(seString);
        }
        if (!ShouldAutoRecord || _recordingManual)
            return false; // don't care

        var isDuty = cfcId != default;
        if (_recordingDuty == isDuty)
            return false; // don't care
        _recordingDuty = isDuty;

        if (isDuty && !IsRecording())
        {
            StartRecording("");
            return true;
        }

        if (!isDuty && _recordingActiveModules <= 0 && IsRecording())
        {
            StopRecording();
            return true;
        }

        return false;
    }

    private static readonly Dictionary<uint, bool> StaticDutyImportance = GenerateStaticDutyMap();

    private static Dictionary<uint, bool> GenerateStaticDutyMap()
    {
        var map = new Dictionary<uint, bool>();

        uint[] alwaysImportantDuties =
        [
            280u, 539u, 694u, 788u, 908u,
            1006u, 700u, 736u, 779u, 878u,
            879u, 946u, 947u, 979u, 980u,
            801u, 807u, 809u, 811u, 873u,
            877u, 881u, 884u, 937u, 939u,
            941u, 943u, 993u, 1060u, 819u,
            909u, 688u, 745u, 268u, 276u,
            586u
        ];
        for (var i = 0; i < 36; ++i)
        {
            map[alwaysImportantDuties[i]] = true;
        }

        // ignored duties
        uint[] alwaysIgnoredDuties = [0u, 650u, 127u, 130u, 195u, 756u, 180u, 701u, 473u, 721u];
        for (var i = 0; i < 10; ++i)
        {
            map[alwaysIgnoredDuties[i]] = false;
        }

        static void AddRange(Dictionary<uint, bool> dict, uint start, uint end)
        {
            for (var i = start; i <= end; ++i)
            {
                dict[i] = false;
            }
        }

        AddRange(map, 197u, 199u); // lord of verminion
        AddRange(map, 481u, 535u); // chocobo races
        AddRange(map, 552u, 579u); // lord of verminion
        AddRange(map, 599u, 608u); // hidden gorge + leap of faith
        AddRange(map, 640u, 645u); // air force one + mahjong
        AddRange(map, 705u, 713u); // leap of faith
        AddRange(map, 730u, 734u); // ocean fishing
        AddRange(map, 766u, 775u); // mahjong + ocean fishing
        AddRange(map, 835u, 843u); // crystalline conflict
        AddRange(map, 847u, 864u); // crystalline conflict
        AddRange(map, 912u, 923u); // crystalline conflict
        AddRange(map, 927u, 935u); // leap of faith
        AddRange(map, 952u, 957u); // ocean fishing
        AddRange(map, 967u, 978u); // crystalline conflict
        AddRange(map, 1012u, 1014u); // tutorial
        AddRange(map, 1046u, 1057u); // crystalline conflict

        // check modules for WIP and non existing
        foreach (var module in BossModuleRegistry.RegisteredModules.Values)
        {
            if (module.Maturity != BossModuleInfo.Maturity.WIP)
            {
                var id = module.GroupID;
                if (!map.ContainsKey(id))
                {
                    map[id] = false;
                }
            }
        }

        return map;
    }

    private bool IsImportantDuty(uint cfcId)
    {
        if (StaticDutyImportance.TryGetValue(cfcId, out var isImportant))
        {
            return isImportant;
        }
        return true;
    }

    private void OnModuleActivation(BossModule m)
    {
        if (!ShouldAutoRecord || _recordingManual)
            return; // don't care

        ++_recordingActiveModules;
        if (!IsRecording())
            StartRecording($"{m.GetType().Name}-");
    }

    private void OnModuleDeactivation(BossModule m)
    {
        if (!ShouldAutoRecord || _recordingManual || _recordingActiveModules <= 0)
            return; // don't care

        --_recordingActiveModules;
        if (_recordingActiveModules <= 0 && !_recordingDuty && IsRecording())
            StopRecording();
    }

    public void StartRecording(string prefix)
    {
        if (IsRecording())
            return; // already recording

        _logDir.Create();

        // if there are too many replays, delete oldest
        if (_config.MaxReplays > 0)
        {
            try
            {
                var replays = _logDir.GetFiles();
                replays.Sort(static (a, b) => a.LastWriteTime.CompareTo(b.LastWriteTime));
                foreach (var f in replays.Take(replays.Length - _config.MaxReplays))
                    f.Delete();
            }
            catch (Exception ex)
            {
                Service.Log($"Failed to delete old replays: {ex}");
            }
        }

        try
        {
            _recorder = new(_ws, _config.WorldLogFormat, true, _logDir, prefix + GetPrefix());
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to start recording: {ex}");
        }

        UpdateTitle();
    }

    public void StopRecording()
    {
        if (_config.ImportantDutyAlert && IsImportantDuty(_recorder?.CFCID ?? default))
        {
            var path = _recorder?.LogPath;
            _uploadLinkPayload ??= Service.ChatGui.AddChatLinkHandler(1u, (id, str) =>
            {
                if (id == 1u)
                {
                    Task.Run(() =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "https://forms.gle/z6czgekaEnFBtgbB6",
                            UseShellExecute = true
                        });
                    });
                    Service.ChatGui.Print($"[BMR] The path to your replay is: {path}");
                }
            });
            var alertPayload =
                new TextPayload(
                    "[BMR] You recorded a duty without a complete module. Uploading this replay helps with module development. ");
            var linkTextPayload = new TextPayload("[Upload the replay]");
            var seString = new SeStringBuilder().Add(alertPayload).Add(_uploadLinkPayload).Add(linkTextPayload).Add(RawPayload.LinkTerminator).Build();
            Service.ChatGui.Print(seString);
        }
        _recordingManual = false;
        _recordingDuty = false;
        _recordingActiveModules = 0;
        _recorder?.Dispose();
        _recorder = null;
        UpdateTitle();
    }

    public void UpdateLogDirectory()
    {
        var newLogDir = string.IsNullOrEmpty(_config.ReplayFolder) ? _logDir : new DirectoryInfo(_config.ReplayFolder);
        _logDir = newLogDir;
        _manager.SetLogDirectory(_logDir.FullName);
    }

    private unsafe string GetPrefix()
    {
        string? prefix = null;
        if (_ws.CurrentCFCID != default)
            prefix = Service.LuminaRow<ContentFinderCondition>(_ws.CurrentCFCID)?.Name.ToString();
        if (_ws.CurrentZone != default)
            prefix ??= Service.LuminaRow<TerritoryType>(_ws.CurrentZone)?.PlaceName.ValueNullable?.NameNoArticle.ToString();
        prefix ??= "World";
        prefix = Utils.StringToIdentifier(prefix);

        var player = _ws.Party.Player();
        if (player != null)
        {
            prefix += "_" + player.Class + player.Level + "_";

            var nameParts = player.Name.Split(' ');
            List<string> shortenedParts = [];
            var nameLen = nameParts.Length;
            for (var i = 0; i < nameLen; ++i)
            {
                ref var part = ref nameParts[i];
                var len = Math.Min(2, part.Length);
                shortenedParts.Add(part[..len]);
            }

            prefix += string.Join("_", shortenedParts);
        }

        var cf = FFXIVClientStructs.FFXIV.Client.Game.UI.ContentsFinder.Instance();
        if (cf->IsUnrestrictedParty)
            prefix += "_U";
        if (cf->IsLevelSync)
            prefix += "_LS";
        if (cf->IsMinimalIL)
            prefix += "_MI";
        if (cf->IsSilenceEcho)
            prefix += "_NE";

        return prefix;
    }

    private string OpenDirectory(DirectoryInfo dir)
    {
        if (!dir.Exists)
            return $"Directory '{dir}' not found.";

        try
        {
            Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening directory {dir}: {e}");
            return $"Failed to open folder '{dir}', open it manually.";
        }
    }
}
