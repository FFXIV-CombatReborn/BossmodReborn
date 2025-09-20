namespace BossMod.Components;

// generic 'exaflare' component - these mechanics are a bunch of moving aoes, with different lines either staggered or moving with different speed
[SkipLocalsInit]
public class Exaflare(BossModule module, AOEShape shape, uint aid = default) : GenericAOEs(module, aid, "GTFO from exaflare!")
{
    public sealed class Line(WPos next, WDir advance, DateTime nextExplosion, double timeToMove, int explosionsLeft, int maxShownExplosions, Angle rotation = default)
    {
        public WPos Next = next;
        public WDir Advance = advance;
        public DateTime NextExplosion = nextExplosion;
        public double TimeToMove = timeToMove;
        public int ExplosionsLeft = explosionsLeft;
        public int MaxShownExplosions = maxShownExplosions;
        public Angle Rotation = rotation;
    }

    public readonly AOEShape Shape = shape;
    public uint ImminentColor = Colors.Danger;
    public uint FutureColor = Colors.AOE;
    public readonly List<Line> Lines = [];
    protected AOEInstance[] _aoes = [];
    protected int currentVersion, lastVersion, lastCount;

    public bool Active => Lines.Count != 0;

    public Exaflare(BossModule module, float radius, uint aid = default) : this(module, new AOEShapeCircle(radius), aid) { }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void Update()
    {
        var linesCount = Lines.Count;
        if (lastCount != linesCount || currentVersion != lastVersion)
        {
            var futureAOEs = CollectionsMarshal.AsSpan(FutureAOEs(linesCount));
            var imminentAOEs = ImminentAOEs(linesCount);
            var futureLen = futureAOEs.Length;
            var imminentLen = imminentAOEs.Length;

            _aoes = new AOEInstance[futureLen + imminentLen];
            for (var i = 0; i < futureLen; ++i)
            {
                ref var aoe = ref futureAOEs[i];
                _aoes[i] = new(Shape, aoe.Item1, aoe.Item3, aoe.Item2, FutureColor);
            }

            for (var i = 0; i < imminentLen; ++i)
            {
                ref var aoe = ref imminentAOEs[i];
                _aoes[futureLen + i] = new(Shape, aoe.Item1, aoe.Item3, aoe.Item2, ImminentColor);
            }
            lastCount = linesCount;
            lastVersion = currentVersion;
        }
    }

    protected (WPos, DateTime, Angle)[] ImminentAOEs(int count)
    {
        var exas = new (WPos, DateTime, Angle)[count];
        for (var i = 0; i < count; ++i)
        {
            var l = Lines[i];
            if (l.ExplosionsLeft != 0)
            {
                exas[i] = (l.Next.Quantized(), l.NextExplosion, l.Rotation);
            }
        }
        return exas;
    }

    protected List<(WPos, DateTime, Angle)> FutureAOEs(int count)
    {
        var exas = new List<(WPos, DateTime, Angle)>(count);
        var currentTime = WorldState.CurrentTime;
        for (var i = 0; i < count; ++i)
        {
            var l = Lines[i];
            var num = Math.Min(l.ExplosionsLeft, l.MaxShownExplosions);
            var pos = l.Next;
            var time = l.NextExplosion > currentTime ? l.NextExplosion : currentTime;
            for (var j = 1; j < num; ++j)
            {
                pos += l.Advance;
                time = time.AddSeconds(l.TimeToMove);
                exas.Add((pos.Quantized(), time, l.Rotation));
            }
        }
        return exas;
    }

    protected void AdvanceLine(Line l, WPos pos)
    {
        l.Next = pos + l.Advance;
        l.NextExplosion = WorldState.FutureTime(l.TimeToMove);
        --l.ExplosionsLeft;
        ++currentVersion;
    }
}

[SkipLocalsInit]
public class SimpleExaflare(BossModule module, AOEShape shape, uint aidFirst, uint aidRest, float distance, double timeToMove, int explosionsLeft, int maxShownExplosions, bool castEvent = false,
bool locationBased = false, Angle rotation = default) : Exaflare(module, shape)
{
    private readonly uint AIDFirst = aidFirst;
    private readonly uint AIDRest = aidRest;
    private readonly float Distance = distance;
    private readonly double TimeToMove = timeToMove;
    private readonly int ExplosionsLeft = explosionsLeft;
    private readonly int MaxShownExplosions = maxShownExplosions;
    private readonly bool CastEvent = castEvent; // if exaflare gets advanced by castevent instead of castfinished
    private readonly bool LocationBased = locationBased; // if cast is location based
    private readonly Angle Rotation = rotation;
    public int NumLinesFinished;

    public SimpleExaflare(BossModule module, float radius, uint aidFirst, uint aidRest, float distance, double timeToMove, int explosionsLeft, int maxShownExplosions, bool castEvent = false, bool locationBased = false)
    : this(module, new AOEShapeCircle(radius), aidFirst, aidRest, distance, timeToMove, explosionsLeft, maxShownExplosions, castEvent, locationBased) { }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == AIDFirst)
        {
            Lines.Add(new(LocationBased ? spell.LocXZ : caster.Position, Distance * caster.Rotation.ToDirection(), Module.CastFinishAt(spell), TimeToMove, ExplosionsLeft, MaxShownExplosions, Rotation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (!CastEvent && spell.Action.ID is var id && (id == AIDFirst || id == AIDRest))
        {
            var count = Lines.Count;
            var pos = spell.LocXZ;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                        ++NumLinesFinished;
                    }
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CastEvent && spell.Action.ID is var id && (id == AIDFirst || id == AIDRest))
        {
            var count = Lines.Count;
            var pos = LocationBased ? spell.TargetXZ : caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                        ++NumLinesFinished;
                    }
                    return;
                }
            }
        }
    }
}
