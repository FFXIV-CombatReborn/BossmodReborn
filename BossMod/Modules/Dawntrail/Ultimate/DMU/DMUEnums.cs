namespace BossMod.Dawntrail.Ultimate.DMU;

public enum OID : uint
{
    // Phase 1
    Kefka = 0x4C30, // R6.000, x1
    Helper = 0x233C, // R0.500, x37, Helper type
    GravenImage = 0x4C31, // R0.500, x9
    StatueWaveCannon = 0x1EBFBB, // R0.500, x2, EventObj type
    GravitasPuddles = 0x1EC022, // R0.500, x0 (spawn during fight), EventObj type
    YellowOrb = 0x1EBFBD, // R0.500, x2, EventObj type // Yellow orb - GravitationalWave
    PurpleOrb = 0x1EBFBC, // R0.500, x2, EventObj type // Purple orb - GravitationalWave
    TeleTrouncingArrow = 0x1EC023, // R0.500, x0 (spawn during fight), EventObj type
    StatuePurpleEye = 0x1EBFBE, // R0.500, x2, EventObj type // Purple eye for sleep
    StatueYellowEye = 0x1EBFBF, // R0.500, x2, EventObj type // Yellow eye for confusion

    Actor1e8536 = 0x1E8536, // R0.500-2.000, x1, EventObj type
    Actor1ebfb4 = 0x1EBFB4, // R0.500, x1, EventObj type

    // Phase 2
    BossP2 = 0x4C32,
    P2KefkaHelpers = 0x4C39, // R3.500, x0 (spawn during fight)
    YellowTriangle = 0x1EBFB2, // R0.500, x0 (spawn during fight), EventObj type
    YellowTriangle1 = 0x1EBFB3, // R0.500, x0 (spawn during fight), EventObj type

    // Phase 3
    Chaos = 0x4C34,
    Exdeath = 0x4C35,
    KefkaP3 = 0x4BFB, // R2.700, x0 (spawn during fight)
    Kefka1P3 = 0x482B, // R6.000, x0 (spawn during fight)
    WindP3 = 0x1EC03C, // R0.500, x0 (spawn during fight), EventObj type
    WaterP3 = 0x1EC03B, // R0.500, x0 (spawn during fight), EventObj type
    FireP3 = 0x1EC03A, // R0.500, x0 (spawn during fight), EventObj type
    BlackHole = 0x4C38, // R1.000, x0 (spawn during fight)

    _Gen_Actor1ec03d = 0x1EC03D, // R0.500, x0 (spawn during fight), EventObj type

    // Phase 4
    KefkaP4 = 0x482B,
    NeoExdeath = 0x4C36, // R9.000, x0 (spawn during fight)
    ChaosP4 = 0x4C33, // R6.000, x0 (spawn during fight)

    // Phase 5
    KefkaP5 = 0x4C37, // R8.010, x0 (spawn during fight)
    IceTower = 0x1EC03F, // R0.500, x0 (spawn during fight), EventObj type
    ThunderTower = 0x1EC040, // R0.500, x0 (spawn during fight), EventObj type
    FireTower = 0x1EC03E, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    // Phase 1
    P1AutoAttack = 49746, // Kefka->player, no cast, single-target
    P1Teleport = 50173, // Kefka->location, no cast, single-target

    RevoltingRuinIIIFirstHit = 50179, // Kefka->self/players, 5.0s cast, range 100 150-degree cone
    RevoltingRuinIIISecondHit = 50401, // Kefka->self, no cast, range 100 150-degree cone

    GravenImage = 48370, // Kefka->self, 3.0s cast, single-target
    PulseWave = 47785, // 4C31->player, no cast, single-target

    MysteryMagic = 47764, // Kefka->self, 5.0s cast, single-target
    BlizzardIIIBlowoutBoss = 47765, // Kefka->self, 5.0s cast, single-target
    BlizzardIIIBlowoutFake = 47771, // Helper->self, 5.0s cast, range 40 90-degree cone - does no damage
    BlizzardIIIBlowout = 47774, // Helper->self, 5.0s cast, range 40 90-degree cone
    BlizzardIIIBlowout1 = 47768, // Helper->self, 5.0s cast, range 40 90-degree cone
    FlagrantFireIIIStack = 47779, // Helper->players, no cast, range 6 circle
    FlagrantFireIIISpread = 47778, // Helper->players, no cast, range 5 circle

