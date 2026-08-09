using BossMod.Dawntrail.Foray.CriticalEngagement;

namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE208AwakenedHydra;

public enum OID : uint
{
    Boss = 0x4BC5, // R3.6, BNpcName 14523, magicked hydra
    LightSphere = 0x4BC6, // R1.0, BNpcName 14524
    FireSphere = 0x4BC7, // R1.0, BNpcName 14525
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 0xC647, // boss->player, no cast, single-target
    ElementalSpillVisual = 0xB850,
    ElementalSpill1 = 0xB851,
    ElementalSpill2 = 0xB852,
    ElementalSpill3 = 0xB853,
    ElementalSpill4 = 0xB854,
    ElementalSpill5 = 0xB855, // helper->location, 6y circles
    CrimsonRay = 0xB856, // fire sphere, 70y long, 4y wide rect
    BlindingFlash = 0xB857, // light sphere, gaze
    RadiantIce = 0xB858, // 40y 20-degree cone
    ToxinScatter = 0xB859,
    Discharge = 0xB85A, // 10y circle
    RingLightningInner = 0xB85B, // 10-20y donut
    RingLightningOuter = 0xB85C, // 20-30y donut
    NearShockwaveVisual = 0xB85D,
    FarShockwaveVisual = 0xB85E,
    ElementalShockwave1 = 0xB85F,
    ElementalShockwave2 = 0xB860,
    ElementalShockwave3 = 0xB861,
    ElementalShockwave4 = 0xB862,
    ElementalShockwave5 = 0xB863, // helper->location, 8y circles
    ManyHeadedBreath1 = 0xB865, // boss self-only head/model transition
    ManyHeadedBreath2 = 0xB866, // boss self-only head/model transition
    ManyHeadedBreath3 = 0xB867, // boss self-only head/model transition
    StarlightBreath = 0xB868,
    QuintetRoar = 0xB869,
    QuintetRoarHit = 0xB86A,
    MultipleBreaths1 = 0xB86C, // 30y 120-degree cone
    MultipleBreathsVisual = 0xB86D,
    MultipleBreaths2 = 0xC5F1,
    MultipleBreaths3 = 0xC5F2,
    MultipleBreaths4 = 0xC5F3
}

// ElementalSpill1 leaves a small poison pool at its resolved location. ToxinScatter then ticks
// once per second at that same position for roughly ten seconds. The tick helpers are recycled
// between waves, so tracking helper instance IDs would keep stale pools or create duplicates;
// key the pools by position instead. A live in-arena toxin tick can also restore a pool when the
// initial spill packet was missed (for example when the module activates mid-mechanic).
sealed class ToxinPools(BossModule module) : Components.GenericAOEs(module)
{
    // The green puddle spawns small and expands over roughly nine seconds (replay tick distances
    // grow from ~2.5y to ~7.5y), so the persistent hazard is drawn at its current radius.
    private const float InitialRadius = 2.5f;
    private const float MaxRadius = 7.5f;
    private const float GrowthPerSecond = 0.55f;
    private const float PositionTolerance = 0.75f;
    private const double PredictedLifetime = 10.5d;
    private const double TickLifetime = 1.25d;

    private sealed class Pool(WPos origin, DateTime createdAt, DateTime expiresAt)
    {
        public readonly WPos Origin = origin;
        public DateTime CreatedAt = createdAt;
        public DateTime ExpiresAt = expiresAt;
    }

    private static float CurrentRadius(DateTime now, DateTime createdAt)
        => MathF.Min(MaxRadius, InitialRadius + (float)(now - createdAt).TotalSeconds * GrowthPerSecond);

