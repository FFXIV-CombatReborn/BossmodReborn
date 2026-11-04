using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using RebornMaterial;

namespace BossMod.RebornUI;

internal static class SettingsRenderer
{
    public sealed class FieldView(ConfigFieldMetadata meta, SettingsLayout.Field? layout, string label)
    {
        public readonly ConfigFieldMetadata Meta = meta;
        public readonly PropertyDisplayAttribute Display = meta.Display!;
        public readonly string Label = label;
        public readonly string Tooltip = layout?.Tooltip ?? meta.Display!.Tooltip;
        public readonly string? Depends = layout?.Depends ?? meta.Display!.Depends;
        public readonly string? Format = layout?.Format;
        public readonly string[]? Options = layout?.Options ?? meta.Combo?.Values;
        public readonly (float Min, float Max, bool Logarithmic)? Slider = layout is { Min: { } min, Max: { } max } ? (min, max, false)
            : meta.Slider is { } slider ? (slider.Min, slider.Max, slider.Logarithmic) : null;
        public string Section = "";
    }

    public sealed class NodeView
    {
        public sealed class Group(string? label, FontAwesomeIcon icon)
        {
            public readonly string? Label = label;
            public readonly FontAwesomeIcon Icon = icon;
            public readonly List<int> Roots = [];
        }

        public required ConfigTypeMetadata Metadata;
        public required SettingsLayout.Node? Layout;
        public required FieldView[] Fields;
        public required Dictionary<string, FieldView> FieldsByName;
        public required List<Group> Groups;
        public required List<int>[] Children;
        public required bool HasCustom;
    }

    private static readonly Dictionary<Type, NodeView> _views = [];
    private static readonly Dictionary<Type, PropertyRenderer> _propertyRenderers = [];
    private static readonly Dictionary<Type, string[]> _enumLabels = [];
    private static readonly Dictionary<uint, bool> _expandedGroups = [];
    private static Dictionary<Type, ConfigNode>? _defaults;

    private static float Scale => M3.Scale;

    private static void DrawClassicCustom(ConfigNode node, UITree tree, WorldState ws) => node.DrawCustom(tree, ws);

    private static void DrawCustom(ConfigNode node, NodeView view, UITree tree, WorldState ws)
    {
        if (view.Layout?.Custom is { } custom)
        {
            custom(node, tree, ws);
        }
        else
        {
            DrawClassicCustom(node, tree, ws);
        }
    }

    public static void DrawNode(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws, ISet<string>? fields = null)
    {
        var type = node.GetType();
        using var id = ImRaii.PushId(type.FullName ?? type.Name);
        var view = View(type);

        foreach (var group in view.Groups)
        {
            if (!AnyVisible(view, group.Roots, fields))
            {
                continue;
            }

            if (group.Label is { Length: > 0 } label)
            {
                M3Widgets.SectionLabel(label);
            }

            DrawFields(node, root, tree, ws, view, group.Roots, fields);
            if (fields == null)
            {
                view.Layout?.SectionExtras?.Invoke(node, group.Label ?? "", tree, ws);
            }
        }

        if (fields == null && view.HasCustom)
        {
            DrawCustom(node, view, tree, ws);
        }
    }

    public static void DrawNodeCards(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws, string? title, FontAwesomeIcon icon = FontAwesomeIcon.None, int accentOffset = 0)
    {
        var type = node.GetType();
        var typeName = type.FullName ?? type.Name;
        using var id = ImRaii.PushId(typeName);
        var view = View(type);

        var last = -1;
        for (var i = 0; i < view.Groups.Count; ++i)
        {
            if (AnyVisible(view, view.Groups[i].Roots, null))
            {
                last = i;
            }
        }

        if (last < 0)
        {
            if (view.HasCustom)
            {
                DrawCustom(node, view, tree, ws);
            }
            return;
        }

        title ??= last > 0 ? "General" : null;
        var shown = 0;
        for (var i = 0; i <= last; ++i)
        {
            var group = view.Groups[i];
            if (!AnyVisible(view, group.Roots, null))
            {
                continue;
            }

            var named = group.Label is { Length: > 0 };
            var cardTitle = named ? group.Label : shown == 0 ? title : null;
            var cardIcon = named ? group.Icon : shown == 0 ? icon : FontAwesomeIcon.None;
            using (M3Card.Begin($"{typeName}#{i}", cardTitle, cardIcon, M3.AccentFor(shown + accentOffset)))
            {
                DrawFields(node, root, tree, ws, view, group.Roots, null);
                view.Layout?.SectionExtras?.Invoke(node, group.Label ?? "", tree, ws);
                if (i == last && view.HasCustom)
                {
                    DrawCustom(node, view, tree, ws);
                }
            }
            ++shown;
        }
    }