    WaveCannon = 47784, // 4C31->self, no cast, range 100 width 6 rect
    TowerExplosion = 47786, // Helper->self, 3.0s cast, range 4 circle
    UnmitigatedExplosion = 47787, // Helper->self, no cast, range 100 circle

    DoubleTroubleTrap = 47782, // Kefka->self, 3.0s cast, single-target
    DoubleTroubleTrapStack = 47783, // Helper->player, no cast, range 6 circle

    ThrummingThunderIII = 47775, // Helper->self, 5.0s cast, range 40 width 10 rect
    ThrummingThunderIII1 = 47777, // Helper->self, 5.0s cast, range 40 width 10 rect
    ThrummingThunderIIIFake = 47776, // Helper->self, 5.0s cast, range 40 width 10 rect

    P1LightOfJudgment = 50722, // Kefka->self, 5.0s cast, range 100 circle
    Hyperdrive = 49739, // Kefka->player, no cast, range 5 circle

    Gravitas = 47788, // 4C31->player, no cast, range 5 circle - Purple puddle - Stack
    Vitrophyre = 47792, // _Gen_GravenImage->players, no cast, range 5 circle - Rock - Spread
    GravityIII = 47791, // Helper->self, no cast, range 5 circle
    GravitationalExplosion = 47789, // Helper->self, no cast, range 100 circle
    GravitationalWave = 47793, // 4C31->self, no cast, range 100 ?-degree cone - Purple left side
    IntemperateWill = 47794, // _Gen_GravenImage->self, no cast, range 100 ?-degree cone - Yellow right side

    TeleTrouncing = 47801, // Kefka->self, 5.0s cast, single-target
    TeleTrouncing1 = 47802, // Helper->players, no cast, range 2 circle
    IdyllicWill = 47798, // _Gen_GravenImage->players, no cast, range 5 circle
    IndulgentWill = 47797, // _Gen_GravenImage->player, no cast, single-target
    AveMaria = 47795, // _Gen_GravenImage->self, no cast, range 100 circle
    IndolentWill = 47796, // _Gen_GravenImage->self, no cast, range 100 circle

    Unknown = 50516, // Kefka->self, 3.0s cast, single-target - most likely if you solve the mechanic correctly or not to get into P3
    Unknown1 = 50517, // Kefka->self, no cast, single-target - Most likely if you fail to solve the arrow mechanic correctly
    P1LightOfJudgmentEnrage = 47803, // Kefka->self, 5.0s cast, range 100 circle

    // Phase 2
    UltimateEmbrace = 49740, // BossP2->players, 5.0s cast, range 5 circle
    Forsaken = 47804, // BossP2->self, 7.0s cast, range 100 circle
    ThePathOfLight = 47806, // Helper->self, no cast, range 4 circle
    TheRiverOfLight = 47807, // Helper->self, no cast, range 100 circle

    Spellscatter = 47809, // Helper->player, no cast, range 5 circle
    Spelldriver = 47808, // Helper->player, no cast, range 5 circle
    Spellwave = 47810, // Helper->self, no cast, range 40 ?-degree cone

    PastsEndCast = 47827, // BossP2->self, 6.4s cast, single-target
    FuturesEndCast = 47826, // BossP2->self, 6.4s cast, single-target
    FuturesEndSpread = 47830, // BossP2->players, no cast, range 5 circle
    FuturesEndSpread1 = 47832, // 4C39->players, no cast, range 5 circle
    PastsEndSpread = 47831, // BossP2->player, no cast, range 5 circle
    PastsEndSpread1 = 47833, // 4C39->players, no cast, range 5 circle

    AllThingsEnding = 47837, // BossP2->self, 5.0s cast, range 100 ?-degree cone
    AllThingsEnding1 = 47836, // 4C39/BossP2->self, 5.0s cast, range 100 ?-degree cone