    private readonly List<Pool> _pools = [with(4)];
    private readonly List<AOEInstance> _active = [with(4)];
    private readonly HashSet<uint> _seenGlobalSequences = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        PruneExpired();
        _active.Clear();
        var now = WorldState.CurrentTime;
        foreach (var pool in _pools)
        {
            var shape = new AOEShapeCircle(CurrentRadius(now, pool.CreatedAt));
            _active.Add(new(shape, pool.Origin, activation: pool.CreatedAt, shapeDistance: shape.Distance(pool.Origin, default)));
        }
        return CollectionsMarshal.AsSpan(_active);
    }

    public override void Update() => PruneExpired();

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID.ElementalSpill1) and not ((uint)AID.ToxinScatter)
            || spell.GlobalSequence != 0 && !_seenGlobalSequences.Add(spell.GlobalSequence))
        {
            return;
        }

        var now = WorldState.CurrentTime;
        PruneExpired();
        var origin = caster.Position;
        var pool = _pools.FirstOrDefault(pool => pool.Origin.InCircle(origin, PositionTolerance));
        var isSpill = spell.Action.ID == (uint)AID.ElementalSpill1;
        if (pool == null)
        {
            if (isSpill || caster.OID == (uint)OID.Helper && Arena.InBounds(origin))
            {
                _pools.Add(new(origin, now, now.AddSeconds(isSpill ? PredictedLifetime : TickLifetime)));
            }
        }
        else
        {
            var refreshedExpiry = now.AddSeconds(isSpill ? PredictedLifetime : TickLifetime);
            // The spill prediction covers the packet gap before the first tick. Once ticks begin,
            // their cadence is authoritative; assigning (rather than only extending) also keeps
            // accelerated client-replay captures from retaining pools for ten wall-clock seconds.
            if (isSpill)
            {
                pool.CreatedAt = now; // a fresh puddle forms, so its expansion restarts
            }
            if (!isSpill || refreshedExpiry > pool.ExpiresAt)
            {
                pool.ExpiresAt = refreshedExpiry;
            }
        }
    }

    private void PruneExpired()
    {
        var now = WorldState.CurrentTime;
        _pools.RemoveAll(pool => now > pool.ExpiresAt);
    }
}

sealed class HydraAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCircle Spill = new(6f);
    private static readonly AOEShapeRect CrimsonRay = new(70f, 2f);
    private static readonly AOEShapeCone Ice = new(40f, 10f.Degrees());
    private static readonly AOEShapeCircle Discharge = new(10f);
    private static readonly AOEShapeDonut InnerRing = new(10f, 20f);
    private static readonly AOEShapeDonut OuterRing = new(20f, 30f);
    private static readonly AOEShapeCircle Shockwave = new(8f);
    private static readonly AOEShapeCone Breath = new(30f, 60f.Degrees());

    // Spill, shockwave and multi-breath packets expose later waves before the first resolves.
    // Preserve every preview while only the earliest simultaneous batch constrains movement.
    // Adjacent elemental waves are ~1.07s apart. Include the next wave immediately so the green
    // spill circles receive an AI forbidden zone before the previous batch finishes resolving.
    protected override double RiskyActivationWindow => 1.25d;

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        >= (uint)AID.ElementalSpill1 and <= (uint)AID.ElementalSpill5 => new(Spill, true),
        (uint)AID.CrimsonRay => new(CrimsonRay),
        (uint)AID.RadiantIce => new(Ice),
        (uint)AID.Discharge => new(Discharge),
        (uint)AID.RingLightningInner => new(InnerRing),
        (uint)AID.RingLightningOuter => new(OuterRing),
        >= (uint)AID.ElementalShockwave1 and <= (uint)AID.ElementalShockwave5 => new(Shockwave, true),
        _ => null
    };
}

// Multiple Breaths uses the fast B86C casts only to reveal the order during the long visual; they
// do not damage players. After the visual resolves, C5F1/C5F2/C5F3 execute that order. Record the
// demonstration without exposing it as an AOE, then publish the six real hits in advance, with the
// current and next cones risky so AI can choose a route through the rotating pattern.
sealed class MultipleBreathsSequence(BossModule module) : Components.GenericAOEs(module)
{
    private sealed class BreathStep(uint actionID, Angle rotation, DateTime activation, ulong actorID)
    {
        public readonly uint ActionID = actionID;
        public Angle Rotation = rotation;
        public DateTime Activation = activation;
        public ulong ActorID = actorID;
    }

    private static readonly AOEShapeCone Shape = new(30f, 60f.Degrees());
    private const double FirstFollowupDelay = 1.03d;
    private const double FollowupInterval = 2.06d;
    private const double RiskWindow = 2.2d;
    private readonly List<Angle> _recorded = [with(6)];
    private readonly List<BreathStep> _steps = [with(6)];
    private readonly List<AOEInstance> _displayed = [with(6)];
    private bool _recording;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Prune();
        _displayed.Clear();
        if (_steps.Count == 0)
            return CollectionsMarshal.AsSpan(_displayed);

