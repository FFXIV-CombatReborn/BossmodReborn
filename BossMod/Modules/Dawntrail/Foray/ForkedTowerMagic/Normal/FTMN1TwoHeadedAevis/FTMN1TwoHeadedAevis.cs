using BossMod.Dawntrail.Foray.CriticalEngagement;

namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

// Normal 魔之塔 Boss1: Two-Headed Aevis. 蓝头剧毒吐息 18m 圈、双头恐惧 40x10 直条、
// 雷/冰簇 15m 圈、雷霜暴风雨把场上剩余球全部 15m 圈、魔法阵信标 60x5 直条、冰焰凝环。
sealed class TwoHeadedAevisAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCircle PoisonBreath = new(18f);
    private static readonly AOEShapeRect TwoTerrors = new(40f, 5f);
    private static readonly AOEShapeCircle OrbBurst = new(15f);
    private static readonly AOEShapeRect ArcaneBeacon = new(30f, 2.5f, 30f);
    private static readonly AOEShapeCircle Blaze = new(5f);
    private static readonly AOEShapeDonut BlazeLoop = new(5f, 60f);

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.Ability_PoisonBreath => new(PoisonBreath, true),
        (uint)AID.Ability_LightningCluster or (uint)AID.Ability_IceCluster1 => new(OrbBurst, true),
        (uint)AID.Ability_TwoTerrors1 => new(TwoTerrors, true),
        (uint)AID.Ability_Shock => new(OrbBurst),
        (uint)AID.Ability_HypothermalCombustion => new(OrbBurst),
        (uint)AID.Ability_ArcaneBeacon => new(ArcaneBeacon),
        (uint)AID.Ability_Blaze1 or (uint)AID.Ability_Blaze3 or (uint)AID.Ability_Blaze5 => new(Blaze, true),
        (uint)AID.Ability_Blazeloop1 => new(BlazeLoop),
        _ => null
    };
}

// 50697/50698 是选择球的 8s 预兆；真正的球在选择判定后才开始短读条。
// ARR 显示 selector 的落点贴着目标球，因此在长读条开始时就把对应球标成危险。
// 雷/冰簇与雷霜暴风雨共享的球追踪器：
// - 雷簇(50697)/冰簇(50698)读条时，把 EffectPosition 15m 内所有对应属性的球画 15m 危险圈（每次通常 2 个），并从未爆列表移除；
// - 雷霜暴风雨(47735)读条时，把场上所有剩余球画 15m 危险圈，然后清空。
// 与可达鸭脚本一致（50697 雷簇/50698 冰簇按 EffectPosition 匹配球；47735 引爆剩余球）。
sealed class OrbExplosions(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(15f);
    private readonly List<AOEInstance> _displayed = [with(16)];
    private readonly Dictionary<ulong, (uint OID, WPos Pos)> _balls = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_displayed);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.BallLightning or (uint)OID.SwirlingOrb)
            _balls[actor.InstanceID] = (actor.OID, actor.Position);
    }

    public override void OnActorDestroyed(Actor actor) => _balls.Remove(actor.InstanceID);

    public override void OnActorDeath(Actor actor) => _balls.Remove(actor.InstanceID);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.EventHappened)
            return;
        var wanted = spell.Action.ID switch
        {
            (uint)AID.Ability_LightningCluster => (uint)OID.BallLightning, // 雷簇 -> 雷球
            (uint)AID.Ability_IceCluster1 => (uint)OID.SwirlingOrb,        // 冰簇 -> 冰球
            (uint)AID.Ability_ThunderfrostTempest => 0u,                   // 雷霜暴风雨 -> 所有剩余球
            _ => 0xFFFFFFFFu
        };
        if (wanted == 0xFFFFFFFFu)
            return;

        var activation = Module.CastFinishAt(spell, 2d);
        _displayed.RemoveAll(a => WorldState.CurrentTime > a.Activation);
        if (wanted == 0u)
        {
            // 雷霜暴风雨：引爆所有剩余球
            foreach (var (_, ball) in _balls)
                _displayed.Add(new(Shape, ball.Pos, activation: activation));
            _balls.Clear();
        }
        else
        {
            // 雷簇/冰簇：引爆 EffectPosition 15m 内所有对应属性的球
            var triggered = _balls.Where(kv => kv.Value.OID == wanted && (kv.Value.Pos - spell.LocXZ).LengthSq() <= 15f * 15f).Select(kv => kv.Key).ToList();
            foreach (var id in triggered)
            {
                _displayed.Add(new(Shape, _balls[id].Pos, activation: activation));
                _balls.Remove(id);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Ability_LightningCluster or (uint)AID.Ability_IceCluster1 or (uint)AID.Ability_ThunderfrostTempest)
            _displayed.RemoveAll(a => WorldState.CurrentTime >= a.Activation);
    }

    public override void Update()
    {
        _displayed.RemoveAll(a => WorldState.CurrentTime > a.Activation.AddSeconds(1d));
        base.Update();
    }
}

// 风暴吐息（48243）：从场中向外击退 14y。
// 47616 是同一时间出现的视觉 cast，不是额外击退源；只画真实的 48243 单条中心击退线。
sealed class StormsBreathKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private const float Distance = 14f;
    private readonly List<Knockback> _knockbacks = [with(2)];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_knockbacks);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.EventHappened || spell.Action.ID != (uint)AID.Ability_StormsBreathAOE)
            return;

        _knockbacks.Clear();
        _knockbacks.Add(new(Module.Arena.Center, Distance, Module.CastFinishAt(spell), actorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Ability_StormsBreathAOE)
            _knockbacks.RemoveAll(kb => kb.ActorID == caster.InstanceID);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_knockbacks.Count == 0)
            return;

        ref readonly var knockback = ref CollectionsMarshal.AsSpan(_knockbacks)[0];
        if (!IsImmune(slot, knockback.Activation))
            hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, knockback.Origin, Distance, 20f), knockback.Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_knockbacks.Count != 0)
            Arena.ZoneCircle(Module.Arena.Center, 5f, Colors.Safe);
    }
}