    LightOfJudgmentP2 = 47805, // BossP2->self, 5.0s cast, range 100 circle

    Trine = 47839, // BossP2->self, 3.0s cast, single-target
    Trine1 = 47840, // Helper->self, no cast, range 6 circle
    WingsOfDestructionLeft = 47821, // BossP2->self, 4.0s cast, range 80 width 40 rect
    WingsOfDestructionRight = 47822, // BossP2->self, 4.0s cast, range 80 width 40 rect
    WingsOfDestructionTB = 50311, // BossP2->self, 4.0s cast, single-target
    WingsOfDestructionTB1 = 47823, // Helper->player, no cast, range 7 circle

    // Phase 3
    AeroIIIAssault = 50167, // Kefka->self, 3.0s cast, range 40 circle
    ExdeathAutoAttack = 49744, // Exdeath->player, no cast, single-target

    TheDecisiveBattle = 49890, // Chaos->self, 3.0s cast, single-target
    TheDecisiveBattle1 = 49891, // Exdeath->self, 3.0s cast, single-target
    DefinitionOfInsanity = 47842, // Kefka->self, 4.0s cast, single-target

    BowelsOfAgony = 47858, // Chaos->self, 5.0s cast, range 100 circle
    Tsunami = 47861, // Helper->player, no cast, range 5 circle
    StraySpray = 47862, // Helper->players, no cast, range ?-10 donut
    StrayFlames = 47859, // Helper->players, no cast, range 5 circle
    Inferno = 47860, // Helper->player, no cast, range ?-10 donut

    LongitudinalImplosion = 47869, // Chaos->self, 5.0+0.8s cast, single-target
    LatitudinalImplosion = 47870, // Chaos->self, 5.0+0.8s cast, single-target
    Shockwave = 47871, // Helper->self, no cast, range 40 ?-degree cone

    ThunderIII = 47890, // Exdeath->self, 7.0s cast, range 11+R circle
    ThunderIIITBCast = 47881, // Exdeath->self, 5.0s cast, single-target
    ThunderIIITB = 47884, // Helper->player, no cast, range 5 circle

    Trance = 49878, // Kefka->self, 3.9s cast, single-target
    UltimaBlaster = 47843, // 4BFB->self, no cast, range 100 circle
    UltimaBlasterBait = 47844, // 4BFB->self, no cast, range 100 width 6 rect
    UmbraSmash = 47872, // Chaos->location, 5.0s cast, range 100 circle
    VacuumWave = 47891, // Exdeath->self, 8.0s cast, range 100 circle

    Cyclone = 47864, // Helper->players, no cast, range 6 circle
    _Ability_Aetherlink = 49892, // Chaos->self, no cast, single-target
    _Ability_Aetherlink1 = 49893, // Exdeath->self, no cast, single-target

    Max = 47845, // Kefka->self, 5.0s cast, single-target
    _Ability_ = 50483, // Kefka->self, no cast, single-target - Most likely what the bosses cast to not be big anymore
    AbilityUnknownTeleport = 50362, // Kefka->self, no cast, single-target - most likely teleport, but not checked

