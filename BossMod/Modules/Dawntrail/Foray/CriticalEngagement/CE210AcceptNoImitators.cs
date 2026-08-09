using BossMod.Dawntrail.Foray.CriticalEngagement;

namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE210AcceptNoImitators;

public enum OID : uint
{
    Boss = 0x4C77, // R3.0, BNpcName 14801, morphing mage
    BoundaryController = 0x4DFD, // non-targetable controller at arena center
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 0xBCCE,
    BlackenedRainVisual = 0xBCCF, // boss->self, 4.0s cast, raidwide visual
    BlackenedRain = 0xBCD0, // three helpers->players, 5.0s cast, raidwide damage
    DarkDealing = 0xBCD1, // boss->player, 5.0s cast, tankbuster
    ChangeFire = 0xBCD2, // boss->self, 4.0s cast, visual
    ChangeWind = 0xBCD3, // boss->self, 4.0s cast, visual
    Revert = 0xBCD4, // boss->self, no cast, visual
    TongueOfFlame = 0xBCD5, // boss->self, 4.0s cast, range 15 circle
    HellfireFetchVisual = 0xBCD6,
    HellwardBound = 0xBCD7, // boss->location, 6.0s cast, visual
    HellwardBoundHit = 0xBCD8,
    HellfireFetch = 0xBCD9, // helper->location, 7.0s cast, range 6 circle
    HellishBreathVisual = 0xBCDA, // boss->self, 6.0s cast, visual
    HellishBreathShort = 0xBCDB, // helper->self, 2.0s cast, range 60 60-degree cone
    HellishBreathMedium = 0xBCDC, // helper->self, 4.0s cast, range 60 60-degree cone
    HellishBreathLong = 0xBCDD, // helper->self, 6.0s cast, range 60 60-degree cone
    HellishBreathHit1 = 0xBCDE,
    HellishBreathHit2 = 0xBCDF,
    HellishBreathHit3 = 0xBCE0,
    HellishBreathHit4 = 0xBCE1,
    HellishBreathQuickCenter = 0xBE16, // helper, 1.1s cast, range 60 60-degree cone
    HellishBreathQuickLeft = 0xBE17, // helper, 1.1s cast, range 60 60-degree cone
    HellishBreathQuickRight = 0xC5F5, // helper, 1.1s cast, range 60 60-degree cone
    CyclonicRing = 0xBCE2, // boss->self, 4.0s cast, range 10-30 donut
    ShapeshiftingSupercellVisual1 = 0xBCE3, // boss->self, 5.5s cast, visual
    ShapeshiftingSupercellVisual2 = 0xBCE4, // boss->self, 5.5s cast, visual
    ShapeshiftingSupercellConeLong = 0xBCE5, // helper->self, 6.0s cast, range 60 90-degree cone
    ShapeshiftingSupercellResolve = 0xBCE6,
    ShapeshiftingSupercellConeShort = 0xBCE7, // helper->self, 1.5s cast, range 60 90-degree cone
    ShapeshiftingSupercellCircle = 0xBCE8, // helper->self, 6.0s cast, range 8 circle
    ShapeshiftingSupercellDonutInner = 0xBCE9, // helper->self, 6.0s cast, range 10-20 donut
    ShapeshiftingSupercellDonutOuter = 0xBCEA, // helper->self, 6.0s cast, range 16-30 donut
    ShapeshiftingSupercellExtraCircle = 0xC64F, // helper->self, 6.0s cast, range 8 circle
    MadeMagicVisual = 0xBCEB, // boss->self, 4.0s cast, visual
    MadeMagic = 0xBCEC, // helper pulses; radius is modified by status 1909
    CycloneCrossingVisual = 0xBCED, // boss->self, 10.5s cast, visual
    CycloneCrossing = 0xBCEE, // helper->self, 11.5s cast, range 60 width 16 cross
    LethalBoundary = 0xBCEF, // controller, persistent 20-30 donut
    UnknownTarget1 = 0xBCF0,
    UnknownTarget2 = 0xBCF1,
    UnknownLocation = 0xC620
}