    public static bool NodeHasSettings(ConfigNode node)
    {
        var view = View(node.GetType());
        return view.HasCustom || view.Fields.Length > 0;
    }

    public static NodeView View(Type type)
    {
        if (_views.TryGetValue(type, out var cached))
        {
            return cached;
        }

        var metadata = GeneratedConfigMetadata.Get(type);
        var layout = SettingsLayout.For(type);
        var display = metadata.DisplayFields;
        var len = display.Length;

        Dictionary<string, string> strip = [];
        if (layout?.Sections is { } listed)
        {
            foreach (var section in listed)
            {
                if (section.StripPrefix is { Length: > 0 } prefix)
                {
                    foreach (var name in section.Fields)
                    {
                        strip[name] = prefix;
                    }
                }
            }
        }

        var fields = new FieldView[len];
        var indexByName = new Dictionary<string, int>(len, StringComparer.Ordinal);
        for (var i = 0; i < len; ++i)
        {
            var meta = display[i];
            var fieldLayout = layout?.Fields.GetValueOrDefault(meta.Name);
            var label = fieldLayout?.Label ?? (strip.TryGetValue(meta.Name, out var prefix) ? StripPrefix(meta.Display!.Label, prefix) : meta.Display!.Label);
            fields[i] = new(meta, fieldLayout, label);
            indexByName[meta.Name] = i;
        }

        var parents = new int[len];
        for (var i = 0; i < len; ++i)
        {
            parents[i] = fields[i].Depends is { Length: > 0 } depends && indexByName.TryGetValue(depends, out var p) && p != i ? p : -1;
        }

        for (var i = 0; i < len; ++i)
        {
            var steps = 0;
            for (var p = parents[i]; p >= 0 && steps <= len; p = parents[p])
            {
                ++steps;
            }
            if (steps > len)
            {
                parents[i] = -1;
            }
        }

        var children = new List<int>[len];
        for (var i = 0; i < len; ++i)
        {
            children[i] = [];
        }
        for (var i = 0; i < len; ++i)
        {
            if (parents[i] >= 0)
            {
                children[parents[i]].Add(i);
            }
        }

        var groups = new List<NodeView.Group>();
        if (layout?.Sections is { } sections)
        {
            var assigned = new HashSet<int>();
            foreach (var section in sections)
            {
                var group = new NodeView.Group(section.Label, section.Icon);
                foreach (var name in section.Fields)
                {
                    if (indexByName.TryGetValue(name, out var i) && assigned.Add(i) && parents[i] < 0)
                    {
                        group.Roots.Add(i);
                    }
                }
                groups.Add(group);
            }

            var rest = new NodeView.Group(sections.Length > 0 ? "Other settings" : null, FontAwesomeIcon.EllipsisH);
            for (var i = 0; i < len; ++i)
            {
                if (!assigned.Contains(i) && parents[i] < 0)
                {
                    rest.Roots.Add(i);
                }
            }
            if (rest.Roots.Count > 0)
            {
                groups.Add(rest);
            }
        }
        else
        {
            groups.Add(new(null, FontAwesomeIcon.None));
            for (var i = 0; i < len; ++i)
            {
                if (display[i].SectionStart is { } section)
                {
                    if (groups[^1].Roots.Count == 0 && groups[^1].Label == null)
                    {
                        groups[^1] = new(section.Label, FontAwesomeIcon.None);
                    }
                    else
                    {
                        groups.Add(new(section.Label, FontAwesomeIcon.None));
                    }
                }

                if (parents[i] < 0)
                {
                    groups[^1].Roots.Add(i);
                }
            }
        }

        foreach (var group in groups)
        {
            foreach (var root in group.Roots)
            {
                SetSection(fields, children, root, group.Label ?? "");
            }
        }

        var hasCustom = layout?.Custom is { } custom
            ? custom != SettingsLayout.NoCustom
            : type.GetMethod(nameof(ConfigNode.DrawCustom))?.DeclaringType != typeof(ConfigNode);

        return _views[type] = new()
        {
            Metadata = metadata,
            Layout = layout,
            Fields = fields,
            FieldsByName = fields.ToDictionary(f => f.Meta.Name),
            Groups = groups,
            Children = children,
            HasCustom = hasCustom,
        };
    }

