using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using System.IO;

namespace BossMod;

public sealed class ConfigUI : IDisposable
{
    private class UINode(ConfigNode? node)
    {
        public ConfigNode? Node = node;
        public string Name = "";
        public int Order;
        public ModuleViewer.SupportedFightSortKey? SupportedFightOrder;
        public UINode? Parent;
        public List<UINode> Children = [];
        public string[] Tags = [];
        public List<BossModuleRegistry.Info> PrePullHintModules = [];

        public List<string> Path = [];
    }

    private readonly List<UINode> _roots = [];
    private readonly UITree _tree = new();
    private readonly UITabs _tabs = new("ConfigUI");
    private readonly AboutTab _about;
    private readonly ModuleViewer _mv;
    private readonly ConfigRoot _root;
    private readonly WorldState _ws;
    private readonly UIPresetDatabaseEditor? _presets;

    private readonly List<List<string>> _filterNodes = [];
    private static readonly Dictionary<Type, PropertyRenderer> _propertyRenderers = [];

    public ConfigUI(ConfigRoot config, WorldState ws, DirectoryInfo? replayDir, RotationDatabase? rotationDB)
    {
        _root = config;
        _ws = ws;
        _about = new(replayDir);
        _mv = new(rotationDB?.Plans, ws);
        _presets = rotationDB != null ? new(rotationDB) : null;

        _tabs.Add("Settings", DrawSettings);
        _tabs.Add("Supported fights", () => _mv.Draw(_tree, _ws));
        _tabs.Add("Autorotation presets", () => _presets?.Draw());
        _tabs.Add("Slash commands", DrawAvailableCommands);
        _tabs.Add("About", _about.Draw);

        Dictionary<Type, UINode> nodes = [];

        foreach (var n in _root._nodes.Values)
        {
            nodes[n.GetType()] = new(n);
        }

        foreach (var (t, n) in nodes)
        {
            var props = GeneratedConfigMetadata.Get(t).Display;
            n.Name = props?.Name ?? GenerateNodeName(t);
            n.Order = props?.Order ?? 0;
            n.Parent = props?.Parent != null ? nodes.GetValueOrDefault(props.Parent) : null;
            n.Tags = props?.Tags ?? [];

            var parentNodes = n.Parent?.Children ?? _roots;
            parentNodes.Add(n);
        }

        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (!info.HasPrePullHints)
            {
                continue;
            }

            if (info.ConfigType != null && nodes.TryGetValue(info.ConfigType, out var configNode))
            {
                configNode.PrePullHintModules.Add(info);
                continue;
            }

            var hintNode = new UINode(null)
            {
                Name = GenerateNodeName(info.ModuleType),
                Order = 0x100000,
                SupportedFightOrder = ModuleViewer.GetSupportedFightSortKey(info),
                Parent = nodes.GetValueOrDefault(ExpansionConfigType(info.Expansion)) ?? nodes.GetValueOrDefault(typeof(ModuleConfig))
            };
            hintNode.PrePullHintModules.Add(info);
            (hintNode.Parent?.Children ?? _roots).Add(hintNode);
        }

