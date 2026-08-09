using BossMod.Dawntrail.Foray.CriticalEngagement;
using static BossMod.Components.GenericKnockback;

namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE205GluttonousCursefiend;

public enum OID : uint
{
    Boss = 0x4C4B, // R3.0, BNpcName 14790, Algol
    Tomato = 0x4C4C, // R0.9, Crescent Tomato
    Onion = 0x4C4D, // R0.9, Crescent Onion
    Controller = 0x4D87, // non-targetable mechanic and arena controller
    Helper = 0x233C
}

public enum AID : uint
{
    Cursevoice = 0xBBE4,
    DrawInVisual1 = 0xBBE5,
    DrawInVisual2 = 0xBBE6,
    DrawIn = 0xBBE7,
    DrawInCone = 0xBBE8, // 60y 30-degree cone
    Devour = 0xBBE9,
    SpitTomato = 0xBBEA,
    SpitOnion = 0xBBEB,
    TomatoMiasma1 = 0xBBED, // 50y long, 6y wide rect
    OnionMiasma1 = 0xBBEE, // 60y 30-degree cone
    TomatoMiasma2 = 0xBBEF,
    OnionMiasma2 = 0xBBF0,
    SpinningDrawInCone = 0xBBF1, // 30y 30-degree cone
    SpinningDrawIn = 0xBBF2, // repeated 30y 30-degree cone, 12y draw-in
    SpinningDrawInEnd = 0xBBF3,
    MiasmaBoundary = 0xBBF6, // controller, persistent 20-30y outer deathwall
    GreatMiasmaCannon1 = 0xBBF4, // 40y long, 50y wide rect
    CorruptMiasma1 = 0xBBF5, // 12y circle
    SpinningDrawInNear = 0xBC79, // repeated 7y 30-degree cone, synchronized with SpinningDrawIn
    CursevoiceAlt = 0xBF4B,
    DevourAlt1 = 0xC4F6, // 12y 120-degree cone
    GreatMiasmaCannonVisual = 0xC4F7,
    GreatMiasmaCannon2 = 0xC4F8,
    CorruptMiasma2 = 0xC4F9, // 11y circle
    PiercingScream = 0xC4FA,
    PiercingScreamAlt = 0xC4FB,
    DevourAlt2 = 0xC523,
    DevourShort = 0xC525, // 8y 120-degree cone
    AutoAttack = 0xC5D4,
    SpinningDrawInAlt = 0xC6FE
}

sealed class MiasmaBoundary(BossModule module) : Components.GenericAOEs(module)
{
    // 实测电网边界约 ±23.5y (玩家抓坐标: Z -23.78 / +23.4)。之前按 28y 画大了, 导致躲地火时
    // 跑到 23.5~28 的假安全区踩进电网。电网外圈 30y (解包 EffectRange), 可见部分贴边绘制。
    private static readonly AOEShapeDonut Shape = new(23.5f, 30f);
    private readonly AOEInstance[] _aoe = [new(Shape, module.Arena.Center)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        Arena.ZoneDonut(Arena.Center, 22.5f, 23.5f, Colors.Danger);
        Arena.ZoneCircleOutlineUnclipped(Arena.Center, 23.5f, Colors.Danger, 3f);
    }
}

// These actions all have usable CastStart packets in the replays. Keep the warning active until
// the matching cast resolves; ActionEffect packet loss must not make the pre-cast telegraph vanish.
sealed class AlgolAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    // Action effects hit targets up to 25y on both sides of the helper, so the helper is the center
    // of the 50x6 strip rather than its rear edge.
    private static readonly AOEShapeRect Tomato = new(25f, 3f, 25f);
    private static readonly AOEShapeCone Onion = new(60f, 15f.Degrees());
    private static readonly AOEShapeRect Cannon = new(40f, 25f);
    private static readonly AOEShapeCircle Corrupt12 = new(12f);
    private static readonly AOEShapeCircle Corrupt11 = new(11f);
    private static readonly AOEShapeCone Devour12 = new(12f, 60f.Degrees());
    private static readonly AOEShapeCone Devour8 = new(8f, 60f.Degrees());

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.TomatoMiasma1 or (uint)AID.TomatoMiasma2 => new(Tomato),
        (uint)AID.OnionMiasma1 or (uint)AID.OnionMiasma2 => new(Onion),
        (uint)AID.GreatMiasmaCannon1 or (uint)AID.GreatMiasmaCannon2 => new(Cannon),
        (uint)AID.CorruptMiasma1 => new(Corrupt12, true),
        (uint)AID.CorruptMiasma2 => new(Corrupt11, true),
        (uint)AID.DevourAlt1 or (uint)AID.DevourAlt2 => new(Devour12),
        (uint)AID.DevourShort => new(Devour8),
        _ => null
    };
}