    SlapHappyRightHand = 47846, // Kefka->self, 5.0s cast, single-target - Right hand
    SlapHappyLeftHand = 47847, // Kefka->self, 5.0s cast, single-target - Left hand
    SlapHappyBigAOE = 47848, // Helper->self, no cast, range 13 circle - AOE where he slams across the map
    SlapHappySmallAOE = 47849, // Helper->self, 1.5s cast, range 6 circle - Middle small hand
    SlapHappyShockingImpactStack = 47850, // Helper->self, no cast, range 100 ?-degree cone - party stack -> right hand
    SlapHappyShockwaveRole = 47851, // Helper->self, no cast, range 100 ?-degree cone - role spread -> left hand
    DamningEdict = 47873, // Chaos->self, 5.0s cast, range 60 width 80 rect - Just a simple AOE cleave from the direction the boss is looking
    WhiteHole = 48486, // Exdeath->self, 5.0s cast, range 80 circle - Simple raidwide where everyone has to be at max HP
    LookUponMeAndDespair = 47852, // Kefka->self, 4.0+1.0s cast, single-target - Seems to be the skill to update his model to slam the map
    LookUponMeAndDespair2 = 47853, // Kefka->self, 4.0+1.0s cast, single-target - Seems to be the skill to update his model to slam the map
    LookUponMeAndDespairAOE = 47854, // Helper->self, 5.0s cast, range 100 width 16 rect - AOE on middle which simply just need to be dodged
    EarthquakeRaidwide = 50545, // Chaos->self, 5.0s cast, single-target
    EarthquakeCast = 50546, // Helper->self, 5.0s cast, range 100 circle
    EarthquakeInstant = 47866, // Helper->self, no cast, range 100 circle
    BlackHole = 47867, // Exdeath->self, 3.0s cast, single-target - summons actors around the map
    Nothingness = 47868, // 4C38->self, no cast, range 125 width 6 rect - Tether AOE?

    BlizzardIIICast = 47887, // Exdeath->self, 3.0s cast, single-target - Bait cast
    BlizzardIIIBaitCast = 47885, // Helper->location, 3.0s cast, range 6 circle - AOE puddle which will explode
    BlizzardIIIRaidwide = 47889, // Exdeath->self, 4.0s cast, range 100 circle - Raidwide + standing still will freeze the target
    KnockDownCast = 47874, // Chaos->self, 5.0s cast, single-target - Stack cast
    KnockDown = 47875, // Helper->players, no cast, range 6 circle - Stack
    BigBangCast = 47877, // Chaos->self, 5.0s cast, single-target - Hidden aoe cast
    BigBang = 47878, // Helper->self, no cast, range 6 circle - AOEs that spawn where the stacks were taken
    StompAMoleCast = 47855, // Kefka->self, 5.0s cast, single-target
    StompAMoleTower = 47856, // Helper->self, 1.5s cast, range 5 circle

    MeteorEnrage = 50718, // Exdeath->self, 10.0s cast, range 100 circle
    BowelsOfAgonyEnrage = 50719, // Chaos->self, 10.0s cast, range 100 circle

    // Phase 4
    KefkaSays = 49884, // KefkaP4->self, 5.0s cast, single-target - Summons Chaos & Exodeath actors
    GrandCross = 47892, // 4C36->self, 9.0s cast, range 100 circle - Applies debuffs? Does it multiple times 3x it seems
    P4Tsunami = 47905, // Helper->self, 9.0s cast, range 100 circle - Applies debuffs?
    P4Tsunami1 = 47903, // 4C33->self, 9.0s cast, single-target - Applies buff to self for telling truth or lie
    P4Inferno = 47904, // Helper->self, 9.0s cast, range 100 circle - Applies debuffs
    P4Inferno1 = 47902, // 4C33->self, 9.0s cast, single-target - Applies buff to self for telling truth or lie

    EdgeOfDeath = 50070, // Helper->self, 5.5s cast, range 48 width 2 rect
    FloodOfNaught = 50066, // NeoExdeath->self, 5.0+0.5s cast, single-target - purple will cast white antilight, white will cast black antilight
    FloodOfNaught2 = 50067, // NeoExdeath->self, 5.0+0.5s cast, single-target - purple will cast white antilight, white will cast black antilight
    FloodOfNaught1 = 50081, // NeoExdeath->self, 5.0+0.5s cast, single-target - purple will cast black antilight, white will cast white antilight
    FloodOfNaught3 = 50082, // NeoExdeath->self, 5.0+0.5s cast, single-target - purple will cast black antilight, white will cast white antilight
    WhiteAntilight = 50068, // Helper->self, 5.5s cast, range 47 width 21 rect
    BlackAntilight = 50069, // Helper->self, 5.5s cast, range 47 width 21 rect
    DeathSurge = 47900, // Helper->self, no cast, range 100 circle - After Antilight casts to ensure it was solved correctly
    DeathSurge1 = 47901, // Helper->self, no cast, range 100 circle - After Antilight casts to ensure it was solved correctly

