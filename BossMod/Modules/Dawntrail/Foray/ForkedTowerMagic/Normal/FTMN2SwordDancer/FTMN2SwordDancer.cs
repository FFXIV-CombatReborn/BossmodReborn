using BossMod.Dawntrail.Foray.CriticalEngagement;

namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN2SwordDancer;

// Normal 魔之塔 Boss2: Sword Dancer. 秘法剑为施法者前方的 96y 半圆、突进 30x6、旋转月环/钢铁、
// 剑舞直条 60x20。剑刃矩形（ObjectEffect 2015283 四连）会在真实读条前预绘。
sealed class SwordDancerAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    // 可达鸭 + ARR：49585 以 Helper 为圆心、面向 SourceRotation 的 96y 半圆。
    // 宽 96 的矩形在 47.4y 圆场内会把整张场地铺满，不能用 CastType 的矩形默认解读。
    private static readonly AOEShapeCone MartialMystique = new(96f, 90f.Degrees());

    // 49585 的两段交错半场刀可能在 replay 重同步时同时留在 pending；只暴露最早一段，
    // 避免两片相反半圆叠成“全场危险”，并让 AI 先处理当前刀。
    protected override int MaxDisplayed => 1;

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.MartialMystique2 => new(MartialMystique),
        _ => null
    };
}

// 跃进步法的四把剑落点：49595「戳地」是每把剑脚下的 5y 圆形预兆。
// 独立组件可在 replay cast 不完整时仍按实机 cast 事件绘制落点。
sealed class LeapLandingAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pierce, 5f);

