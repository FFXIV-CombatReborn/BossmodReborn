namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

public enum OID : uint
{
    GuardianArkveld = 0x48E5, // R7.8
    ArenaFeatures = 0x1EA1A1, // R2.0
    CrackedCrystal1 = 0x48E7, // R1.2
    CrackedCrystal2 = 0x48E6, // R0.7
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 43342, // GuardianArkveld->player, no cast, single-target
    Teleport1 = 45175, // GuardianArkveld->location, no cast, single-target
    Teleport2 = 43827, // GuardianArkveld->location, no cast, single-target

    Roar1 = 43950, // GuardianArkveld->self, 5.0s cast, range 60 circle, raidwide
    Roar2 = 45202, // GuardianArkveld->self, 5.0s cast, range 60 circle, raidwide
    Roar3 = 43951, // GuardianArkveld->self, 5.0s cast, range 60 circle, raidwide

    ChainbladeBlowVisual1 = 43887, // GuardianArkveld->self, 5.0s cast, single-target
    ChainbladeBlowVisual2 = 43894, // GuardianArkveld->self, no cast, single-target
    ChainbladeBlowVisual3 = 43888, // GuardianArkveld->self, 5.0s cast, single-target
    ChainbladeBlowVisual4 = 43893, // GuardianArkveld->self, no cast, single-target
    ChainbladeBlowVisual5 = 45082, // GuardianArkveld->self, 4.0s cast, single-target
    ChainbladeBlowVisual6 = 45089, // GuardianArkveld->self, no cast, single-target
    ChainbladeBlowVisual7 = 45090, // GuardianArkveld->self, no cast, single-target
    ChainbladeBlowVisual8 = 45081, // GuardianArkveld->self, 4.0s cast, single-target

    ChainbladeBlowFirst1 = 43889, // Helper->self, 6.2s cast, range 40 width 4 rect
    ChainbladeBlowFirst2 = 43890, // Helper->self, 6.6s cast, range 40 width 4 rect
    ChainbladeBlowFirst3 = 45077, // Helper->self, 6.2s cast, range 40 width 4 rect
    ChainbladeBlowFirst4 = 45078, // Helper->self, 6.6s cast, range 40 width 4 rect
    ChainbladeBlowFirst5 = 45086, // Helper->self, 5.2s cast, range 40 width 4 rect
    ChainbladeBlowFirst6 = 45087, // Helper->self, 5.6s cast, range 40 width 4 rect
    ChainbladeBlowFirst7 = 45083, // Helper->self, 5.2s cast, range 40 width 4 rect
    ChainbladeBlowFirst8 = 45084, // Helper->self, 5.6s cast, range 40 width 4 rect
    ChainbladeBlowRest1 = 45079, // Helper->self, 1.2s cast, range 40 width 4 rect
    ChainbladeBlowRest2 = 45080, // Helper->self, 1.6s cast, range 40 width 4 rect
    ChainbladeBlowRest3 = 43895, // Helper->self, 1.2s cast, range 40 width 4 rect
    ChainbladeBlowRest4 = 43896, // Helper->self, 1.6s cast, range 40 width 4 rect
    ChainbladeBlowRest5 = 45091, // Helper->self, 1.2s cast, range 40 width 4 rect
    ChainbladeBlowRest6 = 45092, // Helper->self, 1.6s cast, range 40 width 4 rect
    ChainbladeBlowRest7 = 45094, // Helper->self, 1.2s cast, range 40 width 4 rect
    ChainbladeBlowRest8 = 45095, // Helper->self, 1.6s cast, range 40 width 4 rect

    WyvernsRadianceCleaveFirstL1 = 43891, // Helper->self, 7.2s cast, range 80 width 28 rect
    WyvernsRadianceCleaveFirstR2 = 45088, // Helper->self, 6.2s cast, range 80 width 28 rect
    WyvernsRadianceCleaveFirstR1 = 43892, // Helper->self, 7.2s cast, range 80 width 28 rect
    WyvernsRadianceCleaveFirstL2 = 45085, // Helper->self, 6.2s cast, range 80 width 28 rect
    WyvernsRadianceCleaveSecondR1 = 43898, // Helper->self, 2.2s cast, range 80 width 28 rect
    WyvernsRadianceCleaveSecondL2 = 45093, // Helper->self, 2.2s cast, range 80 width 28 rect
    WyvernsRadianceCleaveSecondL1 = 43897, // Helper->self, 2.2s cast, range 80 width 28 rect
    WyvernsRadianceCleaveSecondR2 = 45096, // Helper->self, 2.2s cast, range 80 width 28 rect

