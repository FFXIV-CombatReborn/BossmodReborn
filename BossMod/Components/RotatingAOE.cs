namespace BossMod.Components;

// generic 'rotating aoes' component - a sequence of aoes (typically cones) with same origin and increasing rotation
[SkipLocalsInit]
public class GenericRotatingAOE(BossModule module) : GenericAOEs(module)
{
    public struct Sequence(AOEShape shape, WPos origin, Angle rotation, Angle increment, DateTime nextActivation, double secondsBetweenActivations, int numRemainingCasts, int maxShownAOEs = 2, ulong actorID = default)
    {
        public AOEShape Shape = shape;
        public WPos Origin = origin;
        public Angle Rotation = rotation;
        public Angle Increment = increment;
        public DateTime NextActivation = nextActivation;
        public double SecondsBetweenActivations = secondsBetweenActivations;
        public int NumRemainingCasts = numRemainingCasts;
        public int MaxShownAOEs = maxShownAOEs;
        public ulong ActorID = actorID;
    }

    public readonly List<Sequence> Sequences = [];
    public uint ImminentColor = Colors.Danger;
    public uint FutureColor = Colors.AOE;
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

            var curTime = WorldState.CurrentTime;
            var sequences = CollectionsMarshal.AsSpan(Sequences);
            for (var j = 0; j < count; ++j)
            {
                ref readonly var s = ref sequences[j];
                var remaining = s.NumRemainingCasts;
                var num = Math.Min(remaining, s.MaxShownAOEs);
                var rot = s.Rotation;
                var nextAct = s.NextActivation;
                var time = nextAct > curTime ? nextAct : curTime;
                var shape = s.Shape;
                var origin = s.Origin;
                // future AOEs
                if (num > 0)
                {
                    var timeBetween = s.SecondsBetweenActivations;
                    var inc = s.Increment;

                    for (var i = 1; i < num; ++i)
                    {
                        rot += inc;
                        time = time.AddSeconds(timeBetween);
                        _aoes.Add(new(shape, origin, rot, time, FutureColor, shapeDistance: shape.Distance(origin, rot)));
                    }
                }
                // imminent AOEs
                if (remaining != 0)
                {
                    var rot2 = s.Rotation;
                    _aoes.Add(new(shape, origin, rot2, nextAct, remaining > 1 ? ImminentColor : FutureColor, shapeDistance: shape.Distance(origin, rot2)));
                }
            }
        }
    }

    public void AdvanceSequence(int index, DateTime currentTime, bool removeWhenFinished = true)
    {
        ++NumCasts;

        if (index < 0 || index >= Sequences.Count)
            return;

        ref var s = ref Sequences.Ref(index);
        if (--s.NumRemainingCasts <= 0 && removeWhenFinished)
        {
            Sequences.RemoveAt(index);
        }
        else
        {
            s.Rotation += s.Increment;
            s.NextActivation = currentTime.AddSeconds(s.SecondsBetweenActivations);
        }
    }

    // return false if sequence was not found
    public bool AdvanceSequence(WPos origin, Angle rotation, DateTime currentTime, bool removeWhenFinished = true)
    {
        var count = Sequences.Count;
        var sequences = CollectionsMarshal.AsSpan(Sequences);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var s = ref sequences[i];
            if (s.Origin.AlmostEqual(origin, 1f) && s.Rotation.AlmostEqual(rotation, 0.05f))
            {
                AdvanceSequence(i, currentTime, removeWhenFinished);
                return true;
            }
        }
        return false;
    }

    public bool AdvanceSequence(WPos origin, Angle rotation, ulong instanceID, DateTime currentTime, bool removeWhenFinished = true)
    {
        var count = Sequences.Count;
        var sequences = CollectionsMarshal.AsSpan(Sequences);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var s = ref sequences[i];
            if (s.ActorID == instanceID && s.Origin.AlmostEqual(origin, 1f) && s.Rotation.AlmostEqual(rotation, 0.05f))
            {
                AdvanceSequence(i, currentTime, removeWhenFinished);
                return true;
            }
        }
        return false;
    }

    public bool AdvanceSequence(ulong instanceID, DateTime currentTime, bool removeWhenFinished = true)
    {
        var count = Sequences.Count;
        var sequences = CollectionsMarshal.AsSpan(Sequences);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var s = ref sequences[i];
            if (s.ActorID == instanceID)
            {
                AdvanceSequence(i, currentTime, removeWhenFinished);
                return true;
            }
        }
        return false;
    }
}
