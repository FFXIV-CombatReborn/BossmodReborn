namespace BossMod.Dawntrail.Trial.T06Arkveld;

public enum OID : uint
{
    GuardianArkveld = 0x48E2, // R7.800
    SmallCrystal = 0x48E3, // R0.700, cracked crystal
    BigCrystal = 0x48E4, // R1.200, cracked crystal
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 43341, // GuardianArkveld->player, no cast, single-target

    Roar1 = 43886, // GuardianArkveld->self, 5.0s cast, range 60 circle
    Roar2 = 45201, // GuardianArkveld->self, 5.0s cast, range 60 circle

    ChainbladeBlowBossCast1 = 43828, // GuardianArkveld->self, 5.0s cast, single-target
    ChainbladeBlowBossCast2 = 43829, // GuardianArkveld->self, 5.0s cast, single-target
    ChainbladeBlowBossCast3 = 45052, // GuardianArkveld->self, 4.0s cast, single-target
    ChainbladeBlowBossCast4 = 45053, // GuardianArkveld->self, 4.0s cast, single-target

    ChainbladeBlow1 = 43830, // Helper->self, 6.2s cast, range 40 width 4 rect
    ChainbladeBlow2 = 43831, // Helper->self, 6.6s cast, range 40 width 4 rect
    ChainbladeBlow3 = 45050, // Helper->self, 6.2s cast, range 40 width 4 rect
    ChainbladeBlow4 = 45051, // Helper->self, 6.6s cast, range 40 width 4 rect
    ChainbladeBlow5 = 45054, // Helper->self, 5.2s cast, range 40 width 4 rect
    ChainbladeBlow6 = 45055, // Helper->self, 5.6s cast, range 40 width 4 rect
    ChainbladeBlow7 = 45057, // Helper->self, 5.2s cast, range 40 width 4 rect
    ChainbladeBlow8 = 45058, // Helper->self, 5.6s cast, range 40 width 4 rect

    ChainbladeSide1 = 43832, // Helper->self, 7.2s cast, range 80 width 28 rect
    ChainbladeSide2 = 43833, // Helper->self, 7.2s cast, range 80 width 28 rect
    ChainbladeSide3 = 45056, // Helper->self, 6.2s cast, range 80 width 28 rect
    ChainbladeSide4 = 45059, // Helper->self, 6.2s cast, range 80 width 28 rect

    WyvernsRadianceExawaveFirst = 43877, // Helper->self, 2.5s cast, range 8 width 40 rect
    WyvernsRadianceExawaveRest = 43878, // Helper->self, 1.0s cast, range 8 width 40 rect

    WyvernsRadiance1 = 43840, // Helper->self, 10.0s cast, range 40 width 18 rect
    WyvernsRadiance2 = 43839, // Helper->self, 10.0s cast, range 40 width 18 rect
    WyvernsRadiance3 = 45072, // Helper->self, 9.0s cast, range 40 width 18 rect
    WyvernsRadiance4 = 45073, // Helper->self, 9.0s cast, range 40 width 18 rect

    WyvernsRadiancePuddleSmall = 43857, // SmallCrystal->self, 1.4s cast, range 6 circle
    WyvernsRadiancePuddleLarge = 43858, // BigCrystal->self, 1.4s cast, range 12 circle
    WyvernsRadianceCrystalSmall = 44807, // Helper->self, 2.0s cast, range 6 circle
    WyvernsRadianceCrystalLarge = 44808, // Helper->self, 2.0s cast, range 12 circle

    WyvernsRadianceQuake1 = 43844, // Helper->self, 7.5s cast, range 8 circle
    WyvernsRadianceQuake2 = 43845, // Helper->self, 9.5s cast, range 8-14 donut
    WyvernsRadianceQuake3 = 43846, // Helper->self, 11.5s cast, range 14-20 donut
    WyvernsRadianceQuake4 = 43847, // Helper->self, 13.5s cast, range 20-26 donut

    WyveCannonMiddle = 43880, // Helper->self, 3.5s cast, range 40 width 8 rect
    WyveCannonRepeat = 43881, // Helper->self, 1.0s cast, range 40 width 4 rect
    WyveCannonEdge = 43882, // Helper->self, 2.0s cast, range 40 width 4 rect

    SiegeflightBoss1 = 43834, // GuardianArkveld->location, 5.0s cast, range 40 width 4 rect
    SiegeflightBoss2 = 43837, // GuardianArkveld->location, 5.0s cast, range 40 width 4 rect
    SiegeflightBoss3 = 45067, // GuardianArkveld->location, 4.0s cast, range 40 width 4 rect
    SiegeflightBoss4 = 45068, // GuardianArkveld->location, 4.0s cast, range 40 width 4 rect

    SiegeflightCast1 = 43835, // Helper->self, 6.5s cast, range 40 width 8 rect
    SiegeflightCast2 = 43838, // Helper->self, 6.5s cast, range 40 width 8 rect
    SiegeflightCast3 = 45069, // Helper->self, 5.5s cast, range 40 width 8 rect
    SiegeflightCast4 = 45071, // Helper->self, 5.5s cast, range 40 width 8 rect