    private static void SetSection(FieldView[] fields, List<int>[] children, int index, string section)
    {
        fields[index].Section = section;
        foreach (var child in children[index])
        {
            SetSection(fields, children, child, section);
        }
    }

    private static string StripPrefix(string label, string prefix)
    {
        if (!label.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || label.Length == prefix.Length)
        {
            return label;
        }
        var rest = label[prefix.Length..];
        return char.ToUpperInvariant(rest[0]) + rest[1..];
    }

    private static bool AnyVisible(NodeView view, List<int> indices, ISet<string>? fields)
    {
        foreach (var i in indices)
        {
            if (fields?.Contains(view.Fields[i].Meta.Name) != false || AnyVisible(view, view.Children[i], fields))
            {
                return true;
            }
        }
        return false;
    }

    private static void DrawFields(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws, NodeView view, List<int> indices, ISet<string>? fields)
    {
        foreach (var i in indices)
        {
            var field = view.Fields[i];
            var visible = fields?.Contains(field.Meta.Name) != false;
            if (visible)
            {
                DrawField(node, root, tree, ws, view, field);
            }

            var children = view.Children[i];
            if (children.Count > 0 && AnyVisible(view, children, fields))
            {
                if (visible)
                {
                    using var group = M3SubGroup.Begin(M3.Scheme.Primary);
                    DrawFields(node, root, tree, ws, view, children, fields);
                }
                else
                {
                    DrawFields(node, root, tree, ws, view, children, fields);
                }
            }
        }
    }

    private static void DrawField(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws, NodeView view, FieldView field)
    {
        var value = field.Meta.Getter(node);
        var enabled = IsEnabled(node, view, field);
        var transient = root.IsTransient(node, field.Meta);

        bool modified;
        if (field.Display.Renderer is { } rendererType)
        {
            using (ImRaii.Disabled(!enabled))
            {
                modified = GetPropertyRenderer(rendererType).Draw(field.Display, false, node, value!, root, tree, ws);
            }
        }
        else
        {
            var row = new RowText(field.Label, field.Meta.Name, transient ? TransientTooltip(field.Tooltip) : field.Tooltip, enabled, transient ? M3.Scheme.Tertiary : null);
            modified = DrawProperty(row, node, field, value, root, ws);
        }

        if (modified)
        {
            node.Modified.Fire();
        }
    }

    private static string TransientTooltip(string tooltip)
    {
        const string note = "Temporarily changed by another plugin (e.g. AutoDuty). Your own value is kept in the config file and restored when that plugin is done; changing it here keeps your new value.";
        return tooltip.Length > 0 ? $"{tooltip}\n{note}" : note;
    }

    private static bool IsEnabled(ConfigNode node, NodeView view, FieldView field)
        => field.Depends is not { Length: > 0 } depends || (IsDependencyEnabled(node, view, depends, view.Metadata.Fields.Length) ?? true);

    private static bool? IsDependencyEnabled(ConfigNode node, NodeView view, string fieldName, int remainingDepth)
    {
        if (remainingDepth <= 0 || !view.Metadata.FieldsByName.TryGetValue(fieldName, out var dependency))
        {
            return null;
        }

        var parentDepends = view.FieldsByName.TryGetValue(fieldName, out var dependencyView) ? dependencyView.Depends : dependency.Display?.Depends;
        if (parentDepends is { Length: > 0 })
        {
            var parentEnabled = IsDependencyEnabled(node, view, parentDepends, remainingDepth - 1);
            if (parentEnabled != true)
            {
                return parentEnabled;
            }
        }

        return dependency.Getter(node) switch
        {
            bool value => value,
            null when dependency.FieldType == typeof(bool?) => false,
            _ => null
        };
    }

    private static PropertyRenderer GetPropertyRenderer(Type type)
        => _propertyRenderers.TryGetValue(type, out var renderer) ? renderer : (_propertyRenderers[type] = GeneratedFactories.CreatePropertyRenderer(type));

