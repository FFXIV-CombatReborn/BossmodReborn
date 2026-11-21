using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;
using System.Globalization;

namespace BossMod.ReplayAnalysis;

sealed class ParticipantInfo : CommonEnumInfo
{
    sealed class ParticipantData
    {
        public List<ActorType> Types = [];
        public List<(uint zoneId, uint cfcId)> Zones = [];
        public List<(string name, uint id)> Names = [];
        public List<int> SpawnedPreFight = [];
        public bool SpawnedMidFight;
        public bool SeenTargetable;
        public float MinRadius = float.MaxValue;
        public float MaxRadius = float.MinValue;
    }

    private readonly Dictionary<uint, ParticipantData> _data = [];

    public ParticipantInfo(List<Replay> replays, uint oid)
    {
        var moduleInfo = BossModuleRegistry.FindByOID(oid);
        _oidType = moduleInfo?.ObjectIDType;
        foreach (var replay in replays)
        {
            for (var ei = 0; ei < replay.Encounters.Count; ++ei)
            {
                var enc = replay.Encounters[ei];
                if (enc.OID != oid)
                {
                    continue;
                }
                var minExistence = enc.Time.End.AddSeconds(-1d); // we don't want to add actors that spawned right before wipe, they could belong to reset
                foreach (var (commonOID, participants) in enc.ParticipantsByOID)
                {
                    var data = _data.GetOrAdd(commonOID);
                    var spawnedPreFight = 0;
                    for (var pi = 0; pi < participants.Count; ++pi)
                    {
                        var p = participants[pi];
                        if (IsIgnored(p) || p.EffectiveExistence.Start > minExistence)
                        {
                            continue;
                        }
                        data.Types.Add(p.Type);
                        data.Zones.Add((p.ZoneID, p.CFCID));
                        for (var ni = 0; ni < p.NameHistory.Count; ++ni)
                        {
                            data.Names.Add(p.NameHistory[ni].Value);
                        }

                        if (p.ExistsInWorldAt(enc.Time.Start))
                        {
                            ++spawnedPreFight;
                        }
                        else
                        {
                            data.SpawnedMidFight = true;
                        }

                        data.SeenTargetable |= p.TargetableHistory.Count > 0;
                        data.MinRadius = Math.Min(data.MinRadius, p.MinRadius);
                        data.MaxRadius = Math.Max(data.MaxRadius, p.MaxRadius);
                    }
                    data.SpawnedPreFight.Add(spawnedPreFight);
                }
            }
        }
        FinishBuild();
    }

    public ParticipantInfo(List<Replay> replays)
    {
        foreach (var replay in replays)
        {
            for (var pi = 0; pi < replay.Participants.Count; ++pi)
            {
                var p = replay.Participants[pi];
                if (IsIgnored(p))
                {
                    continue;
                }
                var data = _data.GetOrAdd(p.OID);
                data.Types.Add(p.Type);
                data.Zones.Add((p.ZoneID, p.CFCID));
                for (var ni = 0; ni < p.NameHistory.Count; ++ni)
                {
                    data.Names.Add(p.NameHistory[ni].Value);
                }
                data.SeenTargetable = p.TargetableHistory.Count > 0;
                data.MinRadius = Math.Min(data.MinRadius, p.MinRadius);
                data.MaxRadius = Math.Max(data.MaxRadius, p.MaxRadius);
            }
        }
        FinishBuild();
    }

    public void Draw(UITree tree)
    {
        UITree.NodeProperties map(KeyValuePair<uint, ParticipantData> kv)
        {
            var name = _oidType?.GeneratedEnumName(kv.Key);
            var typeName = kv.Value.Types.Count switch
            {
                0 => "???",
                1 => kv.Value.Types[0].ToString(),
                _ => "mixed!"
            };
            // for global, highlight by targetable; for encounter, highlight by being defined in enum
            var highlight = _oidType != null ? name == null : !kv.Value.SeenTargetable;
            var firstName = kv.Value.Names.Count > 0 ? kv.Value.Names[0].name : null;
            return new($"{kv.Key:X} ({_oidType?.GeneratedEnumName(kv.Key)}) '{firstName}' ({typeName})", false, highlight ? Colors.TextColor2 : Colors.TextColor1);
        }
        foreach (var (oid, data) in tree.Nodes(_data, map, kv => DrawSubContextMenu(kv.Key, kv.Value)))
        {
            foreach (var n in tree.Node($"Types ({data.Types.Count})", data.Types.Count == 0))
            {
                tree.LeafNodes(data.Types, t => t.ToString());
            }

            foreach (var n in tree.Node($"Zones ({data.Zones.Count})", data.Zones.Count == 0))
            {
                tree.LeafNodes(data.Zones, z => $"{z.zoneId} '{Service.LuminaRow<TerritoryType>(z.zoneId)?.PlaceName.ValueNullable?.Name}' (cfc={z.cfcId})");
            }

            foreach (var n in tree.Node($"Names ({data.Names.Count})", data.Names.Count == 0))
            {
                tree.LeafNodes(data.Names, n => $"[{n.id}] {n.name}");
            }

            tree.LeafNode($"Spawned pre fight: {string.Join(", ", data.SpawnedPreFight)}");
            tree.LeafNode($"Spawned mid fight: {data.SpawnedMidFight}");
            tree.LeafNode($"Radius: {RadiusString(data)}");
            tree.LeafNode($"Seen targetable: {data.SeenTargetable}");
        }
    }