    GuardianSiegeflightVisual1 = 43899, // GuardianArkveld->location, 5.0s cast, range 40 width 4 rect
    GuardianSiegeflightVisual2 = 45125, // Helper->self, 7.2s cast, range 40 width 8 rect
    GuardianSiegeflightVisual3 = 45097, // GuardianArkveld->location, 4.0s cast, range 40 width 4 rect
    GuardianSiegeflightVisual4 = 45126, // Helper->self, 6.2s cast, range 40 width 8 rect
    WyvernsSiegeflightVisual1 = 43902, // GuardianArkveld->location, 5.0s cast, range 40 width 4 rect
    WyvernsSiegeflightVisual2 = 45111, // Helper->self, 7.2s cast, range 40 width 8 rect
    WyvernsSiegeflightVisual3 = 45098, // GuardianArkveld->location, 4.0s cast, range 40 width 4 rect
    WyvernsSiegeflightVisual4 = 45104, // Helper->self, 6.2s cast, range 40 width 8 rect

    GuardianSiegeflight1 = 43900, // Helper->self, 6.5s cast, range 40 width 8 rect
    GuardianSiegeflight2 = 45099, // Helper->self, 5.5s cast, range 40 width 8 rect
    GuardianResonanceRect1 = 43901, // Helper->self, 10.0s cast, range 40 width 16 rect
    GuardianResonanceRect2 = 45100, // Helper->self, 9.0s cast, range 40 width 16 rect
    WyvernsSiegeflight1 = 43903, // Helper->self, 6.5s cast, range 40 width 8 rect
    WyvernsSiegeflight2 = 45101, // Helper->self, 5.5s cast, range 40 width 8 rect
    WyvernsRadianceSides1 = 43905, // Helper->self, 10.0s cast, range 40 width 18 rect
    WyvernsRadianceSides2 = 43904, // Helper->self, 10.0s cast, range 40 width 18 rect
    WyvernsRadianceSides3 = 45102, // Helper->self, 9.0s cast, range 40 width 18 rect
    WyvernsRadianceSides4 = 45103, // Helper->self, 9.0s cast, range 40 width 18 rect

    WyvernsRadianceRect1 = 43944, // Helper->self, 3.5s cast, range 40 width 8 rect
    WyvernsRadianceRect2 = 43945, // Helper->self, 1.0s cast, range 40 width 4 rect
    WyvernsRadianceRect3 = 43946, // Helper->self, 2.0s cast, range 40 width 4 rect

    WhiteFlash = 43906, // Helper->players, 8.0s cast, range 6 circle, light party stack
    Dragonspark = 43907, // Helper->players, 8.0s cast, range 6 circle, light party stack
    WildEnergy = 43932, // Helper->players, 8.0s cast, range 6 circle, spread

    WyvernsRattle = 43939, // GuardianArkveld->self, no cast, single-target
    WyvernsRadianceExaflareFirst = 43940, // Helper->self, 2.5s cast, range 8 width 40 rect
    WyvernsRadianceExaflareRest = 43941, // Helper->self, 1.0s cast, range 8 width 40 rect
    WyvernsRadianceCircle = 43942, // Helper->location, 5.0s cast, range 6 circle, baited AOEs

    WyvernsRadianceChargeTelegraph = 43908, // Helper->location, 2.5s cast, width 12 rect charge
    WyvernsRadianceConcentricTelegraph = 45110, // Helper->self, 3.5s cast, range 8 circle
    RushFirst = 43909, // GuardianArkveld->location, 6.0s cast, width 12 rect charge
    RushRest = 43910, // GuardianArkveld->location, no cast, width 12 rect charge
    WyvernsRadianceConcentric1 = 43911, // Helper->self, 7.5s cast, range 8 circle
    WyvernsRadianceConcentric2 = 43912, // Helper->self, 9.5s cast, range 8-14 donut
    WyvernsRadianceConcentric3 = 43913, // Helper->self, 11.5s cast, range 14-20 donut
    WyvernsRadianceConcentric4 = 43914, // Helper->self, 13.5s cast, range 20-26 donut

    WyvernsOurobladeVisual1 = 43915, // GuardianArkveld->self, 6.0+1,5s cast, single-target
    WyvernsOurobladeVisual2 = 43917, // GuardianArkveld->self, 6.0+1,5s cast, single-target
    WyvernsOurobladeVisual3 = 45105, // GuardianArkveld->self, 5.0+1,5s cast, single-target
    WyvernsOurobladeVisual4 = 45106, // GuardianArkveld->self, 5.0+1,5s cast, single-target
    WyvernsOuroblade1 = 43916, // Helper->self, 7.0s cast, range 40 180-degree cone
    WyvernsOuroblade2 = 43918, // Helper->self, 7.0s cast, range 40 180-degree cone
    WyvernsOuroblade3 = 45107, // Helper->self, 6.0s cast, range 40 180-degree cone
    WyvernsOuroblade4 = 45108, // Helper->self, 6.0s cast, range 40 180-degree cone

