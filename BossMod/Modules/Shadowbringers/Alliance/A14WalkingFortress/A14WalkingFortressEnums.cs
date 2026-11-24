namespace BossMod.Shadowbringers.Alliance.A14WalkingFortress;

public enum OID : uint
{
    Boss = 0x2C74, // R6.500, x1
    Helper = 0x233C, // R0.500, x24 (spawn during fight), Helper type
    Marx1 = 0x2C0B, // R0.700, x1
    Marx2 = 0x2C0C, // R0.700, x1
    FlightUnit = 0x2C75, // R2.800, x1
    MarxCrane = 0x2C78, // R12.000, x0 (spawn during fight)
    GoliathTank = 0x2C77, // R9.600, x0 (spawn during fight)
    SerialJointedServiceModel = 0x2C76, // R3.360, x0 (spawn during fight)
    TetherHelper1 = 0x2C86, // R1.000, x0 (spawn during fight)
    TetherHelper2 = 0x2C87, // R1.000, x0 (spawn during fight)
    TetherHelper3 = 0x2C88, // R1.000, x0 (spawn during fight)
    TetherHelper4 = 0x2CC7, // R1.000, x0 (spawn during fight)
    TetherHelper5 = 0x2CC8, // R1.000, x0 (spawn during fight)
    Helper2P = 0x2C66 // R0.512, x1, 2P ally
}

public enum AID : uint
{
    AutoAttack = 18884, // Boss->player, no cast, single-target
    LaserSaturation = 18678, // Boss->self, 4.0s cast, range 85 circle
    LaserTurretCast = 18679, // Boss->self, 4.0s cast, single-target
    LaserTurret = 19060, // Helper->self, 4.3s cast, range 90 width 8 rect
    GroundToGroundMissile = 18680, // Boss->self, no cast, single-target
    BallisticImpactLocation = 18804, // Helper->location, 3.5s cast, range 6 circle
    BallisticImpactSpread = 18681, // Helper->players, 5.0s cast, range 6 circle
    DualFlankCannons = 18654, // Boss->self, 5.0s cast, single-target
    ForeHindCannons = 18655, // Boss->self, 5.0s cast, single-target
    LaserSuppression = 18656, // Helper->self, 5.0s cast, range 60 90-degree cone

    EngageMarxSupport = 18643, // Boss->self, 3.0s cast, single-target
    MarxImpact = 18644, // MarxCrane->self, 5.0s cast, range 22 circle
    UndockCast = 19255, // Boss->self, 4.0s cast, single-target
    Undock = 19038, // Boss->self, no cast, single-target
    Undock2 = 18645, // Boss->self, 4.0s cast, single-target

    BallisticImpactExa = 18652, // Helper->self, no cast, range 15 width 20 rect
    BallisticImpactIndicator1 = 18649, // Helper->self, 1.0s cast, range 60 width 20 rect
    BallisticImpactIndicator2 = 18650, // Helper->self, 3.5s cast, range 60 width 20 rect
    BallisticImpactIndicator3 = 18651, // Helper->self, 6.0s cast, range 60 width 20 rect
    Neutralization = 18677, // Boss->player, 4.0s cast, single-target

    AntiPersonnelMissile2x = 18657, // Boss->self, 6.0s cast, single-target
    AntiPersonnelMissile3x = 18658, // Boss->self, 9.0s cast, single-target
    AntiPersonnelMissile = 19217, // Boss->self, 5.0s cast, single-target
    BallisticImpactFloor = 18660, // Helper->self, no cast, range 15 width 15 rect

    EngageGoliathTankSupport = 18661, // Boss->self, 3.0s cast, single-target
    LaserTurretTank = 18662, // GoliathTank->self, no cast, range 85 width 10 rect
    HackGoliathTank = 18663, // Boss->GoliathTank, 10.0s cast, single-target
    ConvenientSelfDestructionLOS = 18664, // GoliathTank->self, 10.0s cast, range 85 circle
    ConvenientSelfDestruction = 18665, // GoliathTank->self, 8.0s cast, range 22 circle