        SortByOrder(_roots);
        ResolvePaths(_roots, []);
    }

    private void ResolvePaths(List<UINode> nodes, List<string> parent)
    {
        var count = nodes.Count;
        for (var i = 0; i < count; ++i)
        {
            var n = nodes[i];
            n.Path = [.. parent, n.Name];
            ResolvePaths(n.Children, n.Path);
        }
    }

    public void Dispose() => _mv.Dispose();

    public void ShowTab(string name) => _tabs.Select(name);

    public void Draw() => _tabs.Draw();

    private string _searchText = "";

    private void DrawSettings()
    {
        ImGui.SetNextItemWidth(300f);
        if (ImGui.InputTextEx("##ConfigSearch", "Search for a setting...", ref _searchText))
        {
            FilterNodes();
        }

        ImGui.SameLine();
        using (ImRaii.Disabled(_searchText.Length == 0))
        {
            if (ImGui.Button("Clear"))
            {
                _searchText = "";
                FilterNodes();
            }
        }

        DrawNodes(_roots);
    }

    private static readonly (string, string)[] _availableAICommands =
    [
        ( "on", "Enables the AI." ),
        ( "off", "Disables the AI." ),
        ( "toggle", "Toggles the AI on/off." ),
        ( "targetmaster", "Toggles the focus on target leader." ),
        ( "follow slotX", "Follows the specified slot, eg. Slot1." ),
        ( "follow name", "Follows the specified party member by name." ),
        ( "ui", "Toggles the AI menu." ),
        ( "forbidactions", "Toggles the forbidding of actions. (only for autorotation)" ),
        ( "forbidactions on/off", "Sets forbid actions to on or off. (only for autorotation)" ),
        ( "forbidmovement", "Toggles the forbidding of movement." ),
        ( "forbidmovement on/off", "Sets forbid movement to on or off." ),
        ( "idlewhilemounted", "Toggles the idling while mounted." ),
        ( "idlewhilemounted on/off", "Sets idle while mounted to on or off." ),
        ( "followcombat", "Toggles following during combat." ),
        ( "followcombat on/off", "Sets following following during combat to on or off." ),
        ( "followmodule", "Toggles following during active boss module." ),
        ( "followmodule on/off", "Sets following following during active boss module to on or off." ),
        ( "followoutofcombat", "Toggles following during out of combat." ),
        ( "followoutofcombat on/off", "Sets following target out of combat to on or off." ),
        ( "followtarget", "Toggles following targets during combat." ),
        ( "followtarget on/off", "Sets following target during combat to on or off." ),
        ( "positional X", "Switch to positional when following targets. (any, rear, flank, front)" ),
        ( "maxdistancetarget X", "Sets max distance to target. (default = 2.6)" ),
        ( "maxdistanceslot X", "Sets max distance to slot. (default = 1)" ),
        ( "mindistance X", "Sets min distance to hitbox. (default = 0)" ),
        ( "prefdistance X", "Sets preferred distance to forbidden zones. (default = 0)" ),
        ( "movedelay X", "Sets AI movement decision delay. (default = 0)" ),
        ( "obstaclemaps", "Toggles loading obstacle maps." ),
        ( "obstaclemaps on/off", "Sets the loading of obstacle maps to on or off." ),
        ( "setpresetname X", "Sets an autorotation preset for the AI, eg. setpresetname vbm default." )
    ];

    private static readonly (string, string)[] _autorotationCommands =
    [
        ( "ar clear", "Clear current preset; autorotation will do nothing unless plan is active" ),
        ( "ar disable", "Force disable autorotation; no actions will be executed automatically even if plan is active." ),
        ( "ar set Preset", "Start executing specified preset." ),
        ( "ar toggle", "Force disable autorotation if not already; otherwise clear overrides." ),
        ( "ar toggle Preset", "Start executing specified preset unless it's already active; clear otherwise" ),
        ( "ar ui", "Toggle autorotation ui." ),
    ];

    private static readonly (string, string)[] _availableOtherCommands =
    [
        ( "restorerotation", "Toggle restore character orientation after action use setting." ),
        ( "resetcolors", "Resets all colors to their default values." ),
        ( "d", "Opens the debug menu." ),
        ( "r", "Opens the replay menu." ),
        ( "r on/off", "Starts/stops recording a replay." ),
        ( "gc", "Triggers the garbage collection." ),
        ( "radar", "toggles radar display" ),
        ( "radar on/off", "Sets radar display to on or off." ),
        ( "cfg", "Lists all configs." )
    ];

    private static void DrawAvailableCommands()
    {
        ImGui.Text("Available Commands:");
        ImGui.Separator();
        ImGui.Text("AI:");
        ImGui.Separator();
        for (var i = 0; i < 30; ++i)
        {
            ref var text = ref _availableAICommands[i];
            ImGui.Text($"/bmrai {text.Item1}: {text.Item2}");
        }
        ImGui.Separator();
        ImGui.Text("Autorotation commands:");
        ImGui.Separator();
        for (var i = 0; i < 6; ++i)
        {
            ref var text = ref _autorotationCommands[i];
            ImGui.Text($"/bmr {text.Item1}: {text.Item2}");
        }
        ImGui.Separator();
        ImGui.Text("Other commands:");
        ImGui.Separator();
        for (var i = 0; i < 9; ++i)
        {
            ref var text = ref _availableOtherCommands[i];
            ImGui.Text($"/bmr {text.Item1}: {text.Item2}");
        }
    }

    private void FilterNodes()
    {
        _filterNodes.Clear();

        if (_searchText.Length == 0)
        {
            return;
        }

        var count = _roots.Count;
        for (var i = 0; i < count; ++i)
        {
            var paths = WalkNodes(_roots[i]);
            var countP = paths.Count;
            for (var j = 0; j < countP; ++j)
            {
                _filterNodes.Add(paths[j]);
            }
        }
    }

    private List<List<string>> WalkNodes(UINode node)
    {
        var results = new List<List<string>>();
        WalkNodesInternal(node, [], results);
        return results;
    }

    private void WalkNodesInternal(UINode node, List<string> path, List<List<string>> results)
    {
        if (Utils.TextMatch(node.Name, _searchText) || TagsMatch(node.Tags))
        {
            var matchPath = new List<string>(path) { node.Name, "*" };
            results.Add(matchPath);
            return;
        }

        if (node.Node != null)
        {
            var fields = GeneratedConfigMetadata.Get(node.Node).DisplayFields;
            var len = fields.Length;
            for (var i = 0; i < len; ++i)
            {
                var field = fields[i];
                var props = field.Display!;
                if (Utils.TextMatch(props.Label, _searchText) || TagsMatch(props.Tags) || field.SectionStart is { Label.Length: > 0 } section && Utils.TextMatch(section.Label, _searchText))
                {
                    var matchPath = new List<string>(path) { node.Name, props.Label };
                    results.Add(matchPath);
                }
            }
        }

        var hintCount = node.PrePullHintModules.Count;
        for (var i = 0; i < hintCount; ++i)
        {
            var label = PrePullHintSettingLabel(node.PrePullHintModules[i], hintCount > 1);
            if (Utils.TextMatch(label, _searchText))
            {
                results.Add([with(path), node.Name, label]);
            }
        }

        path.Add(node.Name);
        var children = node.Children;
        var count = children.Count;
        for (var i = 0; i < count; ++i)
        {
            WalkNodesInternal(children[i], path, results);
        }
        path.RemoveAt(path.Count - 1);
    }

    private bool TagsMatch(string[] tags)
    {
        var len = tags.Length;
        for (var i = 0; i < len; ++i)
        {
            if (Utils.TextMatch(tags[i], _searchText))
            {
                return true;
            }
        }
        return false;
    }

    public static void DrawNode(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws, Func<PropertyDisplayAttribute, bool>? filter = null)
    {
        // draw standard properties
        var metadata = GeneratedConfigMetadata.Get(node);
        var fields = metadata.DisplayFields;
        var len = fields.Length;
        for (var i = 0; i < len; ++i)
        {
            var field = fields[i];
            var props = field.Display!;

            if (filter?.Invoke(props) == false)
            {
                continue;
            }

            if (field.SectionStart is { } section)
            {
                if (section.Separator)
                {
                    ImGui.Separator();
                }
                if (section.Label.Length > 0)
                {
                    ImGui.TextUnformatted(section.Label);
                }
            }

            var value = field.Getter(node);
            var enabled = IsPropertyEnabled(node, metadata, field);
            bool modified;
            using (ImRaii.Disabled(!enabled))
            {
                modified = props.Renderer is { } rendererType
                    ? GetPropertyRenderer(rendererType).Draw(props, false, node, value!, root, tree, ws)
                    : DrawProperty(props.Label, root.IsTransient(node, field) ? TransientTooltip(props.Tooltip) : props.Tooltip, node, field, value, root, tree, ws);
            }
            if (modified)
            {
                node.Modified.Fire();
            }

            if (props.Separator)
            {
                ImGui.Separator();
            }
        }

        // draw custom stuff
        node.DrawCustom(tree, ws);
    }

    private static string TransientTooltip(string tooltip)
    {
        const string note = "Temporarily changed by another plugin (e.g. AutoDuty). Your own value is kept in the config file and restored when that plugin is done; changing it here keeps your new value.";
        return tooltip.Length > 0 ? $"{tooltip}\n\n{note}" : note;
    }

    private static bool IsPropertyEnabled(ConfigNode node, ConfigTypeMetadata metadata, ConfigFieldMetadata field)
    {
        var depends = field.Display?.Depends;
        if (string.IsNullOrEmpty(depends))
        {
            return true;
        }

        return IsDependencyEnabled(node, metadata, depends, metadata.Fields.Length) ?? true;
    }

    private static bool? IsDependencyEnabled(ConfigNode node, ConfigTypeMetadata metadata, string fieldName, int remainingDepth)
    {
        // Invalid and circular dependencies fail open. Silently locking the setting would make an
        // authoring mistake unnecessarily difficult to recover from.
        if (remainingDepth <= 0 || !metadata.FieldsByName.TryGetValue(fieldName, out var dependency))
        {
            return null;
        }

        if (dependency.Display?.Depends is { Length: > 0 } parentDependency)
        {
            var parentEnabled = IsDependencyEnabled(node, metadata, parentDependency, remainingDepth - 1);
            if (parentEnabled != true)
            {
                return parentEnabled;
            }
        }

        // Dependencies are deliberately boolean for now. Nullable bool boxes as bool when it has a
        // value; null is treated as disabled. Other field types are considered an invalid dependency.
        return dependency.Getter(node) switch
        {
            bool value => value,
            null when dependency.FieldType == typeof(bool?) => false,
            _ => null
        };
    }

    private static PropertyRenderer GetPropertyRenderer(Type type)
        => _propertyRenderers.TryGetValue(type, out var renderer) ? renderer : (_propertyRenderers[type] = GeneratedFactories.CreatePropertyRenderer(type));

    internal static void DrawPrePullHintSetting(BossModuleRegistry.Info info, string label = "Show pre-fight hint popup for this encounter")
    {
        var show = BossModuleManager.Config.ShowPrePullHintsFor(info.PrimaryActorOID);
        if (ImGui.Checkbox($"{label}##PrePullHints{info.PrimaryActorOID:X8}", ref show))
        {
            BossModuleManager.Config.SetShowPrePullHintsFor(info.PrimaryActorOID, show);
        }

        if (!BossModuleManager.Config.ShowPrePullHints)
        {
            ImGui.SameLine();
            ImGui.TextDisabled("(globally disabled)");
        }
    }

    private static string PrePullHintSettingLabel(BossModuleRegistry.Info info, bool disambiguate)
        => disambiguate ? $"Show pre-fight hint popup for {GenerateNodeName(info.ModuleType)}" : "Show pre-fight hint popup for this encounter";

    private static Type ExpansionConfigType(BossModuleInfo.Expansion expansion) => expansion switch
    {
        BossModuleInfo.Expansion.RealmReborn => typeof(RealmReborn.RealmRebornConfig),
        BossModuleInfo.Expansion.Heavensward => typeof(Heavensward.HeavenswardConfig),
        BossModuleInfo.Expansion.Stormblood => typeof(Stormblood.StormbloodConfig),
        BossModuleInfo.Expansion.Shadowbringers => typeof(Shadowbringers.ShadowbringersConfig),
        BossModuleInfo.Expansion.Endwalker => typeof(Endwalker.EndwalkerConfig),
        BossModuleInfo.Expansion.Dawntrail => typeof(Dawntrail.DawntrailConfig),
        BossModuleInfo.Expansion.Global => typeof(Global.GlobalConfig),
        _ => typeof(ModuleConfig)
    };

    private static string GenerateNodeName(Type t) => t.Name.EndsWith("Config", StringComparison.Ordinal) ? t.Name[..^"Config".Length] : t.Name;

    private static void SortByOrder(List<UINode> nodes)
    {
        nodes.Sort(static (a, b) =>
        {
            var order = a.Order.CompareTo(b.Order);
            if (order != 0)
                return order;

            if (a.SupportedFightOrder.HasValue != b.SupportedFightOrder.HasValue)
                return a.SupportedFightOrder.HasValue ? 1 : -1;

            if (a.SupportedFightOrder is { } aSupported && b.SupportedFightOrder is { } bSupported)
            {
                var supportedOrder = aSupported.CompareTo(bSupported);
                if (supportedOrder != 0)
                    return supportedOrder;
            }

            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        });
        var count = nodes.Count;
        for (var i = 0; i < count; ++i)
        {
            SortByOrder(nodes[i].Children);
        }
    }

    private void DrawNodes(List<UINode> nodes)
    {
        var count = nodes.Count;
        var filteredNodes = new List<UINode>(count);
        for (var i = 0; i < count; ++i)
        {
            var n = nodes[i];
            if (MatchesFilter(n.Path))
            {
                filteredNodes.Add(n);
            }
        }

        foreach (var n in _tree.Nodes(filteredNodes, n => new(n.Name)))
        {
            var hintCount = n.PrePullHintModules.Count;
            for (var i = 0; i < hintCount; ++i)
            {
                var info = n.PrePullHintModules[i];
                var label = PrePullHintSettingLabel(info, hintCount > 1);
                if (MatchesFilter([.. n.Path, label]))
                {
                    DrawPrePullHintSetting(info, label);
                }
            }

            if (n.Node != null)
            {
                DrawNode(n.Node, _root, _tree, _ws, props => MatchesFilter([.. n.Path, props.Label]));
            }
            DrawNodes(n.Children);
        }
    }

    private bool MatchesFilter(List<string> path)
    {
        if (_filterNodes.Count == 0)
        {
            return true;
        }

        bool matchesOneFilter(List<string> filter)
        {
            var i = 0;
            var count = filter.Count;
            for (var j = 0; j < count; ++j)
            {
                var f = filter[j];
                if (f == "*" || i >= path.Count)
                {
                    return true;
                }

                if (f != path[i])
                {
                    return false;
                }

                ++i;
            }

            return true;
        }

        var count = _filterNodes.Count;
        for (var i = 0; i < count; ++i)
        {
            if (matchesOneFilter(_filterNodes[i]))
            {
                return true;
            }
        }

        return false;
    }

    public static void DrawHelp(string tooltip, bool nested = false)
    {
        // draw tooltip marker with proper alignment
        ImGui.AlignTextToFramePadding();
        if (tooltip.Length > 0)
        {
            UIMisc.HelpMarker(tooltip);
        }
        else
        {
            using var invisible = ImRaii.PushColor(ImGuiCol.Text, 0x00000000);
            UIMisc.IconText(Dalamud.Interface.FontAwesomeIcon.InfoCircle);
        }
        ImGui.SameLine();
        if (nested)
            DrawNesting();
    }

    private static void DrawNesting()
    {
        var sHeight = ImGui.GetFrameHeight();
        var sBox = new Vector2(sHeight, sHeight);

        var bar = "└";
        var pos = ImGui.GetCursorScreenPos();
        var size = ImGui.CalcTextSize(bar);

        ImGui.Dummy(new(sHeight - ImGui.GetStyle().ItemInnerSpacing.X, 0));
        ImGui.SameLine();
        ImGui.GetWindowDrawList().AddText(pos + (sBox - size) * 0.5f, ImGui.GetColorU32(ImGuiCol.Text), bar);
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, ConfigFieldMetadata member, object? value, ConfigRoot root, UITree tree, WorldState ws) => value switch
    {
        bool v => DrawProperty(label, tooltip, node, member, v),
        Enum v => DrawProperty(label, tooltip, node, member, v),
        float v => DrawProperty(label, tooltip, node, member, v),
        int v => DrawProperty(label, tooltip, node, member, v),
        string v => DrawProperty(label, tooltip, node, member, v),
        Color v => DrawProperty(label, tooltip, node, member, v),
        Color[] v => DrawProperty(label, tooltip, node, member, v),
        GroupAssignment v => DrawProperty(label, tooltip, node, member, v, root, tree, ws),
        _ => false
    };

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, ConfigFieldMetadata member, bool v)
    {
        DrawHelp(tooltip);
        var combo = member.Combo;
        if (combo != null)
        {
            if (UICombo.Bool(label, combo.Values, ref v))
            {
                member.Setter(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.Checkbox(label, ref v))
            {
                member.Setter(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, ConfigFieldMetadata member, Enum v)
    {
        DrawHelp(tooltip);
        if (UICombo.Enum(label, member.FieldType, ref v))
        {
            member.Setter(node, v);
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, ConfigFieldMetadata member, float v)
    {
        DrawHelp(tooltip);
        var slider = member.Slider;
        if (slider != null)
        {
            var flags = ImGuiSliderFlags.None;
            if (slider.Logarithmic)
            {
                flags |= ImGuiSliderFlags.Logarithmic;
            }

            ImGui.SetNextItemWidth(Math.Min(ImGui.GetWindowWidth() * 0.30f, 175));
            if (ImGui.DragFloat(label, ref v, slider.Speed, slider.Min, slider.Max, "%.3f", flags))
            {
                member.Setter(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.InputFloat(label, ref v))
            {
                member.Setter(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, ConfigFieldMetadata member, int v)
    {
        DrawHelp(tooltip);
        var slider = member.Slider;
        if (slider != null)
        {
            var flags = ImGuiSliderFlags.None;
            if (slider.Logarithmic)
            {
                flags |= ImGuiSliderFlags.Logarithmic;
            }

            ImGui.SetNextItemWidth(Math.Min(ImGui.GetWindowWidth() * 0.30f, 175));
            if (ImGui.DragInt(label, ref v, slider.Speed, (int)slider.Min, (int)slider.Max, "%d", flags))
            {
                member.Setter(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.InputInt(label, ref v))
            {
                member.Setter(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, ConfigFieldMetadata member, string v)
    {
        DrawHelp(tooltip);
        if (ImGui.InputText(label, ref v, 256))
        {
            member.Setter(node, v);
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, ConfigFieldMetadata member, Color v)
    {
        DrawHelp(tooltip);
        var col = v.ToFloat4();
        if (ImGui.ColorEdit4(label, ref col, ImGuiColorEditFlags.PickerHueWheel))
        {
            member.Setter(node, Color.FromFloat4(col));
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, ConfigFieldMetadata member, Color[] v)
    {
        var modified = false;
        var len = v.Length;
        for (var i = 0; i < len; ++i)
        {
            DrawHelp(tooltip);
            var col = v[i].ToFloat4();
            if (ImGui.ColorEdit4($"{label} {i}", ref col, ImGuiColorEditFlags.PickerHueWheel))
            {
                v[i] = Color.FromFloat4(col);
                member.Setter(node, v);
                modified = true;
            }
        }
        return modified;
    }

    public static void DrawGroupPresetIndicator(string text, Action contextMenu)
    {
        ImGui.AlignTextToFramePadding();
        if (UIMisc.IconButton(Dalamud.Interface.FontAwesomeIcon.ListUl, $"###{text}open"))
        {
            ImGui.OpenPopup($"{text}popup");
        }

        if (ImGui.BeginPopup($"{text}popup"))
        {
            contextMenu();
            ImGui.EndPopup();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Select a preset");
        }

        ImGui.SameLine();
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, ConfigFieldMetadata member, GroupAssignment v, ConfigRoot root, UITree tree, WorldState ws)
    {
        var group = member.Group;
        if (group == null)
        {
            return false;
        }

        var spaced = false;

        ImGui.AlignTextToFramePadding();
        if (tooltip.Length > 0)
        {
            spaced = true;
            UIMisc.HelpMarker(tooltip);
            ImGui.SameLine();
        }

        var hasPreset = member.GroupPresets.Length > 0;
        if (hasPreset)
        {
            spaced = true;
            DrawGroupPresetIndicator(label, () => DrawPropertyContextMenu(node, member, v));
        }

        if (!spaced)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, 0))
            {
                UIMisc.IconText(Dalamud.Interface.FontAwesomeIcon.InfoCircle);
            }
        }

        var modified = false;
        foreach (var tn in tree.Node(label, false, v.Validate() ? Colors.TextColor1 : Colors.TextColor2))
        {
            using var indent = ImRaii.PushIndent();
            var names = group.Names;
            var len = names.Length;
            using var table = ImRaii.Table("table", len + 2, ImGuiTableFlags.SizingFixedFit);
            if (!table)
            {
                continue;
            }

            for (var i = 0; i < len; ++i)
            {
                ImGui.TableSetupColumn(names[i]);
            }

            ImGui.TableSetupColumn("----");
            ImGui.TableSetupColumn("Name");
            ImGui.TableHeadersRow();

            var assignments = root.Get<PartyRolesConfig>().SlotsPerAssignment(ws.Party);
            for (var i = 0; i < (int)PartyRolesConfig.Assignment.Unassigned; ++i)
            {
                var r = (PartyRolesConfig.Assignment)i;
                ImGui.TableNextRow();
                for (var c = 0; c < group.Names.Length; ++c)
                {
                    ImGui.TableNextColumn();
                    if (ImGui.RadioButton($"###{r}:{c}", v[r] == c))
                    {
                        v[r] = c;
                        modified = true;
                    }
                }
                ImGui.TableNextColumn();
                if (ImGui.RadioButton($"###{r}:---", v[r] < 0 || v[r] >= group.Names.Length))
                {
                    v[r] = -1;
                    modified = true;
                }

                var name = r.ToString();
                if (assignments.Length > 0)
                {
                    name += $" ({ws.Party[assignments[i]]?.Name})";
                }

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(name);
            }
        }
        return modified;
    }

    private static void DrawPropertyContextMenu(ConfigNode node, ConfigFieldMetadata member, GroupAssignment v)
    {
        var presets = member.GroupPresets;
        var len = presets.Length;
        for (var i = 0; i < len; ++i)
        {
            var preset = presets[i];
            if (ImGui.MenuItem(preset.Name))
            {
                var lenPP = preset.Preset.Length;
                for (var j = 0; j < lenPP; ++j)
                {
                    v.Assignments[j] = preset.Preset[j];
                }

                node.Modified.Fire();
            }
        }
    }
}
