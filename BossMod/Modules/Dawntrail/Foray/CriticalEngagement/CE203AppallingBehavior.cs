using BossMod.Dawntrail.Foray.CriticalEngagement;

namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE203AppallingBehavior;

public enum OID : uint
{
    Boss = 0x4D8F, // R3.0, BNpcName 14714, Pallmagia
    Pallkeeper = 0x4D90, // BNpcName 14715
    Anchor = 0x4D91, // non-targetable Pallmagia controller
    Helper = 0x233C,
    RouletteInnerGuide = 0x1EC02B, // event object; EAnim selects the two opposite inner sectors
    RouletteOuterGuide = 0x1EC02C // event object; EAnim selects the two opposite outer sectors
}

public enum TetherID : uint
{
    EsotericOrder = 0xE // Pallkeeper -> boss, emitted in execution order during C26D/C26E
}

public enum AID : uint
{
    ElectricBoundary = 0xC26B, // anchor, persistent arena-control pulse (not an 18-25y donut)
    Summon = 0xC26C,
    EsotericInstruction = 0xC26D,
    EsotericInstructionReverse = 0xC26E,
    ReversePolarity = 0xC26F,

    BadBreathKeeperVisual = 0xC270, // Pallkeeper, self-only visual immediately after C271 resolves
    BadBreathInstruction = 0xC271, // helper, 50y 100-degree cone
    PlaincrackerKeeperVisual = 0xC272, // Pallkeeper, self-only visual immediately after C273 resolves
    PlaincrackerInstruction = 0xC273, // helper, 30y circle

    SwapOpposites = 0xC278, // four Pallkeepers teleport to the opposite cardinal point
    SwapClockwise = 0xC279, // north/south Pallkeepers teleport clockwise
    SwapCounterclockwise = 0xC27A, // east/west Pallkeepers teleport counterclockwise

    Roulette = 0xC27B,
    RouletteCenter = 0xC27C, // helper, 5y center cell
    RouletteInner = 0xC27D, // helper, 5-12y 120-degree donut sector; two opposite helpers
    RouletteOuter = 0xC27E, // helper, 12-20y 90-degree donut sector; two opposite helpers

    LilliputianLyric = 0xC27F,
    LilliputianLyricAOE = 0xC280, // helper, 40y 180-degree cone
    MagicHammer = 0xC281,
    MagicHammerAOE = 0xC282, // helper->location, 8y circle
    OccultMissile = 0xC283,
    OccultMissileAOE = 0xC285, // helper->location, 6y circle
    GreatWhirlwind = 0xC286,
    GreatWhirlwindVisual = 0xC287,
    GreatWhirlwindHit = 0xC512,

    BadBreath = 0xC53A,
    BadBreathAOE = 0xC53B, // helper, 50y 100-degree cone
    Plaincracker = 0xC53C,
    PlaincrackerAOE = 0xC53D, // helper, 15y circle
    AutoAttack = 0xC53E
}

