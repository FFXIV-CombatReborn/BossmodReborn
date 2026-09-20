namespace BossMod.Shadowbringers.Raid.E11NFatebreaker;

public enum OID : uint
{
    Boss = 0x3070, // R5.004
    FatebreakersImage = 0x3071, // R5.004
    DemiGukumatz = 0x3073, // R3.780
    HaloOfFlame = 0x3074, // R1.000
    HaloOfLevin = 0x3075, // R1.000
    PrismaticImage = 0x3076, // R0.500
    Helper = 0x233C // R0.500
}

public enum AID : uint
{
    AutoAttack = 870,
    Teleport = 22792,

    BoundOfFaithFire = 19224, // Boss->self, 8.0s cast, visual
    BoundOfFaithLightning = 19225, // Boss->self, 8.0s cast, visual
    BoundOfFaithHoly = 19227, // Boss->self, 8.0s cast, visual
    FloatingFetters = 22772, // Boss->player, no cast, applies fetters

    BurntStrikeFire = 22060, // Boss->self, 8.0s cast, range 80 width 10 rect
    Blastburn = 22061, // Helper->self, 10.0s cast, range 80 width 50 rect, lateral knockback
    BurntStrikeLightning = 22062, // Boss->self, 8.0s cast, range 80 width 10 rect
    Burnout = 22063, // Helper->self, 9.7s cast, range 80 width 20 rect
    BurntStrikeHoly = 22064, // Boss->self, 8.0s cast, range 80 width 10 rect
    ShiningBlade = 22065, // Helper->location, 3.0s cast, range 6 circle

    SolemnChargeFire = 22066, // Boss->player, no cast, visual
    Sinsmoke = 22067, // Helper->players, no cast, range 6 circle stack
    SolemnChargeLightning = 22068, // Boss->player, no cast, visual
    Sinsmite = 22069, // Helper->player, no cast, single-target
    SolemnChargeHoly = 22070, // Boss->player, no cast, visual
    Sinsight = 22071, // Helper->player, no cast, range 6 circle
    MortalBurnMark = 22072, // Helper->self, no cast, range 40 circle

    TurnOfTheHeavensFire = 22073, // Boss->self, 8.0s cast, visual
    TurnOfTheHeavensLightning = 22074, // Boss->self, 8.0s cast, visual
    BrightfireSmall = 22075, // HaloOfFlame/HaloOfLevin->self, 8.0s cast, range 5 circle
    BrightfireLarge = 22076, // HaloOfFlame/HaloOfLevin->self, 8.0s cast, range 10 circle

    PrismaticDeception = 22077, // Boss->self, 6.0s cast, visual
    BlastingZone = 22078, // FatebreakersImage->self, no cast, range 50 width 16 rect
    ShiftingSky = 22079, // Boss->self, 8.0s cast, visual

    ImageBurntStrikeFire = 22083, // FatebreakersImage->self, 8.0s cast, range 80 width 10 rect
    ImageBlastburn = 22084, // Helper->self, 10.0s cast, range 80 width 50 rect, lateral knockback
    ImageBurntStrikeLightning = 22085, // FatebreakersImage->self, 8.0s cast, range 80 width 10 rect
    ImageBurnout = 22086, // Helper->self, 9.7s cast, range 80 width 20 rect

    DemiGukumatzAppear = 22091, // Helper->self, 4.0s cast, visual
    AgelessSerpent = 22092, // DemiGukumatz->self, no cast, visual
    ResoundingCrack = 22093, // DemiGukumatz->self, 5.0s cast, range 40 270-degree cone

    PowderMark = 22094, // Boss->player, 5.0s cast, tankbuster
    BurnMark = 22095, // Helper->self, no cast, range 10 circle
    BurnishedGlory = 22096, // Boss->self, 5.0s cast, range 40 circle raidwide
    BlastingZoneVisual = 22097 // PrismaticImage->self, 14.0s cast, visual
}

public enum SID : uint
{
    PowderMark = 2451, // Boss->player, delayed Burn Mark explosion
    PrismaticImage = 1621 // FatebreakersImage, extra 0x1 identifies visible Blasting Zone clones
}

public enum IconID : uint
{
    FireStack = 100,
    HolyBait = 101,
    PowderMark = 138
}

public enum TetherID : uint
{
    Holy = 2,
    Fire = 5,
    Lightning = 6
}