        var deadline = _steps[0].Activation.AddSeconds(RiskWindow);
        foreach (var step in _steps)
        {
            var risky = step.Activation <= deadline;
            _displayed.Add(new(Shape, Module.Arena.Center, step.Rotation, step.Activation,
                risky ? Colors.Danger : Colors.AOE, risky, step.ActorID, Shape.Distance(Module.Arena.Center, step.Rotation)));
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void Update() => Prune();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MultipleBreathsVisual && !spell.EventHappened)
        {
            // The first helper cast can arrive in the same frame just before the boss visual.
            // Preserve that direction when it already opened this recording window.
            if (!_recording)
                _recorded.Clear();
            _steps.Clear();
            _recording = true;
            return;
        }

        if (spell.Action.ID == (uint)AID.MultipleBreaths1 && !spell.EventHappened)
        {
            // The module can activate after the long visual cast has already started. In that
            // case its first observable packet is one of these demonstration casts, so begin a
            // fresh recording here instead of discarding the whole first sequence.
            if (!_recording)
            {
                _recorded.Clear();
                _steps.Clear();
                _recording = true;
            }
            if (_recorded.Count < 6)
                _recorded.Add(spell.Rotation);
            return;
        }

        if (spell.Action.ID is (uint)AID.MultipleBreaths2 or (uint)AID.MultipleBreaths3 or (uint)AID.MultipleBreaths4)
        {
            // Replace the prediction with authoritative rotation/timing as soon as the short cast
            // packet arrives, while retaining the rest of the learned sequence.
            var index = _steps.FindIndex(step => step.ActionID == spell.Action.ID);
            if (index < 0 && _steps.Count != 0)
                index = 0;
            if (index >= 0)
            {
                _steps[index].Rotation = spell.Rotation;
                _steps[index].Activation = Module.CastFinishAt(spell);
                _steps[index].ActorID = caster.InstanceID;
                _steps.Sort((left, right) => left.Activation.CompareTo(right.Activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MultipleBreathsVisual)
        {
            _recording = false;
            _steps.Clear();
            if (_recorded.Count == 6)
            {
                var first = WorldState.FutureTime(FirstFollowupDelay);
                for (var i = 0; i < _recorded.Count; ++i)
                {
                    var rotation = _recorded[i];
                    _steps.Add(new(FollowupAction(rotation), rotation, first.AddSeconds(i * FollowupInterval), 0));
                }
            }
            return;
        }

        if (spell.Action.ID is not ((uint)AID.MultipleBreaths2) and not ((uint)AID.MultipleBreaths3)
            and not ((uint)AID.MultipleBreaths4))
            return;

        var index = _steps.FindIndex(step => step.ActionID == spell.Action.ID && step.Activation <= WorldState.FutureTime(0.75d));
        if (index < 0 && _steps.Count != 0)
            index = 0;
        if (index >= 0)
            _steps.RemoveAt(index);
        ++NumCasts;
    }

    private uint FollowupAction(Angle rotation)
    {
        var relative = (rotation - Module.PrimaryActor.Rotation).Normalized().Rad;
        if (MathF.Abs(relative) < 30f * Angle.DegToRad)
            return (uint)AID.MultipleBreaths2;
        return relative < 0f ? (uint)AID.MultipleBreaths3 : (uint)AID.MultipleBreaths4;
    }

    private void Prune()
    {
        var now = WorldState.CurrentTime;
        _steps.RemoveAll(step => now > step.Activation.AddSeconds(1d));
    }
}

// B86A is emitted by several helpers and splits the damage packets; the boss visual is the
// stable cast-bar warning for the unavoidable hit.
sealed class QuintetRoar(BossModule module) : Components.RaidwideCast(module, (uint)AID.QuintetRoar);
sealed class BlindingFlash(BossModule module) : Components.CastGaze(module, (uint)AID.BlindingFlash)
{
    // Rotation control needs a small lead to stop target-facing before the server snapshots gaze.
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlindingFlash && !spell.EventHappened)
            Eyes.Add(new(caster.Position, Module.CastFinishAt(spell, -0.75f), actorID: caster.InstanceID));
    }
}

sealed class AwakenedHydraStates : StateMachineBuilder
{
    public AwakenedHydraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HydraAOEs>()
            .ActivateOnEnter<MultipleBreathsSequence>()
            .ActivateOnEnter<ToxinPools>()
            .ActivateOnEnter<BlindingFlash>()
            .ActivateOnEnter<QuintetRoar>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(AwakenedHydraStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 62u,
    SortOrder = 7)]
public sealed class AwakenedHydra(WorldState ws, Actor primary) : BossModule(ws, primary, new(-82f, 485f), new ArenaBoundsCircle(20f));
