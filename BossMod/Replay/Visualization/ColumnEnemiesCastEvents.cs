using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayVisualization;

// a set of columns containing cast events (typically by non-players)
// by default contains a single column showing all actions from all sources, but extra columns can be added and per-column filters can be assigned
public sealed class ColumnEnemiesCastEvents : Timeline.ColumnGroup
{
    private readonly StateMachineTree _tree;
    private readonly List<int> _phaseBranches;
    private readonly Replay.Encounter _encounter;
    private readonly BossModuleRegistry.Info? _moduleInfo;
    private readonly List<Replay.Action> _actions;
    private readonly Dictionary<ActionID, List<(Replay.Participant source, BitMask cols)>> _filters = []; // [action][sourceid]

    public ColumnEnemiesCastEvents(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc)
        : base(timeline)
    {
        //Name = "Enemy cast events";
        _tree = tree;
        _phaseBranches = phaseBranches;
        _encounter = enc;
        _moduleInfo = BossModuleRegistry.FindByOID(enc.OID);
        _actions = [];
        foreach (var a in replay.EncounterActions(enc))
        {
            if (a.Source.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.Buddy)
            {
                continue;
            }
            _actions.Add(a);
            var f = _filters.GetOrAdd(a.ID);
            var found = false;
            for (var i = 0; i < f.Count; ++i)
            {
                if (f[i].source == a.Source)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                f.Add((a.Source, new(1)));
            }
        }
        AddColumn();
        RebuildEvents();
    }

    public void DrawConfig(UITree tree)
    {
        if (ImGui.Button("Add new!"))
        {
            AddColumn();
        }

        var needRebuild = false;
        foreach (var na in tree.Nodes(_filters, kv => new($"{kv.Key} ({_moduleInfo?.ActionIDType?.GeneratedEnumName(kv.Key.ID)})")))
        {
            foreach (ref var v in na.Value.AsSpan())
            {
                var startTime = v.source.WorldExistence.Count > 0 ? v.source.WorldExistence[0].Start : default;
                needRebuild |= DrawConfigColumns(ref v.cols, $"{ReplayUtils.ParticipantString(v.source, startTime)} ({_moduleInfo?.ObjectIDType?.GeneratedEnumName(v.source.OID)})");
            }
        }

        if (needRebuild)
        {
            RebuildEvents();
        }
    }

    private void AddColumn() => Add(new ColumnGenericHistory(Timeline, _tree, _phaseBranches));

    private bool DrawConfigColumns(ref BitMask mask, string name)
    {
        var changed = false;
        for (var c = 0; c < Columns.Count; ++c)
        {
            var set = mask[c];
            if (ImGui.Checkbox($"###{name}/{c}", ref set))
            {
                mask[c] = set;
                changed = true;
            }
            ImGui.SameLine();
        }
        ImGui.TextUnformatted(name);
        return changed;
    }

    private void RebuildEvents()
    {
        for (var i = 0; i < Columns.Count; ++i)
        {
            ((ColumnGenericHistory)Columns[i]).Entries.Clear();
        }

        foreach (var a in _actions)
        {
            var cols = _filters[a.ID].Find(c => c.source == a.Source).cols;
            if (cols.None())
            {
                continue;
            }

            var name = $"{a.ID} ({_moduleInfo?.ActionIDType?.GeneratedEnumName(a.ID.ID)}) {ReplayUtils.ParticipantString(a.Source, a.Timestamp)} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}";
            var color = EventColor(a);
            foreach (var c in cols.SetBits())
            {
                var col = (ColumnGenericHistory)Columns[c];
                col.AddHistoryEntryDot(_encounter.Time.Start, a.Timestamp, name, color).AddActionTooltip(a);
            }
        }
    }

    private uint EventColor(Replay.Action action)
    {
        var phys = false;
        var magic = false;
        var targets = action.Targets;
        var count = targets.Count;
        for (var i = 0; i < count; ++i)
        {
            var t = targets[i];
            if (t.Target.Type is ActorType.Player or ActorType.Buddy)
            {
                var effects = t.Effects.ValidEffects();
                var len = effects.Length;
                for (var j = 0; j < len; ++j)
                {
                    ref readonly var e = ref effects[j];
                    if (e.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage)
                    {
                        switch (e.DamageType)
                        {
                            case DamageType.Slashing:
                            case DamageType.Piercing:
                            case DamageType.Blunt:
                            case DamageType.Shot:
                                phys = true;
                                break;
                            case DamageType.Magic:
                                magic = true;
                                break;
                            default:
                                phys = magic = true; // TODO: reconsider
                                break;
                        }
                    }
                }
            }
        }
        return phys ? (magic ? Colors.TextColor1 : Colors.TextColor5) : (magic ? Colors.TextColor6 : Colors.TextColor7);
    }
}
