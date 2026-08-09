using BossMod.Dawntrail.Foray.CriticalEngagement;
using static BossMod.Components.GenericKnockback;

namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE211DoubledTrouble;

public enum OID : uint
{
    Boss = 0x4BB8, // R5.5, BNpcName 14517, Calofisteri Doppelganger
    HairBinding = 0x4BB9, // R4.44, BNpcName 14518, targetable Garrote/Malicious Weave add
    Hair = 0x4BBA, // R1.0, BNpcName 14519, short-lived Graft add
    DashingCutWaypoint = 0x4BBB, // R1.0, BNpcName 108, non-targetable route marker
    DashingCutTarget = 0x4BBC, // R1.0, BNpcName 108, non-targetable first-destination marker
    Helper = 0x233C
}

public enum AID : uint
{
    AsymmetricCoifChange1 = 0xB7CE, // boss->self, 3.0s cast, selects a Dual Cut pattern
    AsymmetricCoifChange2 = 0xB7CF, // boss->self, 3.0s cast, selects a Dual Cut pattern
    CoifChange1 = 0xB7D0, // boss->self, no cast, visual
    CoifChange2 = 0xB7D1, // boss->self, no cast, visual
    DualCutVisual1 = 0xB7D2, // boss->self, 2.0s cast, visual for C603/C604
    DualCutVisual2 = 0xB7D3, // boss->self, 2.0s cast, visual for C603/C604
    DualCutAnimation1 = 0xB7D4, // boss->self, no cast, visual
    DualCutAnimation2 = 0xB7D5, // boss->self, no cast, visual
    ResettingSpray1 = 0xB7D6, // boss->self, no cast, visual
    ResettingSpray2 = 0xB7D7, // boss->self, no cast, visual
    ResettingSpray3 = 0xB7D8, // boss->self, no cast, visual
    ResettingSpray4 = 0xB7D9, // boss->self, no cast, visual
    DashingCutMarker = 0xB7DA, // DashingCutTarget->self, no cast, route-selection event
    DashingCutVisualLong = 0xB7DB, // boss->location, 6.0s cast, visual for BF9C
    DashingCutVisualShort = 0xB7DC, // boss->location, 0.5s cast, visual for BF9D
    Extension = 0xB7DD, // boss->self, 3.0s cast, spawns Hair/HairBinding adds
    Graft = 0xB7DE, // Hair->self, 3.0s cast, 6y circle
    BalefulBlowout = 0xB7DF, // boss->self, 5.0s cast, visual for Malicious Weave
    MaliciousWeaveLong = 0xB7E0, // HairBinding->self, 5.5s cast, 6y circle and draw-in
    Garrote = 0xB7E1, // HairBinding->self, 10.0s cast, 6y circle; cancelled when add dies
    GarroteCancel = 0xB7E2, // HairBinding->self, no cast, cancellation/death event
    HairShearsVisual = 0xB7E3, // boss->self, 5.0s cast, visual
    HairShearsCircle = 0xB7E4, // helper->self, 5.0s cast, 10y circle
    HairShearsLine = 0xB7E5, // helper->self, 5.0s cast, 60y long 4y wide cross
    MaliciousWeaveShort = 0xB7E6, // HairBinding->self, 1.0s cast, 6y circle and draw-in
    AuraBurstVisual = 0xB7E7, // boss->self, 5.0s cast, raidwide visual
    AuraBurst = 0xB7E8, // three helpers, no cast, raidwide damage
    HairShearsPull = 0xB9EF, // helpers, no cast, delayed draw-in for players hit by B7E5
    DashingCutLong = 0xBF9C, // helper->location, 6.5s cast, dynamic-length 10y wide charge
    DashingCutShort = 0xBF9D, // helper->location, 1.0s cast, dynamic-length 10y wide charge
    AutoAttack = 0xC3CA, // boss->player, no cast, single-target
    DualCutFirst = 0xC603, // helper->self, 2.8s cast, 60y 180-degree cone (no telegraph decal, omen=0)
    DualCutSecond = 0xC604 // helper->self, 4.8s cast, 60y 180-degree cone opposite the first (no telegraph decal, omen=0)
}