    private static object? DefaultValue(ConfigNode node, ConfigFieldMetadata field)
    {
        if (_defaults == null)
        {
            _defaults = [];
            GeneratedRegistries.RegisterConfigNodes((type, n) => _defaults[type] = n);
        }
        return _defaults.TryGetValue(node.GetType(), out var defaults) ? field.Getter(defaults) : null;
    }

    public static void DrawPrePullHintSetting(BossModuleRegistry.Info info, string label = "Show pre-fight hint popup for this encounter")
    {
        var show = BossModuleManager.Config.ShowPrePullHintsFor(info.PrimaryActorOID);
        var supporting = BossModuleManager.Config.ShowPrePullHints ? null : "Pre-fight hint popups are turned off for every encounter in Radar and hints.";
        if (M3Widgets.RowSwitch($"{label}###PrePullHints{info.PrimaryActorOID:X8}", ref show, supporting))
        {
            BossModuleManager.Config.SetShowPrePullHintsFor(info.PrimaryActorOID, show);
        }
    }

    private readonly record struct RowText(string Label, string Id, string Supporting, bool Enabled, Vector4? LabelColor);

    private static M3RowInfo BeginRow(in RowText text, Vector2 controlSize)
    {
        var row = M3SettingRow.Begin(text.Label, text.Supporting.Length > 0 ? text.Supporting : null, controlSize, text.LabelColor, disabled: !text.Enabled);
        ImGui.SetCursorScreenPos(row.ControlPosition);
        return row;
    }

    private static bool DrawProperty(in RowText row, ConfigNode node, FieldView field, object? value, ConfigRoot root, WorldState ws) => value switch
    {
        bool v => DrawProperty(row, node, field, v),
        Enum v => DrawProperty(row, node, field, v),
        float v => DrawProperty(row, node, field, v),
        double v => DrawProperty(row, node, field, v),
        int v => DrawProperty(row, node, field, v),
        string v => DrawProperty(row, node, field, v),
        Color v => DrawProperty(row, node, field, v),
        Color[] v => DrawProperty(row, node, field, v),
        GroupAssignment v => DrawProperty(row, node, field, v, root, ws),
        _ => false
    };

    private static bool DrawProperty(in RowText row, ConfigNode node, FieldView field, bool v)
    {
        if (field.Options is { Length: >= 2 } options)
        {
            var index = v ? 1 : 0;
            if (DrawComboRow(row, ref index, 2, i => options[i]))
            {
                field.Meta.Setter(node, index != 0);
                return true;
            }
            return false;
        }

        var info = BeginRow(row, M3Widgets.SwitchSize());
        var changed = M3Widgets.Switch($"##{row.Id}", ref v, row.Enabled);
        M3SettingRow.End(info);
        if (changed)
        {
            field.Meta.Setter(node, v);
        }
        return changed;
    }

    private static bool DrawProperty(in RowText row, ConfigNode node, FieldView field, Enum v)
    {
        var type = field.Meta.FieldType;
        var values = GeneratedEnumMetadata.Values(type);
        var labels = EnumLabels(type);
        if (field.Options is { } options && options.Length == labels.Length)
        {
            labels = options;
        }

        var index = GeneratedEnumMetadata.IndexOf(type, v);
        bool changed;
        if (field.Meta.Radio)
        {
            var segments = new M3Segment[labels.Length];
            for (var i = 0; i < labels.Length; ++i)
            {
                segments[i] = new(labels[i]);
            }

            var width = MathF.Min(M3Widgets.SegmentedWidth(segments), M3SettingRow.MaxControlWidth());
            var info = BeginRow(row, new(width, M3Widgets.SegmentedHeight));
            var clicked = M3Widgets.SegmentedButtons($"##{row.Id}", segments, index, width, enabled: row.Enabled);
            M3SettingRow.End(info);
            changed = clicked >= 0 && clicked != index;
            index = clicked;
        }
        else
        {
            changed = DrawComboRow(row, ref index, labels.Length, i => labels[i]);
        }

        if (changed)
        {
            field.Meta.Setter(node, (Enum)values.GetValue(index)!);
        }
        return changed;
    }