    ManaCharge = 47780, // KefkaP4->self, 3.0s cast, single-target

    ForkedLightningFake = 47897, // Helper->players, no cast, range 8 circle
    ForkedLightningReal = 47896, // Helper->player, no cast, range 8 circle
    CompressedWaterFake = 47898, // Helper->players, no cast, range 8 circle
    CompressedWaterReal = 47899, // Helper->player, no cast, range 8 circle

    CursedShriekFake = 47895, // Helper->self, no cast, range 100 circle
    CursedShriekReal = 47894, // Helper->self, no cast, range 100 circle

    UltimaUpsurge = 49738, // KefkaP4->self, 5.0s cast, range 100 circle

    StrayFlamesP4Puddle = 47906, // Helper->location, 5.0s cast, range 6 circle
    StraySprayP4Puddle = 47909, // Helper->location, 5.0s cast, range 6 circle
    StraySprayP4Donut = 47908, // Helper->location, 5.0s cast, range 6-40 donut
    StrayFlamesP4Donut = 47907, // Helper->location, 5.0s cast, range 6-40 donut

    _Ability_StrayEarth = 47865, // Helper->player, no cast, single-target - Gaze if you get hit most likely, but unknown
    _Ability_ThrummingThunderIII = 50654, // KefkaP4->self, 5.0s cast, single-target - can this is the cast that will be stored for later?
    _Ability_ManaRelease = 47781, // KefkaP4->self, 7.0s cast, single-target
    _Ability_BlackSpark = 48333, // Helper->player, no cast, single-target - Unknown, maybe blackhole if you walk into it?
    _Ability_UnmitigatedImpact = 47857, // Helper->self, no cast, range 100 circle

    // Phase 5
    UltimaRepeaterCast = 47936, // KefkaP5->self, 4.0+1.0s cast, single-target
    UltimaRepeaterRaidwide = 47937, // Helper->self, no cast, range 100 circle

    _Ability_1 = 50770, // KefkaP5->self, no cast, single-target - TODO Unknown most likely a teleport mid or something to ready for fell forces

    // TODO verify the same skill id always goes to the same target role and check who gets the 3f circle one (currently guessing tank)
    FellForces = 50771, // Helper->players, no cast, range 3 circle
    FellForces1 = 50772, // Helper->players, no cast, range 5 circle
    FellForces2 = 50773, // Helper->players, no cast, range 5 circle

    ChaoticFlood = 49471, // KefkaP5->self, 5.0+1.2s cast, single-target
    ChaoticFloodStack = 47951, // Helper->players, no cast, range 6 circle
    ChaoticFloodAOEDisplay = 49539, // Helper->self, 1.5s cast, range 40 width 10 rect
    ChaoticFloodAOE = 49769, // Helper->self, no cast, range 40 width 10 rect

    MaddeningOrchestra = 47952, // KefkaP5->self, 5.0+0.8s cast, single-target
    MaddeningOrchestra1 = 47953, // KefkaP5->self, no cast, single-target - This is used after the first wave of baits, but doesn't do anything
    Holy = 47956, // Helper->players, no cast, range 5 circle
    Flare = 47954, // Helper->player, no cast, range 5 circle
    ChaoticFlareTB = 47955, // Helper->players, no cast, range 5 circle - The tank buster both tanks should share
    FlareDiffusion = 47957, // Helper->players, no cast, range 25 circle
    ChaoticHoly = 47958, // Helper->player, no cast, range 6 circle

    Celestriad = 47938, // KefkaP5->self, 5.0s cast, single-target
    P4FireIII = 47939, // Helper->self, no cast, range 3 circle
    P4ThunderIII = 47941, // Helper->self, no cast, range 3 circle
    P4BlizzardIII = 47940, // Helper->self, no cast, range 3 circle
    StardustThunderIII = 47944, // Helper->self, no cast, range 100 circle - This is most likely for missing the thunder tower