    ServiceModelAuto = 872, // SerialJointedServiceModel->player, no cast, single-target
    ClangingBlow = 18672, // SerialJointedServiceModel->player, 4.0s cast, single-target
    CentrifugalSpinVisual = 19076, // SerialJointedServiceModel->self, 6.0s cast, single-target
    CentrifugalSpin = 19077, // Helper->self, 6.3s cast, range 30 width 8 rect
    SidestrikingSpinVisual = 19078, // SerialJointedServiceModel->self, 6.0s cast, single-target
    SidestrikingSpin = 19079, // Helper->self, 6.3s cast, range 12 width 30 rect
    ShrapnelImpact = 18675, // Helper->players, 5.0s cast, range 6 circle
    DeployDefenses = 18671, // Helper2P->self, no cast, single-target, shield appears
    TotalAnnihilationManeuverCast = 18667, // Boss->self, 10.0s cast, single-target
    TotalAnnihilationManeuver = 18668, // Helper->self, 13.0s cast, range 80 circle

    // 2P ally abilities
    BladeFlurry1 = 18608, // Helper2P->Boss/GoliathTank/SerialJointedServiceModel, no cast, single-target
    BladeFlurry2 = 18609, // Helper2P->Boss/GoliathTank/SerialJointedServiceModel, no cast, single-target
    DancingBlade = 19048, // Helper2P->Boss/GoliathTank/SerialJointedServiceModel, no cast, width 2 rect charge
    BalancedEdge = 19049, // Helper2P->self, 2.0s cast, range 5 circle
    WhirlingAssault = 19050, // Helper2P->self, 2.0s cast, range 40 width 4 rect
    Unk2P = 18683, // Helper2P->location, no cast, single-target

    // visual only
    FlightUnitUnk1 = 19037, // FlightUnit->self, no cast, single-target
    FlightUnitUnk2 = 18646, // FlightUnit->self, 4.0s cast, single-target
    FlightUnitUnk3 = 18647, // FlightUnit->self, no cast, single-target
    FlightUnitUnk4 = 18648, // FlightUnit->self, no cast, single-target
    FlightUnitUnk5 = 19223, // FlightUnit->self, no cast, single-target
    BossUnk1 = 18653, // Boss->self, no cast, single-target
    HelperUnk = 18659, // Helper->self, no cast, single-target
    BossUnk2 = 18666, // Boss->self, no cast, single-target
    TetherHelperUnk1 = 18764, // TetherHelper1->self, no cast, single-target
    TetherHelperUnk2 = 18765, // TetherHelper2->self, no cast, single-target
    TetherHelperUnk3 = 18766, // TetherHelper3->self, no cast, single-target
    TetherHelperUnk4 = 19031, // TetherHelper4->self, no cast, single-target
    TetherHelperUnk5 = 19032 // TetherHelper5->self, no cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 2213, // Helper/MarxCrane->player, extra=0x1/0x2/0x3/0x4/0x5/0x7/0x6
    UnknownStatus1 = 2056, // Boss->MarxCrane/Boss/GoliathTank/TetherHelper1-3, extra=0x92/0x91/0x90/0x94/0x8C
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Preoccupied = 1619, // none->player/Helper2P, extra=0x0
    Invincibility = 775, // none->Boss, extra=0x0
    DamageUp = 2220, // none->Boss, extra=0x1/0x2
    Invincibility2 = 1570, // none->player, extra=0x0
    UnknownStatus2 = 2193, // none->TetherHelper1-3, extra=0x8D
    VulnerabilityDown = 350, // none->player, extra=0x0
    UnknownStatus3 = 2160 // none->Helper2P, extra=0x1C94
}

public enum IconID : uint
{
    Spread = 139, // player->self
    Tankbuster = 198, // player->self
    BallisticImpactGroundSquare = 199, // Helper->self
    TurretLockon = 164, // player->self
    Hacking = 200, // GoliathTank->self
    Stack = 161 // player->self
}

public enum TetherID : uint
{
    GoliathTankToBoss = 100, // GoliathTank->Boss
    BossToGoliathTank = 54 // Boss->GoliathTank
}