public enum SID : uint
{
    AreaOfInfluenceUp = 1909 // Made Magic helper, extra 1-7; circle radius = extra * 2.5y
}

// The official Action sheet (0xBCEF, eff=10 donut, xAxis=30) puts the persistent electric fence
// outer kill ring at 30y; the walkable circle is 25y, so the danger band covers 25-30.
sealed class LethalBoundary(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut Shape = new(24.5f, 30f);
    private readonly AOEInstance[] _aoe = [new(Shape, module.Arena.Center)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // The 24.5-30 donut gets clipped to the 25y walkable circle, leaving only a sliver that is
        // effectively invisible. Draw a visible 24-25 band plus the fence outline so the kill ring
        // reads clearly.
        Arena.ZoneDonut(Arena.Center, 24f, 25f, Colors.Danger);
        Arena.ZoneCircleOutlineUnclipped(Arena.Center, 25f, Colors.Danger, 3f);
    }
}


// Every avoidable AOE below has an authoritative cast-start packet from the actor that owns the
// shape. The helpers also carry their actual origin/rotation, so none of the patterns are inferred
// from the boss transformation visuals.
sealed class MorphingMageAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCircle Tongue = new(15f);
    private static readonly AOEShapeCircle Hellfire = new(6f);
    private static readonly AOEShapeCone HellishBreath = new(60f, 30f.Degrees());
    private static readonly AOEShapeDonut CyclonicRing = new(10f, 30f);
    private static readonly AOEShapeCone SupercellCone = new(60f, 45f.Degrees());
    private static readonly AOEShapeCircle SupercellCircle = new(8f);
    private static readonly AOEShapeDonut SupercellInner = new(10f, 20f);
    private static readonly AOEShapeDonut SupercellOuter = new(16f, 30f);
    private static readonly AOEShapeCross CycloneCross = new(60f, 8f);

    // Several patterns expose the full sequence at once (notably the 2/4/6s breath cones).
    // Keep simultaneous casts dangerous, but later preview steps must not block AI movement yet.
    protected override double RiskyActivationWindow => 0.5d;

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.TongueOfFlame => new(Tongue),
        (uint)AID.HellfireFetch => new(Hellfire, true),
        (uint)AID.HellishBreathShort or (uint)AID.HellishBreathMedium or (uint)AID.HellishBreathLong
            or (uint)AID.HellishBreathQuickCenter or (uint)AID.HellishBreathQuickLeft or (uint)AID.HellishBreathQuickRight => new(HellishBreath),
        (uint)AID.CyclonicRing => new(CyclonicRing),
        (uint)AID.ShapeshiftingSupercellConeLong or (uint)AID.ShapeshiftingSupercellConeShort => new(SupercellCone),
        (uint)AID.ShapeshiftingSupercellCircle or (uint)AID.ShapeshiftingSupercellExtraCircle => new(SupercellCircle),
        (uint)AID.ShapeshiftingSupercellDonutInner => new(SupercellInner),
        (uint)AID.ShapeshiftingSupercellDonutOuter => new(SupercellOuter),
        (uint)AID.CycloneCrossing => new(CycloneCross),
        _ => null
    };
}