    private static bool DrawProperty(in RowText row, ConfigNode node, FieldView field, float v)
    {
        var changed = field.Slider is { } slider ? DrawSliderRow(row, ref v, slider, field.Format) : DrawNumberRow(row, ref v);
        if (changed)
        {
            field.Meta.Setter(node, v);
        }
        return changed;
    }

    private static bool DrawProperty(in RowText row, ConfigNode node, FieldView field, double v)
    {
        var f = (float)v;
        var changed = field.Slider is { } slider ? DrawSliderRow(row, ref f, slider, field.Format) : DrawNumberRow(row, ref f);
        if (changed)
        {
            field.Meta.Setter(node, (double)f);
        }
        return changed;
    }

    private static bool DrawProperty(in RowText row, ConfigNode node, FieldView field, int v)
    {
        bool changed;
        if (field.Slider is { } slider)
        {
            var trackWidth = 150f * Scale;
            var min = (int)slider.Min;
            var max = (int)slider.Max;
            var info = BeginRow(row, new(trackWidth + ReadoutGutter(min.ToString(), max.ToString()), M3Widgets.ButtonHeight));
            changed = M3Widgets.SliderInt($"##{row.Id}", ref v, min, max, v.ToString(), trackWidth, slider.Logarithmic, row.Enabled);
            M3SettingRow.End(info);
        }
        else
        {
            var width = M3Widgets.NumberFieldWidth(true);
            var info = BeginRow(row, new(width, M3Widgets.NumberFieldHeight));
            changed = M3Widgets.NumberField($"##{row.Id}", ref v, width, 1, enabled: row.Enabled);
            M3SettingRow.End(info);
        }

        if (changed)
        {
            field.Meta.Setter(node, v);
        }
        return changed;
    }

    private static bool DrawProperty(in RowText row, ConfigNode node, FieldView field, string v)
    {
        var width = MathF.Min(240f * Scale, M3SettingRow.MaxControlWidth() * 0.6f);
        var info = BeginRow(row, new(width, M3TextField.Height));
        bool changed;
        using (ImRaii.Disabled(!row.Enabled))
        {
            changed = M3TextField.Draw($"##{row.Id}", "", ref v, width);
        }
        M3SettingRow.End(info);

        if (changed)
        {
            field.Meta.Setter(node, v);
        }
        return changed;
    }

    private static bool DrawProperty(in RowText row, ConfigNode node, FieldView field, Color v)
    {
        var current = v.ToFloat4();
        Vector4? fallback = DefaultValue(node, field.Meta) is Color def ? def.ToFloat4() : null;
        if (DrawColorRow(row, ref current, fallback))
        {
            field.Meta.Setter(node, Color.FromFloat4(current));
            return true;
        }
        return false;
    }

    private static bool DrawProperty(in RowText row, ConfigNode node, FieldView field, Color[] v)
    {
        var swatch = M3Widgets.ColorSwatchSize;
        var gap = M3.Space1;
        var count = v.Length;

        var maxWidth = M3SettingRow.MaxControlWidth();
        var perLine = Math.Clamp((int)((maxWidth + gap) / (swatch + gap)), 1, Math.Max(1, count));
        var lines = (count + perLine - 1) / perLine;
        var size = new Vector2(perLine * (swatch + gap) - gap, lines * (swatch + gap) - gap);

        var info = BeginRow(row, size);
        var origin = info.ControlPosition;
        var changed = false;
        for (var i = 0; i < count; ++i)
        {
            ImGui.SetCursorScreenPos(origin + new Vector2(i % perLine * (swatch + gap), i / perLine * (swatch + gap)));
            var color = v[i].ToFloat4();
            Vector4? fallback = DefaultValue(node, field.Meta) is Color[] defaults && i < defaults.Length ? defaults[i].ToFloat4() : null;
            if (M3Widgets.ColorSwatch($"##{row.Id}{i}", ref color, fallback, true, row.Enabled, $"{row.Label} {i + 1}"))
            {
                v[i] = Color.FromFloat4(color);
                changed = true;
            }
        }
        M3SettingRow.End(info);

        if (changed)
        {
            field.Meta.Setter(node, v);
        }
        return changed;
    }