    CatastrophicChoiceQuake = 49742, // KefkaP5->self, 4.3+0.7s cast, single-target
    CatastrophicChoiceTornado = 49743, // KefkaP5->self, 4.3+0.7s cast, single-target
    Quake = 47946, // Helper->self, no cast, range 10 circle
    Tornado = 47947, // Helper->self, no cast, range ?-40 donut // TODO verify AOE size

    StrayApocalypseCast = 47931, // KefkaP5->self, 4.0s cast, single-target
    StrayApocalypseExaFlareCast = 47932, // Helper->self, 4.0s cast, range 6 circle
    StrayApocalypseExaFlare = 47933, // Helper->self, no cast, range 6 circle

    StrayEntropyCast = 47934, // KefkaP5->self, 5.0s cast, single-target
    StrayEntropySpread = 47935, // Helper->player, no cast, range 5 circle

    ForsakenCast = 47925, // KefkaP5->self, 10.0s cast, range 100 circle - Cast - Raidwide
    ForsakenGround = 47927, // Helper->self, 5.0s cast, range 8 circle - AOEs that just spawn
    ForsakenAOEBait = 47928, // Helper->self, 5.0s cast, range 8 circle - Bait puddle
    ForsakenBonds = 47929, // Helper->players, no cast, range 6 circle - Stack

    _Ability_Forsaken2 = 47926, // KefkaP5->self, no cast, range 100 circle

    _Ability_ForsakenNull = 47930, // KefkaP5->self, 26.0s cast, range 100 circle - Enrage
}

public enum SID : uint
{
    // Default
    DamageDown = 2911, // Helper->player, extra=0x0
    MagicVulnerabilityUp = 2941, // Helper/4C31->player, extra=0x0
    DownForTheCount = 774, // Kefka->player, extra=0xEC7
    InEvent = 1268, // none->player, extra=0x0

    // Phase 1
    DoubleTroubleTrap = 5078, // none->player, extra=0x0
    TelePortentRIGHT = 4878, // Helper->player, extra=0x0
    TelePortentUP = 4876, // Helper->player, extra=0x0
    TelePortentDOWN = 4877, // Helper->player, extra=0x0
    TelePortentLEFT = 4879, // Helper->player, extra=0x0
    TelePortentUP2 = 5079, // Helper->player, extra=0x0
    TelePortentDOWN2 = 5080, // Helper->player, extra=0x0
    TelePortentLEFT2 = 5082, // Helper->player, extra=0x0
    TelePortentRIGHT2 = 5081, // Helper->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0
    Confused = 1283, // _Gen_GravenImage->player, extra=0x0
    Sleep = 4894, // _Gen_GravenImage->player, extra=0x0

    // Phase 3
    FatedHero = 4194, // none->player, extra=0x0
    EpicHero = 4192, // none->player, extra=0x0
    EpicVillain = 4193, // none->Chaos, extra=0x0
    FatedVillain = 4195, // none->Exdeath, extra=0x0
    Entropy = 1600, // none->player, extra=0x0
    DynamicFluid = 1601, // none->player, extra=0x0
    Tailwind = 1603, // none->player, extra=0x0
    Headwind = 1602, // none->player, extra=0x0
    KefkaMax = 2536, // none->Kefka, extra=0x1FA
    PrimordialCrust = 5454, // none->player, extra=0x0
    FirstInLine = 3004, // none->player, extra=0x0
    SecondInLine = 3005, // none->player, extra=0x0
    ThirdInLine = 3006, // none->player, extra=0x0
    Accretion = 1604, // none->player, extra=0x0

    _Gen_SpellsTrouble = 5083, // none->player, extra=0x4/0x3
    _Gen_1 = 5085, // none->player, extra=0x0
    _Gen_2 = 5086, // none->player, extra=0x0
    _Gen_3 = 5084, // none->player, extra=0x0
    _Gen_10 = 2273, // Kefka->Kefka, extra=0x1FF/0x22B
    _Gen_WindResistanceDownII = 1052, // Helper->player, extra=0x0
    _Gen_EarthResistanceDownII = 3372, // Helper->player, extra=0x0
    _Gen_Unbecoming = 5452, // 4C38->player, extra=0x1
    _Gen_MeanestExistence = 5453, // 4C38->player, extra=0x0

