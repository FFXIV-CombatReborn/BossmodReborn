using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility.Raii;
using System.IO;
using System.Threading;

namespace BossMod;

public sealed class ReplayManager(RotationDatabase rotationDB, string logDirectory) : IDisposable
{
    private sealed class ReplayEntry : IDisposable
    {
        public string Path;
        public float Progress;
        public CancellationTokenSource Cancel = new();
        public Task<Replay> Replay;
        public ReplayVisualization.ReplayDetailsWindow? Window;
        public bool AutoShowWindow;
        public bool Selected;
        public bool Disposed;
        public bool Disposing;
        public DateTime? InitialTime;

        public ReplayEntry(string path, bool autoShow, SemaphoreSlim parseThrottle, DateTime? initialTime = null)
        {
            Path = path;
            AutoShowWindow = autoShow;
            InitialTime = initialTime;

            var tcs = new TaskCompletionSource<Replay>(TaskCreationOptions.RunContinuationsAsynchronously);
            Replay = tcs.Task;
            _ = parseThrottle.WaitAsync(Cancel.Token).ContinueWith(t =>
            {
                if (t.IsCanceled || Cancel.IsCancellationRequested)
                {
                    tcs.TrySetResult(new());
                    return;
                }

                var result = new Replay();
                try
                {
                    result = ReplayParserLog.Parse(path, ref Progress, Cancel.Token);
                }
                catch (Exception ex)
                {
                    Service.Log($"[ReplayManager] Parse failed for {path}: {ex}");
                }
                finally
                {
                    try
                    {
                        parseThrottle.Release();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    tcs.TrySetResult(result);
                }
            }, TaskScheduler.Default);
        }

        public void Dispose()
        {
            if (Disposing)
            {
                return;
            }
            Disposing = true;
            Window?.Dispose();
            Cancel.Cancel();
            Disposed = true;
            _ = Replay.ContinueWith(static (t, s) =>
            {
                ((CancellationTokenSource)s!).Dispose();
                t.Dispose();
            }, Cancel, TaskScheduler.Default);
        }

        public void Show(RotationDatabase rotationDB)
        {
            Window ??= new(Replay.Result, rotationDB, InitialTime);
            Window.IsOpen = true;
            Window.BringToFront();
        }
    }

    private sealed record class AnalysisEntry(string Identifier, List<ReplayEntry> Replays) : IDisposable
    {
        public ReplayAnalysis.AnalysisManager? Analysis;
        public UISimpleWindow? Window;
        public bool Disposed;

        public void Dispose()
        {
            Window?.Dispose();
            Analysis?.Dispose();
            Disposed = true;
        }

        public void Show()
        {
            if (Analysis == null)
            {
                var loaded = new List<Replay>(Replays.Count);
                for (var i = 0; i < Replays.Count; ++i)
                {
                    var r = Replays[i];
                    if (r.Replay.IsCompletedSuccessfully && r.Replay.Result.Ops.Count > 0)
                    {
                        loaded.Add(r.Replay.Result);
                    }
                }
                Analysis = new(loaded);
            }
            Window ??= new($"Multiple logs: {Identifier}", Analysis.Draw, false, new(1200f, 800f));
            Window.IsOpen = true;
        }
    }

    private readonly List<ReplayEntry> _replayEntries = [];
    private readonly List<AnalysisEntry> _analysisEntries = [];
    private readonly SemaphoreSlim _parseThrottle = new(Math.Max(1, Environment.ProcessorCount / 2));
    private int _nextAnalysisId;
    private int _activeConversions;
    private string _path = "";
    private bool _convertAnonymize;
    private FileDialog? _fileDialog;
    private string _logDirectory = logDirectory;
    private readonly RotationDatabase _rotationDB = rotationDB;

    public void SetLogDirectory(string logDirectory) => _logDirectory = logDirectory;

