namespace BossMod.Dawntrail.Alliance.A35ShinryuParadox;

public enum OID : uint
{
    ShinryuParadox = 0x4D92, // R25.000, x1
    UnkownActor = 0x4EB3, // R2.000, x5
    ArcaneSphere1 = 0x4D97, // R1.000, x1
    ArcaneSphere2 = 0x4DCD, // R1.000, x1
    ShinryuAutos = 0x4D9A, // R0.000, x3, Part type
    ShinryuGroin = 0x4D93, // R25.000, x1, Part type
    Helper = 0x233C, // R0.500, x24, Helper type
    GuloolJaJa = 0x4E53, // R5.000, x0 (spawn during fight)
    HollowKing = 0x4D96, // R25.000, x0 (spawn during fight)
    HollowKingAutos = 0x4D9B, // R0.000, x0 (spawn during fight), Part type

}

public enum AID : uint
{
    ShinryuAutoVisual = 49137, // ShinryuParadox->self, no cast, single-target
    ShinryuAuto = 49138, // ShinryuParadoxPart1->player, no cast, single-target

    CosmicBreathVisual1 = 49105, // ShinryuParadox->self, 6.0+1.0s cast, single-target
    CosmicBreathVisual2 = 49106, // ShinryuParadoxPart2->self, 6.0+1.0s cast, single-target
    CosmicBreath = 49107, // Helper->self, 7.0s cast, range 50 width 70 rect
    CosmicTailVisual1 = 49108, // Boss->self, 6.0+1.0s cast, single-target
    CosmicTailVisual2 = 49109, // ShinryusGroin->self, 6.0+1.0s cast, single-target
    CosmicTail = 49110, // Helper->self, 7.0s cast, range 50 width 70 rect
    CloakOfTwilight1 = 49111, // Boss->self, 3.0s cast, single-target
    CloakOfTwilight2 = 49112, // ShinryusGroin->self, 3.0s cast, single-target
    TwilightNebula1 = 49113, // Boss->self, 6.0s cast, single-target
    TwilightNebula2 = 49114, // ShinryusGroin->self, 6.0s cast, single-target
    TwilightRadiance = 49115, // Helper->self, no cast, range 60 circle
    TwilightShadow = 49116, // Helper->self, no cast, range 60 circle
    StarflareVisual1 = 49124, // Boss->self, 3.0s cast, single-target
    StarflareVisual2 = 49125, // ShinryusGroin->self, 3.0s cast, single-target
    StarflareP1Fast = 49126, // Helper->self, 5.0s cast, range 60 width 10 rect
    StarflareP1Slow = 49127, // Helper->self, 7.0s cast, range 60 width 10 rect
    CataclysmicVortexVisual1 = 49121, // Boss->self, 7.0s cast, single-target
    CataclysmicVortexVisual2 = 49122, // ShinryusGroin->self, 7.0s cast, single-target
    CataclysmicVortexFail = 49123, // Helper->player, no cast, single-target
    DarkNovaVisual1 = 49134, // Boss->self, 5.0s cast, single-target
    DarkNovaVisual2 = 49135, // ShinryusGroin->self, 5.0s cast, single-target
    DarkNova = 49136, // Helper->player, no cast, range 6 circle
    AtomicTailVisual1 = 49128, // Boss->self, 6.0+1.0s cast, single-target
    AtomicTailVisual2 = 49129, // ShinryusGroin->self, 6.0+1.0s cast, single-target
    AtomicTail = 49130, // Helper->self, 7.0s cast, range 50 width 70 rect
    GyreChargeVisual1 = 49131, // Boss->self, no cast, single-target
    GyreChargeVisual2 = 49132, // ShinryusGroin->self, no cast, single-target
    GyreCharge = 49133, // Helper->self, 0.5s cast, range 60 circle

