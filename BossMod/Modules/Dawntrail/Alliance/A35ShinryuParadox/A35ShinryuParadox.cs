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

            // By returning in each case directly instead of passing back a list
            // we update the AOE pattern as pc changes floors.
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

class TwilightNebula(BossModule module) : FloorAOE(module, (uint)AID.TwilightNebula1)
{
    readonly int[] colors = Utils.MakeArray(PartyState.MaxAllies, -1);

    protected override int GetDangerFloor(int slot, Actor actor)
    {
        var x = 1 - colors[slot];
        return x > 1 ? -1 : x;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.CloakOfWaningLight:
                var s1 = Raid.FindSlot(actor.InstanceID);
                if (s1 >= 0)
                    colors[s1] = 1;
                break;
            case SID.CloakOfWaxingDark:
                var s2 = Raid.FindSlot(actor.InstanceID);
                if (s2 >= 0)
                    colors[s2] = 0;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TwilightRadiance)
        {
            NumCasts++;
            Casters.Clear();
        }
    }
}

// Starflare: Two sets of crisscrossing line AoE telegraphs, hitting both levels at once.
// TODO: better filtering.  Currently shows all of StarflareP1Fast and all of StarflareP1Slow instead of just 5 casts.
class Starflare(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.StarflareP1Fast, (uint)AID.StarflareP1Slow], new AOEShapeRect(60, 5), maxCasts: 5, expectedNumCasters:20)
{
    protected List<Actor> _casters = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];

        for (var i = 0; i < count; i++)
        {
            var c = _casters[i];

            if (Helpers.Level(c) == Helpers.Level(actor))
            {
                // show aoe on our floor
                // This version works(ish), but we want to limit to 5 entries
                aoes[i] = new AOEInstance(Shape, c.CastInfo!.LocXZ, c.CastInfo!.Rotation,
                    Module.CastFinishAt(c.CastInfo));
            }
            else
            {
                aoes[i] = new AOEInstance(new AOEShapeRect(0, 0), c.CastInfo!.LocXZ, c.CastInfo!.Rotation,
                    Module.CastFinishAt(c.CastInfo));
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.StarflareP1Fast || (AID)spell.Action.ID is AID.StarflareP1Slow)
        {
            _casters.Add(caster);
            ++NumCasts;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.StarflareP1Fast || (AID)spell.Action.ID is AID.StarflareP1Slow)
        {
            _casters.Remove(caster);
        }
    }
}

//TODO VortexNoMove

class VortexStayMove(BossModule module) : Components.StayMove(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        Requirement r;
        switch ((IconID)iconID)
        {
            case IconID.NoMove:
                r = Requirement.Stay;
                break;
            case IconID.Move:
                r = Requirement.Move;
                break;
            default:
                return;
        }

        var p = Raid.FindSlot(actor.InstanceID);
        if (p >= 0)
            SetState(p, new(r, WorldState.FutureTime(7)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CataclysmicVortexVisual1 or AID.CataclysmicBladeVisual)
            Array.Fill(PlayerStates, default);
    }
}

class UpDownCounter(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.CosmicBreath, (uint)AID.CosmicTail]);

//TODO DarkNova tankbuster baits are not disappearing.
class DarkNova(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Tankbuster, (uint)AID.DarkNovaP2, 5.1f, tankbuster: true);

class DarkNovaP2(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Tankbuster, (uint)AID.DarkNovaP2, 5.1f, tankbuster: true);

class CelestialTrail(BossModule module) : Components.CastTowers(module, (uint)AID.CelestialTrailTower, 2, 1, 10)
{
    BitMask _forbidden;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID == SID.HPRecoveryDown)
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            for (var i = 0; i < Towers.Count; i++)
                Towers.Ref(i).ForbiddenSoakers = _forbidden;
        }
    }
}

class EmptyProclamation(BossModule module) : Components.RaidwideCast(module, (uint)AID.EmptyProclamation);
class Swordscross1(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightSwordscross1, (uint)AID.LeftSwordscross1], new AOEShapeRect(60, 15));
class Swordscross2(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightSwordscross2, (uint)AID.LeftSwordscross2], new AOEShapeRect(70, 18));
class TwinBlaze1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwinBlazeIn, new AOEShapeDonutSector(20, 60, 45.Degrees()));
class TwinBlaze2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwinBlazeOut, new AOEShapeCone(35, 45.Degrees()));
class CataclysmicBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CataclysmicBladeCone, new AOEShapeCone(60, 22.5f.Degrees()));


[SkipLocalsInit]
sealed class ShinryuParadoxStates : StateMachineBuilder
{
    readonly A35ShinryuParadox _module;

    public ShinryuParadoxStates(A35ShinryuParadox module) : base(module)
    {
        _module = module;

        TrivialPhase()

            .ActivateOnEnter<CosmicBreath>()
            .ActivateOnEnter<CosmicTail>()
            .ActivateOnEnter<AtomicTail>()
            .ActivateOnEnter<AtomicTailArena>()
            .ActivateOnEnter<TwilightNebula>()
            .ActivateOnEnter<Starflare>()
            .ActivateOnEnter<VortexStayMove>()
            .ActivateOnEnter<UpDownCounter>()
            .ActivateOnEnter<DarkNova>()
            .ActivateOnEnter<DarkNovaP2>()
            .ActivateOnEnter<CelestialTrail>()
            .ActivateOnEnter<EmptyProclamation>()
            .ActivateOnEnter<Swordscross1>()
            .ActivateOnEnter<Swordscross2>()
            .ActivateOnEnter<TwinBlaze1>()
            .ActivateOnEnter<TwinBlaze2>()
            .ActivateOnEnter<CataclysmicBlade>()

            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.Enemies((uint)OID.HollowKing).All(k => k.IsDeadOrDestroyed);
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