    public void Dispose()
    {
        for (var i = 0; i < _analysisEntries.Count; ++i)
        {
            _analysisEntries[i].Dispose();
        }

        var pending = new List<Task>(_replayEntries.Count);
        for (var i = 0; i < _replayEntries.Count; ++i)
        {
            var e = _replayEntries[i];
            pending.Add(e.Replay);
            e.Dispose();
        }
        var allCompleted = true;
        if (pending.Count > 0)
        {
            try
            {
                allCompleted = Task.WhenAll(pending).Wait(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                allCompleted = false;
                Service.Log($"[ReplayManager] Error waiting for replay parse shutdown: {ex}");
            }
        }
        if (allCompleted)
        {
            _parseThrottle.Dispose();
        }
    }

    public void Update()
    {
        // remove disposed entries
        _replayEntries.RemoveAll(static e => e.Disposed);
        _analysisEntries.RemoveAll(static e => e.Disposed);

        // auto-show replay windows that are now ready
        for (var i = 0; i < _replayEntries.Count; ++i)
        {
            var e = _replayEntries[i];
            if (e.AutoShowWindow && e.Window == null && e.Replay.IsCompletedSuccessfully && e.Replay.Result.Ops.Count > 0)
            {
                e.Show(_rotationDB);
            }
        }
        // auto-show analysis windows that are now ready, auto dispose entries that had their windows closed
        for (var i = 0; i < _analysisEntries.Count; ++i)
        {
            var e = _analysisEntries[i];
            if (e.Analysis == null && AllReplaysCompleted(e.Replays))
            {
                e.Show();
            }
            if (e.Window != null && !e.Window.IsOpen)
            {
                e.Dispose();
            }
        }
    }

    public void Draw()
    {
        DrawNewEntry();
        if (_activeConversions > 0)
        {
            ImGui.TextUnformatted($"Converting {_activeConversions} replay(s) in background...");
        }
        DrawEntries();
        DrawEntriesOperations();

        if (_fileDialog?.Draw() ?? false)
        {
            if (_fileDialog.GetIsOk())
            {
                var results = _fileDialog.GetResults();
                _path = results.Count > 0 ? results[0] : "";
                _logDirectory = _fileDialog.GetCurrentPath();
            }
            _fileDialog.Hide();
            _fileDialog = null;
        }
    }

    private void DrawEntries()
    {
        using var table = ImRaii.Table("entries", 3);

        if (!table)
        {
            return;
        }

        var dispose = false;
        ImGui.TableSetupColumn("op", ImGuiTableColumnFlags.WidthFixed, 100f);
        ImGui.TableSetupColumn("unload", ImGuiTableColumnFlags.WidthFixed, 70f);

        for (var i = 0; i < _replayEntries.Count; ++i)
        {
            var e = _replayEntries[i];
            using var idScope = ImRaii.PushId(e.Path);

            ImGui.TableNextColumn();
            if (!e.Replay.IsCompleted)
            {
                ImGui.ProgressBar(e.Progress, new Vector2(100f, default));
            }
            else if (e.Replay.IsFaulted || e.Replay.Result.Ops.Count == 0)
            {
                ImGui.TextUnformatted("(failed)");
            }
            else
            {
                if (ImGui.Button("Actions...", new(100f, default)))
                {
                    ImGui.OpenPopup("ctx");
                }

                using var popup = ImRaii.Popup("ctx");
                if (popup)
                {
                    if (ImGui.MenuItem("Show"))
                    {
                        e.Show(_rotationDB);
                    }
                    if (ImGui.MenuItem("Convert to verbose"))
                    {
                        ConvertLog(e.Replay.Result, ReplayLogFormat.TextVerbose, _convertAnonymize);
                    }

                    if (ImGui.MenuItem("Convert to short text"))
                    {
                        ConvertLog(e.Replay.Result, ReplayLogFormat.TextCondensed, _convertAnonymize);
                    }

                    if (ImGui.MenuItem("Convert to uncompressed binary"))
                    {
                        ConvertLog(e.Replay.Result, ReplayLogFormat.BinaryUncompressed, _convertAnonymize);
                    }

                    if (ImGui.MenuItem("Convert to compressed binary"))
                    {
                        ConvertLog(e.Replay.Result, ReplayLogFormat.BinaryCompressed, _convertAnonymize);
                    }
                    ImGui.Separator();
                    ImGuiP.PushItemFlag(ImGuiItemFlags.SelectableDontClosePopup, true);
                    if (ImGui.MenuItem("Anonymize replay during conversion", _convertAnonymize))
                    {
                        _convertAnonymize = !_convertAnonymize;
                    }
                    ImGuiP.PopItemFlag();
                }
            }

            ImGui.TableNextColumn();
            if (ImGui.Button(e.Replay.IsCompleted ? "Unload" : "Cancel"))
            {
                e.Dispose();
                for (var ai = 0; ai < _analysisEntries.Count; ++ai)
                {
                    var a = _analysisEntries[ai];
                    if (!a.Disposed && a.Replays.Contains(e))
                    {
                        a.Dispose();
                    }
                }
                dispose = true;
            }

            ImGui.TableNextColumn();
            ImGui.Checkbox($"{e.Path}", ref e.Selected);
        }
        if (dispose) // replays somehow don't get cleaned up correctly without this?
        {
            Plugin.GarbageCollection();
        }
    }

    private void DrawEntriesOperations()
    {
        if (_replayEntries.Count == 0)
        {
            return;
        }

        var dispose = false;
        var numSelected = 0;
        for (var i = 0; i < _replayEntries.Count; ++i)
        {
            if (_replayEntries[i].Selected)
            {
                ++numSelected;
            }
        }
        var shouldSelectAll = _replayEntries.Count == 0 || numSelected < _replayEntries.Count;
        if (ImGui.Button(shouldSelectAll ? "Select all" : "Unselect all"))
        {
            for (var i = 0; i < _replayEntries.Count; ++i)
            {
                _replayEntries[i].Selected = shouldSelectAll;
            }
        }
        using (ImRaii.Disabled(numSelected == 0))
        {
            ImGui.SameLine();
            if (ImGui.Button("Analyze selected"))
            {
                var selected = new List<ReplayEntry>(numSelected);
                for (var i = 0; i < _replayEntries.Count; ++i)
                {
                    if (_replayEntries[i].Selected)
                    {
                        selected.Add(_replayEntries[i]);
                    }
                }
                _analysisEntries.Add(new((++_nextAnalysisId).ToString(), selected));
            }
            ImGui.SameLine();
            if (ImGui.Button("Unload selected"))
            {
                for (var i = 0; i < _analysisEntries.Count; ++i)
                {
                    var a = _analysisEntries[i];
                    for (var j = 0; j < a.Replays.Count; ++j)
                    {
                        if (a.Replays[j].Selected)
                        {
                            a.Dispose();
                            break;
                        }
                    }
                }

                for (var i = 0; i < _replayEntries.Count; ++i)
                {
                    if (_replayEntries[i].Selected)
                    {
                        _replayEntries[i].Dispose();
                        dispose = true;
                    }
                }
            }
        }
        ImGui.SameLine();
        if (ImGui.Button("Unload all"))
        {
            for (var i = 0; i < _replayEntries.Count; ++i)
            {
                _replayEntries[i].Dispose();
            }

            for (var i = 0; i < _analysisEntries.Count; ++i)
            {
                _analysisEntries[i].Dispose();
            }

            dispose = true;
        }
        if (dispose) // replays somehow don't get cleaned up correctly without this?
        {
            Plugin.GarbageCollection();
        }
    }

    private void DrawNewEntry()
    {
        ImGui.InputText("###path", ref _path, 500);
        ImGui.SameLine();
        if (ImGui.Button("..."))
        {
            _fileDialog ??= new FileDialog("select_log", "Select file or directory", "Log files{.log},All files{.*}", _logDirectory, "", ".log", 1, false, ImGuiFileDialogFlags.SelectOnly);
            // work around an oversight(?) in dalamud
            // TODO: we should use FileDialogManager instead
            _fileDialog.SelectionChanged += static (e, s) => { };
            _fileDialog.Show();
        }

        var cleanPath = GetCleanPath();
        ImGui.SameLine();
        using (ImRaii.Disabled(cleanPath.Length == 0 || HasReplayEntry(cleanPath)))
        {
            if (ImGui.Button("Open"))
            {
                _path = cleanPath;
                _replayEntries.Add(new(cleanPath, true, _parseThrottle));
            }
        }
        ImGui.SameLine();
        using (ImRaii.Disabled(cleanPath.Length == 0 || HasAnalysisEntry(cleanPath)))
        {
            if (ImGui.Button("Analyze all"))
            {
                _path = cleanPath;
                var replays = LoadAll(cleanPath);
                if (replays.Count > 0)
                {
                    _analysisEntries.Add(new(cleanPath, replays));
                }
            }
        }
        ImGui.SameLine();
        using (ImRaii.Disabled(cleanPath.Length == 0))
        {
            if (ImGui.Button("Load all"))
            {
                _path = cleanPath;
                LoadAll(cleanPath);
            }
        }
    }

    private string GetCleanPath() => _path.Length >= 2 && _path.StartsWith('"') && _path.EndsWith('"') ? _path[1..^1] : _path;

    private bool HasReplayEntry(string path)
    {
        for (var i = 0; i < _replayEntries.Count; ++i)
        {
            if (_replayEntries[i].Path == path)
            {
                return true;
            }
        }
        return false;
    }

    private bool HasAnalysisEntry(string identifier)
    {
        for (var i = 0; i < _analysisEntries.Count; ++i)
        {
            if (_analysisEntries[i].Identifier == identifier)
            {
                return true;
            }
        }
        return false;
    }

    private static bool AllReplaysCompleted(List<ReplayEntry> replays)
    {
        for (var i = 0; i < replays.Count; ++i)
        {
            if (!replays[i].Replay.IsCompleted)
            {
                return false;
            }
        }
        return true;
    }

    private List<ReplayEntry> LoadAll(string path)
    {
        try
        {
            var res = new List<ReplayEntry>();
            var di = new DirectoryInfo(path);
            var pattern = "*.log";
            if (!di.Exists && (di.Parent?.Exists ?? false))
            {
                pattern = di.Name;
                di = di.Parent;
            }
            foreach (var fi in di.EnumerateFiles(pattern, new EnumerationOptions { RecurseSubdirectories = true }))
            {
                ReplayEntry? r = null;
                for (var i = 0; i < _replayEntries.Count; ++i)
                {
                    if (_replayEntries[i].Path == fi.FullName)
                    {
                        r = _replayEntries[i];
                        break;
                    }
                }
                if (r == null)
                {
                    r = new ReplayEntry(fi.FullName, false, _parseThrottle);
                    _replayEntries.Add(r);
                }
                res.Add(r);
            }
            return res;
        }
        catch (Exception e)
        {
            Service.Log($"Failed to read {path}: {e}");
            return [];
        }
    }

    private void ConvertLog(Replay r, ReplayLogFormat format, bool anonymize)
    {
        if (r.Ops.Count == 0)
        {
            return;
        }

        var outDir = new DirectoryInfo(_logDirectory);
        Interlocked.Increment(ref _activeConversions);
        _ = Task.Run(() =>
        {
            try
            {
                var player = new ReplayPlayer(r);
                player.WorldState.Frame.Timestamp = r.Ops[0].Timestamp; // so that we get correct name etc.
                using var relogger = new ReplayRecorder(player.WorldState, format, false, outDir, format.ToString(), anonymize);
                player.AdvanceTo(DateTime.MaxValue, static () => { });
            }
            catch (Exception ex)
            {
                Service.Log($"Failed to convert replay {r.Path}: {ex}");
            }
            finally
            {
                Interlocked.Decrement(ref _activeConversions);
            }
        });
    }
}
