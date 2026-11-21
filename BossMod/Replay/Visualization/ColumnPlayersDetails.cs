using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayVisualization;

public sealed class ColumnPlayersDetails : Timeline.ColumnGroup
{
    private readonly StateMachineTree _tree;
    private readonly List<int> _phaseBranches;
    private readonly Replay _replay;
    private readonly Replay.Encounter _encounter;
    private readonly PlanDatabase _planDB;
    private readonly ColumnPlayerDetails?[] _columns;

    public bool AnyPlanModified
    {
        get
        {
            for (var i = 0; i < _columns.Length; ++i)
            {
                if (_columns[i]?.PlanModified ?? false)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public ColumnPlayersDetails(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, BitMask showPlayers, PlanDatabase planDB)
        : base(timeline)
    {
        _tree = tree;
        _phaseBranches = phaseBranches;
        _replay = replay;
        _encounter = enc;
        _planDB = planDB;
        _columns = new ColumnPlayerDetails[enc.PartyMembers.Count];
        foreach (var i in showPlayers.SetBits())
        {
            var (p, c, _) = enc.PartyMembers[i];
            _columns[i] = Add(new ColumnPlayerDetails(Timeline, _tree, _phaseBranches, _replay, _encounter, p, c, planDB));
        }
    }

    public void DrawConfig(UITree tree)
    {
        for (var i = 0; i < _columns.Length; ++i)
        {
            var (p, c, l) = _encounter.PartyMembers[i];
            var startTime = p.WorldExistence.Count > 0 ? p.WorldExistence[0].Start : default;
            foreach (var n in tree.Node($"{c} {ReplayUtils.ParticipantString(p, startTime)}"))
            {
                var col = _columns[i];
                if (col != null)
                {
                    col.DrawConfig(tree);
                }
                else if (ImGui.Button("Show details..."))
                {
                    _columns[i] = Add(new ColumnPlayerDetails(Timeline, _tree, _phaseBranches, _replay, _encounter, p, c, _planDB));
                }
            }
        }
    }

    public void SaveAll()
    {
        foreach (var c in _columns)
        {
            c?.SaveChanges();
        }
    }
}