// Helpers can be teleported (and, after Reverse Polarity, swapped to the opposite keeper) between
// cast-start and effect. Keep the activation from the cast packet, but follow the live helper for
// self-targeted shapes instead of freezing the initial, often deliberately fake, coordinates.
sealed class AppallingAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private readonly record struct AOEConfig(AOEShape Shape, bool LocationTargeted = false);
    private sealed record Pending(uint ActionID, ulong ActorID, AOEShape? Shape, WPos Origin, Angle Rotation, DateTime Activation, bool FollowCaster, bool PredictedInstruction = false, int InstructionSlot = -1);

    private static readonly AOEShapeCone BadBreath = new(50f, 50f.Degrees());
    private static readonly AOEShapeCircle PlaincrackerLarge = new(30f);
    private static readonly AOEShapeCircle PlaincrackerSmall = new(15f);
    private static readonly AOEShapeCone Lilliputian = new(40f, 90f.Degrees());
    private static readonly AOEShapeCircle Hammer = new(8f);
    private static readonly AOEShapeCircle Missile = new(6f);
    private readonly List<Pending> _pending = [];
    private readonly List<AOEInstance> _displayed = [];
    private readonly HashSet<uint> _seenSequences = [];
    private readonly HashSet<ulong> _instructionSources = [];
    // C26E does not identify its first shape. Keep its ordered tether slots hidden until the first
    // helper cast supplies the polarity, then calibrate all four slots without losing swap targets.
    private DateTime _instructionFirstActivation;
    private bool? _instructionCircleFirst;
    private int _instructionTethers;
    private bool _instructionReverse;

    private static AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.BadBreathInstruction or (uint)AID.BadBreathAOE => new(BadBreath),
        (uint)AID.PlaincrackerInstruction => new(PlaincrackerLarge),
        (uint)AID.PlaincrackerAOE => new(PlaincrackerSmall),
        (uint)AID.LilliputianLyricAOE => new(Lilliputian),
        (uint)AID.MagicHammerAOE => new(Hammer, true),
        (uint)AID.OccultMissileAOE => new(Missile, true),
        _ => null
    };

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Prune();
        _displayed.Clear();
        var ordered = _pending.Where(p => p.Shape != null).OrderBy(p => p.Activation).ToArray();
        // Magic Hammer resolves as three one-second-spaced batches. If only the current batch is
        // risky, AI picks a technically safe point that is already covered by the second batch and
        // cannot leave the 8y circle in time. Keep the current and next batch forbidden so it plans
        // the step pattern; including all three at once would incorrectly erase every useful route.
        var riskWindow = ordered.Length > 0 && ordered[0].ActionID == (uint)AID.MagicHammerAOE ? 1.25d : 0.25d;
        var riskyDeadline = ordered.Length > 0 ? ordered[0].Activation.AddSeconds(riskWindow) : DateTime.MinValue;
        foreach (var pending in ordered)
        {
            var shape = pending.Shape!;
            var source = pending.FollowCaster ? WorldState.Actors.Find(pending.ActorID) : null;
            var origin = source?.Position ?? pending.Origin;
            // 学习指令的锥在 Pallkeeper 传送 (磁场转换) 后必须朝向场中, 而不是沿用 Pallkeeper 的
            // 旧朝向 - 否则 Swap 后锥形指令会画错方向.
            var rotation = pending.FollowCaster && pending.ActionID is (uint)AID.BadBreathInstruction or (uint)AID.BadBreathAOE
                ? Angle.FromDirection(Module.Arena.Center - origin)
                : source?.Rotation ?? pending.Rotation;
            var imminent = pending.Activation <= riskyDeadline;
            _displayed.Add(new(shape, origin, rotation, pending.Activation,
                imminent ? Colors.Danger : Colors.AOE, imminent, pending.ActorID, shape.Distance(origin, rotation)));
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // During Reverse Esoteric Instruction the four keepers have already been assigned and
        // are moving to their swap destinations, but the first C271/C273 cast does not reveal
        // polarity yet. Keep the AI in the central 5y safe cell until that first real cast;
        // otherwise the generic melee target fallback leaves it stationary at the boss and it
        // gets caught when the charge sequence starts.
        if (_instructionReverse && _instructionCircleFirst == null && _instructionTethers > 0
            && WorldState.CurrentTime <= _instructionFirstActivation)
        {
            // This is an immediate navigation constraint; the state/timeout above controls its
            // lifetime. Passing the future activation here would leave the center fallback active
            // until the first mechanic actually resolves.
            hints.AddForbiddenZone(new AOEShapeDonut(5f, 40f), Arena.Center);
        }

        foreach (ref readonly var aoe in ActiveAOEs(slot, actor))
        {
            // Hammer and learned instructions are ordered movement puzzles. Later previews are not
            // currently dangerous for display purposes, but pathfinding needs their activation
            // times now or it can choose a dead-end safe spot for the preceding hit.
            // Occult missile spawns 4-per-wave for three consecutive waves; only marking the risky
            // (0.25s) wave lets AI stand inside a future wave and get hit. Forbid all waves.
            if (aoe.Risky || ReferenceEquals(aoe.Shape, Hammer) || ReferenceEquals(aoe.Shape, BadBreath) || ReferenceEquals(aoe.Shape, PlaincrackerLarge) || ReferenceEquals(aoe.Shape, Missile))
                hints.AddForbiddenZone(aoe.ShapeDistance ?? aoe.Shape.Distance(aoe.Origin, aoe.Rotation), aoe.Activation);
        }
    }

    public override void Update() => Prune();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.EsotericInstruction or (uint)AID.EsotericInstructionReverse)
        {
            _pending.RemoveAll(p => p.InstructionSlot >= 0);
            _instructionSources.Clear();
            _instructionTethers = 0;
            // 连线时无法预知圆/锥顺序 (实测第一轮 Normal 是锥先, 第二轮 Reverse 也不同) - 一律
            // 等第一个 BadBreath/Plaincracker 施法校准, 否则连线阶段会画出错误顺序的危险区.
            _instructionCircleFirst = null;
            var reverse = spell.Action.ID == (uint)AID.EsotericInstructionReverse;
            _instructionReverse = reverse;
            _instructionFirstActivation = Module.CastFinishAt(spell, reverse ? 12.7d : 6.3d);
            return;
        }

        if (ConfigFor(spell.Action.ID) is not { } config || spell.EventHappened)
            return;

        var activation = Module.CastFinishAt(spell);
        if (activation <= WorldState.CurrentTime)
            return;

        if (spell.Action.ID is (uint)AID.BadBreathInstruction or (uint)AID.PlaincrackerInstruction && UpdateInstructionPrediction(caster, spell, config.Shape, activation))
            return;

        _pending.RemoveAll(p => p.ActionID == spell.Action.ID && p.ActorID == caster.InstanceID);
        var origin = config.LocationTargeted ? spell.LocXZ : caster.Position;
        _pending.Add(new(spell.Action.ID, caster.InstanceID, config.Shape, origin, spell.Rotation, activation, !config.LocationTargeted));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.EventHappened && spell.Action.ID is not (uint)AID.BadBreathInstruction and not (uint)AID.PlaincrackerInstruction)
            _pending.RemoveAll(p => p.ActionID == spell.Action.ID && p.ActorID == caster.InstanceID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SwapOpposites or (uint)AID.SwapClockwise or (uint)AID.SwapCounterclockwise)
        {
            UpdateInstructionDestination(caster, spell);
            return;
        }

        if (ConfigFor(spell.Action.ID) == null || spell.GlobalSequence != 0 && !_seenSequences.Add(spell.GlobalSequence))
            return;

        var removed = _pending.RemoveAll(p => p.ActionID == spell.Action.ID && p.ActorID == caster.InstanceID);
        if (removed == 0 && spell.Action.ID is (uint)AID.BadBreathInstruction or (uint)AID.PlaincrackerInstruction)
        {
            var index = EarliestInstructionSlot();
            if (index >= 0)
                _pending.RemoveAt(index);
        }
        ++NumCasts;
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.EsotericOrder || source.OID != (uint)OID.Pallkeeper || _instructionFirstActivation == default
            || _instructionTethers >= 4 || !_instructionSources.Add(source.InstanceID))
            return;

        var slot = _instructionTethers++;
        var circle = _instructionCircleFirst is { } circleFirst && circleFirst == (slot % 2 == 0);
        var actionID = _instructionCircleFirst == null ? 0 : circle ? (uint)AID.PlaincrackerInstruction : (uint)AID.BadBreathInstruction;
        AOEShape? shape = _instructionCircleFirst == null ? null : circle ? PlaincrackerLarge : BadBreath;
        var activation = _instructionFirstActivation.AddSeconds(4.5d * slot);
        _pending.Add(new(actionID, source.InstanceID, shape, source.Position, source.Rotation, activation, true, true, slot));
    }

    public override void OnActorDestroyed(Actor actor) => _pending.RemoveAll(p => p.ActorID == actor.InstanceID);

    private void Prune()
    {
        var now = WorldState.CurrentTime;
        _pending.RemoveAll(p => now > p.Activation.AddSeconds(2d));
    }

    private bool UpdateInstructionPrediction(Actor caster, ActorCastInfo spell, AOEShape shape, DateTime activation)
    {
        if (_instructionCircleFirst == null)
            CalibrateInstruction(spell.Action.ID == (uint)AID.PlaincrackerInstruction, activation);

        var index = EarliestInstructionSlot(predictedOnly: true);
        if (index < 0)
            return false;

        _pending[index] = _pending[index] with
        {
            ActionID = spell.Action.ID,
            ActorID = caster.InstanceID,
            Shape = shape,
            Origin = caster.Position,
            Rotation = spell.Rotation,
            Activation = activation,
            FollowCaster = true,
            PredictedInstruction = false
        };
        return true;
    }

    private void CalibrateInstruction(bool circleFirst, DateTime firstActivation)
    {
        _instructionCircleFirst = circleFirst;
        _instructionReverse = false;
        _instructionFirstActivation = firstActivation;
        for (var i = 0; i < _pending.Count; ++i)
        {
            var pending = _pending[i];
            if (pending.InstructionSlot < 0)
                continue;

            var circle = circleFirst == (pending.InstructionSlot % 2 == 0);
            _pending[i] = pending with
            {
                ActionID = circle ? (uint)AID.PlaincrackerInstruction : (uint)AID.BadBreathInstruction,
                Shape = circle ? PlaincrackerLarge : BadBreath,
                Activation = firstActivation.AddSeconds(4.5d * pending.InstructionSlot)
            };
        }
    }

    private void UpdateInstructionDestination(Actor caster, ActorCastEvent spell)
    {
        var index = _pending.FindIndex(p => p.PredictedInstruction && p.ActorID == caster.InstanceID);
        if (index < 0)
            return;

        var distance = spell.Action.ID == (uint)AID.SwapOpposites ? 40f : 20f * MathF.Sqrt(2f);
        var origin = caster.Position + distance * spell.Rotation.ToDirection();
        _pending[index] = _pending[index] with
        {
            Origin = origin,
            Rotation = Angle.FromDirection(Module.Arena.Center - origin),
            FollowCaster = false
        };
    }

    private int EarliestInstructionSlot(bool predictedOnly = false)
    {
        var result = -1;
        for (var i = 0; i < _pending.Count; ++i)
        {
            var candidate = _pending[i];
            if (candidate.InstructionSlot >= 0 && (!predictedOnly || candidate.PredictedInstruction)
                && (result < 0 || candidate.Activation < _pending[result].Activation))
                result = i;
        }
        return result;
    }
}

