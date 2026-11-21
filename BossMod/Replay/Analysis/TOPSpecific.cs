using BossMod.Endwalker.Ultimate.TOP;

namespace BossMod.ReplayAnalysis;

sealed class TOPSpecific
{
    struct FlamethrowerData
    {
        public Replay Replay;
        public DateTime Timestamp;
        public Angle Difference;
    }

    private readonly List<FlamethrowerData> _flamethrowers = [];
    private readonly UIPlot _plotFlamethrowers = new();

    public TOPSpecific(List<Replay> replays, uint oid)
    {
        _plotFlamethrowers.DataMin = new(-180, 0);
        _plotFlamethrowers.DataMax = new(180, 1);
        _plotFlamethrowers.TickAdvance = new(5, 1);
        foreach (var replay in replays)
        {
            var aidFlamethrower = ActionID.MakeSpell(AID.FlameThrowerFirst);
            for (var pi = 0; pi < replay.Participants.Count; ++pi)
            {
                var p = replay.Participants[pi];
                for (var ci = 0; ci < p.Casts.Count; ++ci)
                {
                    var cast = p.Casts[ci];
                    if (cast.ID != aidFlamethrower)
                    {
                        continue;
                    }
                    Replay.Participant? boss = null;
                    for (var bi = 0; bi < replay.Participants.Count; ++bi)
                    {
                        var cand = replay.Participants[bi];
                        if ((OID)cand.OID == OID.Boss && cand.ExistsInWorldAt(cast.Time.Start))
                        {
                            boss = cand;
                            break;
                        }
                    }
                    if (boss != null)
                    {
                        _flamethrowers.Add(new() { Replay = replay, Timestamp = cast.Time.Start, Difference = (cast.Rotation - boss.PosRotAt(cast.Time.Start).W.Radians()).Normalized() });
                    }
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        foreach (var _ in tree.Node("Flamethrower rotation offsets"))
        {
            _plotFlamethrowers.Begin();
            foreach (var i in _flamethrowers)
            {
                _plotFlamethrowers.Point(new(i.Difference.Deg, 0.5f), Colors.TextColor2, () => $"{i.Replay.Path} @ {i.Timestamp:O}");
            }

            _plotFlamethrowers.End();
        }
    }
}
