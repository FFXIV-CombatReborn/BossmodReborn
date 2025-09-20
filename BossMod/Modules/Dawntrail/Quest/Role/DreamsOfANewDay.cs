namespace BossMod.Dawntrail.Quest.Role.DreamsOfANewDay;

public enum OID : uint
{
    Boss = 0x4481, // R1.5
    BossP2 = 0x44A1, // R1.5
    TentoawaTheWideEye = 0x447D, // R0.5
    UnboundRavager1 = 0x4484, // R0.5
    UnboundRavager2 = 0x4572, // R0.5
    UnboundRavager3 = 0x4585, // R0.5
    UnboundRavager4 = 0x4587, // R0.5
    UnboundRavager5 = 0x4586, // R0.5
    UnboundRavager6 = 0x4485, // R0.5
    UnboundRavager7 = 0x456F, // R0.5
    UnboundRavager8 = 0x456E, // R0.5
    UnboundRaider1 = 0x4482, // R0.5
    UnboundRaider2 = 0x4483, // R0.5
    UnboundRaider3 = 0x456D, // R0.5
    UnboundRaider4 = 0x456C, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss/TentoawaTheWideEye->player/Boss/TentoawaTheWideEye, no cast, single-target
    AutoAttack2 = 39796, // UnboundRavager4->ZunduWarrior, no cast, single-target
    AutoAttack3 = 873, // UnboundRavager5->ZunduWarrior, no cast, single-target
    Teleport = 39054, // Boss->location, no cast, single-target
    ZunduWarriorVisual = 39033, // Helper->self, no cast, single-target

    BladestormVisual = 39043, // Boss->self, 3.0+1,0s cast, single-target
    Bladestorm = 39044, // Helper->self, 4.0s cast, range 20 90-degree cone
    KeenTempestVisual = 39034, // Boss->self, 5.0s cast, single-target
    KeenTempest = 39035, // Helper->self, 6.0s cast, range 8 circle
    AethericBurstVisual = 39047, // Boss->self, 5.0s cast, single-target
    AethericBurst = 39049, // Helper->self, no cast, range 100 circle
    ConvictionVisual = 39036, // Boss->self, 10.0s cast, single-target, towers
    Conviction = 39037, // Helper->self, 10.0s cast, range 4 circle
    ConvictionFail = 39038, // Helper->self, no cast, range 100 circle

    ActivateShield = 38329, // TentoawaTheWideEye->self, no cast, single-target
    CradleOfTheSleepless = 39050, // BossP2->self, 15.0s cast, range 100 circle
    DaggerflightVisual = 39052, // BossP2->self, 8.0s cast, single-target
    Daggerflight = 39053, // BossP2->player/TentoawaTheWideEye, no cast, single-target
    AetherialRayVisual = 39039, // BossP2->self, 6.0+1,0s cast, single-target
    AetherialRay = 39040, // Helper->self, 7.0s cast, range 40 width 4 rect
    Aethershot = 39042, // Helper->player/TentoawaTheWideEye/LoazenikweTheShutEye, 5.0s cast, range 5 circle, spread
    BloodyTrinity = 39032, // BossP2->players, 5.0s cast, single-target, tankbuster
    AetherialExposure = 39041, // Helper->LoazenikweTheShutEye, 8.0s cast, range 6 circle, stack
    PoisonDaggersVisual = 39045, // BossP2->self, 8.0s cast, single-target
    PoisonDaggers = 39046 // Helper->player/TentoawaTheWideEye/LoazenikweTheShutEye, no cast, single-target
}

sealed class Bladestorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bladestorm, new AOEShapeCone(20f, 45f.Degrees()));
sealed class KeenTempest(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.KeenTempest], 8f);