// Every stationary avoidable AOE has an authoritative cast packet from the actor that owns its
// origin and rotation. Actor death/destruction cleanup is important for Garrote: killing a binding
// cancels its long cast without an action effect.
sealed class CalofisteriAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCircle SixYalms = new(6f);
    private static readonly AOEShapeCircle HairShearsCircle = new(10f);
    // Official sheet marks B7E5 as a 60y cross; replays confirm both arms: each helper fires the
    // same AID at two rotations 45 deg apart, and hits land along the cast direction (|perp|<=2y)
    // as well as perpendicular to it (|proj|<=2y), so a 4y-wide cross matches the kill points.
    private static readonly AOEShapeCross HairShearsLine = new(60f, 2f);

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.Graft or (uint)AID.MaliciousWeaveLong or (uint)AID.MaliciousWeaveShort or (uint)AID.Garrote => new(SixYalms),
        (uint)AID.HairShearsCircle => new(HairShearsCircle),
        (uint)AID.HairShearsLine => new(HairShearsLine),
        _ => null
    };
}

// Two helpers cast C603 (2.8s) and C604 (4.8s) simultaneously from the same spot with opposite
// rotations; replay hits reach +-90 degrees of each cast rotation, so each cut is a 180-degree
// half-arena cleave. Drawing both as risky would forbid the entire arena and freeze the AI, so
// only the earliest unresolved cut is risky - dodge into the second half after the first resolves.
sealed class DualCuts(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCone DualCut = new(60f, 90f.Degrees());

    protected override int MaxRisky => 1;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var cuts = ActiveAOEs(slot, actor);
        if (cuts.Length < 2)
            return;

        // C603 and C604 are exactly opposite and resolve two seconds apart. Staying deep in the
        // first safe half makes the return crossing too long, which is why automation sometimes
        // failed the second cut even though its AOE was tracked. Stage one close to the dividing
        // line (still on the safe side); after C603 resolves, C604's forbidden half moves the AI
        // only a few yalms across that line instead of across the arena.
        var origin = cuts[0].Origin;
        var direction = cuts[0].Rotation.ToDirection();
        hints.GoalZones.Add(position =>
        {
            var forward = (position - origin).Dot(direction);
            return forward is >= -3f and <= -1f ? 0.75f : 0f;
        });
    }

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.DualCutFirst or (uint)AID.DualCutSecond => new(DualCut),
        _ => null
    };
}

// A binding only proceeds to the lethal Garrote if its weave actually hit at least one target.
// The weave event arrives about 3.1s before Garrote starts, so use it as the primary target-switch
// signal and keep the Garrote cast-start as a packet-loss/late-join fallback.
sealed class GarroteTargets(BossModule module) : BossComponent(module)
{
    private readonly HashSet<ulong> _urgent = [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var enemy in hints.PotentialTargets)
        {
            if (enemy.Actor.OID == (uint)OID.Hair)
                enemy.ForcePriority(AIHints.Enemy.PriorityPointless);
            else if (enemy.Actor.OID == (uint)OID.HairBinding)
                enemy.ForcePriority(_urgent.Contains(enemy.Actor.InstanceID) ? 3 : AIHints.Enemy.PriorityPointless);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Garrote && !spell.EventHappened)
            _urgent.Add(caster.InstanceID);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Garrote)
            _urgent.Remove(caster.InstanceID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.MaliciousWeaveLong:
            case (uint)AID.MaliciousWeaveShort:
                if (spell.Targets.Count != 0)
                    _urgent.Add(caster.InstanceID);
                else
                    _urgent.Remove(caster.InstanceID);
                break;
            case (uint)AID.GarroteCancel:
                _urgent.Remove(caster.InstanceID);
                break;
        }
    }

    public override void OnActorDeath(Actor actor) => _urgent.Remove(actor.InstanceID);
    public override void OnActorDestroyed(Actor actor) => _urgent.Remove(actor.InstanceID);
}

// The BF9C/BF9D cast rotation is a fixed packet value and does not point along the charge. Build
// each rectangle from the helper's live position to spell.LocXZ instead. The small resolved-cast
// tombstone prevents an accelerated replay's stale cast-start packet from resurrecting a charge.
sealed class DashingCuts(BossModule module) : Components.GenericAOEs(module)
{
    private const double EventResolveTolerance = 0.5d;
    private const double CastMatchTolerance = 0.75d;
    private const double TombstoneWindow = 1d;
    private const double EventDedupWindow = 2d;
    private const double ExpireDelay = 2d;

    private readonly record struct PendingCharge(uint ActionID, AOEInstance AOE);
    private readonly record struct ResolvedCharge(uint ActionID, ulong ActorID, DateTime Activation, DateTime ExpiresAt);
    private readonly record struct EventKey(uint GlobalSequence, uint ActionID, ulong ActorID);

    private readonly List<PendingCharge> _pending = [with(4)];
    private readonly List<AOEInstance> _displayed = [with(4)];
    private readonly List<ResolvedCharge> _resolved = [with(4)];
    private readonly Dictionary<EventKey, DateTime> _seenEvents = [];
    private DateTime? _routeFirstActivation;
    private int _routeMarkers;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        PruneExpired();
        _displayed.Clear();
        foreach (var pending in _pending)
            _displayed.Add(pending.AOE);
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (!IsWatched(spell.Action.ID))
            return;

