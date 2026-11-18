namespace BossMod.Dawntrail.FATE.Ttokrrone;

public enum OID : uint
{
    Boss = 0x41DF, // R=13.0
    SandSphere = 0x425E, //R=2.0
    Helper = 0x4225
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport1 = 37342, // Boss->location, no cast, single-target
    Teleport2 = 36758, // Boss->location, no cast, single-target

    Devour = 37327, // Boss->location, 3.5s cast, range 8 circle

    TailwardSandspoutVisual = 37314, // Boss->self, 5.2+0,8s cast, single-target
    TailwardSandspout = 39814, // Helper->location, 2.1s cast, range 60 90-degree cone
    DeadlyDustcloakTailward = 39842, // Helper->location, 2.1s cast, range 13 circle

    RightwardSandspoutVisual = 37315, // Boss->self, 5.2+0,8s cast, single-target
    RightwardSandspout = 39843, // Helper->location, 2.1s cast, range 13 circle
    DeadlyDustcloakRightward = 39815, // Helper->location, 2.1s cast, range 60 90-degree cone

    LeftwardSandspoutVisual = 37316, // Boss->self, 5.2+0,8s cast, single-target
    LeftwardSandspout = 39816, // Helper->location, 2.1s cast, range 60 90-degree cone
    DeadlyDustcloakLeftward = 39844, // Helper->location, 2.1s cast, range 13 circle

    FangwardSandspoutVisual = 37313, // Boss->self, 5.2+0,8s cast, single-target
    FangwardSandspout = 39813, // Helper->location, 2.1s cast, range 60 90-degree cone
    DeadlyDustcloakFangward = 39841, // Helper->location, 2.1s cast, range 13 circle

    DesertTempestVisualDonut = 37332, // Boss->self, 7.3+0,7s cast, single-target
    DesertTempestVisualCircle = 37331, // Boss->self, 7.3+0,7s cast, single-target
    DesertTempestVisualConeDonutSegment = 37333, // Boss->self, 7.3+0,7s cast, single-target
    DesertTempestVisualDonutSegmentCone = 37334, // Boss->self, 7.3+0,7s cast, single-target
    DesertTempestDonut = 37329, // Helper->location, 1.1s cast, range 14-60 donut
    DesertTempestCone1 = 37336, // Helper->location, 1.1s cast, range 19 180-degree cone
    DesertTempestCone2 = 37335, // Helper->location, 1.1s cast, range 19 180-degree cone
    DesertTempestDonutSegment1 = 37337, // Helper->location, 1.1s cast, range 14-60 180-degree donut segment
    DesertTempestDonutSegment2 = 37338, // Helper->location, 1.1s cast, range 14-60 180-degree donut segment
    DesertTempestCircle = 37328, // Helper->location, 1.1s cast, range 19 circle

    LandSwallowTelegraph1 = 37578, // Helper->location, 13.8s cast, range 14 width 22 rect
    LandSwallowTelegraph2 = 37579, // Helper->location, 14.8s cast, range 14 width 22 rect
    LandSwallowTelegraph3 = 37580, // Helper->location, 16.2s cast, range 14 width 22 rect
    LandSwallowTelegraph4 = 37581, // Helper->location, 17.6s cast, range 14 width 22 rect
    LandSwallowTelegraph5 = 37582, // Helper->location, 19.0s cast, range 14 width 22 rect
    LandSwallowTelegraph6 = 37583, // Helper->location, 20.4s cast, range 14 width 22 rect
    Landswallow1 = 38642, // Boss->location, 14.0s cast, range 38 width 27 rect
    Landswallow2 = 38644, // Boss->location, no cast, range 68 width 27 rect
    Landswallow3 = 38645, // Boss->location, no cast, range 50 width 27 rect
    Landswallow4 = 38646, // Boss->location, no cast, range 63 width 27 rect

    Touchdown = 37339, // Boss->self, 5.0s cast, range 60 circle

    SummoningSands = 38647, // SandSphere->self, 6.3s cast, range 6 circle
    Sandburst1 = 39245, // SandSphere->location, 8.0s cast, range 12 circle
    Sandburst2 = 39246, // SandSphere->location, 6.0s cast, range 12 circle

    TailwardDustdevilVisualCCW = 37322, // Boss->self, 7.2+0,8s cast, single-target
    TailwardDustdevilVisualCW = 37318, // Boss->self, 7.2+0,8s cast, single-target
    TailwardDustdevilFirst = 39818, // Helper->location, 2.1s cast, range 60 90-degree cone
    DeadlyDustcloakTailwardDDFirst = 39846, // Helper->location, 2.1s cast, range 13 circle
    DeadlyDustcloakTailwardDDRest = 39848, // Helper->location, 0.9s cast, range 13 circle
    LeftwardSandspoutDDVisual = 37326, // Boss->self, no cast, single-target
    LeftwardSandspoutDDRest = 39820, // Helper->location, 0.9s cast, range 60 90-degree cone

    FangwardDustdevilVisualCCW = 37321, // Boss->self, 7.2+0,8s cast, single-target
    FangwardDustdevilVisualCW = 37317, // Boss->self, 7.2+0,8s cast, single-target
    FangwardDustdevilFirst = 39817, // Helper->location, 2.1s cast, range 60 90-degree cone
    DeadlyDustcloakFangwardDDFirst = 39845, // Helper->location, 2.1s cast, range 13 circle
    DeadlyDustcloakdFangwardDDRest = 39847, // Helper->location, 0.9s cast, range 13 circle
    RightwardSandspoutDDRest = 39819, // Helper->location, 0.9s cast, range 60 90-degree cone
    RightwardSandspoutDDVisual = 37325 // Boss->self, no cast, single-target
}

sealed class Devour(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Devour, 8f);
sealed class Touchdown(BossModule module) : Components.RaidwideCast(module, (uint)AID.Touchdown);

sealed class TtokrroneStates : StateMachineBuilder
{
    public TtokrroneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Devour>()
            .ActivateOnEnter<DesertTempest>()
            .ActivateOnEnter<Sandspout>()
            .ActivateOnEnter<Landswallow>()
            .ActivateOnEnter<Sandspheres>()
            .ActivateOnEnter<DustcloakDustdevil>()
            .ActivateOnEnter<DesertDustdevil>()
            .ActivateOnEnter<Touchdown>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Fate, GroupID = 1871, NameID = 12733)]
public sealed class Ttokrrone(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(53f, -820f), 29.5f, 48)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && (arena.Center - player.Position).LengthSq() < 900f;
}