// C26B is the persistent electric fence at the arena edge. The official Action sheet (0xC26B,
// eff=10 donut, xAxis=25) puts the outer kill ring at 25y; keep the inner edge at the 20y
// walkable circle so the danger band sits exactly on the fence.
sealed class ElectricBoundary(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut Shape = new(19.5f, 25f);
    private readonly AOEInstance[] _aoe = [new(Shape, module.Arena.Center)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // The 19.5-25 donut gets clipped to the 20y walkable circle, leaving only a sliver that is
        // effectively invisible. Draw a visible 19-20 band plus the fence outline so the kill ring
        // reads clearly.
        Arena.ZoneDonut(Arena.Center, 19f, 20f, Colors.Danger);
        Arena.ZoneCircleOutlineUnclipped(Arena.Center, 20f, Colors.Danger, 3f);
    }
}

// Death Roulette is a polar grid, not the C26B 18-25y donut that used to be drawn here. ARR v5
// recordings show five fixed helpers owned by the boss: boss+6 is the center, boss+37/+38 are the
// opposite inner sectors and boss+39/+40 the opposite outer sectors. Their 0x022A movement packets
// update position/rotation immediately before the five consecutive C27C-C27E effects, so build the
// cells from the live actors and clear the whole snapshot atomically after all five sequences.
sealed class DeathRouletteGrid(BossModule module) : Components.GenericAOEs(module)
{
    // Keep a small movement margin around the replay-verified action geometry. Roulette resolves
    // as five almost consecutive effects, so aiming exactly at a cell edge is not reliable for AI.
    private static readonly AOEShapeCircle CenterCell = new(5.5f);
    // Replay hit coordinates put inner-ring victims as far as 56.1 degrees from the helper's
    // facing. The action sectors are therefore 120/90 degrees wide (the shape API takes a
    // half-angle), rather than the accidentally halved 60/45-degree display used previously.
    // 中间圈是 6 格 (每格 60°), 危险区按 2 个对侧格 (120°) 建模, 半角 60°.
    private static readonly AOEShapeDonutSector InnerCell = new(4.5f, 12.5f, 60f.Degrees());
    // 第三圈 (外圈) 8 格 (每格 45°): 两个对侧安全区各 45° (合计 2 格), 危险覆盖其余 6 格 (270°).
    // 两个对侧危险扇区各 135° 宽 (半角 67.5°), 中间留两个 45° 缺口.
    private static readonly AOEShapeDonutSector OuterCell = new(11.5f, 20.5f, 67.5f.Degrees());
    private readonly List<AOEInstance> _displayed = [];
    private readonly HashSet<uint> _seenSequences = [];
    private readonly Dictionary<ulong, Angle> _orientationBaseline = [];
    private Angle? _innerDirection;
    private Angle? _outerDirection;
    private DateTime _activation;
    private int _resolvedCells;
    private bool _armed;
    private bool _directionsFresh;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Prune();
        _displayed.Clear();
        if (!_armed)
            return CollectionsMarshal.AsSpan(_displayed);