        PruneExpired();
        var activation = Module.CastFinishAt(spell);
        if (spell.EventHappened || activation <= WorldState.CurrentTime || WasRecentlyResolved(spell.Action.ID, caster.InstanceID, activation))
            return;

        var direction = spell.LocXZ - caster.Position;
        var length = direction.Length();
        if (length <= 0.1f)
            return;

        var rotation = Angle.FromDirection(direction);
        var shape = new AOEShapeRect(length, 5f);
        var aoe = new AOEInstance(shape, caster.Position, rotation, activation, actorID: caster.InstanceID, shapeDistance: shape.Distance(caster.Position, rotation));
        if (spell.Action.ID == (uint)AID.DashingCutLong && (_routeFirstActivation == null || Math.Abs((_routeFirstActivation.Value - activation).TotalSeconds) > CastMatchTolerance))
        {
            _pending.Clear();
            _routeFirstActivation = activation;
            _routeMarkers = 0;
        }

        var match = spell.Action.ID == (uint)AID.DashingCutShort
            ? _pending.FindIndex(pending => pending.ActionID == spell.Action.ID && pending.AOE.ActorID == 0
                && pending.AOE.Origin.AlmostEqual(caster.Position, 0.2f)
                && Math.Abs((pending.AOE.Rotation - rotation).Normalized().Rad) < 1f.Degrees().Rad
                && Math.Abs((pending.AOE.Activation - activation).TotalSeconds) <= CastMatchTolerance)
            : -1;
        if (match < 0)
            match = _pending.FindIndex(pending => pending.ActionID == spell.Action.ID
                && pending.AOE.ActorID == caster.InstanceID
                && Math.Abs((pending.AOE.Activation - activation).TotalSeconds) <= CastMatchTolerance);
        if (match >= 0)
            _pending[match] = new(spell.Action.ID, aoe);
        else
            _pending.Add(new(spell.Action.ID, aoe));
        _pending.Sort((left, right) => left.AOE.Activation.CompareTo(right.AOE.Activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (!IsWatched(spell.Action.ID))
            return;

        var now = WorldState.CurrentTime;
        var activation = Module.CastFinishAt(spell);
        RemoveMatchingCast(spell.Action.ID, caster.InstanceID, activation);
        if (spell.EventHappened || activation <= now.AddSeconds(EventResolveTolerance))
            RememberResolved(spell.Action.ID, caster.InstanceID, activation, now);
        ResetRouteIfFinished();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DashingCutMarker)
        {
            AddRouteMarker(caster, spell);
            return;
        }
        if (!IsWatched(spell.Action.ID))
            return;

        var now = WorldState.CurrentTime;
        PruneExpired();
        if (IsDuplicateEvent(spell.GlobalSequence, spell.Action.ID, caster.InstanceID, now))
            return;

        ++NumCasts;
        var activation = RemoveResolvedByEvent(spell.Action.ID, caster.InstanceID, now) ?? now;
        RememberResolved(spell.Action.ID, caster.InstanceID, activation, now);
        ResetRouteIfFinished();
    }

    public override void OnActorDeath(Actor actor) => RemoveActor(actor.InstanceID);
    public override void OnActorDestroyed(Actor actor) => RemoveActor(actor.InstanceID);

    private static bool IsWatched(uint actionID) => actionID is (uint)AID.DashingCutLong or (uint)AID.DashingCutShort;

    private void AddRouteMarker(Actor caster, ActorCastEvent spell)
    {
        var now = WorldState.CurrentTime;
        PruneExpired();
        if (_routeFirstActivation == null || _routeMarkers >= 2 || IsDuplicateEvent(spell.GlobalSequence, spell.Action.ID, caster.InstanceID, now))
            return;

        var direction = spell.TargetXZ - caster.Position;
        var length = direction.Length();
        if (length <= 0.1f)
            return;

        var rotation = Angle.FromDirection(direction);
        var shape = new AOEShapeRect(length, 5f);
        var activation = _routeFirstActivation.Value.AddSeconds(7d * (_routeMarkers + 1));
        _pending.Add(new((uint)AID.DashingCutShort, new(shape, caster.Position, rotation, activation,
            actorID: 0, shapeDistance: shape.Distance(caster.Position, rotation))));
        ++_routeMarkers;
        _pending.Sort((left, right) => left.AOE.Activation.CompareTo(right.AOE.Activation));
    }

