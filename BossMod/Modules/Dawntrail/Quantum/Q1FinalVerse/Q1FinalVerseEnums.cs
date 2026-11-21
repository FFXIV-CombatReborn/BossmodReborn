namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

public enum OID : uint
{
    EminentGrief = 0x48EE, // R28.5
    DevouredEater = 0x48EF, // R15.0
    Crystal = 0x1EBE70, // R0.5
    BallOfFire = 0x48F2, // R1.0
    VodorigaMinion = 0x48F0, // R1.2
    BloodguardMinion = 0x48F1, // R2.0
    HellishEarth = 0x1EBEC0, // R0.5
    ArcaneFont = 0x48F4, // R2.0
    Flameborn = 0x48F3, // R2.0-3.9
    EminentGriefHelper = 0x486C, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackVisual1 = 44820, // EminentGriefHelper->player, 0.5s cast, single-target
    AutoAttackVisual2 = 44802, // EminentGriefHelper->player, 0.5s cast, single-target
    AutoAttackVisual3 = 44814, // Helper->player, 0.8s cast, single-target
    AutoAttackVisual4 = 44137, // Helper->player, 0.8s cast, single-target
    AutoAttack1 = 44136, // DevouredEater->self, no cast, single-target
    AutoAttack2 = 44135, // EminentGrief->self, no cast, single-target
    AutoAttackAdd1 = 45197, // VodorigaMinion->player, no cast, single-target
    AutoAttackAdd2 = 45198, // BloodguardMinion->player, no cast, single-target

    ScourgingBlazeVisualWE1 = 44797, // EminentGrief->self, 3.0s cast, single-target
    ScourgingBlazeVisualNS1 = 44798, // EminentGrief->self, 3.0s cast, single-target
    ScourgingBlazeVisualNS2 = 44800, // EminentGrief->self, no cast, single-target
    ScourgingBlazeVisualWE2 = 44799, // EminentGrief->self, no cast, single-target
    SpawnCrystal = 44115, // Helper->location, no cast, single-target
    ScourgingBlazeFirst = 44118, // Helper->location, 7.0s cast, range 5 circle
    ScourgingBlazeRest = 44119, // Helper/player->location, no cast, range 5 circle

    BoundsOfSinVisual = 44120, // DevouredEater->self, 3.3+0,7s cast, single-target
    BoundsOfSinPull = 44121, // Helper->self, 4.0s cast, range 40 circle, pull into middle
    BoundsOfSin = 44122, // Helper/player->self, 3.0s cast, range 3 circle
    BoundsOfSinEndCircle = 44123, // Helper->self, no cast, range 8 circle
    BoundsOfSinEndDonut = 44124, // Helper->self, no cast, range 8-30 donut

    AbyssalSun = 44139, // Helper->self, no cast, range 30 circle, raidwide

    BladeOfFirstLightVisual1 = 44102, // DevouredEater->self, 4.2+0,8s cast, single-target
    BladeOfFirstLightVisual2 = 44103, // DevouredEater->self, 4.2+0,8s cast, single-target
    BladeOfFirstLightVisual3 = 44108, // DevouredEater->self, 7.2+0,8s cast, single-target
    BladeOfFirstLightVisual4 = 44109, // DevouredEater->self, 7.2+0,8s cast, single-target
    BladeOfFirstLight1 = 44104, // Helper->self, 5.0s cast, range 30 width 15 rect
    BladeOfFirstLight2 = 44110, // Helper->self, 8.0s cast, range 30 width 15 rect

    BallOfFireVisual1 = 44105, // EminentGrief->self, 8.0s cast, single-target
    BallOfFireVisual2 = 44097, // EminentGrief->self, 5.0s cast, single-target
    BallOfFire = 44098, // Helper->location, 2.1s cast, range 6 circle

    SearingChains = 44144, // Helper->self, no cast, range 50 width 6 cross

    SpinelashVisual1 = 44125, // EminentGrief->self, 2.0s cast, single-target
    SpinelashVisual2 = 44126, // EminentGrief->self, 2.2+0,8s cast, single-target
    Spinelash = 45119, // Helper->self, 3.0s cast, range 60 width 8 rect, wild charge with tanks in front
    SpinelashEndVisual = 44127, // EminentGrief->self, no cast, single-target