    private static bool DrawProperty(in RowText row, ConfigNode node, FieldView field, GroupAssignment v, ConfigRoot root, WorldState ws)
    {
        var group = field.Meta.Group;
        if (group == null)
        {
            return false;
        }

        var key = ImGui.GetID(row.Id);
        var expanded = _expandedGroups.GetValueOrDefault(key);
        var presets = field.Meta.GroupPresets;
        var hasPresets = presets.Length > 0;
        var button = M3Widgets.IconButtonSize;
        var controls = new Vector2(button * (hasPresets ? 2 : 1) + (hasPresets ? M3.Space1 : 0), button);

        var valid = v.Validate();
        var text = valid ? row : row with { LabelColor = M3.Scheme.Error, Supporting = row.Supporting.Length > 0 ? $"{row.Supporting}\nEvery role needs a valid, unique group." : "Every role needs a valid, unique group." };
        var info = BeginRow(text, controls);

        var modified = false;
        if (hasPresets)
        {
            var popup = $"##{row.Id}presets";
            if (M3Widgets.IconButton($"##{row.Id}preset", FontAwesomeIcon.ListUl, "Apply a preset", enabled: row.Enabled))
            {
                M3Menu.Open(popup);
            }

            using (var menu = M3Menu.Begin(popup))
            {
                if (menu.IsOpen)
                {
                    foreach (var preset in presets)
                    {
                        if (M3Menu.Item($"##{preset.Name}", preset.Name))
                        {
                            var len = Math.Min(preset.Preset.Length, v.Assignments.Length);
                            for (var j = 0; j < len; ++j)
                            {
                                v.Assignments[j] = preset.Preset[j];
                            }
                            modified = true;
                        }
                    }
                }
            }

            ImGui.SameLine(0f, M3.Space1);
        }

        if (M3Widgets.IconButton($"##{row.Id}expand", expanded ? FontAwesomeIcon.ChevronUp : FontAwesomeIcon.ChevronDown, expanded ? "Hide assignments" : "Edit assignments"))
        {
            _expandedGroups[key] = expanded = !expanded;
        }
        M3SettingRow.End(info);

        if (expanded)
        {
            var indent = M3.Space3;
            ImGui.Indent(indent);
            modified |= DrawGroupAssignmentTable(row, v, group, root, ws);
            ImGui.Unindent(indent);
        }
        return modified;
    }

    private static bool DrawGroupAssignmentTable(in RowText row, GroupAssignment v, GroupDetailsAttribute group, ConfigRoot root, WorldState ws)
    {
        var modified = false;
        var names = group.Names;
        var columns = names.Length;
        using var table = ImRaii.Table($"##{row.Id}table", columns + 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg);
        if (!table)
        {
            return modified;
        }

        var radioWidth = M3Widgets.RadioSize().X;
        for (var i = 0; i < columns; ++i)
        {
            ImGui.TableSetupColumn(names[i], ImGuiTableColumnFlags.WidthFixed, MathF.Max(radioWidth, ImGui.CalcTextSize(names[i]).X));
        }
        ImGui.TableSetupColumn("None", ImGuiTableColumnFlags.WidthFixed, MathF.Max(radioWidth, ImGui.CalcTextSize("None").X));
        ImGui.TableSetupColumn("Role");
        ImGui.TableHeadersRow();

        var assignments = root.Get<PartyRolesConfig>().SlotsPerAssignment(ws.Party);
        for (var i = 0; i < (int)PartyRolesConfig.Assignment.Unassigned; ++i)
        {
            var r = (PartyRolesConfig.Assignment)i;
            ImGui.TableNextRow();
            for (var c = 0; c < columns; ++c)
            {
                ImGui.TableNextColumn();
                if (M3Widgets.RadioButton($"##{r}:{c}", v[r] == c, row.Enabled))
                {
                    v[r] = c;
                    modified = true;
                }
            }

            ImGui.TableNextColumn();
            if (M3Widgets.RadioButton($"##{r}:none", v[r] < 0 || v[r] >= columns, row.Enabled))
            {
                v[r] = -1;
                modified = true;
            }

            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            M3Text.Draw(assignments.Length > 0 ? $"{r} ({ws.Party[assignments[i]]?.Name})" : r.ToString());
        }
        return modified;
    }

