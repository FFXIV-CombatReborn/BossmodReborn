using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayAnalysis;

sealed class StatusInfo : CommonEnumInfo
{
    public record struct Instance(Replay Replay, Replay.Status Status);

    private class StatusData
    {
        public readonly HashSet<uint> SourceOIDs = [];
        public readonly HashSet<uint> TargetOIDs = [];
        public readonly HashSet<ushort> Extras = [];
        public readonly List<Instance> Instances = [];
    }

    private readonly Type? _sidType;
    private readonly Dictionary<uint, StatusData> _data = [];

    public StatusInfo(List<Replay> replays, uint oid)
    {
        var moduleInfo = BossModuleRegistry.FindByOID(oid);
        _oidType = moduleInfo?.ObjectIDType;
        _sidType = moduleInfo?.StatusIDType;
        foreach (var replay in replays)
        {
            for (var i = 0; i < replay.Encounters.Count; ++i)
            {
                var enc = replay.Encounters[i];
                if (enc.OID != oid)
                {
                    continue;
                }
                foreach (var status in replay.EncounterStatuses(enc))
                {
                    if (ReplayVisualization.OpList.BoringSIDs.Contains(status.ID) ||
                        status.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.Buddy ||
                        status.Target.Type is ActorType.Pet or ActorType.Chocobo)
                    {
                        continue;
                    }
                    var data = _data.GetOrAdd(status.ID);
                    if (status.Source != null)
                    {
                        data.SourceOIDs.Add(status.Source.OID);
                    }

                    data.TargetOIDs.Add(status.Target.OID);
                    data.Extras.Add(status.StartingExtra);
                    data.Instances.Add(new(replay, status));
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        UITree.NodeProperties map(KeyValuePair<uint, StatusData> kv)
        {
            var name = _sidType?.GeneratedEnumName(kv.Key);
            return new($"{Utils.StatusString(kv.Key)} ({name})", false, name == null ? Colors.TextColor2 : Colors.TextColor1);
        }
        foreach (var (sid, data) in tree.Nodes(_data, map))
        {
            tree.LeafNode($"Source IDs: {OIDListString(data.SourceOIDs)}");
            tree.LeafNode($"Target IDs: {OIDListString(data.TargetOIDs)}");
            tree.LeafNode($"Extras: {FormatExtras(data.Extras, ", ", "")}");
            foreach (var n in tree.Node($"Instances ({data.Instances.Count})###instances"))
            {
                tree.LeafNodes(data.Instances, i => $"{i.Replay.Path} @ {i.Status.Time.Start}: at {ReplayUtils.ParticipantString(i.Status.Target, i.Status.Time.Start)}");
            }
        }
    }

    public void DrawContextMenu()
    {
        if (ImGui.MenuItem("Generate enum for boss module"))
        {
            var sb = new StringBuilder("public enum SID : uint\n{\n");
            foreach (var (sid, data) in _data)
            {
                sb.Append($"    {EnumMemberString(sid, data)}\n");
            }

            sb.Append("\n}\n");
            ImGui.SetClipboardText(sb.ToString());
        }

        if (ImGui.MenuItem("Generate missing enum values for boss module"))
        {
            var sb = new StringBuilder();
            foreach (var (sid, data) in _data)
            {
                if (_sidType?.GeneratedEnumName(sid) == null)
                {
                    sb.AppendLine(EnumMemberString(sid, data));
                }
            }

            ImGui.SetClipboardText(sb.ToString());
        }
    }

    private static string FormatExtras(HashSet<ushort> extras, string separator, string prefix)
    {
        var sb = new StringBuilder();
        foreach (var extra in extras)
        {
            if (sb.Length > 0)
            {
                sb.Append(separator);
            }
            sb.Append(prefix).Append($"{extra:X}");
        }
        return sb.ToString();
    }

    private string EnumMemberString(uint sid, StatusData data)
    {
        var name = _sidType?.GeneratedEnumName(sid) ?? $"_Gen_{Utils.StringToIdentifier(Service.LuminaRow<Lumina.Excel.Sheets.Status>(sid)?.Name.ToString() ?? $"Status{sid}")}";
        var extrasStr = data.Extras.Count > 0 ? FormatExtras(data.Extras, "/", "0x") : "none";
        return $"{name} = {sid}, // {OIDListString(data.SourceOIDs)}->{OIDListString(data.TargetOIDs)}, extra={extrasStr}";
    }
}