    public void DrawContextMenu()
    {
        if (ImGui.MenuItem("Generate enum for boss module"))
        {
            ImGui.SetClipboardText(AddOIDEnum(new()).ToString());
        }

        if (ImGui.MenuItem("Generate missing enum values for boss module"))
        {
            var rawMembers = new List<(string, string)>();
            foreach (var d in _data)
            {
                if (_oidType?.GeneratedEnumName(d.Key) == null)
                {
                    rawMembers.Add(EnumMemberString(d.Key, d.Value));
                }
            }
            var sb = new StringBuilder();
            foreach (var (name, val) in Utils.DedupKeys(rawMembers))
            {
                sb.AppendLine($"{name} = {val}");
            }
            ImGui.SetClipboardText(sb.ToString());
        }
    }

    private void FinishBuild()
    {
        List<uint> toDel = [];
        foreach (var (curOID, data) in _data)
        {
            if (data.Types.Count == 0)
            {
                toDel.Add(curOID);
            }
            else
            {
                data.Types.SortAndRemoveDuplicates();
                data.Zones.SortAndRemoveDuplicates();
                data.Names.SortAndRemoveDuplicates();
                data.SpawnedPreFight.SortAndRemoveDuplicates();
            }
        }
        foreach (var curOID in toDel)
        {
            _data.Remove(curOID);
        }
    }

    private void DrawSubContextMenu(uint oid, ParticipantData data)
    {
        if (ImGui.MenuItem("Generate module stub (trivial states)"))
        {
            ImGui.SetClipboardText(AddBossModuleStub(new(), oid, data, false).ToString());
        }
        if (ImGui.MenuItem("Generate module stub (with state machine)"))
        {
            ImGui.SetClipboardText(AddBossModuleStub(new(), oid, data, true).ToString());
        }
    }

    private static bool IsIgnored(Replay.Participant p) => p.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.Area or ActorType.Treasure or ActorType.Buddy;
    private string RadiusString(ParticipantData d) => d.MinRadius != d.MaxRadius ? string.Create(CultureInfo.InvariantCulture, $"{d.MinRadius:f3}-{d.MaxRadius:f3}") : string.Create(CultureInfo.InvariantCulture, $"{d.MinRadius:f3}");
    private string GuessName(uint oid, ParticipantData d) => Utils.StringToIdentifier(d.Names.Count > 0 ? d.Names[0].name : $"Actor{oid:X}");

    private (string Name, string Value) EnumMemberString(uint oid, ParticipantData data, string? forcedName = null)
    {
        var enumName = forcedName ?? _oidType?.GeneratedEnumName(oid) ?? ("_Gen_" + GuessName(oid, data));
        var spawnStr = data.SpawnedPreFight.Count switch
        {
            0 => "?",
            1 => data.SpawnedPreFight[0].ToString(),
            _ => $"{data.SpawnedPreFight[0]}-{data.SpawnedPreFight[^1]}",
        };
        if (data.SpawnedMidFight)
        {
            spawnStr += " (spawn during fight)";
        }

        var typeStr = data.Types.Count switch
        {
            0 => ", ??? type",
            1 => data.Types[0] == ActorType.Enemy ? "" : $", {data.Types[0]} type",
            _ => ", mixed types"
        };
        return (enumName, $"0x{oid:X}, // R{RadiusString(data)}, x{spawnStr}{typeStr}");
    }

    private StringBuilder AddOIDEnum(StringBuilder sb, uint forcedBossOID = 0)
    {
        var members = new List<(string, string)>(_data.Count);
        foreach (var d in _data)
        {
            members.Add(EnumMemberString(d.Key, d.Value, d.Key == forcedBossOID ? "Boss" : null));
        }

        sb.AppendLine("public enum OID : uint");
        sb.AppendLine("{");
        foreach (var (key, val) in Utils.DedupKeys(members))
        {
            sb.AppendLine($"    {key} = {val}");
        }

        sb.AppendLine("}");
        return sb;
    }

    private StringBuilder AddBossModuleStub(StringBuilder sb, uint oid, ParticipantData data, bool withStates)
    {
        var name = GuessName(oid, data);
        sb.AppendLine("public enum OID : uint");
        sb.AppendLine("{");
        sb.AppendLine($"    {name} = 0x{oid:X},");
        sb.AppendLine($"    Helper = 0x233C,");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"sealed class {name}States : StateMachineBuilder");
        sb.AppendLine("{");
        sb.AppendLine($"    public {name}States(BossModule module) : base(module)");
        sb.AppendLine("    {");
        if (withStates)
        {
            sb.AppendLine($"        DeathPhase(default, SinglePhase);");
        }
        else
        {
            sb.AppendLine($"        TrivialPhase();");
        }

        sb.AppendLine("    }");
        if (withStates)
        {
            sb.AppendLine();
            sb.AppendLine("    private void SinglePhase(uint id)");
            sb.AppendLine("    {");
            sb.AppendLine("        SimpleState(id + 0xFF0000u, 10000f, \"???\");");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    //private void XXX(uint id, float delay)");
        }
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.{name}, Contributors = \"\",");
        sb.AppendLine("Category = BossModuleInfo.Category.Placeholder, GroupType = BossModuleInfo.GroupType.CFC,");
        sb.AppendLine($"GroupID = {(data.Zones.Count != 0 ? data.Zones[0].cfcId : default)}u, NameID = {(data.Names.Count != 0 ? data.Names[0].id : default)}u, SortOrder = 1)]");
        sb.AppendLine($"public sealed class {name}(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));");
        return sb;
    }
}
