namespace BossMod.Dawntrail.Foray.CriticalEngagement;

// Cast packets in accelerated/replayed encounters can be duplicated or resynchronized. This
// component keys warnings by action + caster and keeps short tombstones for already resolved casts,
// so a stale cast-start packet cannot resurrect an AOE from the previous wave.
abstract class ReplayValidatedCastAOEs(BossModule module) : Components.GenericAOEs(module)
{
    protected readonly record struct AOEConfig(AOEShape Shape, bool LocationTargeted = false);

    private const double EventResolveTolerance = 0.5d;
    private const double CastMatchTolerance = 0.75d;
    private const double TombstoneWindow = 1d;
    private const double EventDedupWindow = 2d;
    private const double ExpireDelay = 2d;

    private sealed class PendingAOE(uint actionID, AOEInstance aoe)
    {
        public readonly uint ActionID = actionID;
        public AOEInstance AOE = aoe;
    }

    private readonly record struct ResolvedCast(uint ActionID, ulong ActorID, DateTime Activation, DateTime ExpiresAt);
    private readonly record struct EventKey(uint GlobalSequence, uint ActionID, ulong ActorID);

    private readonly List<PendingAOE> _pending = [with(16)];
    private readonly List<AOEInstance> _displayed = [with(16)];
    private readonly List<ResolvedCast> _resolved = [with(16)];
    private readonly Dictionary<EventKey, DateTime> _seenEvents = [];

    protected abstract AOEConfig? ConfigFor(uint actionID);
    protected virtual int MaxDisplayed => int.MaxValue;
    protected virtual int MaxRisky => int.MaxValue;
    protected virtual double RiskyActivationWindow => double.PositiveInfinity;
    // Some mechanics split one timeline across several components. Let a component contribute an
    // earlier activation so later previews stay visible without becoming forbidden too soon.
    protected virtual DateTime? CompetingActivation => null;