    ShacklesOfGreaterSanctity = 44801, // DevouredEater->self, 3.0s cast, single-target
    ShacklesOfSanctityMarkerDD = 44148, // Helper->player, no cast, single-target
    ShacklesOfSanctityMarkerHealer = 44147, // Helper->player, no cast, single-target
    ShacklesOfPenitenceSmall = 44150, // Helper->player, no cast, range 8 circle, defamation on dps
    ShacklesOfPenitenceBig = 44149, // Helper->players, no cast, range 21 circle, defamations on healers

    HellishEarthVisual = 44151, // EminentGrief->location, 5.0+1,0s cast, single-target, raidwide, pull
    HellishEarth1 = 44152, // Helper->self, 6.0s cast, ???, damage on tank
    HellishEarth2 = 44153, // Helper->self, 6.0s cast, ???, damage on rest
    HellishTorment1 = 44155, // Helper->player, no cast, single-target, failure
    HellishTorment2 = 44154, // Helper->self, no cast, range 50 circle, failure

    Eruption = 44156, // Helper->location, 3.0s cast, range 6 circle

    ManifoldLashingsVisual1 = 44157, // EminentGrief->self, 5.0+1,3s cast, single-target
    ManifoldLashingsVisual2 = 44158, // EminentGrief->self, 5.0+1,3s cast, single-target
    ManifoldLashingsFirst = 44159, // Helper->self, 6.3s cast, range 2 circle
    ManifoldLashingsRest = 44160, // Helper->self, no cast, range 2 circle
    ManifoldLashingsEnd = 44161, // Helper->self, 2.3s cast, range 42 width 9 rect

    UnholyDarknessVisual1 = 44163, // DevouredEater->self, 6.0+0,7s cast, single-target
    UnholyDarknessVisual2 = 44175, // DevouredEater->self, 9.0+0,7s cast, single-target
    UnholyDarkness1 = 44164, // Helper->self, 6.7s cast, range 30 circle
    UnholyDarkness2 = 44176, // Helper->self, 9.7s cast, range 30 circle

    CrimeAndPunishmentVisual = 44165, // DevouredEater->self, 6.0+0,7s cast, single-target
    CrimeAndPunishment = 44166, // Helper->player, no cast, single-target, applies sin bearer

    ChainsOfCondemnationVisual1 = 44099, // EminentGrief->location, 4.3+0,7s cast, single-target
    ChainsOfCondemnationVisual2 = 44106, // EminentGrief->location, 7.3+0,7s cast, single-target
    ChainsOfCondemnation1 = 44100, // Helper->location, 5.0s cast, range 30 circle
    ChainsOfCondemnation2 = 44107, // Helper->location, 8.0s cast, range 30 circle

    BloodyClaw = 45116, // VodorigaMinion->player, no cast, single-target
    TerrorEye = 45117, // VodorigaMinion->location, 3.0s cast, range 6 circle
    AetherialOffering = 44128, // BloodguardMinion->EminentGrief, 3.0s cast, single-target, damage buff on boss
    VoidTrap = 45200, // BloodguardMinion->location, 3.0s cast, range 6 circle

    DrainAetherVisual1 = 44131, // DevouredEater->self, 6.0+1,0s cast, single-target
    DrainAetherVisual2 = 44133, // DevouredEater->self, 11.0+1,0s cast, single-target
    DrainAether1 = 44129, // EminentGrief->self, 7.0s cast, range 50 width 50 rect, need light
    DrainAether2 = 44134, // EminentGriefHelper->self, 12.0s cast, range 50 width 50 rect, need dark
    DrainAether3 = 44132, // EminentGriefHelper->self, 7.0s cast, range 50 width 50 rect, need dark
    DrainAether4 = 44130, // EminentGrief->self, 12.0s cast, range 50 width 50 rect, need light
    DrainAetherFail1 = 44271, // Helper->EminentGrief, no cast, single-target, grief gets healed up
    DrainAetherFail2 = 44313, // Helper->DevouredEater, no cast, single-target, eater gets healed up

