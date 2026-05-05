using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using System.Globalization;

namespace BossMod.AI;

sealed class AIManagementWindow : UIWindow
{
    private static readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private readonly AIManager _manager;
    private readonly EventSubscriptions _subscriptions;
    private const string _title = $"AI: off{_windowID}";
    private const string _windowID = "###AI debug window";
    private static readonly string[] positionals = Enum.GetNames<Positional>();

    public AIManagementWindow(AIManager manager) : base(_windowID, false, new(100f, 100f))
    {
        WindowName = _title;
        _manager = manager;
        _subscriptions = new
        (
            _config.Modified.ExecuteAndSubscribe(() => IsOpen = _config.DrawUI)
        );
        RespectCloseHotkey = false;
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    public void SetVisible(bool vis)
    {
        if (_config.DrawUI != vis)
        {
            _config.DrawUI = vis;
            _config.Modified.Fire();
        }
    }

    public override void Draw()
    {
        var configModified = false;

        ImGui.TextUnformatted($"Navi={_manager.Controller.NaviTargetPos}");

        configModified |= ImGui.Checkbox("Forbid actions", ref _config.ForbidActions);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Forbid movement", ref _config.ForbidMovement);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Idle while mounted", ref _config.ForbidAIMovementMounted);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Follow during combat", ref _config.FollowDuringCombat);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Must be enabled for follow target.");
            ImGui.EndTooltip();
        }
        ImGui.Spacing();
        configModified |= ImGui.Checkbox("Follow during active boss module", ref _config.FollowDuringActiveBossModule);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Must be enabled for following targets during active boss modules.");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Follow out of combat", ref _config.FollowOutOfCombat);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Follow target", ref _config.FollowTarget);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Follow the target with your distance settings.\nFollow during combat and follow during active boss module need to be activated.");
            ImGui.EndTooltip();
        }
        ImGui.Spacing();
        configModified |= ImGui.Checkbox("Manual targeting", ref _config.ManualTarget);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Allows manual targeting with an active AI autorotation.");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Disable loading obstacle maps", ref _config.DisableObstacleMaps);

        ImGui.Text("Follow party slot");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(250);
        ImGui.SetNextWindowSizeConstraints(default, new Vector2(float.MaxValue, ImGui.GetTextLineHeightWithSpacing() * 50f));
        if (ImRaii.Combo("##Leader", _manager.Beh == null ? "<idle>" : _manager.WorldState.Party[_manager.MasterSlot]?.Name ?? "<unknown>"))
        {
            if (ImGui.Selectable("<idle>", _manager.Beh == null))
            {
                _manager.SwitchToIdle();
            }
            var party = _manager.WorldState.Party.WithSlot(true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                var slot = p.Item1;
                if (ImGui.Selectable(p.Item2.Name, _manager.MasterSlot == slot))
                {
                    _manager.SwitchToFollow(slot);
                    _config.FollowSlot = slot;
                    configModified = true;
                }
            }
            ImGui.EndCombo();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Select player to follow to enable AI. Usually you select yourself for this.");
            ImGui.EndTooltip();
        }
        ImGui.Separator();
        ImGui.Text("Desired positional");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var positionalIndex = (int)_config.DesiredPositional;
        if (ImGui.Combo("##DesiredPositional", ref positionalIndex, positionals, 4))
        {
            _config.DesiredPositional = (Positional)positionalIndex;
            configModified = true;
        }
        ImGui.SameLine();
        ImGui.Text("Max distance - to targets");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        var maxDistanceTargetStr = _config.MaxDistanceToTarget.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MaxDistanceToTarget", ref maxDistanceTargetStr, 64))
        {
            maxDistanceTargetStr = maxDistanceTargetStr.Replace(',', '.');
            if (float.TryParse(maxDistanceTargetStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var maxDistance))
            {
                _config.MaxDistanceToTarget = maxDistance;
                configModified = true;
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Maximum distance in yalms to keep away from targets.");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        ImGui.Text("- to slots");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var maxDistanceSlotStr = _config.MaxDistanceToSlot.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MaxDistanceToSlot", ref maxDistanceSlotStr, 64))
        {
            maxDistanceSlotStr = maxDistanceSlotStr.Replace(',', '.');
            if (float.TryParse(maxDistanceSlotStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var maxDistance))
            {
                _config.MaxDistanceToSlot = maxDistance;
                configModified = true;
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Maximum distance in yalms to keep away from followed allies.");
            ImGui.EndTooltip();
        }
        ImGui.Text("Minimum distance");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var minDistanceStr = _config.MinDistance.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MinDistance", ref minDistanceStr, 64))
        {
            minDistanceStr = minDistanceStr.Replace(',', '.');
            if (float.TryParse(minDistanceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var minDistance))
            {
                _config.MinDistance = minDistance;
                configModified = true;
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Distance in yalms to keep away from target hitbox.");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        ImGui.Text("Pref distance to forbidden zones");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var prefDistanceStr = _config.PreferredDistance.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##PrefDistance", ref prefDistanceStr, 64))
        {
            prefDistanceStr = prefDistanceStr.Replace(',', '.');
            if (float.TryParse(prefDistanceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var prefDistance))
            {
                _config.PreferredDistance = prefDistance;
                configModified = true;
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Distance in yalms to keep away from forbidden zones.");
            ImGui.EndTooltip();
        }
        ImGui.Text("Distance variance (humanize)");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var distVarStr = _config.DistanceVariance.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##DistanceVariance", ref distVarStr, 64))
        {
            distVarStr = distVarStr.Replace(',', '.');
            if (float.TryParse(distVarStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var distVar))
            {
                _config.DistanceVariance = Math.Max(0f, distVar);
                configModified = true;
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Adds a random +/- offset to minimum and preferred distances to look human.\nRecommended: 0.05. Set to 0 to disable.");
            ImGui.EndTooltip();
        }
        ImGui.Text("Movement decision delay");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var movementDelayStr = _config.MoveDelay.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MovementDelay", ref movementDelayStr, 64))
        {
            movementDelayStr = movementDelayStr.Replace(',', '.');
            if (float.TryParse(movementDelayStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var delay))
            {
                _config.MoveDelay = delay;
                configModified = true;
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Minimum time to start moving after movement decision has been made.\nAvoid setting this too high depending on the content.");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        ImGui.Text("Delay variance %");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var delayVarStr = _config.MoveDelayVariance.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MoveDelayVariance", ref delayVarStr, 64))
        {
            delayVarStr = delayVarStr.Replace(',', '.');
            if (float.TryParse(delayVarStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var delayVar))
            {
                _config.MoveDelayVariance = Math.Max(0f, delayVar);
                configModified = true;
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Randomizes the move delay by this percentage to simulate human reaction time.\nRecommended: 20. Set to 0 to disable.");
            ImGui.EndTooltip();
        }
        ImGui.Text("Autorotation AI preset");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(250f);
        ImGui.SetNextWindowSizeConstraints(default, new Vector2(float.MaxValue, ImGui.GetTextLineHeightWithSpacing() * 50f));
        var aipreset = _config.AIAutorotPresetName;
        var presets = _manager.Autorot.Database.Presets.AllPresets;

        var count = presets.Count;
        List<string> presetNames = new(count + 1);
        for (var i = 0; i < count; ++i)
        {
            presetNames.Add(presets[i].Name);
        }

        if (aipreset != null)
            presetNames.Add("Deactivate");
        var countnames = presetNames.Count;
        var selectedIndex = presetNames.IndexOf(aipreset ?? "");

        if (ImGui.Combo("##AI preset", ref selectedIndex, [.. presetNames], countnames))
        {
            if (selectedIndex == countnames - 1 && aipreset != null)
            {
                _manager.SetAIPreset(null);
                configModified = true;
                selectedIndex = -1;
            }
            else if (selectedIndex >= 0 && selectedIndex < count)
            {
                var selectedPreset = presets[selectedIndex];
                _manager.SetAIPreset(selectedPreset);
                configModified = true;
            }
        }
        if (configModified)
            _config.Modified.Fire();
    }

    public override void OnClose() => SetVisible(false);

    public void UpdateTitle()
    {
        var masterSlot = _manager?.MasterSlot ?? -1;
        var masterName = _manager?.Autorot?.WorldState?.Party[masterSlot]?.Name ?? "unknown";
        var masterSlotNumber = masterSlot != -1 ? (masterSlot + 1).ToString() : "N/A";

        WindowName = $"AI: {(_manager?.Beh != null ? "on" : "off")}, master={masterName}[{masterSlotNumber}]{_windowID}";
    }
}
