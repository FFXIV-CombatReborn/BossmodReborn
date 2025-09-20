namespace BossMod.Components;

// generic 'twister' component: a set of aoes that appear under players, but can't be accurately predicted until it's too late
// normally you'd predict them at the end (or slightly before the end) of some cast, or on component creation
[SkipLocalsInit]
public class GenericTwister(BossModule module, float radius, uint oid, uint aid = default) : GenericAOEs(module, aid, "GTFO from twister!")
{
    private readonly AOEShapeCircle _shape = new(radius);
    private readonly uint _twisterOID = oid;
    protected readonly List<Actor> Twisters = module.Enemies(oid);
    protected DateTime PredictedActivation;
    protected readonly List<WPos> PredictedPositions = [];

    public ReadOnlySpan<Actor> ActiveTwisters
    {
        get
        {
            var count = Twisters.Count;
            var result = new Actor[count];
            var index = 0;

            for (var i = 0; i < count; ++i)
            {
                var twister = Twisters[i];
                if (twister.EventState != 7)
                    result[index++] = twister;
            }
            return result.AsSpan(0, index);
        }
    }

    public bool Active => ActiveTwisters.Length != 0;

    public void AddPredicted(float activationDelay)
    {
        PredictedPositions.Clear();
        PredictedPositions.AddRange(Raid.WithoutSlot().Select(a => a.Position));
        PredictedActivation = WorldState.FutureTime(activationDelay);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countPredictedPositions = PredictedPositions.Count;
        var active = ActiveTwisters;
        var lenActiveTwisters = active.Length;
        var predictedSpan = CollectionsMarshal.AsSpan(PredictedPositions);

        var count = countPredictedPositions + lenActiveTwisters;
        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        var index = 0;

        for (var i = 0; i < countPredictedPositions; ++i)
            aoes[index++] = new AOEInstance(_shape, predictedSpan[i], default, PredictedActivation);
        for (var i = 0; i < lenActiveTwisters; ++i)
            aoes[index++] = new AOEInstance(_shape, active[i].Position);

        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == _twisterOID)
            PredictedPositions.Clear();
    }
}

// twister that activates immediately on init
[SkipLocalsInit]
public class ImmediateTwister : GenericTwister
{
    public ImmediateTwister(BossModule module, float radius, uint oid, float activationDelay) : base(module, radius, oid)
    {
        AddPredicted(activationDelay);
    }
}

// twister that activates on cast end, or slightly before
[SkipLocalsInit]
public class CastTwister(BossModule module, float radius, uint oid, uint aid, float activationDelay, float predictBeforeCastEnd = 0) : GenericTwister(module, radius, oid, aid)
{
    private readonly float _activationDelay = activationDelay; // from cast-end to twister spawn
    private readonly float _predictBeforeCastEnd = predictBeforeCastEnd;
    private DateTime _predictStart = DateTime.MaxValue;

    public override void Update()
    {
        if (PredictedPositions.Count == 0 && Twisters.Count == 0 && WorldState.CurrentTime >= _predictStart)
        {
            AddPredicted(_predictBeforeCastEnd + _activationDelay);
            _predictStart = DateTime.MaxValue;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction && _predictStart == DateTime.MaxValue)
        {
            _predictStart = Module.CastFinishAt(spell, -_predictBeforeCastEnd);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction && _predictStart < DateTime.MaxValue)
        {
            // cast finished earlier than expected, just activate things now
            AddPredicted(_activationDelay);
            _predictStart = DateTime.MaxValue;
        }
    }
}
