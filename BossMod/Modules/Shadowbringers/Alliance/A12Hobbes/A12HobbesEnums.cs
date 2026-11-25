namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

public enum OID : uint
{
    Boss = 0x2C2B, // R25.000, x1
    SmallExploder = 0x2C62, // R0.960, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x?, Helper type
    AutoPart = 0x2C73, // R1.000, x3, Part type
    ImpactHelper = 0x2C2D, // R1.000, x1
    RobotArm = 0x2C2C, // R1.000, x1
    TetherHelper = 0x18D6, // R0.500, x6
    Helper2P = 0x2C66, // R0.512, x1, 2P ally
    Anogg = 0x2C83, // R0.500, x?
    Konogg = 0x2C82, // R0.500, x?
    ShockingDischargeVoidzone = 0x1EAEE6 // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 18430, // AutoPart->player, no cast, single-target
    LaserResistanceTest = 18437, // Boss->self, 4.0s cast, range 50 circle
    LaserResistanceTestRepeat = 18438, // Boss->self, no cast, range 50 circle

    ShockingDischarge = 18443, // Helper->self, 2.0s cast, range 5 circle
    VariableCombatTestCast = 18446, // RobotArm->self, 5.0s cast, single-target

    ImpactRotation1 = 18709, // ImpactHelper->self, no cast, single-target
    ImpactRotation2 = 18710, // ImpactHelper->self, no cast, single-target
    ImpactRotation3 = 18711, // ImpactHelper->self, no cast, single-target
    ImpactRotation4 = 18712, // ImpactHelper->self, no cast, single-target
    Impact = 18450, // Helper->self, 7.7s cast, range 20 circle
    Towerfall = 18451, // Helper->self, no cast, range 20 width 8 rect

    FireResistanceTestInside = 18454, // Helper->self, no cast, range 70 20-degree cone
    FireResistanceTestOutside = 18455, // Helper->self, no cast, range 12 width 38 rect
    FireResistanceTestHalf = 18456, // Helper->self, no cast, range 22 width 42 rect

    VariableCombatConeSlow = 18885, // Helper->self, 5.7s cast, range 20 60-degree cone
    VariableCombatCircleSlow = 18886, // Helper->self, 5.7s cast, range 2 circle
    VariableCombatDonutSlow = 18887, // Helper->self, 5.7s cast, range 7-19 donut
    VariableCombatConeFast = 18447, // Helper->self, 2.0s cast, range 20 60-degree cone
    VariableCombatCircleFast = 18448, // Helper->self, 2.0s cast, range 2 circle
    VariableCombatDonutFast = 18449, // Helper->self, 2.0s cast, range 7-19 donut
    ConvenientSelfDestruction = 18460, // SmallExploder->self, no cast, range 5 circle

    RingLaser1Cast = 18431, // Boss->self, 3.4s cast, single-target
    RingLaser2Cast = 18432, // Boss->self, 1.0s cast, single-target
    RingLaser3Cast = 18433, // Boss->self, 1.0s cast, single-target
    RingLaser1 = 18434, // Helper->self, 4.0s cast, range 15-20 donut
    RingLaser2 = 18435, // Helper->self, 4.0s cast, range 10-15 donut
    RingLaser3 = 18436, // Helper->self, 4.0s cast, range 5-10 donut

    LaserSightCast = 18439, // Boss->self, 8.0s cast, single-target
    LaserSightTarget = 18440, // Helper->player, no cast, single-target
    LaserSightStack = 18441, // Helper->self, no cast, range 65 width 8 rect

    UnwillingCargo = 18458, // Helper->self, no cast, range 40 width 7 rect, 15y knockback
    ElectromagneticPulse = 18457, // Helper->self, no cast, range 40 width 5 rect
    OilWell = 18459, // Helper->self, no cast, hits either the 4 circular platforms on SE platform or everything except the 4 circular platforms
    ShortRangeMissile = 18453, // Helper->players, 8.0s cast, range 8 circle

    ArmMotion1 = 18701, // RobotArm->self, no cast, single-target
    ArmMotion2 = 18702, // RobotArm->self, no cast, single-target
    ArmMotion3 = 18703, // RobotArm->self, no cast, single-target
    ArmMotion4 = 18704, // RobotArm->self, no cast, single-target
    ArmMotion5 = 18705, // RobotArm->self, no cast, single-target
    ArmMotion6 = 18706, // RobotArm->self, no cast, single-target

    ArmUnk1 = 18442, // RobotArm->self, no cast, single-target
    ArmUnk2 = 18444, // RobotArm->self, no cast, single-target
    Unk1 = 18707, // ImpactHelper->self, no cast, single-target
    UnkVariableCombatTest = 18445, // RobotArm->self, 5.0s cast, single-target
    UnkShortRangeMissile = 18452, // Helper->self, no cast, single-target

    // 2P ally abilities
    BalancedEdge = 19049, // Helper2P->self, 2.0s cast, range 5 circle
    BladeFlurry1 = 18608, // Helper2P->Boss, no cast, single-target
    BladeFlurry2 = 18609, // Helper2P->Boss, no cast, single-target
    DancingBlade = 19048, // Helper2P->Boss, no cast, width 2 rect charge
    WhirlingAssault = 19050, // Helper2P->self, 2.0s cast, range 40 width 4 rect
    Unk2P = 18683 // Helper2P->location, no cast, single-target
}

public enum SID : uint
{
    Unknown = 2056, // RobotArm->RobotArm, extra=0x8F
    Burns = 2199, // Helper->player, extra=0x1/0x2/0x3
    PhysicalVulnerabilityUp = 2090, // Helper->player, extra=0x0
    MagicVulnerabilityUp = 2091, // Helper->player, extra=0x0
    Electrocution = 2200, // Helper->player, extra=0x1/0x2
    Oil = 2157 // Helper->player, extra=0x32
}

public enum IconID : uint
{
    RotateCCW = 168, // RobotArm->self
    RotateCW = 167, // RobotArm->self
    ShortRangeMissile = 196 // player->self
}

public enum TetherID : uint
{
    BossToWall = 99, // TetherHelper->TetherHelper
    ConvenientSelfDestruction = 84 // SmallExploder->player
}