        // The center cell is unconditional and must be available from the roulette cast even if
        // one of the four sector helpers has not spawned/streamed in yet.
        Add(CenterCell, default, true);
        var inner1 = Helper(37);
        var inner2 = Helper(38);
        var outer1 = Helper(39);
        var outer2 = Helper(40);
        if (inner1 == null || inner2 == null || outer1 == null || outer2 == null)
            return CollectionsMarshal.AsSpan(_displayed);

        if (_innerDirection is { } inner && _outerDirection is { } outer)
        {
            Add(InnerCell, inner, true, inner1.InstanceID);
            Add(InnerCell, inner + 180f.Degrees(), true, inner2.InstanceID);
            Add(OuterCell, outer, true, outer1.InstanceID);
            Add(OuterCell, outer + 180f.Degrees(), true, outer2.InstanceID);
        }
        else
        {
            UpdateDirectionFreshness(inner1, inner2, outer1, outer2);
            if (_directionsFresh)
            {
                Add(InnerCell, inner1.Rotation, true, inner1.InstanceID);
                Add(InnerCell, inner2.Rotation, true, inner2.InstanceID);
                Add(OuterCell, outer1.Rotation, true, outer1.InstanceID);
                Add(OuterCell, outer2.Rotation, true, outer2.InstanceID);
            }
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_armed)
            hints.Add("Death roulette: watch the polar grid");
    }

    public override void Update() => Prune();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Roulette && !spell.EventHappened)
            Arm(Module.CastFinishAt(spell, 14.38f));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        var p2 = state & 0xFFFF;
        if (actor.OID == (uint)OID.RouletteInnerGuide)
        {
            _innerDirection = p2 switch
            {
                0x20 => actor.Rotation + 60f.Degrees(),
                0x10 => actor.Rotation,
                _ => _innerDirection
            };
        }
        else if (actor.OID == (uint)OID.RouletteOuterGuide)
        {
            _outerDirection = p2 switch
            {
                0x10 => actor.Rotation - 45f.Degrees(),
                0x20 => actor.Rotation - 90f.Degrees(),
                _ => _outerDirection
            };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Roulette)
        {
            if (!_armed)
                Arm(WorldState.FutureTime(14.38d));
            return;
        }

        if (spell.Action.ID is < (uint)AID.RouletteCenter or > (uint)AID.RouletteOuter || spell.GlobalSequence != 0 && !_seenSequences.Add(spell.GlobalSequence))
            return;

        if (++_resolvedCells >= 5)
            Clear();
        ++NumCasts;
    }

    private Actor? Helper(ulong offset)
    {
        var actor = WorldState.Actors.Find(Module.PrimaryActor.InstanceID + offset);
        return actor?.OID == (uint)OID.Helper ? actor : null;
    }

    private void Add(AOEShape shape, Angle rotation, bool risky, ulong actorID = 0)
        => _displayed.Add(new(shape, Module.Arena.Center, rotation, _activation, risky ? Colors.Danger : Colors.AOE, risky, actorID, shape.Distance(Module.Arena.Center, rotation)));

    private void Arm(DateTime activation)
    {
        _armed = true;
        _activation = activation;
        _resolvedCells = 0;
        _seenSequences.Clear();
        _orientationBaseline.Clear();
        _innerDirection = null;
        _outerDirection = null;
        foreach (var offset in new ulong[] { 37, 38, 39, 40 })
            if (Helper(offset) is { } helper)
                _orientationBaseline[helper.InstanceID] = helper.Rotation;
        _directionsFresh = false;
    }

    private void Clear()
    {
        _armed = false;
        _resolvedCells = 0;
        _displayed.Clear();
        _orientationBaseline.Clear();
        _innerDirection = null;
        _outerDirection = null;
        _directionsFresh = false;
    }

    private void UpdateDirectionFreshness(params Actor[] helpers)
    {
        if (_directionsFresh || _orientationBaseline.Count != 4)
            return;

        _directionsFresh = helpers.All(helper => _orientationBaseline.TryGetValue(helper.InstanceID, out var baseline)
            && Math.Abs((helper.Rotation - baseline).Normalized().Rad) > 1f.Degrees().Rad);
    }

    private void Prune()
    {
        if (_armed && WorldState.CurrentTime > _activation.AddSeconds(1d))
            Clear();
    }
}

// The three C512 helper casts each hit the raid; the boss cast is the stable warning packet.
sealed class GreatWhirlwind(BossModule module) : Components.RaidwideCast(module, (uint)AID.GreatWhirlwind);

sealed class AppallingBehaviorStates : StateMachineBuilder
{
    public AppallingBehaviorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricBoundary>()
            .ActivateOnEnter<AppallingAOEs>()
            .ActivateOnEnter<DeathRouletteGrid>()
            .ActivateOnEnter<GreatWhirlwind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(AppallingBehaviorStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    TetherIDType = typeof(TetherID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 59u,
    SortOrder = 2)]
public sealed class AppallingBehavior(WorldState ws, Actor primary) : BossModule(ws, primary, new(807f, -562f), new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Pallkeeper));
    }
}