// 定时诅咒（5403 东风 / 5404 西风）：中诅咒后 13s 沿固定方向击退 15y。
// StatusAdd 立即画击退箭头，并在落点画绿色圆环安全区，方便提前站位。
sealed class TimedCurseKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private const float Distance = 15f;
    private const double FallbackDelay = 13d;
    private const double LandingAOEWindow = 3d;
    private readonly List<Knockback> _knockbacks = [with(8)];
    private readonly List<Knockback> _filtered = [with(2)];
    private readonly List<Components.GenericAOEs.AOEInstance> _landingAOEs = [with(16)];
    private readonly TwoHeadedAevisAOEs _castAOEs = module.FindComponent<TwoHeadedAevisAOEs>()!;
    private readonly OrbExplosions _orbAOEs = module.FindComponent<OrbExplosions>()!;

    // 定时诅咒是全员同时中的状态：若不按 ActorID 过滤，GenericKnockback 会把全队每个人的
    // 击退线都从本地玩家脚下画出，24 条红线铺满全场（可达鸭脚本用 TargetId==Me + Owner=Me 过滤）。
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _filtered.Clear();
        var span = CollectionsMarshal.AsSpan(_knockbacks);
        for (var i = 0; i < span.Length; ++i)
            if (span[i].ActorID == actor.InstanceID)
                _filtered.Add(span[i]);
        return CollectionsMarshal.AsSpan(_filtered);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var knockbacks = ActiveKnockbacks(slot, actor);
        if (knockbacks.Length != 0)
        {
            CollectLandingAOEs(slot, actor, knockbacks[0].Activation);
            foreach (var aoe in _landingAOEs)
                if (aoe.Check(pos))
                    return true;
        }
        return !Arena.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var knockbacks = ActiveKnockbacks(slot, actor);
        if (knockbacks.Length == 0)
            return;

        ref readonly var knockback = ref knockbacks[0];
        if (IsImmune(slot, knockback.Activation))
            return;

        CollectLandingAOEs(slot, actor, knockback.Activation);
        var direction = knockback.Distance * knockback.Direction.ToDirection();
        hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirectionPlusMixedAOEs(
            Arena.Center, direction, 19.5f, [.. _landingAOEs], _landingAOEs.Count), knockback.Activation);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var direction = status.ID switch
        {
            (uint)SID.EasterlyReprise => -90f.Degrees(), // 东风 -X
            (uint)SID.WesterlyReprise => 90f.Degrees(), // 西风 +X
            _ => (Angle?)null
        };
        if (direction is not { } dir)
            return;

        // ARR status duration is the authoritative hit time.  The fixed 13s value is retained
        // only for truncated replay packets that do not carry a usable expiration timestamp.
        var activation = status.ExpireAt > WorldState.CurrentTime
            ? status.ExpireAt
            : WorldState.FutureTime(FallbackDelay);
        _knockbacks.RemoveAll(kb => kb.ActorID == actor.InstanceID);
        _knockbacks.Add(new(actor.Position, Distance, activation, default, dir, Kind.DirForward, actorID: actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.EasterlyReprise or (uint)SID.WesterlyReprise)
            _knockbacks.RemoveAll(kb => kb.ActorID == actor.InstanceID);
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        _knockbacks.RemoveAll(kb => now > kb.Activation.AddSeconds(1d));
        base.Update();
    }

    private void CollectLandingAOEs(int slot, Actor actor, DateTime activation)
    {
        _landingAOEs.Clear();
        AddLandingAOEs(_castAOEs.ActiveAOEs(slot, actor), activation);
        AddLandingAOEs(_orbAOEs.ActiveAOEs(slot, actor), activation);
    }

    private void AddLandingAOEs(ReadOnlySpan<Components.GenericAOEs.AOEInstance> aoes, DateTime activation)
    {
        foreach (ref readonly var aoe in aoes)
            if (Math.Abs((aoe.Activation - activation).TotalSeconds) <= LandingAOEWindow)
                _landingAOEs.Add(aoe);
    }
}


[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    Contributors = "KanoNoUta (based on Equilius + gynorhino)",
    PrimaryActorOID = (uint)OID.TwoHeadedAevis,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14489u,
    SortOrder = 1,
    Category = BossModuleInfo.Category.Foray,
    Expansion = BossModuleInfo.Expansion.Dawntrail)]
public sealed class TwoHeadedAevis : BossModule
{
    public TwoHeadedAevis(WorldState ws, Actor primary) : base(ws, primary, new(-900f, 700f), new ArenaBoundsSquare(20f))
        => Service.Logger.Information($"[FT] {GetType().Name} created (oid={primary.OID:X})");

    protected override bool CheckPull()
        => base.CheckPull()
        || Enemies((uint)OID.GreenHead1).Any(h => h.IsTargetable && h.InCombat)
        || Enemies((uint)OID.BlueHead1).Any(h => h.IsTargetable && h.InCombat);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var head in Enemies((uint)OID.GreenHead1))
            Arena.Actor(head, allowDeadAndUntargetable: true);
        foreach (var head in Enemies((uint)OID.BlueHead1))
            Arena.Actor(head, allowDeadAndUntargetable: true);
    }
}
