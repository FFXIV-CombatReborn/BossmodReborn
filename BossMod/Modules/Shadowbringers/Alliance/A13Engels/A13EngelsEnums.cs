namespace BossMod.Shadowbringers.Alliance.A13Engels;

public enum OID : uint
{
    Boss = 0x2557, // R30.000, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    Helper2 = 0x2CCF, // R0.500, x1
    MarxHelper1 = 0x2C0B, // R0.700, x1
    MarxHelper2 = 0x2C0C, // R0.700, x1
    MarxL = 0x2C09, // R18.500, x0 (spawn during fight)
    MarxR = 0x2C0A, // R18.500, x0 (spawn during fight)
    Ally2P1 = 0x2CC4, // R2.800, x?
    Ally2P2 = 0x2CCE, // R0.500, x?
    ReverseJointedGoliath = 0x2C07, // R3.600, x0 (spawn during fight)
    SmallBiped = 0x2C08, // R0.960, x0 (spawn during fight)
    Anogg = 0x2C83, // R0.500, x?
    Konogg = 0x2C82, // R0.500, x?
    EnergyDispersalTower = 0x1EAEC7, // R0.500, EventObj type, spawn during fight
    IncendiaryBomb = 0x1EAEC8 // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttackSmall = 872, // SmallBiped->player, no cast, single-target
    AutoAttackLarge = 18264, // ReverseJointedGoliath->player, no cast, single-target
    ArmLaser = 18263, // ReverseJointedGoliath->self, 3.0s cast, range 30 90-degree cone

    CrushingWheelVisual = 18248, // Boss->self, no cast, single-target
    CrushingWheel = 18251, // MarxL/MarxR->self, 12.0s cast, range 20 width 30 rect
    RadiateHeat = 18252, // Helper->self, no cast, range 50 circle
    Frack = 18253, // Helper->self, no cast, range 15 circle
    IncendiarySaturationBombingVisual = 18254, // Boss->self, 3.0s cast, single-target
    IncendiarySaturationBombing = 18255, // Helper->self, 6.0s cast, range 30 width 60 rect
    AreaBombardment = 18256, // Boss->self, 3.0s cast, single-target

    AutoAttackBoss = 18257, // Boss->self, no cast, single-target
    AutoAttackHelper = 18258, // Helper->player, no cast, single-target

    DiffuseLaser = 18261, // Boss->self, 4.0s cast, range 60 width 60 rect

    GuidedMissileCast = 18229, // Boss->self, 3.5s cast, single-target
    GuidedMissileFirst = 18230, // Helper->location, 5.0s cast, range 6 circle
    GuidedMissileRest = 18231, // Helper->location, no cast, range 6 circle

    IncendiaryBombingCast = 18232, // Boss->self, 5.0s cast, single-target
    IncendiaryBombing = 18233, // Helper->location, 4.0s cast, range 8 circle

    LaserSight = 18234, // Boss->self, 4.1s cast, range 100 width 20 rect
    LaserSightRepeat = 18235, // Helper->self, no cast, range 100 width 20 rect

    EnergyBarrage = 18236, // Boss->self, 3.0s cast, single-target
    EnergyDispersal = 18237, // Helper->self, no cast, range 4 circle, tower soak
    EnergyBlast = 18238, // Helper->self, no cast, range 75 circle, tower explosion
    Unk1 = 18239, // Boss->self, no cast, single-target

    SurfaceMissileCast = 18227, // Boss->self, 3.5s cast, single-target
    SurfaceMissile = 18228, // Helper->location, 4.0s cast, range 6 circle

    MarxActivation = 18600, // Boss->self, 3.0s cast, single-target
    DemolishStructureVisual = 18244, // Boss->self, no cast, single-target
    DemolishStructure = 18245, // Helper->self, 9.3s cast, range 50 circle
    MarxCrushVisual = 18246, // Boss->self, 6.0s cast, single-target
    MarxCrush = 18247, // Helper->self, 6.0s cast, range 15 width 30 rect

    MarxSmashLeftBoss = 18214, // Boss->self, 6.0s cast, single-target, left side
    MarxSmashRightBoss = 18215, // Boss->self, 6.0s cast, single-target, right side
    MarxSmashLeft = 18216, // Helper->self, 1.6s cast, range 60 width 30 rect
    MarxSmashRight = 18217, // Helper->self, 1.6s cast, range 60 width 30 rect
    MarxSmashInsideFirst = 18218, // Boss->self, 6.0s cast, single-target
    MarxSmashInsideNear = 18219, // Helper->self, 1.5s cast, range 30 width 60 rect, front half
    MarxSmashInsideSecond = 18220, // Boss->self, no cast, single-target
    MarxSmashInsideMiddle = 18221, // Helper->self, 0.5s cast, range 60 width 30 rect, middle front to back
    MarxSmashOutsideFirst = 18222, // Boss->self, 6.0s cast, single-target
    MarxSmashOutsideFar = 18223, // Helper->self, 1.5s cast, range 35 width 60 rect, back half
    MarxSmashOutsideSecond = 18224, // Boss->self, no cast, single-target
    MarxSmashOutsideRight = 18225, // Helper->self, 0.5s cast, range 60 width 20 rect, sides
    MarxSmashOutsideLeft = 18226, // Helper->self, 0.5s cast, range 60 width 20 rect, sides

    MarxThrustVisual = 18262, // MarxHelper2/MarxHelper1->self, 5.0s cast, single-target
    MarxThrust = 18684, // Helper->self, 5.5s cast, range 30 width 20 rect

    PrecisionGuidedMissileCast = 18259, // Boss->self, 4.0s cast, single-target
    PrecisionGuidedMissile = 18260, // Helper->players, 4.0s cast, range 6 circle

    WideAngleDiffuseLaserVisual = 18240, // Boss->self, no cast, single-target
    WideAngleDiffuseLaser = 18241, // Helper->self, no cast, range 60 width 60 rect
    Unk2 = 18243, // Boss->self, no cast, single-target

    // 2P abilities, ignore
    AutoAttack2P1 = 19041, // Ally2P1->self, no cast, range 30 circle
    AutoAttack2P2 = 19043, // Ally2P1->ReverseJointedGoliath, no cast, single-target
    Unk2P = 19045, // Ally2P1->location, no cast, single-target
    SaturationBombingManeuver1 = 18896, // Ally2P1->self, 4.0s cast, range 75 circle
    SaturationBombingManeuver2 = 19042, // Ally2P2->self, 5.0s cast, range 75 circle
    SaturationBombingManeuver3 = 19044 // Ally2P2->self, 5.0s cast, range 75 circle
}

public enum SID : uint
{
    Burns = 2194, // Boss->player, extra=0x0
    Hover = 1515 // none->Ally2P1, extra=0xFA
}

public enum IconID : uint
{
    Tankbuster = 198, // player->self
    IncendiaryBombing = 23, // player->self
    GuidedMissile = 197 // player->self, chasing AOE
}