sealed class SwordSpinAOEs(BossModule module) : Components.GenericAOEs(module)
{
    // Action data and ARR agree on the real inner/outer radii.  Do not collapse these to a
    // generic 10y inner radius: that turns safe strips into danger and makes BMR run across
    // the following sweep.
    private static readonly AOEShapeDonut SpinDonut = new(15f, 60f);
    private static readonly AOEShapeCircle SpinSmall = new(15f);
    private static readonly AOEShapeCircle SpinLarge = new(20f);
    private static readonly AOEShapeDonutSector TurnInnerWide = new(9f, 14f, 45f.Degrees());
    private static readonly AOEShapeDonutSector TurnOuterWide = new(19f, 24f, 45f.Degrees());
    private static readonly AOEShapeDonutSector TurnInnerNarrow = new(9f, 14f, 32.5f.Degrees());
    private static readonly AOEShapeDonutSector TurnOuterNarrow = new(19f, 24f, 27f.Degrees());
    private readonly List<AOEInstance> _aoes = [with(16)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.EventHappened)
            return;
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.Spin => SpinDonut,
            (uint)AID.Spin1 => SpinSmall,
            (uint)AID.Spin2 => SpinLarge,
            (uint)AID.Turn1 => TurnInnerWide,
            (uint)AID.Turn2 => TurnOuterWide,
            (uint)AID.Turn5 => TurnInnerNarrow,
            (uint)AID.Turnabout => TurnOuterNarrow,
            _ => null
        };
        if (shape == null)
            return;

        // 伤害判定：cast 结束后约 1s（ARR EFF 实测 +0.98s）
        var activation = Module.CastFinishAt(spell, 0.5d);
        _aoes.Add(new(shape, caster.Position, spell.Rotation, activation, actorID: caster.InstanceID,
            shapeDistance: shape.Distance(caster.Position, spell.Rotation)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Spin or (uint)AID.Spin1 or (uint)AID.Spin2
            or (uint)AID.Turn1 or (uint)AID.Turn2 or (uint)AID.Turn5 or (uint)AID.Turnabout)
            _aoes.RemoveAll(a => a.ActorID == caster.InstanceID);
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        _aoes.RemoveAll(a => now > a.Activation.AddSeconds(2d));
        base.Update();
    }
}
// 剑舞（普通）：0x1EC033 事件物件发 EAnim(1,2)，按顺序刷出四条 20x60 剑刃矩形。
// 可达鸭画法：4 条按顺序，第一条立即 6s，之后 6000/8500/11000ms 延迟各持续 2.5s；每条正反两方向。
sealed class SwordBladeRects(BossModule module) : Components.GenericAOEs(module)
{
    // 剑舞判定以事件物件为中心，形成贯穿场中的 60x20 直条。
    private static readonly AOEShapeRect Shape = new(30f, 10f, 30f);
    private readonly List<(ulong ActorID, WPos Position, Angle Rotation, DateTime At)> _warnings = [with(4)];
    private readonly List<AOEInstance> _displayed = [with(8)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_displayed.Count == 0)
            return [];
        return CollectionsMarshal.AsSpan(_displayed)[..1];
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state != 0x00010002 || actor.OID != (uint)OID.Actor1ec033)
            return;
        if (_warnings.Any(w => w.ActorID == actor.InstanceID))
            return;

        _warnings.Add((actor.InstanceID, actor.Position, actor.Rotation, WorldState.CurrentTime));
        if (_warnings.Count < 4)
            return;

        List<(ulong ActorID, WPos Position, Angle Rotation, DateTime At)> rects = [.. _warnings];
        _warnings.Clear();
        for (var i = 0; i < rects.Count; ++i)
        {
            // ARR：第四个标记到齐后，按顺序在 6.4s、8.9s、11.4s、13.9s 结算。
            var activation = WorldState.FutureTime(6.4d + 2.5d * i);
            _displayed.Add(new(Shape, rects[i].Position, rects[i].Rotation, activation: activation, actorID: rects[i].ActorID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.SwordDance6 || _displayed.Count == 0)
            return;

        var now = WorldState.CurrentTime;
        var resolved = _displayed.FindIndex(aoe => aoe.Activation <= now.AddSeconds(0.75d));
        if (resolved >= 0)
            _displayed.RemoveAt(resolved);
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        _warnings.RemoveAll(w => now > w.At.AddSeconds(5d));
        _displayed.RemoveAll(aoe => now > aoe.Activation.AddSeconds(1d));
    }
}
sealed class SwordRush(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(4)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.EventHappened || spell.Action.ID is not ((uint)AID.Rush1 or (uint)AID.Rush2))
            return;

        // 50525/50526 是同一次投剑产生的两个 charge 变体；长度与朝向由各自落点决定。
        var direction = spell.LocXZ - caster.Position;
        var length = direction.Length();
        if (length < 0.1f)
            return;
        var rotation = Angle.FromDirection(direction);
        var shape = new AOEShapeRect(length, 3.5f);
        _aoes.Add(new(shape, caster.Position, rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID,
            shapeDistance: shape.Distance(caster.Position, rotation)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Rush1 or (uint)AID.Rush2)
            _aoes.RemoveAll(a => a.ActorID == caster.InstanceID);
    }

    public override void Update() => _aoes.RemoveAll(a => WorldState.CurrentTime > a.Activation.AddSeconds(1d));
}

// 强袭剑出鞘：每把剑真实开始 49616 读条时独立显示一条 30x6 直线。
// 读条本身按机制顺序错开，因此不要用 Timeline 预排整列，也不要截断为单个 Actor。
sealed class SurgeswordSequence(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RushSurgesword, new AOEShapeRect(30f, 3f));