// The normal cone resolves at the end of its cast. Spinning Inhale then emits 15-degree ticks for
// roughly five seconds after the visual cast; predict the next tick instead of dropping the hint at
// cast finish. BBE7/C6FE only pull vegetables and must not drive player movement hints.
// GenericKnockback does not feed the AI any forbidden zones by itself, so AddAIHints is overridden
// here: players pulled by either cone are subsequently devoured, so the safe play is simply to
// never stand in the cone. For the spinning version the sweep advances -15 degrees every 0.2s
// (replay-verified, one full turn), so the AI is kept out of the next few predicted sectors and
// naturally trails behind the sweep.
sealed class AlgolDrawIn(BossModule module) : Components.GenericKnockback(module)
{
    private const float PullDistance = 12f;
    private const double SpinTickInterval = 0.2d;
    private static readonly Angle DefaultSpinStep = -15f.Degrees();
    private static readonly AOEShapeCone LongCone = new(60f, 15f.Degrees());
    private static readonly AOEShapeCone ShortCone = new(30f, 15f.Degrees());
    private static readonly AOEShapeCone SafeCone = new(12f, 15f.Degrees());
    // slightly wider than the drawn cones so the AI keeps a margin from the sweep edge
    private static readonly AOEShapeCone HintCone = new(30f, 22.5f.Degrees());
    private readonly List<Knockback> _active = [with(2)];
    private Knockback? _normal;
    private Knockback? _spinning;
    private Angle _spinInitialDirection;
    private Angle _spinLastDirection;
    private Angle _spinStep;
    private DateTime _spinExpiresAt;
    private bool _spinEnding;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        PruneExpired();
        _active.Clear();
        if (_normal is { } normal)
            _active.Add(normal);
        if (_spinning is { } spinning)
            _active.Add(spinning);
        _active.Sort((left, right) => left.Activation.CompareTo(right.Activation));
        return CollectionsMarshal.AsSpan(_active);
    }

    public override void Update() => PruneExpired();

    // GenericKnockback only draws the pull arrow for the own player; the cones themselves would be
    // invisible on the arena. Draw the active cone (danger) and, for the spinning version, outline
    // the predicted next sector so the sweep direction is readable.
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_normal is { } normal)
            LongCone.Draw(Arena, normal.Origin, normal.Direction);
        if (_spinning is { } spinning)
        {
            var step = _spinStep == default ? DefaultSpinStep : _spinStep;
            ShortCone.Draw(Arena, spinning.Origin, spinning.Direction);
            ShortCone.Outline(Arena, spinning.Origin, spinning.Direction + step);
            // 绿色安全扇形: 危险区起点后面 (已扫过侧), 贴着 boss 跟着绕圈.
            var safeDir = spinning.Direction - step * 3;
            SafeCone.Draw(Arena, spinning.Origin, safeDir, Colors.Safe);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_normal is { } normal)
            hints.AddForbiddenZone(LongCone, normal.Origin, normal.Direction, normal.Activation);
        if (_spinning is { } spinning)
        {
            var step = _spinStep == default ? DefaultSpinStep : _spinStep;
            var direction = spinning.Direction;
            var activation = spinning.Activation;
            for (var i = 0; i < 3; ++i)
            {
                hints.AddForbiddenZone(HintCone, spinning.Origin, direction, activation);
                direction += step;
                activation = activation.AddSeconds(SpinTickInterval);
            }

            // 旋转吸引: 安全位置在危险区起点后面 (已扫过侧), AI 贴着 boss 绕圈走, 而不是追 boss/被吸进去.
            // 目标点 = boss 位置 + 旋转反方向 3 步 (危险区刚扫过的后方), 距离短保证移速能跟上绕圈.
            var safeDir = spinning.Direction - step * 3;
            hints.GoalZones.Add(AIHints.GoalSingleTarget(spinning.Origin + safeDir.ToDirection() * 9f, 4f));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DrawInCone:
                if (!spell.EventHappened)
                    _normal = new(caster.Position, PullDistance, Module.CastFinishAt(spell), LongCone, spell.Rotation, Kind.TowardsOrigin, actorID: caster.InstanceID);
                break;
            case (uint)AID.SpinningDrawInCone:
                if (spell.EventHappened)
                    break;
                _spinInitialDirection = _spinLastDirection = spell.Rotation;
                _spinStep = default;
                _spinEnding = false;
                var activation = Module.CastFinishAt(spell).AddSeconds(SpinTickInterval);
                _spinExpiresAt = activation.AddSeconds(0.75d);
                _spinning = new(caster.Position, PullDistance, activation, ShortCone, spell.Rotation, Kind.TowardsOrigin, actorID: caster.InstanceID);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DrawInCone && _normal is { } normal && normal.ActorID == caster.InstanceID)
            _normal = null;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DrawInCone:
                _normal = null;
                ++NumCasts;
                break;
            case (uint)AID.SpinningDrawIn:
                ++NumCasts;
                if (_spinEnding && spell.Rotation.AlmostEqual(_spinInitialDirection, 2f.Degrees().Rad))
                {
                    ClearSpin();
                    break;
                }

                var step = (spell.Rotation - _spinLastDirection).Normalized();
                if (MathF.Abs(step.Deg) is >= 5f and <= 30f)
                    _spinStep = step;
                _spinLastDirection = spell.Rotation;
                var effectiveStep = _spinStep == default ? DefaultSpinStep : _spinStep;
                var predictedDirection = spell.Rotation + effectiveStep;
                var activation = WorldState.FutureTime(SpinTickInterval);
                _spinExpiresAt = WorldState.FutureTime(0.75d);
                _spinning = new(caster.Position, PullDistance, activation, ShortCone, predictedDirection, Kind.TowardsOrigin, actorID: Module.PrimaryActor.InstanceID);
                break;
            case (uint)AID.SpinningDrawInEnd:
                _spinEnding = true;
                break;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (_normal is { } normal && normal.ActorID == actor.InstanceID)
            _normal = null;
        if (actor.InstanceID == Module.PrimaryActor.InstanceID)
            ClearSpin();
    }

    private void PruneExpired()
    {
        var now = WorldState.CurrentTime;
        if (_normal is { } normal && now > normal.Activation.AddSeconds(0.5d))
            _normal = null;
        if (_spinning != null && now > _spinExpiresAt)
            ClearSpin();
    }

    private void ClearSpin()
    {
        _spinning = null;
        _spinExpiresAt = default;
        _spinEnding = false;
        _spinStep = default;
    }
}

sealed class AlgolRaidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Cursevoice, (uint)AID.CursevoiceAlt, (uint)AID.PiercingScream, (uint)AID.PiercingScreamAlt]);

sealed class GluttonousCursefiendStates : StateMachineBuilder
{
    public GluttonousCursefiendStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MiasmaBoundary>()
            .ActivateOnEnter<AlgolAOEs>()
            .ActivateOnEnter<AlgolDrawIn>()
            .ActivateOnEnter<AlgolRaidwides>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(GluttonousCursefiendStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 54u,
    SortOrder = 4)]
// Replay player positions and the 28.6y boundary hits show the arena is 28y, not 20y; the old
// 20y circle clipped the outer halves of the long fire strips.
public sealed class GluttonousCursefiend(WorldState ws, Actor primary) : BossModule(ws, primary, new(765f, 0f), new ArenaBoundsCircle(23.5f));