    FeveredFlame = 44170, // EminentGrief->self, 4.0s cast, single-target
    SelfDestruct = 44171, // Flameborn->self, 2.0s cast, range 60 circle
    Fuse = 44172, // Flameborn->Flameborn, no cast, single-target
    FlamebornFail = 44173, // Helper->self, no cast, ???, some punishment related to Flameborns

    Explosion1 = 44167, // Helper->players, no cast, range 4 circle
    Explosion2 = 44140, // Helper->player, no cast, single-target
    Explosion3 = 44142, // Helper->player, no cast, single-target
    BigBurst1 = 44168, // Helper->players, no cast, range 60 circle
    BigBurst2 = 44143, // Helper->self, no cast, range 60 circle
    BigBurst3 = 44141, // Helper->self, no cast, range 60 circle
    BigBurst4 = 44162, // Helper->self, no cast, range 80 circle

    BurningChains = 44145, // Helper->self, no cast, ???, chains fail

    HellishEarthEnrage = 44174 // EminentGrief->location, 27.0s cast, range 60 circle
}

public enum SID : uint
{
    PhysicalDamageUp = 1018, // none->EminentGrief, extra=0x8
    FireDamageUp = 4570, // none->EminentGrief/Flameborn, extra=0x8
    DarkDamageUp = 4571, // none->EminentGrief, extra=0x8
    HPBoost = 586, // none->ArcaneFont/EminentGrief/DevouredEater, extra=0x8
    LightDamageUp = 4572, // none->DevouredEater, extra=0x8
    LightVengeance = 4560, // none->player, extra=0x0
    DarkVengeance = 4559, // none->player, extra=0x0
    _Gen_Suppuration = 4512, // Helper->player, extra=0x1/0x2/0x3/0x4/0x5
    SearingChains = 4563, // none->player, extra=0x0
    _Gen_FireResistanceDownII = 2937, // Helper/Flameborn->player, extra=0x0
    _Gen_Burns = 2082, // Helper->player, extra=0x0
    _Gen_ = 3913, // EminentGrief->EminentGrief/Flameborn, extra=0x3C6/0x399/0x39A
    _Gen_ShackledAbilities = 4565, // none->player, extra=0x0
    ShackledHealing = 4564, // none->player, extra=0x0
    _Gen_DamageDown = 3304, // Helper->player, extra=0x1/0x2
    HellishEarth = 4566, // none->player, extra=0x0
    _Gen2_ = 4684, // none->EminentGrief/DevouredEater, extra=0x0

    _Gen3_ = 3572, // EminentGrief->EminentGriefHelper, extra=0x19

    _Gen_Poison = 3462, // Helper->player, extra=0x1/0x2/0x3
    _Gen_Bleeding = 2088, // Helper->player, extra=0x0
    SinBearer = 4567, // Helper->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xA/0xB/0xC/0xD/0xE/0xF/0x10
    ChainsOfCondemnation = 4562, // Helper->player, extra=0x0
    WithoutSin = 4569, // none->player, extra=0x0
    Doom = 4594, // none->player, extra=0x0
    _Gen_VulnerabilityDown = 2198, // none->Flameborn, extra=0x0
    _Gen_Stoneskin = 151, // none->Flameborn, extra=0x0
    _Gen_DamageUp = 2550, // none->Flameborn/DevouredEater/EminentGrief, extra=0x1/0x2/0x3/0x5/0x4/0x6/0x7/0xF/0xE
    _Gen_Hysteria = 296, // none->player, extra=0x0
    _Gen_SumOfAllSins = 4568, // none->player, extra=0xEC7
    _Gen4_ = 4685, // none->player, extra=0x0
    _Gen_Rehabilitation = 4191, // none->EminentGrief/DevouredEater, extra=0x1/0x2/0x3/0x5/0x4/0x6/0x7/0xF/0xE
    _Gen5_ = 3357, // none->player, extra=0x5B
}

public enum IconID : uint
{
    Dark = 78, // player->self
    Light = 77, // player->self
    SearingChains = 97, // player->self
    Spinelash = 23, // player->self
    _Gen_Icon_share_laser_3sec_0t = 527, // EminentGrief->player
}

public enum TetherID : uint
{
    SearingChains = 9, // player->player
    HellishEarth = 5, // EminentGrief->player
    _Gen_Tether_chn_tergetfix1f = 17, // Flameborn->player
}
