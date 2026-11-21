using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayVisualization;

// per-enemy details columns
public sealed class ColumnEnemiesDetails : Timeline.ColumnGroup
{
    class PerOID
    {
        public uint OID;
        public List<(Replay.Participant, ColumnEnemyDetails?)> Columns = [];
    }

    private readonly StateMachineTree _tree;
    private readonly List<int> _phaseBranches;
    private readonly Replay _replay;
    private readonly Replay.Encounter _encounter;
    private readonly List<PerOID> _data = [];

    public ColumnEnemiesDetails(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc)
        : base(timeline)
    {
        _tree = tree;
        _phaseBranches = phaseBranches;
        _replay = replay;
        _encounter = enc;
        foreach (var (oid, participants) in enc.ParticipantsByOID)
        {
            var data = new PerOID() { OID = oid };
            for (var i = 0; i < participants.Count; ++i)
            {
                var p = participants[i];
                if ((p.HasAnyActions || p.Casts.Count > 0) && p.Type is not (ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.Buddy))
                {
                    data.Columns.Add((p, null));
                }
            }

            if (data.Columns.Count > 0)
            {
                _data.Add(data);
            }
        }
    }

    public void DrawConfig(UITree tree)
    {
        var moduleInfo = BossModuleRegistry.FindByOID(_encounter.OID);
        foreach (var n in tree.Nodes(_data, d => new($"{d.OID:X} ({moduleInfo?.ObjectIDType?.GeneratedEnumName(d.OID)})")))
        {
            for (var i = 0; i < n.Columns.Count; ++i)
            {
                var (p, c) = n.Columns[i];
                if (c != null)
                {
                    c.DrawConfig(tree);
                }
                else
                {
                    var startTime = p.WorldExistence.Count > 0 ? p.WorldExistence[0].Start : default;
                    if (ImGui.Button($"Show details for {ReplayUtils.ParticipantString(p, startTime)}"))
                    {
                        n.Columns[i] = (p, Add(new ColumnEnemyDetails(Timeline, _tree, _phaseBranches, _replay, _encounter, p)));
                    }
                }
            }
        }
    }
}