// Made Magic creates four fixed helpers 7.8y from center (cardinal on one cast, diagonal on the
// next). Status 1909 grows from extra 1 through 7; the whole circle out to extra*2.5y is dangerous
// on every pulse (replay: a player standing 8-10y from a helper at extra 7 / 17.5y max was still
// hit), so both the drawn warning and the AI forbidden zone must be a filled circle, not a thin
// ring. The wave stops at extra 7 = 17.5y.
//
// The union of four 17.5y circles still leaves the four arena-edge pockets that sit >20y from every
// helper (the exact spots survivors stand on), so the AI is parked in a pocket that is safe for the
// whole sequence and never has to cross a circle - guaranteeing it is never clipped by the poison.
sealed class MadeMagic(BossModule module) : Components.GenericAOEs(module)
{
    private const float MaxRadius = 17.5f; // extra 7 * 2.5
    private static readonly AOEShapeCircle FinalSweep = new(MaxRadius);
    private readonly Dictionary<ulong, AOEInstance> _pending = [];
    private readonly Dictionary<ulong, int> _extra = [];
    private readonly List<AOEInstance> _displayed = new(4);
    private readonly HashSet<uint> _seenGlobalSequences = [];
    private DateTime? _maxExtraAt;
    private bool _mechanicFinished;
    // extra 7 (max radius) holds for a couple seconds, then the mechanic is over and rings must
    // not be rebuilt even though the helpers keep the growth status (that caused infinite
    // clear->rebuild flicker).
    private const double MaxExtraHold = 2d;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _displayed.Clear();
        _displayed.AddRange(_pending.Values);
        return CollectionsMarshal.AsSpan(_displayed);
    }

    // Reserve the complete sweep as soon as the first growth status arrives. Expanding this hint
    // pulse-by-pulse makes automation walk a few yalms after every hit; the final 17.5y footprint
    // sends it to one of the four edge pockets in a single route. ActiveAOEs still draws only the
    // current real radius, so the visual timing remains faithful to the mechanic.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var aoe in _pending.Values)
            hints.AddForbiddenZone(FinalSweep, aoe.Origin);
    }

    public override void Update()
    {
        // Components can be activated after the helpers already received their growth status.
        // Recover that live state instead of waiting for a status-gain packet that will never repeat.
        var now = WorldState.CurrentTime;
        var helpers = Module.Enemies((uint)OID.Helper).Where(h => !h.IsDeadOrDestroyed).ToList();
        if (!_mechanicFinished)
        {
            foreach (var helper in helpers)
            {
                var status = helper.FindStatus((uint)SID.AreaOfInfluenceUp);
                if (status is { } current && current.Extra is >= 1 and <= 7
                    && (!_extra.TryGetValue(helper.InstanceID, out var knownExtra) || knownExtra != current.Extra))
                {
                    SetRing(helper, current.Extra);
                    if (current.Extra == 7)
                        _maxExtraAt = now;
                }
            }
        }

        // Stale helpers without the growth status (or destroyed without a clean packet) must not
        // keep their rings drawn until the next mechanic.
        var live = helpers.ToDictionary(h => h.InstanceID);
        foreach (var key in _pending.Keys.ToArray())
        {
            if (!live.TryGetValue(key, out var helper) || helper.FindStatus((uint)SID.AreaOfInfluenceUp) == null)
                Remove(key);
        }

        // Mechanic end: after every ring reached max radius (extra 7) and held it briefly, the
        // sequence is over - clear all rings and refuse to rebuild them (helpers keep the growth
        // status, which previously caused an infinite clear/rebuild loop).
        if (!_mechanicFinished && _maxExtraAt is { } maxAt && now > maxAt.AddSeconds(MaxExtraHold) && _pending.Count != 0)
        {
            Service.Logger.Information($"[CE210] MadeMagic finished, clearing {_pending.Count} rings");
            _pending.Clear();
            _extra.Clear();
            _mechanicFinished = true;
            _maxExtraAt = null;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID != (uint)OID.Helper || !actor.Position.InCircle(Arena.Center, 30f)
            || status.ID != (uint)SID.AreaOfInfluenceUp || status.Extra is < 1 or > 7)
            return;

        // After the mechanic finished, ignore stray gains until the status actually drops.
        if (_mechanicFinished)
            return;
        // Repeated gain for the same extra (no lose in between) would rebuild the ring every frame
        // and make it flicker; keep the existing ring instead.
        if (_extra.TryGetValue(actor.InstanceID, out var known) && known == status.Extra)
            return;
        SetRing(actor, status.Extra);
        if (status.Extra == 7)
            _maxExtraAt = WorldState.CurrentTime;
        Service.Logger.Information($"[CE210] MadeMagic status gain helper={actor.InstanceID:X} extra={status.Extra}");
    }

    private void SetRing(Actor actor, int extra)
    {
        var outer = extra * 2.5f;
        // Whole circle is lethal (see comment above); AddAIHints additionally fills the circle so
        // automation treats the swept area as forbidden, not just the currently drawn edge.
        AOEShape shape = new AOEShapeCircle(outer);
        _pending[actor.InstanceID] = new(shape, actor.Position, color: Colors.Danger, risky: false,
            activation: WorldState.FutureTime(0.3f), actorID: actor.InstanceID, shapeDistance: shape.Distance(actor.Position, default));
        _extra[actor.InstanceID] = extra;
        Service.Logger.Information($"[CE210] MadeMagic ring helper={actor.InstanceID:X} extra={extra} r={outer:f1}");
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.AreaOfInfluenceUp)
        {
            Remove(actor.InstanceID);
            _mechanicFinished = false;
            _maxExtraAt = null;
        }
    }

    public override void OnActorDeath(Actor actor) => Remove(actor.InstanceID);
    public override void OnActorDestroyed(Actor actor) => Remove(actor.InstanceID);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.MadeMagic
            || spell.GlobalSequence != 0 && !_seenGlobalSequences.Add(spell.GlobalSequence)
            || !_pending.TryGetValue(caster.InstanceID, out var current))
            return;

        // Ignore pulses after the mechanic finished so a repeated/duplicated event cannot keep
        // refreshing the rings forever.
        if (_mechanicFinished)
            return;

        // Every status step normally pulses twice (extra 7 pulses three times). The first event is
        // therefore not the end of the ring; move the same warning to the next observed cadence.
        // A new status replaces its geometry, and status loss performs the final cleanup.
        _pending[caster.InstanceID] = current with { Activation = WorldState.FutureTime(0.58d) };
    }

    private void Remove(ulong id)
    {
        _pending.Remove(id);
        _extra.Remove(id);
    }
}

