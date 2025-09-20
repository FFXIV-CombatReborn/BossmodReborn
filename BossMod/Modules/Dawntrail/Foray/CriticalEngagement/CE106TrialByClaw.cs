namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE106TrialByClaw;

public enum OID : uint
{
    Boss = 0x4715, // R4.2
    DraconicDouble = 0x4716, // R4.2
    Crystal = 0x4717, // R1.7
    CrystalAnim = 0x1EBD50, // R0.5
    CrystallizedChaos1 = 0x1EBD8D, // R0.5
    CrystallizedChaos2 = 0x1EBD8E, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    PrimalRoarVisual = 41762, // Boss->self, 4.1+0,9s cast, single-target
    PrimalRoar = 41763, // Helper->self, 5.0s cast, ???
    FearsomeFacet = 41749, // Boss->self, 3.0s cast, single-target
    PrismaticWingCircleVisual = 41750, // DraconicDouble->self, 6.2+0,8s cast, single-target
    PrismaticWingCircle = 42766, // Helper->self, 7.0s cast, range 22 circle
    PrismaticWingDonutVisual = 41751, // DraconicDouble->self, 6.0+1,0s cast, single-target
    PrismaticWingDonut = 42767, // Helper->self, 7.0s cast, range 5-31 donut
    MadeMagicCircleVisual = 41752, // Helper->self, 4.3+0,2s cast, single-target
    MadeMagicCircle = 42768, // Helper->self, 4.5s cast, range 22 circle
    MadeMagicDonutVisual = 41753, // Helper->self, 4.1+0,4s cast, single-target
    MadeMagicDonut = 42769, // Helper->self, 4.5s cast, range 5-31 donut
    CrystalMirror = 41754, // Boss->self, 6.0s cast, single-target
    CrystalCall = 41748, // Boss->self, 3.0s cast, single-target
    CrystallizedChaosVisual1 = 41755, // Boss->self, 7.0s cast, single-target
    CrystallizedChaosVisual2 = 41756, // Boss->self, 10.0s cast, single-target
    CrystallizedChaosVisual3 = 41757, // Boss->self, no cast, single-target
    CrystallizedEnergy1 = 42728, // Helper->self, 7.0s cast, range 7 circle
    CrystallizedChaos1 = 42729, // Crystal->self, 7.0s cast, range 7-13 donut
    CrystallizedChaos2 = 42730, // Crystal->self, 7.0s cast, range 13-19 donut
    CrystallizedChaos3 = 42731, // Crystal->self, 7.0s cast, range 19-25 donut
    CrystallizedEnergy2 = 42732, // Helper->self, 10.0s cast, range 7 circle
    CrystallizedChaos4 = 42733, // Crystal->self, 10.0s cast, range 7-13 donut
    CrystallizedChaos5 = 42734, // Crystal->self, 10.0s cast, range 13-19 donut
    CrystallizedChaos6 = 42735, // Crystal->self, 10.0s cast, range 19-25 donut
    CrystallizedEnergy3 = 41758, // Helper->self, 4.0s cast, range 7 circle
    CrystallizedChaos7 = 41759, // Crystal->self, 4.0s cast, range 7-13 donut
    CrystallizedChaos8 = 41760, // Crystal->self, 4.0s cast, range 13-19 donut
    CrystallizedChaos9 = 41761 // Crystal->self, 4.0s cast, range 19-25 donut
}

sealed class PrimalRoar(BossModule module) : Components.RaidwideCast(module, (uint)AID.PrimalRoar);
sealed class PrismaticWingMadeMagicCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PrismaticWingCircle, (uint)AID.MadeMagicCircle], 22f, riskyWithSecondsLeft: 4d);
sealed class PrismaticWingMadeMagicDonut(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PrismaticWingDonut, (uint)AID.MadeMagicDonut], new AOEShapeDonut(5f, 31f), riskyWithSecondsLeft: 5d);
sealed class CrystallizedEnergy(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CrystallizedEnergy1, (uint)AID.CrystallizedEnergy2, (uint)AID.CrystallizedEnergy3], 7f, riskyWithSecondsLeft: 5d);
sealed class CrystallizedChaosDonut1(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CrystallizedChaos1, (uint)AID.CrystallizedChaos4, (uint)AID.CrystallizedChaos7], new AOEShapeDonut(7f, 13f), riskyWithSecondsLeft: 5d);
sealed class CrystallizedChaosDonut2(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CrystallizedChaos2, (uint)AID.CrystallizedChaos5, (uint)AID.CrystallizedChaos8], new AOEShapeDonut(13f, 19f), riskyWithSecondsLeft: 5d);
sealed class CrystallizedChaosDonut3(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CrystallizedChaos3, (uint)AID.CrystallizedChaos6, (uint)AID.CrystallizedChaos9], new AOEShapeDonut(19f, 25f), riskyWithSecondsLeft: 5d);

sealed class CE106TrialByClawStates : StateMachineBuilder
{
    public CE106TrialByClawStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PrimalRoar>()
            .ActivateOnEnter<PrismaticWingMadeMagicCircle>()
            .ActivateOnEnter<PrismaticWingMadeMagicDonut>()
            .ActivateOnEnter<CrystallizedEnergy>()
            .ActivateOnEnter<CrystallizedChaosDonut1>()
            .ActivateOnEnter<CrystallizedChaosDonut2>()
            .ActivateOnEnter<CrystallizedChaosDonut3>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 38)]
public sealed class CE106TrialByClaw(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-413.9f, 75f), 24.5f, 32)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