sealed class AethericBurst(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.AethericBurstVisual, (uint)AID.AethericBurst, 0.9f);
sealed class AetherialExposure(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.AetherialExposure, 6f, 3, 3);
sealed class Conviction(BossModule module) : Components.CastTowers(module, (uint)AID.Conviction, 4f)
{
    private readonly AetherialExposure _stack = module.FindComponent<AetherialExposure>()!;

    public override void Update()
    {
        var count = Towers.Count;
        if (count == 0)
            return;

        var stack = _stack.ActiveStacks.Count != 0;
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            ref var t = ref towers[i];
            t.MinSoakers = t.MaxSoakers = stack ? 0 : 1;
        }
    }
}

sealed class AetherialRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherialRay, new AOEShapeRect(40f, 2f), 10);
sealed class Aethershot(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Aethershot, 5f);
sealed class BloodyTrinity(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.BloodyTrinity);
sealed class PoisonDaggers(BossModule module) : Components.CastInterruptHint(module, (uint)AID.PoisonDaggersVisual);
sealed class Daggerflight(BossModule module) : Components.InterceptTether(module, (uint)AID.DaggerflightVisual)
{
    private DateTime _activation;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.DaggerflightVisual)
            _activation = Module.CastFinishAt(spell, 0.2d);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active)
        {
            var source = Module.PrimaryActor;
            var target = Module.Enemies((uint)OID.TentoawaTheWideEye)[0];
            hints.AddForbiddenZone(new SDInvertedRect(target.Position + (target.HitboxRadius + 0.1f) * target.DirectionTo(source), source.Position, 0.5f), _activation);
        }
    }
}

sealed class CradleOfTheSleepless(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    private static readonly AOEShapeCone cone = new(8f, 60f.Degrees(), invertForbiddenZone: true);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.ActivateShield)
        {
            _aoe = [new(cone, caster.Position, caster.Rotation + 180f.Degrees(), default, Colors.SafeFromAOE)];
        }
        else if (id == (uint)AID.CradleOfTheSleepless)
        {
            _aoe = [];
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Length == 0)
        {
            return;
        }
        ref var aoe = ref _aoe[0];
        hints.Add("Go behind shield or duty fails!", !aoe.Check(actor.Position));
    }
}

abstract class DreamsOfANewDayStates : StateMachineBuilder
{
    public DreamsOfANewDayStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Bladestorm>()
            .ActivateOnEnter<KeenTempest>()
            .ActivateOnEnter<AethericBurst>()
            .ActivateOnEnter<AetherialExposure>()
            .ActivateOnEnter<Conviction>()
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<Aethershot>()
            .ActivateOnEnter<BloodyTrinity>()
            .ActivateOnEnter<PoisonDaggers>()
            .ActivateOnEnter<CradleOfTheSleepless>()
            .ActivateOnEnter<Daggerflight>();
    }
}

public abstract class DreamsOfANewDay(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-757f, -719f), 19.5f, 20)]);
    private static readonly uint[] all = [(uint)OID.Boss, (uint)OID.UnboundRaider1, (uint)OID.UnboundRaider2, (uint)OID.UnboundRaider3, (uint)OID.UnboundRaider4,
    (uint)OID.UnboundRavager1, (uint)OID.UnboundRavager4, (uint)OID.UnboundRavager6, (uint)OID.UnboundRavager7, (uint)OID.UnboundRavager8, (uint)OID.BossP2];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, all);
    }

    protected override bool CheckPull() => Raid.Player()!.InCombat;
}

sealed class DreamsOfANewDayP1States(BossModule module) : DreamsOfANewDayStates(module) { }
sealed class DreamsOfANewDayP2States(BossModule module) : DreamsOfANewDayStates(module) { }

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70359, NameID = 13046, SortOrder = 1)]
public sealed class DreamsOfANewDayP1(WorldState ws, Actor primary) : DreamsOfANewDay(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.BossP2, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70359, NameID = 13046, SortOrder = 2)]
public sealed class DreamsOfANewDayP2(WorldState ws, Actor primary) : DreamsOfANewDay(ws, primary);