    private static bool DrawComboRow(in RowText row, ref int index, int count, Func<int, string> label)
    {
        var widest = "";
        var widestWidth = 0f;
        for (var i = 0; i < count; ++i)
        {
            var text = label(i);
            var textWidth = ImGui.CalcTextSize(text).X;
            if (textWidth > widestWidth)
            {
                widest = text;
                widestWidth = textWidth;
            }
        }

        var width = MathF.Min(MathF.Max(160f * Scale, M3Widgets.ComboWidthFor(widest)), MathF.Max(120f * Scale, M3SettingRow.MaxControlWidth() * 0.6f));
        var info = BeginRow(row, new(width, M3Widgets.ComboHeight));
        var changed = M3Widgets.Combo($"##{row.Id}", ref index, count, label, width, search: count > 12, enabled: row.Enabled);
        M3SettingRow.End(info);
        return changed;
    }

    private static bool DrawSliderRow(in RowText row, ref float v, (float Min, float Max, bool Logarithmic) slider, string? format)
    {
        var range = slider.Max - slider.Min;
        format ??= range >= 50f ? "%.0f" : range >= 5f ? "%.1f" : "%.2f";
        var trackWidth = 150f * Scale;
        var info = BeginRow(row, new(trackWidth + ReadoutGutter(FormatReadout(format, slider.Min), FormatReadout(format, slider.Max)), M3Widgets.ButtonHeight));
        var changed = M3Widgets.Slider($"##{row.Id}", ref v, slider.Min, slider.Max, FormatReadout(format, v), trackWidth, slider.Logarithmic, row.Enabled);
        M3SettingRow.End(info);
        return changed;
    }

    // Room for the value beside a slider track. A negative minimum can be wider than the maximum.
    private static float ReadoutGutter(string min, string max) => MathF.Max(M3Widgets.SliderValueGutter(min), M3Widgets.SliderValueGutter(max));

    private static bool DrawNumberRow(in RowText row, ref float v)
    {
        var width = M3Widgets.NumberFieldWidth();
        var info = BeginRow(row, new(width, M3Widgets.NumberFieldHeight));
        var changed = M3Widgets.NumberField($"##{row.Id}", ref v, width, format: "%.3f", enabled: row.Enabled);
        M3SettingRow.End(info);
        return changed;
    }

    private static bool DrawColorRow(in RowText row, ref Vector4 color, Vector4? fallback)
    {
        var swatch = M3Widgets.ColorSwatchSize;
        var reset = 32f * Scale;
        var canReset = fallback is { } f && f != color;
        var size = new Vector2(swatch + (canReset ? M3.Space1 + reset : 0f), MathF.Max(swatch, reset));
        var info = BeginRow(row, size);
        var origin = info.ControlPosition;
        ImGui.SetCursorScreenPos(origin + new Vector2(0f, (size.Y - swatch) * 0.5f));
        var changed = M3Widgets.ColorSwatch($"##{row.Id}", ref color, fallback, true, row.Enabled);
        if (canReset)
        {
            ImGui.SetCursorScreenPos(origin + new Vector2(swatch + M3.Space1, (size.Y - reset) * 0.5f));
            if (M3Widgets.IconButton($"##{row.Id}reset", FontAwesomeIcon.Undo, "Reset to default", diameter: reset, enabled: row.Enabled))
            {
                color = fallback!.Value;
                changed = true;
            }
        }
        M3SettingRow.End(info);
        return changed;
    }

    private static string FormatReadout(string format, float value)
    {
        var start = format.IndexOf('%', StringComparison.Ordinal);
        var end = start >= 0 ? format.IndexOf('f', start) : -1;
        if (start < 0 || end < 0)
        {
            return value.ToString("0.##");
        }

        var digits = 0;
        var dot = format.IndexOf('.', start);
        if (dot > start && dot < end)
        {
            _ = int.TryParse(format.AsSpan(dot + 1, end - dot - 1), out digits);
        }
        return string.Concat(format.AsSpan(0, start), value.ToString($"F{digits}"), format.AsSpan(end + 1));
    }

    private static string[] EnumLabels(Type type)
    {
        if (_enumLabels.TryGetValue(type, out var labels))
        {
            return labels;
        }

        var values = GeneratedEnumMetadata.Values(type);
        labels = new string[values.Length];
        for (var i = 0; i < labels.Length; ++i)
        {
            labels[i] = GeneratedEnumMetadata.DisplayName(type, (Enum)values.GetValue(i)!);
        }
        return _enumLabels[type] = labels;
    }
}