    private DateTime? RemoveResolvedByEvent(uint actionID, ulong actorID, DateTime now)
    {
        var index = -1;
        for (var i = 0; i < _pending.Count; ++i)
        {
            var pending = _pending[i];
            if (pending.ActionID == actionID && pending.AOE.ActorID == actorID && pending.AOE.Activation <= now.AddSeconds(EventResolveTolerance))
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
            var pending = _pending[i];
            if (pending.ActionID != actionID || pending.AOE.ActorID != actorID)
                continue;

            var delta = Math.Abs((pending.AOE.Activation - activation).TotalSeconds);
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
        _pending.RemoveAll(pending => now > pending.AOE.Activation.AddSeconds(ExpireDelay));
        _resolved.RemoveAll(resolved => now > resolved.ExpiresAt);
        foreach (var key in _seenEvents.Where(entry => now > entry.Value).Select(entry => entry.Key).ToArray())
            _seenEvents.Remove(key);
        if (_routeFirstActivation is { } first && now > first.AddSeconds(16d))
        {
            _routeFirstActivation = null;
            _routeMarkers = 0;
        }
    }

    private void ResetRouteIfFinished()
    {
        if (_pending.Count != 0)
            return;
        _routeFirstActivation = null;
        _routeMarkers = 0;
    }

    private void RemoveActor(ulong actorID)
    {
        _pending.RemoveAll(pending => pending.AOE.ActorID == actorID);
        _resolved.RemoveAll(resolved => resolved.ActorID == actorID);
        foreach (var key in _seenEvents.Keys.Where(key => key.ActorID == actorID).ToArray())
            _seenEvents.Remove(key);
    }
}

// Both weave variants pull affected players all the way to the binding. The six-yalm shape keeps
// the movement preview limited to players who will actually be hit by the cast.
sealed class MaliciousWeavePulls(BossModule module) : Components.SimpleKnockbackGroups(module,
    [(uint)AID.MaliciousWeaveLong, (uint)AID.MaliciousWeaveShort], 60f, shape: new AOEShapeCircle(6f), kind: Kind.TowardsOrigin);

// Hair Shears first resolves B7E5, then B9EF pulls each player hit by that line to its source about
// 1.08 seconds later. The pull uses separate helpers, so resolve it by origin+rotation rather than
// helper instance id. The affected shape is the same 4y-wide cross as the damaging AOE.
sealed class HairShearsPulls(BossModule module) : Components.GenericKnockback(module)
{
    private static readonly AOEShapeCross Shape = new(60f, 2f);
    private readonly List<Knockback> _pulls = [with(8)];
    private readonly HashSet<uint> _seenGlobalSequences = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        PruneExpired();
        return CollectionsMarshal.AsSpan(_pulls);
    }

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.HairShearsLine || spell.EventHappened)
            return;

        _pulls.RemoveAll(pull => pull.ActorID == caster.InstanceID);
        _pulls.Add(new(caster.Position, 60f, Module.CastFinishAt(spell, 1.1d), Shape, spell.Rotation, Kind.TowardsOrigin, actorID: caster.InstanceID));
        _pulls.Sort((left, right) => left.Activation.CompareTo(right.Activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.HairShearsPull || spell.GlobalSequence != 0 && !_seenGlobalSequences.Add(spell.GlobalSequence))
            return;

        var index = _pulls.FindIndex(pull => pull.Origin.AlmostEqual(caster.Position, 0.5f) && pull.Direction.AlmostEqual(spell.Rotation, 2f.Degrees().Rad));
        if (index >= 0)
            _pulls.RemoveAt(index);
        ++NumCasts;
    }

    public override void OnActorDestroyed(Actor actor) => _pulls.RemoveAll(pull => pull.ActorID == actor.InstanceID);

    private void PruneExpired()
    {
        var now = WorldState.CurrentTime;
        _pulls.RemoveAll(pull => now > pull.Activation.AddSeconds(1d));
    }
}

sealed class AuraBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.AuraBurstVisual);

sealed class DoubledTroubleStates : StateMachineBuilder
{
    public DoubledTroubleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GarroteTargets>()
            .ActivateOnEnter<CalofisteriAOEs>()
            .ActivateOnEnter<DualCuts>()
            .ActivateOnEnter<DashingCuts>()
            .ActivateOnEnter<MaliciousWeavePulls>()
            .ActivateOnEnter<HairShearsPulls>()
            .ActivateOnEnter<AuraBurst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(DoubledTroubleStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 50u,
    SortOrder = 10)]
public sealed class DoubledTrouble(WorldState ws, Actor primary) : BossModule(ws, primary, new(-215f, -65f), new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.HairBinding));
        Arena.Actors(Enemies((uint)OID.Hair));
    }
}
