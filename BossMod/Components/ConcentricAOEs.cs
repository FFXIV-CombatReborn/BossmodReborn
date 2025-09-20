namespace BossMod.Components;

// generic 'concentric aoes' component - a sequence of aoes (typically cone then donuts) with same origin and increasing size
[SkipLocalsInit]
public class ConcentricAOEs(BossModule module, AOEShape[] shapes, bool showall = false, double riskyWithSecondsLeft = default) : GenericAOEs(module)
{
    public struct Sequence
    {
        public WPos Origin;
        public Angle Rotation;
        public DateTime NextActivation;
        public int NumCastsDone;
    }

    public readonly double RiskyWithSecondsLeft = riskyWithSecondsLeft; // can be used to delay risky status of AOEs, so AI waits longer to dodge, if 0 it will just use the bool Risky

    public readonly AOEShape[] Shapes = shapes;
    public readonly List<Sequence> Sequences = [];
    protected readonly List<AOEInstance> _aoes = [];
    protected int lastVersion, lastCount;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        var count = Sequences.Count;
        if (lastCount != count || lastVersion != NumCasts)
        {
            lastCount = count;
            lastVersion = NumCasts;
            _aoes.Clear();
            if (count == 0)
            {
                return;
            }
            var time = WorldState.CurrentTime;
            var sequences = CollectionsMarshal.AsSpan(Sequences);

            for (var i = 0; i < count; ++i)
            {
                ref var s = ref sequences[i];
                var risky = true;
                var act = s.NextActivation;
                if (RiskyWithSecondsLeft != default)
                {
                    risky = act.AddSeconds(-RiskyWithSecondsLeft) <= time;
                }
                if (!showall)
                {
                    _aoes.Add(new(Shapes[s.NumCastsDone], s.Origin, s.Rotation, act, risky: risky));
                }
                else
                {
                    var len = Shapes.Length;
                    for (var j = s.NumCastsDone; j < len; ++j)
                    {
                        _aoes.Add(new(Shapes[j], s.Origin, s.Rotation, act, risky: risky));
                    }
                }
            }
        }
    }

    public void AddSequence(WPos origin, DateTime activation = default, Angle rotation = default) => Sequences.Add(new() { Origin = origin, Rotation = rotation, NextActivation = activation });

    // return false if sequence was not found
    public bool AdvanceSequence(int order, WPos origin, DateTime activation = default, Angle rotation = default)
    {
        if (order < 0)
        {
            return true; // allow passing negative as a no-op
        }

        ++NumCasts;

        var sequences = CollectionsMarshal.AsSpan(Sequences);
        var len = sequences.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var s = ref sequences[i];
            if (s.NumCastsDone == order && s.Origin.AlmostEqual(origin, 1f) && s.Rotation.AlmostEqual(rotation, 0.05f))
            {
                ++s.NumCastsDone;
                s.NextActivation = activation;
                if (s.NumCastsDone == Shapes.Length)
                {
                    Sequences.RemoveAt(i);
                }
                return true;
            }
        }
        return false;
    }
}
