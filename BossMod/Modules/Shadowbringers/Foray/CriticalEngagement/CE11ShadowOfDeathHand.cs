namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE11ShadowOfDeathHand;

public enum OID : uint
{
    Boss = 0x2DA7, // R4.32
    TamedCarrionCrow = 0x2DA8, // R1.8
    Beastmaster = 0x2DA9, // R0.5
    Whirlwind = 0x2DAA, // R1.0
    DeathwallHelper = 0x2EE8, // R0.5
    Deathwall = 0x1EB02E, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackBeastmaster = 6497, // Beastmaster->player, no cast, single-target
    AutoAttackBossCrow = 6498, // Boss/TamedCarrionCrow->player, no cast, single-target

    FastBlade = 20155, // Beastmaster->player, no cast, single-target, micro tankbuster
    SavageBlade = 20156, // Beastmaster->player, no cast, single-target, micro tankbuster
    RippingBlade = 20157, // Beastmaster->player, no cast, single-target, micro tankbuster
    BestialLoyalty = 20163, // Beastmaster->self, 3.0s cast, single-target, visual (summon crows)
    BestialLoyaltyAOE = 20164, // Helper->location, no cast, range 5 circle (aoe where crows appear)
    RunWild = 20166, // Beastmaster->self, 4.0s cast, interruptible, buffs enemies with status effect Running Wild, seems to be some kind of damage buff
    Reward = 20169, // Beastmaster->Boss, 3.0s cast, single-target, heal
    WrathOfTheForsaken = 20170, // Boss->self, 3.0s cast, single-target, damage up after beastmaster death
    HardBeak = 20171, // Boss->player, 4.0s cast, single-target, tankbuster
    PiercingBarrageBoss = 20172, // Boss->self, 3.0s cast, range 40 width 8 rect aoe
    Helldive = 20173, // Boss->players, 5.0s cast, range 6 circle stack
    BroadsideBarrage = 20174, // Boss->self, 5.0s cast, range 40 width 40 rect aoe, 'knock-forward' 50 on failure
    BlindsideBarrage = 20175, // Boss->self, 4.0s cast, single-target, visual (raidwide + deathwall)
    BlindsideBarrageAOE = 20182, // Helper->self, 4.5s cast, range 30 circle, raidwide
    StrongWind = 20181, // Deathwall->self, no cast, range 20-30 donut, deathwall
    RollingBarrage = 20180, // Boss->self, 5.0s cast, single-target, visual
    RollingBarrageAOE = 20188, // Helper->self, 5.0s cast, range 8 circle aoe
    GaleForce = 20189, // Whirlwind->self, no cast, range 4 circle aoe around whirlwind, knockback 8 + dot

    NorthWind = 20176, // Boss->self, 7.5s cast, single-target, visual
    SouthWind = 20177, // Boss->self, 7.5s cast, single-target, visual
    EastWind = 20178, // Boss->self, 7.5s cast, single-target, visual
    WestWind = 20179, // Boss->self, 7.5s cast, single-target, visual
    WindVisual = 20183, // Helper->self, 7.5s cast, range 60 width 60 rect, visual (knockback across arena)
    NorthWindAOE = 20184, // Helper->self, no cast, range 10 width 60 rect 'knock-forward' 30
    SouthWindAOE = 20185, // Helper->self, no cast, range 10 width 60 rect 'knock-forward' 30
    EastWindAOE = 20186, // Helper->self, no cast, range 10 width 60 rect 'knock-forward' 30
    WestWindAOE = 20187, // Helper->self, no cast, range 10 width 60 rect 'knock-forward' 30

    HardBeakCrow = 20190, // TamedCarrionCrow->player, 4.0s cast, single-target, micro tankbuster
    PiercingBarrageCrow = 20191 // TamedCarrionCrow->self, 3.0s cast, range 40 width 8 rect
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20f, 30f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlindsideBarrageAOE)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.8d))];
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Deathwall)
        {
            Arena.Bounds = new ArenaBoundsCircle(25f); // default arena got no extra collision, just a donut aoe
            Arena.Center = Arena.Center.Quantized();
            _aoe = [];
        }
    }
}

sealed class BestialLoyalty(BossModule module) : Components.CastHint(module, (uint)AID.BestialLoyalty, "Summon crows");
sealed class RunWild(BossModule module) : Components.CastInterruptHint(module, (uint)AID.RunWild, showNameInHint: true);
sealed class HardBeak(BossModule module) : Components.SingleTargetCast(module, (uint)AID.HardBeak);
sealed class Helldive(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Helldive, 6f, 8);
sealed class BroadsideBarrage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BroadsideBarrage, new AOEShapeRect(40f, 20f));
sealed class BlindsideBarrage(BossModule module) : Components.RaidwideCast(module, (uint)AID.BlindsideBarrage, "Raidwide + deathwall appears");
sealed class RollingBarrage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RollingBarrageAOE, 8f);
sealed class Whirlwind(BossModule module) : Components.Voidzone(module, 4f, GetWhirlwind, 3f)
{
    private static List<Actor> GetWhirlwind(BossModule module) => module.Enemies((uint)OID.Whirlwind);
}

sealed class Wind(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindVisual)
        {
            _kb = [new(spell.LocXZ, 30f, Module.CastFinishAt(spell, 0.1d), direction: spell.Rotation, kind: Kind.DirForward)];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.WestWindAOE:
            case (uint)AID.EastWindAOE:
            case (uint)AID.NorthWindAOE:
            case (uint)AID.SouthWindAOE:
                _kb = [];
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Length != 0)
        {
            ref readonly var kb = ref _kb[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var dir = kb.Direction;
                hints.AddForbiddenZone(new SDCone(Arena.Center - 10f * dir.ToDirection(), 30f, dir, 135f.Degrees()), act);
            }
        }
    }
}

sealed class PiercingBarrage(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PiercingBarrageBoss, (uint)AID.PiercingBarrageCrow], new AOEShapeRect(40f, 4f));

sealed class CE11ShadowOfDeathHandStates : StateMachineBuilder
{
    public CE11ShadowOfDeathHandStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<BestialLoyalty>()
            .ActivateOnEnter<RunWild>()
            .ActivateOnEnter<HardBeak>()
            .ActivateOnEnter<PiercingBarrage>()
            .ActivateOnEnter<Helldive>()
            .ActivateOnEnter<BroadsideBarrage>()
            .ActivateOnEnter<BlindsideBarrage>()
            .ActivateOnEnter<RollingBarrage>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<Wind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 735, NameID = 5)] // bnpcname=9400
public sealed class CE11ShadowOfDeathHand(WorldState ws, Actor primary) : BossModule(ws, primary, startingArena.Center, startingArena)
{
    private static readonly ArenaBoundsCustom startingArena = new([new Polygon(new(825f, 640f), 29.5f, 32)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.Beastmaster));
        Arena.Actors(Enemies((uint)OID.TamedCarrionCrow));
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