    public DateTime? EarliestActivation
    {
        get
        {
            PruneExpired();
            return _pending.Count > 0 ? _pending[0].AOE.Activation : null;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        PruneExpired();
        _displayed.Clear();
        var count = Math.Min(_pending.Count, MaxDisplayed);
        var useRiskLimit = MaxRisky != int.MaxValue || !double.IsPositiveInfinity(RiskyActivationWindow);
        var riskReference = count > 0 ? _pending[0].AOE.Activation : DateTime.MaxValue;
        if (count > 0 && CompetingActivation is { } competing && competing < riskReference)
            riskReference = competing;
        var riskyDeadline = count > 0 && !double.IsPositiveInfinity(RiskyActivationWindow)
            ? riskReference.AddSeconds(RiskyActivationWindow)
            : DateTime.MaxValue;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _pending[i].AOE;
            if (useRiskLimit)
            {
                var imminent = i < MaxRisky && aoe.Activation <= riskyDeadline;
                aoe.Color = imminent ? Colors.Danger : Colors.AOE;
                aoe.Risky = imminent;
            }
            _displayed.Add(aoe);
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (ConfigFor(spell.Action.ID) is not { } config)
        {
            return;
        }

        PruneExpired();
        var activation = Module.CastFinishAt(spell);
        if (spell.EventHappened || activation <= WorldState.CurrentTime || WasRecentlyResolved(spell.Action.ID, caster.InstanceID, activation))
        {
            return;
        }

        var origin = config.LocationTargeted ? spell.LocXZ : caster.Position;
        AddOrRefresh(spell.Action.ID, config.Shape, caster.InstanceID, origin, spell.Rotation, activation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (ConfigFor(spell.Action.ID) == null)
        {
            return;
        }

        var now = WorldState.CurrentTime;
        var activation = Module.CastFinishAt(spell);
        RemoveMatchingCast(spell.Action.ID, caster.InstanceID, activation);
        if (spell.EventHappened || activation <= now.AddSeconds(EventResolveTolerance))
        {
            RememberResolved(spell.Action.ID, caster.InstanceID, activation, now);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (ConfigFor(spell.Action.ID) == null)
        {
            return;
        }

        var now = WorldState.CurrentTime;
        PruneExpired();
        if (IsDuplicateEvent(spell.GlobalSequence, spell.Action.ID, caster.InstanceID, now))
        {
            return;
        }

        ++NumCasts;
        var activation = RemoveResolvedByEvent(spell.Action.ID, caster.InstanceID, now) ?? now;
        RememberResolved(spell.Action.ID, caster.InstanceID, activation, now);
    }

    public override void OnActorDeath(Actor actor) => RemoveActor(actor.InstanceID);
    public override void OnActorDestroyed(Actor actor) => RemoveActor(actor.InstanceID);

    private void AddOrRefresh(uint actionID, AOEShape shape, ulong actorID, WPos origin, Angle rotation, DateTime activation)
    {
        var aoe = new AOEInstance(shape, origin, rotation, activation, actorID: actorID, shapeDistance: shape.Distance(origin, rotation));
        var duplicate = _pending.FindIndex(entry => entry.ActionID == actionID
            && entry.AOE.ActorID == actorID
            && Math.Abs((entry.AOE.Activation - activation).TotalSeconds) <= CastMatchTolerance);
        if (duplicate >= 0)
            _pending[duplicate].AOE = aoe;
        else
            _pending.Add(new(actionID, aoe));
        _pending.Sort((left, right) => left.AOE.Activation.CompareTo(right.AOE.Activation));
    }

    private DateTime? RemoveResolvedByEvent(uint actionID, ulong actorID, DateTime now)
    {
        var index = -1;
        for (var i = 0; i < _pending.Count; ++i)
        {
            var entry = _pending[i];
            if (entry.ActionID == actionID && entry.AOE.ActorID == actorID && entry.AOE.Activation <= now.AddSeconds(EventResolveTolerance))
            {
                index = i;
                break;
            }
        }
        if (index < 0)
            return null;

        var activation = _pending[index].AOE.Activation;
        _pending.RemoveAt(index);
        return activation;
    }

    private DateTime? RemoveMatchingCast(uint actionID, ulong actorID, DateTime activation)
    {
        var index = -1;
        var bestDelta = double.MaxValue;
        for (var i = 0; i < _pending.Count; ++i)
        {
            var entry = _pending[i];
            if (entry.ActionID != actionID || entry.AOE.ActorID != actorID)
                continue;

            var delta = Math.Abs((entry.AOE.Activation - activation).TotalSeconds);
            if (delta <= CastMatchTolerance && delta < bestDelta)
            {
                index = i;
                bestDelta = delta;
            }
        }
        if (index < 0)
            return null;

        var matchedActivation = _pending[index].AOE.Activation;
        _pending.RemoveAt(index);
        return matchedActivation;
    }

    private bool WasRecentlyResolved(uint actionID, ulong actorID, DateTime activation)
        => _resolved.Any(resolved => resolved.ActionID == actionID && resolved.ActorID == actorID && Math.Abs((resolved.Activation - activation).TotalSeconds) <= TombstoneWindow);

    private void RememberResolved(uint actionID, ulong actorID, DateTime activation, DateTime now)
    {
        _resolved.RemoveAll(resolved => resolved.ActionID == actionID
            && resolved.ActorID == actorID
            && Math.Abs((resolved.Activation - activation).TotalSeconds) <= CastMatchTolerance);
        _resolved.Add(new(actionID, actorID, activation, now.AddSeconds(TombstoneWindow)));
    }

    private bool IsDuplicateEvent(uint globalSequence, uint actionID, ulong actorID, DateTime now)
    {
        if (globalSequence == 0)
            return false;

        var key = new EventKey(globalSequence, actionID, actorID);
        if (_seenEvents.TryGetValue(key, out var expiresAt) && now <= expiresAt)
            return true;

        _seenEvents[key] = now.AddSeconds(EventDedupWindow);
        return false;
    }

    private void PruneExpired()
    {
        var now = WorldState.CurrentTime;
        _pending.RemoveAll(entry => now > entry.AOE.Activation.AddSeconds(ExpireDelay));
        _resolved.RemoveAll(resolved => now > resolved.ExpiresAt);
        foreach (var key in _seenEvents.Where(entry => now > entry.Value).Select(entry => entry.Key).ToArray())
            _seenEvents.Remove(key);
    }

    private void RemoveActor(ulong actorID)
    {
        _pending.RemoveAll(entry => entry.AOE.ActorID == actorID);
        _resolved.RemoveAll(entry => entry.ActorID == actorID);
        foreach (var key in _seenEvents.Keys.Where(key => key.ActorID == actorID).ToArray())
            _seenEvents.Remove(key);
    }
}

// Two-step front/back attacks only expose a cast bar for the first hit. Keep both hits as one
// sequence and show just the imminent half; drawing both at once would incorrectly mark the
// entire arena unsafe. This also survives CastInfo re-sync packets from accelerated replays.
abstract class ReplayValidatedOppositeAOEs(BossModule module) : Components.GenericAOEs(module)
{
    protected readonly record struct SequenceConfig(AOEShape FirstShape, AOEShape SecondShape, uint SecondActionID, double SecondDelay, Angle FirstRotationOffset = default);

    private sealed class Sequence(uint firstActionID, uint secondActionID, ulong actorID, AOEInstance first, AOEInstance second)
    {
        public readonly uint FirstActionID = firstActionID;
        public readonly uint SecondActionID = secondActionID;
        public readonly ulong ActorID = actorID;
        public readonly AOEInstance First = first;
        public readonly AOEInstance Second = second;
        public bool FirstResolved;
    }

    private const double ExpireDelay = 2d;
    private const double CastMatchTolerance = 0.75d;
    private const double EventResolveTolerance = 0.5d;
    private const double EventDedupWindow = 2d;
    private readonly record struct EventKey(uint GlobalSequence, uint ActionID, ulong ActorID);
    private readonly List<Sequence> _sequences = [with(4)];
    private readonly List<AOEInstance> _displayed = [with(4)];
    private readonly Dictionary<EventKey, DateTime> _seenEvents = [];

    protected abstract SequenceConfig? ConfigFor(uint firstActionID);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        PruneExpired();
        _displayed.Clear();
        foreach (var sequence in _sequences.OrderBy(sequence => sequence.FirstResolved ? sequence.Second.Activation : sequence.First.Activation))
        {
            _displayed.Add(sequence.FirstResolved ? sequence.Second : sequence.First);
            break;
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (ConfigFor(spell.Action.ID) is not { } config || spell.EventHappened)
            return;

        var activation = Module.CastFinishAt(spell);
        if (activation <= WorldState.CurrentTime)
            return;

        if (_sequences.Any(sequence => sequence.ActorID == caster.InstanceID
            && sequence.FirstActionID == spell.Action.ID
            && Math.Abs((sequence.First.Activation - activation).TotalSeconds) <= CastMatchTolerance))
            return;

        var firstRotation = spell.Rotation + config.FirstRotationOffset;
        var secondRotation = firstRotation + 180f.Degrees();
        var secondActivation = activation.AddSeconds(config.SecondDelay);
        _sequences.Add(new(spell.Action.ID, config.SecondActionID, caster.InstanceID,
            new(config.FirstShape, caster.Position, firstRotation, activation, actorID: caster.InstanceID, shapeDistance: config.FirstShape.Distance(caster.Position, firstRotation)),
            new(config.SecondShape, caster.Position, secondRotation, secondActivation, actorID: caster.InstanceID, shapeDistance: config.SecondShape.Distance(caster.Position, secondRotation))));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var now = WorldState.CurrentTime;
        PruneExpired();
        if (IsDuplicateEvent(spell.GlobalSequence, spell.Action.ID, caster.InstanceID, now))
            return;

        var firstIndex = -1;
        var secondIndex = -1;
        for (var i = 0; i < _sequences.Count; ++i)
        {
            var sequence = _sequences[i];
            if (sequence.ActorID != caster.InstanceID)
                continue;

            if (firstIndex < 0 && spell.Action.ID == sequence.FirstActionID && !sequence.FirstResolved
                && sequence.First.Activation <= now.AddSeconds(EventResolveTolerance))
                firstIndex = i;
            if (secondIndex < 0 && spell.Action.ID == sequence.SecondActionID
                && sequence.Second.Activation <= now.AddSeconds(EventResolveTolerance))
                secondIndex = i;
        }

        if (firstIndex >= 0)
            _sequences[firstIndex].FirstResolved = true;
        else if (secondIndex >= 0)
            _sequences.RemoveAt(secondIndex);
        else
            return;

        ++NumCasts;
    }

    public override void OnActorDeath(Actor actor) => RemoveActor(actor.InstanceID);
    public override void OnActorDestroyed(Actor actor) => RemoveActor(actor.InstanceID);

    private void PruneExpired()
    {
        var now = WorldState.CurrentTime;
        foreach (var sequence in _sequences)
            if (!sequence.FirstResolved && now > sequence.First.Activation.AddSeconds(0.5d))
                sequence.FirstResolved = true;
        _sequences.RemoveAll(sequence => now > sequence.Second.Activation.AddSeconds(ExpireDelay));
        foreach (var key in _seenEvents.Where(entry => now > entry.Value).Select(entry => entry.Key).ToArray())
            _seenEvents.Remove(key);
    }

    private bool IsDuplicateEvent(uint globalSequence, uint actionID, ulong actorID, DateTime now)
    {
        if (globalSequence == 0)
            return false;

        var key = new EventKey(globalSequence, actionID, actorID);
        if (_seenEvents.TryGetValue(key, out var expiresAt) && now <= expiresAt)
            return true;

        _seenEvents[key] = now.AddSeconds(EventDedupWindow);
        return false;
    }

    private void RemoveActor(ulong actorID)
    {
        _sequences.RemoveAll(sequence => sequence.ActorID == actorID);
        foreach (var key in _seenEvents.Keys.Where(key => key.ActorID == actorID).ToArray())
            _seenEvents.Remove(key);
    }
}
