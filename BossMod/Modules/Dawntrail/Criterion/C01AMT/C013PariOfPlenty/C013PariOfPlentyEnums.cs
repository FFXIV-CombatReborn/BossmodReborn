namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

public enum OID : uint
{
    PariOfPlenty = 0x4A70, // R5.016, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    FlyingCarpet = 0x4A74, // R4.356, x8
    FalseFlame = 0x4A71, // R5.016, x4
    FieryBauble = 0x4A72, // R6.000, x8
    IcyBauble = 0x4A73, // R5.000, x1
    PathPoints = 0x4A75, // R1.000, x6 - Used for pathing mainly in the fight
    BurnPuddle = 0x1EBF20, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 45545,
    HeatBurst = 45517,
    Jump = 45421,

    FireflightByEmberlightRight = 45434,
    FireflightByEmberlightLeft = 45435,
    FireflightByPyrelightRight = 45436,
    FireflightByPyrelightLeft = 45437,
    CarpetRideDashRight = 45440,
    CarpetRideDashLeft = 45441,
    CarpetRide1Right = 45442,
    CarpetRide1Left = 45443,
    FireflightByEmberlightStack = 45444,
    FireflightByPyrelightSpread = 45445,
    SunCirclet = 45448,

    WheelOfFableflightCCW = 45478,
    WheelOfFableflightCW = 45479,
    ScatteredKindlingCast = 45536,
    ScatteredKindling = 45537,
    KindledFlame1Cast = 45538,
    KindledFlame1 = 45539,
    WheelOfFireflightRTLBack = 45469,
    WheelOfFireflightRTLFront = 45470,
    WheelOfFireflightLTRFront = 45471,
    WheelOfFireflightLTRBack = 45472,
    FireOfVictory = 45519,

    FireflightFourLongNightsE = 45467,
    FireflightFourLongNightsW = 45468,
    FellFirework = 45476,
    FireWell = 45477,

    ParisCurse = 45551,
    CharmingBaubles = 45503,
    CharmingBaublesInstant = 45497,
    ThievesWeave = 46754,
    CarpetAppear = 45504,
    CarpetDash = 47153,
    Unravel = 45506,
    FableflightRightVerySlow = 45438,
    FableflightLeftVerySlow = 45439,
    BurningGleam = 45548,
    ChillingGleam = 45501,
    FirePowder = 45523,
    HighFirePowder = 45524,

    SpurningFlames = 45482,
    ImpassionedSparksCast = 45483,
    ImpassionedSparksVisual = 45484,
    ImpassionedSparksFireball = 45485,
    ImpassionedSparksPuddle = 45488,
    BurningPillar = 45527,
    HotFoot = 45542,
    ScouringScorn = 45491,

    Doubling = 47041,
    FableflightRightFast = 45449,
    FableflightLeftFast = 45450,
    FableflightRightSlow = 45451,
    FableflightLeftSlow = 45452,
    Explosion = 45532,
    UnmitigatedExplosion = 45533,
    CarpetRideTetherRight = 46573,
    CarpetRideTetherLeft = 46574,

    KindledFlame2Cast = 45540,
    KindledFlame2 = 45541,

    CharmdFlightFourNightsE = 47031,
    CharmdFlightFourNightsW = 47032,

    SpurningFlamesPreEnrage = 45492,
    ScouringScornEnrageCast = 45493,
    ScouringScornEnrage = 45494,
    UtterImmolation = 45525
}

public enum SID : uint
{
    WitchHunt = 2970,
    FireResistanceDownII = 2937,
    MagicVulnerabilityUp = 2941,
    DarkResistanceDown = 3619,
    CurseOfImmolation = 4617,
    CurseOfSolitude = 4615,
    CurseOfCompanionship = 4616,
    Fury = 4627,
    ChainsOfPassionA = 4844,
    ChainsOfPassionB = 4845,
    Burns = 3065,
    BurningChains = 769,
    Prey = 2939
}

public enum IconID : uint
{
    LeftToRight3Sec = 624,
    LeftToLeft3Sec = 625,
    LeftToRight8Sec = 628,
    LeftToLeft8Sec = 629,
    RightToRight3Sec = 644,
    RightToLeft3Sec = 645,
    RightToRight8Sec = 646,
    RightToLeft8Sec = 647,
    Tankbuster = 342,
    Chain = 97,
    Spread10 = 138,
    Rug = 631,
    Stack = 161
}

public enum TetherID : uint
{
    Rug = 355,
    Chain = 9,
    Generic = 17
}