// The three BCD0 helpers split one raidwide across the participant list. The boss cast is the
// stable, non-duplicated warning and starts one second before the helper cast bars.
sealed class BlackenedRain(BossModule module) : Components.RaidwideCast(module, (uint)AID.BlackenedRainVisual);
sealed class DarkDealing(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.DarkDealing);
// BCD7 only moves the boss from center to the cast location. The three damaging BCD8 dashes then
// visit R90(p), -R90(p), and -p around arena center, where p is that first landing offset. Replays
// expose the whole route from BCD7's target six seconds early, so draw the real lanes up front
// instead of following the boss with post-hit movement trails.
sealed class HellwardBound(BossModule module) : Components.GenericAOEs(module)
{
    private const float HalfWidth = 5f;
    private const double FirstDashDelay = 2.2d;
    private const double DashInterval = 2.2d;
    private const double FinalDashGrace = 0.9d;
    private const double DisplayLead = 2d;
    private readonly List<AOEInstance> _lanes = new(3);
    private readonly HashSet<uint> _seenHitSequences = [];
    private readonly AOEInstance[] _current = new AOEInstance[1];
    private DateTime _phaseExpires;

    public bool DashPhaseActive => WorldState.CurrentTime <= _phaseExpires;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // 纯时间驱动: 每段在其落地前 2s 显示 (risky), 落地后 0.5s 消失。不依赖 hit 事件,
        // 避免 hit 时序不稳/缺失导致第二段显示太晚、AI 吃到伤害才躲。
        if (WorldState.CurrentTime > _phaseExpires)
            _lanes.Clear();
        if (_lanes.Count == 0)
            return [];
        var now = WorldState.CurrentTime;
        foreach (var lane in _lanes)
        {
            if (now >= lane.Activation.AddSeconds(-DisplayLead) && now <= lane.Activation.AddSeconds(0.5d))
            {
                _current[0] = new(lane.Shape, lane.Origin, lane.Rotation, lane.Activation, color: Colors.Danger, risky: true,
                    shapeDistance: lane.ShapeDistance);
                return _current;
            }
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // BCD7 is a non-damaging movement cast; it may arrive marked EventHappened, but we still
        // need it to compute the dash lanes.
        if (caster != Module.PrimaryActor || (spell.Action.ID & 0xFFFF) != (uint)AID.HellwardBound)
            return;

        Service.Logger.Information($"[CE210] HellwardBound cast id={spell.Action.ID:X} caster={caster.InstanceID:X} loc=({spell.LocXZ.X:f1},{spell.LocXZ.Z:f1})");
        _lanes.Clear();
        _seenHitSequences.Clear();

        var p = spell.LocXZ - Arena.Center;
        var p90 = p.OrthoL();
        // The named four-part sequence starts with BCD7's non-damaging movement. Only the following
        // three BCD8 paths are lethal; queuing center->p shifts every warning one hit late.
        WPos[] points = [Arena.Center + p, Arena.Center + p90, Arena.Center - p90, Arena.Center - p];
        var firstActivation = Module.CastFinishAt(spell).AddSeconds(FirstDashDelay);
        for (var i = 0; i < 3; ++i)
            AddLane(points[i], points[i + 1], firstActivation.AddSeconds(i * DashInterval), caster.InstanceID);
        _phaseExpires = firstActivation.AddSeconds(2d * DashInterval + FinalDashGrace);
        Service.Logger.Information($"[CE210] HellwardBound lanes={_lanes.Count} p=({p.X:f1},{p.Z:f1}) firstAct={firstActivation:O}");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (caster != Module.PrimaryActor || (spell.Action.ID & 0xFFFF) != (uint)AID.HellwardBoundHit || !DashPhaseActive
            || spell.GlobalSequence != 0 && !_seenHitSequences.Add(spell.GlobalSequence))
            return;

        Service.Logger.Information($"[CE210] HellwardBound hit id={spell.Action.ID:X} lanes={_lanes.Count} active={DashPhaseActive}");
        if (_lanes.Count != 0)
            _lanes.RemoveAt(0);
        if (_lanes.Count == 0)
            _phaseExpires = WorldState.FutureTime(FinalDashGrace);
    }