    // Phase 4
    TellingTruthBuff = 2056, // none->4C36/4C33, extra=0x461/0x45F/0x462/0x460 | NeoExdeath - 1121 is fake, 1122 is real, Chaos - 1119 is fake, 1120 is real
    AllaganField = 454, // none->player, extra=0x0
    BeyondDeath = 1382, // none->player, extra=0x0
    BeyondDeath1 = 5464, // none->player, extra=0x0
    BlackWound = 4888, // none->player, extra=0x0 -> White
    BlackWound1 = 5542, // Helper->player, extra=0x0 -> White
    WhiteWound = 4887, // none->player, extra=0x0 -> Purple
    WhiteWound1 = 5541, // Helper->player, extra=0x0 -> Purple

    ForkedLightning = 5544, // none->player, extra=0x0 - Can be either short or long (per set)
    CompressedWater = 5545, // none->player, extra=0x0 - Can be either short or long (per set)
    AccelerationBomb = 5546, // none->player, extra=0x0 - Can be either short or long (within set)
    CursedShriek = 5543, // none->player, extra=0x0

    DynamicFluidP4 = 5548, // none->player, extra=0x0
    EntropyP4 = 5547, // none->player, extra=0x0

    _Gen_ManaCharge = 1482, // KefkaP4->KefkaP4, extra=0x0
    _Gen_ThunderCharged = 1485, // none->KefkaP4, extra=0x0
    _Gen_BlizzardCharged = 1484, // none->KefkaP4, extra=0x0 -

    // Phase 5
    SurpriseHoly = 5351, // none->player, extra=0x0
    SurpriseFlare = 5350, // none->player, extra=0x0
    IceResistanceDownII = 2903, // Helper->player, extra=0x0
    FireResistanceDownII = 2902, // Helper->player, extra=0x0
    LightningResistanceDownII = 2998, // Helper->player, extra=0x0 - Used for TB in other phases as well
}

public enum IconID : uint
{
    // Phase 1
    TankBuster = 218, // player->self
    P1SpreadIcon = 127, // player->self
    P1StackIcon = 128, // player->self
    FireRingQuestionMark = 673, // Kefka->self
    FireRingBlueOrb = 674, // Kefka->self
    BlueRingQuestionMark = 675, // Kefka->self
    BlueRingBlueOrb = 676, // Kefka->self
    PurpleRingQuestionMark = 677, // Kefka->self
    PurpleRingBlueOrb = 678, // Kefka->self

    // Phase 2
    SharedTankBuster = 259, // player->self
    TowerStackIcon = 715, // player->self
    TowerSpreadIcon = 716, // player->self
    TowerConeIcon = 717, // player->self

    // Phase 3
    OrbNumber1 = 336, // player->self
    OrbNumber2 = 337, // player->self
    OrbNumber3 = 338, // player->self
    OrbNumber4 = 339, // player->self
    OrbNumber5 = 437, // player->self
    OrbNumber6 = 438, // player->self
    OrbNumber7 = 439, // player->self
    OrbNumber8 = 440, // player->self
    StackShare = 161, // player->self

}

public enum Animations : uint
{
    // Phase 1
    PulseOrbStart = 0x400080,
    PulseOrbEnd = 0x1000200,
    PuddleSoakReady = 0x100020,
    PuddleExplosion = 0x40008,
    EyeStart = 0x400080,
    EyeEnd = 0x1000200,

    // Phase 2
    TriangleFlyingDown = 0x100020,
    TriangleLanded = 0x400080,
    TriangleExplosion = 0x40008,

    // Phase 5
    TowerGlow = 0x100020,
    TowerExplosion = 0x10040,
}

public enum TetherID : uint
{
    // Phase 1
    GravenImageTether = 45, // 4C31->player

    // Phase 3
    BlackHoleTether = 84, // 4C38->player
    _Gen_Tether_chn_m0109ac = 64, // Exdeath->GravenImage
}
