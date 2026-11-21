using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayVisualization;

public sealed class ColumnActorStatuses : Timeline.ColumnGroup
{
    private readonly StateMachineTree _tree;
    private readonly List<int> _phaseBranches;
    private readonly Replay _replay;
    private readonly Replay.Encounter _enc;
    private readonly Replay.Participant _target;

    private readonly ColumnSeparator _sep;
    private readonly List<(uint sid, Replay.Participant? source, ColumnGenericHistory? col)> _columns = [];

    public bool Visible => _sep.Width > 0;

    public ColumnActorStatuses(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant actor)
        : base(timeline)
    {
        //Name = "Statuses";
        _tree = tree;
        _phaseBranches = phaseBranches;
        _replay = replay;
        _enc = enc;
        _target = actor;
        _sep = Add(new ColumnSeparator(timeline, width: 0));
        foreach (var s in replay.EncounterStatuses(enc))
        {
            if (s.Target != actor)
            {
                continue;
            }
            var seen = false;
            for (var i = 0; i < _columns.Count; ++i)
            {
                if (_columns[i].sid == s.ID && _columns[i].source == s.Source)
                {
                    seen = true;
                    break;
                }
            }
            if (!seen)
            {
                _columns.Add((s.ID, s.Source, null));
            }
        }
    }

    public void DrawConfig(UITree tree)
    {
        foreach (ref var c in _columns.AsSpan())
        {
            var visible = c.col?.Width > 0;
            var srcTime = c.source != null && c.source.WorldExistence.Count > 0 ? c.source.WorldExistence[0].Start : default;
            if (ImGui.Checkbox($"{Utils.StatusString(c.sid)} from {ReplayUtils.ParticipantString(c.source, srcTime)}", ref visible))
            {
                c.col ??= BuildColumn(c.sid, c.source);
                c.col.Width = visible ? ColumnGenericHistory.DefaultWidth : 0;
            }
        }
        var anyVisible = false;
        for (var i = 0; i < Columns.Count; ++i)
        {
            if (Columns[i] != _sep && Columns[i].Width > 0)
            {
                anyVisible = true;
                break;
            }
        }
        _sep.Width = anyVisible ? 1 : 0;
    }

    private ColumnGenericHistory BuildColumn(uint statusID, Replay.Participant? source)
    {
        var res = AddBefore(new ColumnGenericHistory(Timeline, _tree, _phaseBranches), _sep);
        DateTime prevEnd = default;

        var minTime = _enc.Time.Start.AddSeconds(Timeline.MinTime);
        for (var i = 0; i < _replay.Statuses.Count; ++i)
        {
            var s = _replay.Statuses[i];
            if (s.Time.Start < minTime)
            {
                continue;
            }
            if (s.Time.Start > _enc.Time.End)
            {
                break;
            }
            if (s.ID != statusID || s.Source != source || s.Target != _target)
            {
                continue;
            }
            var e = res.AddHistoryEntryRange(_enc.Time.Start, s.Time, $"{Utils.StatusString(statusID)} ({s.StartingExtra:X}) on {ReplayUtils.ParticipantString(_target, s.Time.Start)} from {ReplayUtils.ParticipantString(s.Source, s.Time.Start)}", 0x80808080);
            e.TooltipExtra = (res, t) =>
            {
                var elapsed = t - (s.Time.Start - _enc.Time.Start).TotalSeconds;
                res.Add($"- remaining: {s.InitialDuration - elapsed:f3} / {s.InitialDuration:f3}");
                res.Add($"- final duration: {s.InitialDuration - s.Time.Duration:f3}");
            };
            if (s.Time.Start == prevEnd)
            {
                res.AddHistoryEntryLine(_enc.Time.Start, prevEnd, "", Colors.TextColor1);
            }

            prevEnd = s.Time.End;
        }
        foreach (var a in _replay.EncounterActions(_enc))
        {
            foreach (var t in a.Targets)
            {
                foreach (var e in t.Effects.ValidEffects())
                {
                    if (e.Type is ActionEffectType.ApplyStatusEffectTarget or ActionEffectType.ApplyStatusEffectSource && e.Value == statusID)
                    {
                        var src = e.FromTarget ? t.Target : a.Source;
                        var tgt = e.AtSource ? a.Source : t.Target;
                        if (src == source && tgt == _target)
                        {
                            var actionName = $"{a.ID} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}";
                            res.AddHistoryEntryDot(_enc.Time.Start, a.Timestamp, actionName, Colors.TextColor1).AddActionTooltip(a);
                        }
                    }
                }
            }
        }
        return res;
    }
}