    CelestialTrailVisual1 = 49139, // HollowKing->self, no cast, single-target
    CelestialTrailTower = 49140, // Helper->self, 8.0s cast, range 2 circle
    CelestialTrailVisual2 = 49141, // HollowKing->self, no cast, single-target
    CelestialTrailHPDown = 49142, // Helper->player/4D98, no cast, single-target
    CelestialTrailKnockback = 49143, // Helper->player/4D98, no cast, single-target
    CelestialTrailVisual3 = 49144, // HollowKing->self, no cast, single-target
    CelestialTrailExplosion = 49147, // Helper->self, 5.5s cast, range 60 circle
    HollowKingAutoVisual = 49180, // HollowKing->self, no cast, single-target
    HollowKingAuto = 49181, // HollowKingAutos->player, no cast, single-target
    EmptyProclamation = 49179, // HollowKing->self, 4.0s cast, range 60 circle
    RightSwordscrossVisual = 49151, // HollowKing->self, 8.0+1.0s cast, single-target
    LeftSwordscrossVisual = 49152, // HollowKing->self, 8.0+1.0s cast, single-target
    RightSwordscross1 = 49153, // Helper->self, 9.0s cast, range 60 width 30 rect
    LeftSwordscross1 = 49154, // Helper->self, 9.0s cast, range 60 width 30 rect
    RightSwordscross2 = 49155, // Helper->self, 9.0s cast, range 70 width 36 rect
    LeftSwordscross2 = 49156, // Helper->self, 9.0s cast, range 70 width 36 rect
    TwinBlazeVisual1 = 49157, // HollowKing->self, 5.0+1.0s cast, single-target
    TwinBlazeVisual2 = 49158, // HollowKing->self, 5.0+1.0s cast, single-target
    TwinBlazeIn = 49159, // Helper->self, 6.0s cast, range 20-60 donut
    TwinBlazeOut = 49160, // Helper->self, 6.0s cast, range 35 90-degree cone
    CataclysmicBladeVisual = 49161, // HollowKing->self, 7.0s cast, single-target
    CataclysmicBladeCone = 49162, // Helper->self, 7.0s cast, range 60 45-degree cone
    CataclysmicBladeFail = 49163, // Helper->player, no cast, single-target
    AtomicRayVisual = 49164, // HollowKing->self, 3.0s cast, single-target
    AtomicRay = 49165, // ArcaneSphere1/ArcaneSphere2->self, 1.5s cast, range 60 width 15 rect
    CosmicFlameVisual = 49166, // HollowKing->self, 5.0s cast, single-target
    CosmicFlameFirst = 49168, // Helper->self, 5.0s cast, range 6 circle
    CosmicFlameRest = 49169, // Helper->self, no cast, range 6 circle
    BurstVisual = 49170, // HollowKing->self, 3.0s cast, single-target
    Burst1 = 49171, // Helper->self, 5.0s cast, range 10 circle
    Burst2 = 49172, // Helper->self, 7.0s cast, range 10-20 donut
    Burst3 = 49173, // Helper->self, 9.0s cast, range 20-30 donut
    StarflareP2Cast = 49174, // HollowKing->self, 3.0s cast, single-target
    StarflareP2Fast = 49175, // Helper->self, 5.0s cast, range 60 width 10 rect
    StarflareP2Slow = 49176, // Helper->self, 7.0s cast, range 60 width 10 rect
    DarkNovaP2Visual = 49177, // HollowKing->self, 5.0s cast, single-target
    DarkNovaP2 = 49178, // Helper->players, no cast, range 6 circle
    SuperNovaVisual = 49182, // HollowKing->self, 5.0s cast, single-target
    SuperNova = 49183, // Helper->players, no cast, range 6 circle
}


public enum SID : uint
{
    Bleeding1 = 3077, // none->player, extra=0x0
    Bleeding2 = 3078, // none->player, extra=0x0
    CloakOfWaningLight = 5352, // none->player, extra=0x0
    CloakOfWaxingDark = 5353, // none->player, extra=0x0
    DownForTheCount = 1963, // Helper->player, extra=0xEC7
    Unk1 = 2202, // none->player, extra=0x0
    Unk3 = 2056, // none->_Gen_HollowKing/_Gen_ArcaneSphere1/_Gen_ArcaneSphere, extra=0x474/0x48E/0x497/0x496
    Unk4 = 2552, // none->player, extra=0x48F
    Clashing = 1271, // none->player, extra=0x317A/0x1836
    HPRecoveryDown = 2852, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    NoLook = 680, // player/Alxaal->self
    Look = 681, // player/Alxaal/Prishe->self
    NoMove = 682, // player/Prishe->self
    Move = 683, // player->self
    Checkmark = 136, // player/Alxaal/Prishe->self
    X = 137, // player->self
    Tankbuster = 344, // player->self
    Countdown = 720, // ArcaneSphere/ArcaneSphere1->self
    Stack = 305, // player->self
}


public enum TetherID : uint
{
    Tether_chn_fire001f = 5, // UnknownActor->HollowKing
}



// Puts AOE or Safezone over launchpad to avoid room wide aoe
abstract class FloorAOE(BossModule module, uint action) : Components.GenericAOEs(module, action)
{
    protected List<Actor> Casters = [];
    protected abstract int GetDangerFloor(int slot, Actor actor);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Casters)
        {
            var activation = Module.CastFinishAt(c.CastInfo);
            var danger = GetDangerFloor(slot, actor);
            var floor = Helpers.Level(actor);

            if (danger == 1 && floor == 0)
                // stay away from launcher on lower floor to avoid danger on top floor.
                return new ReadOnlySpan<AOEInstance>([new AOEInstance(new AOEShapeCircle(2), Arena.Center - new WDir(0, 6), default, activation)]);
            else if (danger == 1 && floor == 1)
                // drop down to lower floor from top floor
                return new ReadOnlySpan<AOEInstance>([new AOEInstance(new AOEShapeDonut(6, 100), Arena.Center - new WDir(0, 6), default, activation)]);
            else if (danger == 0 && floor == 0)
                // hop up to top floor from lower floor
                return new ReadOnlySpan<AOEInstance>([new AOEInstance(new AOEShapeDonut(2, 100), Arena.Center - new WDir(0, 6), default, activation)]);
            else if(danger == 0 && floor == 1)
                // stay on top floor to avoid danger on lower floor
                return new ReadOnlySpan<AOEInstance>([new AOEInstance(new AOEShapeCircle(6), Arena.Center - new WDir(0, 6), default, activation)]);
        }
        return default;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;
            Casters.Remove(caster);
        }
    }
}