    SteeltailThrustVisual1 = 43949, // GuardianArkveld->self, 4.0s cast, range 60 width 6 rect
    SteeltailThrustVisual2 = 44806, // Helper->self, 3.6s cast, range 60 width 6 rect
    SteeltailThrust1 = 44805, // Helper->self, 4.6s cast, range 60 width 6 rect
    SteeltailThrust2 = 45109, // GuardianArkveld->self, 3.0s cast, range 60 width 6 rect

    ChainbladeChargeVisual1 = 43947, // GuardianArkveld->self, 6.0s cast, single-target
    ChainbladeChargeVisual2 = 43948, // GuardianArkveld->player, no cast, single-target
    ChainbladeCharge = 44812, // Helper->location, no cast, range 6 circle
    WyvernsRadianceChainbladeCharge = 43933, // Helper->location, 5.0s cast, range 12 circle

    AethericResonance = 43919, // GuardianArkveld->self, 9.7+1,3s cast, single-target
    GuardianResonanceAOE = 43923, // Helper->location, 3.0s cast, range 6 circle
    GuardianResonanceTower1 = 43921, // Helper->location, 11.0s cast, range 4 circle, tankbuster tower
    GuardianResonanceTower2 = 43920, // Helper->location, 11.0s cast, range 2 circle, regular tower
    GreaterResonance = 43922, // Helper->location, no cast, range 60 circle, tower fail

    WyvernsVengeanceVisual = 43859, // GuardianArkveld->self, no cast, single-target
    WyvernsVengeanceFirst1 = 43926, // Helper->self, 5.0s cast, range 6 circle
    WyvernsVengeanceFirst2 = 43952, // Helper->self, 8.0s cast, range 6 circle
    WyvernsVengeanceRest1 = 43927, // Helper->location, no cast, range 6 circle
    WyvernsVengeanceRest2 = 43953, // Helper->location, no cast, range 6 circle

    WyvernsRadianceCrackedCrystalBigVisual = 43925, // CrackedCrystal1->self, 0.5s cast, range 12 circle
    WyvernsRadianceCrackedCrystalSmallVisual = 43924, // CrackedCrystal2->self, 0.5s cast, range 6 circle
    WyvernsRadianceCrackedCrystalBig = 44810, // Helper->self, 1.0s cast, range 12 circle
    WyvernsRadianceCrackedCrystalSmall = 44809, // Helper->self, 1.0s cast, range 6 circle

    ForgedFuryVisual = 43934, // GuardianArkveld->self, 5.0s cast, single-target, raidwide x3
    ForgedFury1 = 43935, // Helper->self, 7.0s cast, range 60 circle
    ForgedFury2 = 44792, // Helper->self, 7.8s cast, range 60 circle
    ForgedFury3 = 44793, // Helper->self, 10.2s cast, range 60 circle

    ClamorousChaseVisual1 = 43958, // GuardianArkveld->self, 8.0s cast, single-target
    ClamorousChaseVisual2 = 43955, // GuardianArkveld->self, 8.0s cast, single-target
    ClamorousChaseJump1 = 43959, // GuardianArkveld->location, no cast, range 6 circle
    ClamorousChaseJump2 = 43956, // GuardianArkveld->location, no cast, range 6 circle
    ClamorousChaseCleave1 = 43960, // Helper->self, 1.0s cast, range 60 180-degree cone
    ClamorousChaseCleave2 = 43957, // Helper->self, 1.0s cast, range 60 180-degree cone

    WyvernsWealVisual1 = 43936, // GuardianArkveld->self, 8.0s cast, single-target
    WyvernsWealVisual2 = 43872, // GuardianArkveld->self, no cast, single-target
    WyvernsWealVisual3 = 43873, // GuardianArkveld->self, no cast, single-target
    WyvernsWealFirst = 43937, // Helper->self, 2.0s cast, range 60 width 6 rect
    WyvernsWealRepeat = 43938, // Helper->self, 0.5s cast, range 60 width 6 rect

    WrathfulRattle = 43943, // GuardianArkveld->self, 1.0+2,5s cast, single-target

    ForgedFuryEnrageVisual = 43954, // GuardianArkveld->self, 5.0s cast, range 60 circle
    ForgedFuryEnrage = 44878 // Helper->self, 10.2s cast, range 60 circle
}

public enum IconID : uint
{
    ChainbladeCharge = 100, // player->self, also used by other stack markers unfortunately
    Icon1 = 404, // player->self
    Icon2 = 405, // player->self
    Icon3 = 406, // player->self
    Icon4 = 407, // player->self
    Icon5 = 408, // player->self
    Icon6 = 409, // player->self
    Icon7 = 410, // player->self
    Icon8 = 411, // player->self
    WyvernsWeal = 470 // player->self
}