    SiegeflightFollowup1 = 43836, // Helper->self, 10.0s cast, range 40 width 16 rect
    SiegeflightFollowup2 = 45070, // Helper->self, 9.0s cast, range 40 width 16 rect

    ChainbladeChargeCast = 43883, // GuardianArkveld->self, 6.0s cast, single-target
    ChainbladeChargeVisual = 43884, // GuardianArkveld->player, no cast, single-target
    ChainbladeChargeStack = 44811, // Helper->location, no cast, range 6 circle

    RushCast = 43842, // GuardianArkveld->location, 6.0s cast, width 12 rect charge
    RushInstant = 43843, // GuardianArkveld->location, no cast, width 12 rect charge
    RushTelegraph = 43841, // Helper->location, 2.5s cast, width 12 rect charge
    QuakeTelegraph = 45075, // Helper->self, 3.5s cast, range 8 circle

    WyvernsOurobladeCast1 = 43850, // GuardianArkveld->self, 6.0+1.5s cast, single-target
    WyvernsOurobladeCast2 = 45060, // GuardianArkveld->self, 5.0+1.5s cast, single-target
    WyvernsOurobladeCast3 = 45062, // GuardianArkveld->self, 5.0+1.5s cast, single-target
    WyvernsOurobladeCast4 = 43848, // GuardianArkveld->self, 6.0+1.5s cast, single-target
    WyvernsOuroblade1 = 43851, // Helper->self, 7.0s cast, range 40 180-degree cone
    WyvernsOuroblade2 = 45061, // Helper->self, 6.0s cast, range 40 180-degree cone
    WyvernsOuroblade3 = 45063, // Helper->self, 6.0s cast, range 40 180-degree cone
    WyvernsOuroblade4 = 43849, // Helper->self, 7.0s cast, range 40 180-degree cone

    GuardianResonance1 = 43856, // Helper->location, 4.0s cast, range 6 circle
    GuardianResonance2 = 43853, // Helper->location, 13.0s cast, range 2 circle, small tower
    GuardianResonance3 = 43854, // Helper->location, 13.0s cast, range 4 circle, large tower
    GreaterResonance = 43855, // Helper->location, no cast, range 60 circle, tower fail

    FlightResonanceVisual1 = 45123, // Helper->self, 7.2s cast, range 40 width 8 rect
    FlightResonanceVisual2 = 45076, // Helper->self, 7.2s cast, range 40 width 8 rect
    FlightResonanceVisual3 = 45124, // Helper->self, 6.2s cast, range 40 width 8 rect
    FlightResonanceVisual4 = 45074, // Helper->self, 6.2s cast, range 40 width 8 rect

    AethericResonance = 43852, // GuardianArkveld->self, 11.7+1.3s cast, single-target

    WyvernsVengeance = 43860, // Helper->self, 5.0s cast, range 6 circle
    WyvernsVengeance1 = 43861, // Helper->location, no cast, range 6 circle

    WildEnergy = 43866, // Helper->players, 8.0s cast, range 6 circle

    ForgedFuryCast = 43869, // GuardianArkveld->self, 5.0s cast, single-target
    ForgedFuryHit1 = 43870, // Helper->self, 7.0s cast, range 60 circle
    ForgedFuryHit2 = 44790, // Helper->self, 7.8s cast, range 60 circle
    ForgedFuryHit3 = 44791, // Helper->self, 10.2s cast, range 60 circle

    WyvernsWealCast1 = 45046, // GuardianArkveld->self, 6.5+1.5s cast, single-target
    WyvernsWealCast2 = 45047, // GuardianArkveld->self, 6.5+1.5s cast, single-target
    WyvernsWeal1 = 45048, // Helper->self, 8.0s cast, range 60 width 6 rect
    WyvernsWeal2 = 43875, // Helper->self, no cast, range 60 width 6 rect, repeated pulses while the beam sweeps
    WyvernsWeal3 = 45049, // Helper->self, 8.0s cast, range 60 width 6 rect

    WrathfulRattle = 43879, // GuardianArkveld->self, 1.0+2.5s cast, single-target
    WyvernsRattle = 43876, // GuardianArkveld->self, no cast, single-target

    SteeltailThrustCast = 45064, // GuardianArkveld->self, 3.0s cast, range 60 width 6 rect
    SteeltailThrust = 44804, // Helper->self, 3.6s cast, range 60 width 6 rect

    Unk0 = 45175, // GuardianArkveld->location, no cast, single-target
    Unk1 = 43827, // GuardianArkveld->location, no cast, single-target
    Unk2 = 43859, // GuardianArkveld->self, no cast, single-target
    Unk3 = 43873 // GuardianArkveld->self, no cast, single-target
}

public enum IconID : uint
{
    Share = 100, // player->self
    TurnRight = 501, // GuardianArkveld->self
    TurnLeft = 502 // GuardianArkveld->self
}