    private void AddLane(WPos start, WPos end, DateTime activation, ulong actorID)
    {
        var direction = end - start;
        var rotation = Angle.FromDirection(direction);
        var shape = new AOEShapeRect(direction.Length(), HalfWidth);
        _lanes.Add(new(shape, start, rotation, activation, actorID: actorID, shapeDistance: shape.Distance(start, rotation)));
    }
}

sealed class AcceptNoImitatorsStates : StateMachineBuilder
{
    public AcceptNoImitatorsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LethalBoundary>()
            .ActivateOnEnter<MorphingMageAOEs>()
            .ActivateOnEnter<MadeMagic>()
            .ActivateOnEnter<BlackenedRain>()
            .ActivateOnEnter<DarkDealing>()
            .ActivateOnEnter<HellwardBound>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(AcceptNoImitatorsStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 63u,
    SortOrder = 9)]
// Replay-verified 25y circular arena (players reach r24.8, the charge ends at r25 and the fence
// kills at 24.6); the old 20y circle clipped the charge and misplaced the boundary drawing.
public sealed class AcceptNoImitators(WorldState ws, Actor primary) : BossModule(ws, primary, new(500f, -310f), new ArenaBoundsCircle(25f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (FindComponent<HellwardBound>()?.DashPhaseActive == true)
            hints.GoalZones.Add(AIHints.GoalProximity(Arena.Center, 20f, 5f));
    }
}
