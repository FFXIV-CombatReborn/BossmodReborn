using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using RebornMaterial;

namespace BossMod.RebornUI;

internal static class PartyRolesPanel
{
    private static readonly List<Class> _priorityScratch = [];

    public static void Draw(PartyRolesConfig config, WorldState ws)
    {
        var autoWidth = M3Widgets.ButtonWidth(FontAwesomeIcon.Magic, "Auto-assign");
        var row = M3SettingRow.Begin("Auto-assign roles", "Assigns party roles from each member's job and their order in the party.", new(autoWidth, M3Widgets.ButtonHeight));
        ImGui.SetCursorScreenPos(row.ControlPosition);
        if (M3Widgets.Button("##autoassign", "Auto-assign", M3ButtonStyle.Filled, FontAwesomeIcon.Magic))
        {
            config.AutoAssignRoles(ws.Party);
        }
        M3SettingRow.End(row);

        M3Widgets.SectionLabel("Current party");
        DrawPartyAssignments(config, ws);

        M3Widgets.SectionLabel("Tank auto-assign priority");
        M3Text.Draw("Drag to reorder, from highest to lowest priority.", M3TextStyle.Muted, true);
        using (var table = ImRaii.Table("tankpriority", 2, ImGuiTableFlags.SizingStretchSame))
        {
            if (table)
            {
                ImGui.TableNextColumn();
                M3Text.Draw("Main tank", M3TextStyle.Label);
                DrawTankPriority(config, "mt", config.MainTankPriority);
                ImGui.TableNextColumn();
                M3Text.Draw("Off tank", M3TextStyle.Label);
                DrawTankPriority(config, "ot", config.OffTankPriority);
            }
        }

        ImGui.Dummy(new(0f, M3.Space1));
        var clearWidth = M3Widgets.ButtonWidth(FontAwesomeIcon.TrashAlt, "Hold to clear");
        row = M3SettingRow.Begin("Clear all assignments", "Forgets every saved assignment, for example to shrink the config file.", new(clearWidth, M3Widgets.ButtonHeight));
        ImGui.SetCursorScreenPos(row.ControlPosition);
        if (M3Widgets.HoldButton("##clearassignments", "Hold to clear", 1f, M3ButtonStyle.Danger, FontAwesomeIcon.TrashAlt, enabled: config.Assignments.Count > 0))
        {
            config.Assignments.Clear();
            config.Modified.Fire();
        }
        M3SettingRow.End(row);
    }

    private static void DrawTankPriority(PartyRolesConfig config, string id, Class[] order)
    {
        _priorityScratch.Clear();
        _priorityScratch.AddRange(order);
        var selected = -1;
        if (M3Widgets.ReorderList(id, _priorityScratch, static c => c.ToString(), ref selected))
        {
            _priorityScratch.CopyTo(order);
            config.Modified.Fire();
        }
    }

    private static void DrawPartyAssignments(PartyRolesConfig config, WorldState ws)
    {
        List<(ulong cid, string name, char role, PartyRolesConfig.Assignment assignment)> party = [with(PartyState.MaxPartySize)];
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            ref var m = ref ws.Party.Members[i];
            if (m.IsValid())
            {
                party.Add(new(m.ContentId, ws.Party[i]?.Name ?? "", ws.Party[i]?.Role.ToString()[0] ?? '?', config[m.ContentId]));
            }
        }

        if (party.Count == 0)
        {
            M3Text.Draw("Roles can be assigned once you're logged in.", M3TextStyle.Muted, true);
            return;
        }

        using (var table = ImRaii.Table("assignments", 10, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
        {
            if (table)
            {
                var type = typeof(PartyRolesConfig.Assignment);
                var assignments = (PartyRolesConfig.Assignment[])type.GeneratedEnumValues();
                var len = assignments.Length;
                var names = type.GeneratedEnumNames();
                var radio = M3Widgets.RadioSize().X;
                for (var i = 0; i < len; ++i)
                {
                    var name = i == (int)PartyRolesConfig.Assignment.Unassigned ? "None" : names[i];
                    ImGui.TableSetupColumn(name, ImGuiTableColumnFlags.WidthFixed, MathF.Max(radio, ImGui.CalcTextSize(name).X));
                }

                ImGui.TableSetupColumn("Name");
                ImGui.TableHeadersRow();

                party.Sort(static (a, b) => a.role.CompareTo(b.role));
                var partySpan = CollectionsMarshal.AsSpan(party);
                var count = party.Count;
                for (var j = 0; j < count; ++j)
                {
                    ref var p = ref partySpan[j];
                    ImGui.TableNextRow();
                    for (var i = 0; i < len; ++i)
                    {
                        var r = assignments[i];
                        ImGui.TableNextColumn();
                        if (M3Widgets.RadioButton($"###{p.cid:X}:{r}", p.assignment == r))
                        {
                            if (r != PartyRolesConfig.Assignment.Unassigned)
                            {
                                config.Assignments[p.cid] = r;
                            }
                            else
                            {
                                config.Assignments.Remove(p.cid);
                            }

                            config.Modified.Fire();
                        }
                    }
                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    M3Text.Draw($"({p.role}) {p.name}");
                }
            }
        }

        ImGui.Dummy(new(0f, M3.Space1));
        if (config.AssignmentsPerSlot(ws.Party).Length == 0)
        {
            M3Widgets.Banner("##assignmentstatus", "Invalid assignments: there should be exactly one raid member per role.", M3Severity.Warning, FontAwesomeIcon.ExclamationTriangle);
        }
        else
        {
            M3Widgets.Banner("##assignmentstatus", "All good! Every role is assigned exactly once.", M3Severity.Success, FontAwesomeIcon.CheckCircle);
        }
    }
}