class CosmicBreath(BossModule module) : FloorAOE(module, (uint)AID.CosmicBreath)
{
    protected override int GetDangerFloor(int slot, Actor actor) => 1;
}



class CosmicTail(BossModule module) : FloorAOE(module, (uint)AID.CosmicTail)
{
    protected override int GetDangerFloor(int slot, Actor actor) => 0;
}



class AtomicTail(BossModule module) : FloorAOE(module, (uint)AID.AtomicTail)
{
    protected override int GetDangerFloor(int slot, Actor actor) => 0;
}






class AtomicTailArena(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00200010)
        {
            Shape[] arenaOutline = [new Rectangle(Arena.Center, 30f, 20f)];
            // Set a boundary to keep pc from jumping down into atomic tail.
            Shape[] circleOfDanger = [new Circle((Arena.Center - new WDir(0, 6)), 6)];

            // Take the arena rectangle and give it a difference for the hole to prevent jumping down.
            ArenaBoundsCustom atomicTailArenaBounds = new(arenaOutline , circleOfDanger);
            Arena.Bounds = atomicTailArenaBounds;
        }

        if (index == 0x00 && state == 0x02000100)
            Arena.Bounds = new ArenaBoundsRect(30, 20);
    }
}







[SkipLocalsInit]
sealed class ShinryuParadoxStates : StateMachineBuilder
{
    readonly A35ShinryuParadox _module;

    public ShinryuParadoxStates(A35ShinryuParadox module) : base(module)
    {
        _module = module;
        DeathPhase(0, SinglePhase)
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.Enemies((uint)OID.HollowKing).All(k => k.IsDeadOrDestroyed);
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0x100, default, "Catchall")
            .ActivateOnEnter<CosmicBreath>()
            .ActivateOnEnter<CosmicTail>()
            .ActivateOnEnter<AtomicTail>()

            ;
        Cast(id + 0x8000, (uint)AID.AtomicTailVisual1, 6.5f, 6)
            .ActivateOnEnter<AtomicTailArena>()

            ;
        Timeout(id + 0x8010, 1, "Ground floor disappears");

        P2(id + 0x10000, 52.8f);
    }

    void P2(uint id, float delay)
    {
        ActorTargetable(id, _module.BossP2, true, delay, "Boss reappears")
            .DeactivateOnExit<AtomicTailArena>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        Timeout(id + 0x20000, 10000, "Repeat mechanics until death")
            ;
    }
}


[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(ShinryuParadoxStates),
    ConfigType = null, // replace null with typeof(ShinryuParadoxConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.ShinryuParadox,
    Contributors = "Xan, ported by wen",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Alliance,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1117u,
    NameID = 14729u,
    SortOrder = 6,
    PlanLevel = 0)]


// Set up base logic for what level of arena and which phase boss pc is fighting.
public class A35ShinryuParadox(WorldState ws, Actor primary)
    : BossModule(ws, primary, new(820f, -820f), new ArenaBoundsRect(30f, 20f))
{
    Actor? Groin;
    Actor? _bossP2;

    public Actor? BossP2() => _bossP2;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.HollowKing), Colors.Enemy);
    }

    protected override void UpdateModule()
    {
        Groin ??= Enemies((uint)OID.ShinryuGroin).FirstOrDefault();
        _bossP2 = Enemies((uint)OID.HollowKing).FirstOrDefault();
    }

    // If we are on the 0 level we fight the tail.
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment,
        AIHints hints)
    {
        var pBoss = 0;
        var pTail = AIHints.Enemy.PriorityInvincible;
        if (Helpers.Level(actor) == 0)
            (pTail, pBoss) = (pBoss, pTail);

        hints.SetPriority(PrimaryActor, pBoss);
        hints.SetPriority(Groin, pTail);
    }
}

// Helpers.Level is shorthand for which level of the arena pc is on.
// Arena itself is the same base shape with same center.  We do not have to
// change arena on radar during fight, just need to reference which level we are on
static class Helpers
{
    public static int Level(Actor pc) => Level(pc.PosRot);

    public static int Level(Vector4 p) => p.Y < -890 ? 0 : 1;
}
