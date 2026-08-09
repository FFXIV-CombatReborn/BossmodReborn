using BossMod.Dawntrail.Foray.CriticalEngagement;

namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN3Necrophobia;

// Normal 魔之塔 Boss3: Necrophobia. 爆炎 18m 圈、冰封十字 45x15、古代暴雷 60y 45 度扇、
// 灭亡射线 30x6 直条、黑暗奔流 60x10 直条 + 左右步进地火、真空波 180 度。
sealed class NecrophobiaAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCircle Fire = new(18f);
    private static readonly AOEShapeCross Blizzard = new(45f, 7.5f);
    private static readonly AOEShapeCone Thunder = new(60f, 22.5f.Degrees());
    private static readonly AOEShapeRect DeathlyRay = new(30f, 3f);
    private static readonly AOEShapeCone Vacuum = new(30f, 90f.Degrees());

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.AncientFireIII or (uint)AID.SeveredFireIII or (uint)AID.AncientFireIII1 => new(Fire),
        (uint)AID.AncientBlizzardIII or (uint)AID.SeveredBlizzardIII or (uint)AID.AncientBlizzardIII1 => new(Blizzard),
        (uint)AID.AncientThunderIII1 or (uint)AID.AncientThunderIII3 => new(Thunder),
        (uint)AID.DeathlyRay => new(DeathlyRay),
        (uint)AID.VacuumWave => new(Vacuum),
        _ => null
    };
}

// 黑暗奔流的场地步进预兆。47478 自身只有 1s 读条，等它出现才画会让 AI 来不及
// 穿过第一条安全缝；ARR 三次样本均为 47477 开始后约 7.6s / 9.7s 判定两轮。
// 47477 的 LocXZ 是约 30y 外的技能目标点，不是直条起点；整组预兆从 boss 脚下展开。
sealed class DarkCurrentTreadPreview(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Shape = new(30f, 5f, 30f);
    private readonly List<AOEInstance> _pending = [with(6)];
    private readonly List<AOEInstance> _displayed = [with(4)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Prune();
        _displayed.Clear();
        if (_pending.Count == 0)
            return [];

        var first = _pending[0].Activation;
        foreach (var pending in _pending)
        {
            var aoe = pending;
            var imminent = aoe.Activation <= first.AddSeconds(0.5d);
            aoe.Risky = imminent;
            aoe.Color = imminent ? Colors.Danger : Colors.AOE;
            _displayed.Add(aoe);
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.DarkCurrent1 || spell.EventHappened)
            return;

        _pending.Clear();
        var origin = Module.PrimaryActor.Position;
        var side = (spell.Rotation + 90f.Degrees()).ToDirection();

        // The first hit is the centered strip through the boss.  The two side waves
        // only become dangerous after this central strip resolves, so all five strips
        // must share one ordered queue instead of being drawn by separate components.
        _pending.Add(new(Shape, origin, spell.Rotation, Module.CastFinishAt(spell)));
        for (var wave = 0; wave < 2; ++wave)
        {
            // ARR: 47478 casts start +6.58/+8.66s and resolve one second later.
            var activation = Module.CastFinishAt(spell, 2.1d + wave * 2.1d);
            var offset = 10f * (wave + 1);
            _pending.Add(new(Shape, origin + side * offset, spell.Rotation, activation));
            _pending.Add(new(Shape, origin - side * offset, spell.Rotation, activation));
        }
        _pending.Sort((left, right) => left.Activation.CompareTo(right.Activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID.DarkCurrent1 or (uint)AID.DarkCurrent2) || _pending.Count == 0)
            return;

        // Two helpers resolve the left/right strips together.  Remove that complete wave on
        // the first event; duplicate helper events then have nothing stale to resurrect.
        var deadline = WorldState.CurrentTime.AddSeconds(1.25d);
        _pending.RemoveAll(aoe => aoe.Activation <= deadline);
    }

    public override void Update() => Prune();

    private void Prune()
    {
        var now = WorldState.CurrentTime;
        _pending.RemoveAll(aoe => now > aoe.Activation.AddSeconds(1d));
    }
}

// 老三场地是圆形电网。ARR/实测边界点约为 Z=776.06 与 Z=823.82，
// 得到中心 (100, 800)、直径 47.76、半径约 23.88；取 23.9 保留边界余量。
sealed class ElectricBoundary(BossModule module) : Components.GenericAOEs(module)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
        => Arena.ZoneCircleOutlineUnclipped(Arena.Center, 23.9f, Colors.Danger, 3f);
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    Contributors = "KanoNoUta (based on gynorhino)",
    PrimaryActorOID = (uint)OID.Necrophobia,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14503u,
    SortOrder = 3,
    Category = BossModuleInfo.Category.Foray,
    Expansion = BossModuleInfo.Expansion.Dawntrail)]
public sealed class Necrophobia : BossModule
{
    public Necrophobia(WorldState ws, Actor primary) : base(ws, primary, new(100f, 800f), new ArenaBoundsCircle(23.9f))
        => Service.Logger.Information($"[FT] {GetType().Name} created (oid={primary.OID:X})");

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actor(PrimaryActor, allowDeadAndUntargetable: true);
}