// 剑气爆发：SID 2056 Extra=0x47B 表示剑已点燃并即将发动击退。
// ARR/上游实测：四次击退按状态顺序，首段距状态出现 10.7s，后续每 2.5s，距离 24y。
sealed class Steelsbreath(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _knockbacks = [with(4)];
    private DateTime _sequenceStart;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_knockbacks.Count == 0)
            return [];
        // 只显示当前即将结算的一段，避免四条同时出现导致 AI 误判。
        return CollectionsMarshal.AsSpan(_knockbacks)[..1];
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID != (uint)SID.LeapingLift || status.Extra != 0x47B)
            return;

        if (_knockbacks.Count == 0)
            _sequenceStart = WorldState.CurrentTime;
        var activation = _sequenceStart.AddSeconds(10.7d + 2.5d * _knockbacks.Count);
        _knockbacks.Add(new(actor.Position, 24f, activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        // 49599 and its helper mirror 50359 finish together. Consuming on both skips
        // every second sword, so only the real Dancing Sword cast advances the queue.
        if (_knockbacks.Count != 0 && spell.Action.ID == (uint)AID.Steelsbreath1)
            _knockbacks.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_knockbacks.Count == 0)
            return;

        var knockbacks = CollectionsMarshal.AsSpan(_knockbacks);
        ref readonly var knockback = ref knockbacks[0];
        if (!IsImmune(slot, knockback.Activation))
        {
            if (knockbacks.Length == 1)
                hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, knockback.Origin, 25f, 24f), knockback.Activation);
            else
                hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOriginIntoCircle(Arena.Center, knockback.Origin, 25f, 24f,
                    knockbacks[1].Origin, 7f), knockback.Activation);
        }
    }
}

// 舞动之剑预判：DancingSword 播 ActionTimeline 9710 时按 ModelState 姿势提前画月环/钢铁（可达鸭一致）。
// pose: 0=小月环(10-40) 4=月环(15-40) 5=大月环(20-40) 6=小钢铁(10) 7=钢铁(15) 31=大钢铁(20)
sealed class DancingSwordPreview(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut DonutSmall = new(10f, 40f);
    private static readonly AOEShapeDonut DonutMid = new(15f, 40f);
    private static readonly AOEShapeDonut DonutLarge = new(20f, 40f);
    private static readonly AOEShapeCircle SteelSmall = new(10f);
    private static readonly AOEShapeCircle SteelMid = new(15f);
    private static readonly AOEShapeCircle SteelLarge = new(20f);
    private readonly List<AOEInstance> _displayed = [with(8)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_displayed);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id != 9710 || actor.OID != (uint)OID.DancingSwordCyclosword)
            return;

        AOEShape? shape = actor.ModelState.ModelState switch
        {
            0 => DonutSmall,
            4 => DonutMid,
            5 => DonutLarge,
            6 => SteelSmall,
            7 => SteelMid,
            31 => SteelLarge,
            _ => null
        };
        if (shape == null)
            return;

        _displayed.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);
        _displayed.Add(new(shape, actor.Position, activation: WorldState.FutureTime(9d), actorID: actor.InstanceID));
    }

    public override void Update()
    {
        _displayed.RemoveAll(a => WorldState.CurrentTime > a.Activation.AddSeconds(1d));
        base.Update();
    }
}
// 场地电网: 圆形场地边缘的电网，红色圆环标出（用户实测直径 ~47.4m）。
sealed class ElectricBoundary(BossModule module) : Components.GenericAOEs(module)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => [];
    public override void DrawArenaForeground(int pcSlot, Actor pc)
        => Arena.ZoneCircleOutlineUnclipped(Arena.Center, 23.7f, Colors.Danger, 3f);
}
[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    Contributors = "KanoNoUta (based on gynorhino)",
    PrimaryActorOID = (uint)OID.SwordDancer,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14820u,
    SortOrder = 2,
    Category = BossModuleInfo.Category.Foray,
    Expansion = BossModuleInfo.Expansion.Dawntrail)]
public sealed class SwordDancer : BossModule
{
    public SwordDancer(WorldState ws, Actor primary) : base(ws, primary, new(600f, 704f), new ArenaBoundsCircle(23.7f))
        => Service.Logger.Information($"[FT] {GetType().Name} created (oid={primary.OID:X})");

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actor(PrimaryActor, allowDeadAndUntargetable: true);
}
