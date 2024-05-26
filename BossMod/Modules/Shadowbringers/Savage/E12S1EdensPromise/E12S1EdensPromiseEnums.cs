namespace BossMod.Shadowbringers.Savage.E12S1EdensPromise;

public enum OID : uint
{
    Boss = 0x3099, // R7.500, x1
    Helper = 0x233C, // R0.500, x20

    Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
    Actor1eafad = 0x1EAFAD, // R0.500, x0 (spawn during fight), EventObj type
    BombBoulder = 0x309B, // R1.300, x0 (spawn during fight)
    GuardianOfEden = 0x309A, // R115.380, x1
    IcePillar = 0x309C, // R2.000, x0 (spawn during fight)
    Mitron = 0x30A1, // R0.500, x0 (spawn during fight)
    OracleOfDarkness = 0x30A2, // R7.040, x0 (spawn during fight)
    Unknown1 = 0x18D6, // R0.500, x2
    Unknown2 = 0x2CED, // R1.000, x1
    VisionOfGaia = 0x30A0, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 19230, // Boss->player, no cast, single-target
    Cast = 20035, // Boss->self, 7.0s cast, single-target

    DiamondDust1 = 22678, // Boss->self, 5.0s cast, single-target
    DiamondDust2 = 22679, // Helper->self, no cast, range 60 circle

    EarthenFury1 = 22687, // Boss->self, 5.0s cast, single-target
    EarthenFury2 = 22688, // Helper->self, no cast, range 60 circle

    ForceOfTheLand = 22692, // Helper->self, 0.5s cast, range 10 circle

    FormlessJudgment1 = 22697, // Boss->self, 5.0s cast, single-target
    FormlessJudgment2 = 22698, // Boss->self, no cast, single-target
    FormlessJudgment3 = 22699, // Helper->player, no cast, single-target
    FormlessJudgment4 = 22700, // Helper->player, no cast, single-target

    FrigidStone = 22686, // Helper->players, no cast, range 5 circle
    IcePillar = 22682, // IcePillar->self, no cast, range 6 circle
    Impact = 22689, // BombBoulder->self, no cast, range 4 circle
    InitializeRecall = 22668, // Boss->self, no cast, single-target
    JunctionShiva = 22676, // Boss->self, 3.0s cast, single-target
    JunctionTitan = 22677, // Boss->self, 3.0s cast, single-target

    PillarPierce1 = 19263, // IcePillar->self, 15.0s cast, single-target
    PillarPierce2 = 22683, // IcePillar->self, no cast, range 80 width 4 rect

    RapturousReach1 = 22701, // Boss->self, 5.0s cast, range 40 ?-degree cone
    RapturousReach2 = 22702, // Boss->self, 5.0s cast, range 40 ?-degree cone

    Shatter = 22684, // IcePillar->self, no cast, range 8 circle

    ThrumOfDiscord1 = 19265, // Boss->self, 5.0s cast, single-target
    ThrumOfDiscord2 = 20059, // Helper->self, 7.7s cast, range 20 circle

    ThunderousExplosion = 22690, // BombBoulder->self, 2.5s cast, range 40 circle
    UnderTheWeight = 22694, // Helper->players, no cast, range 6 circle
    UnknownAbility1 = 22703, // Boss->location, no cast, single-target
    UnknownAbility2 = 22695, // Boss->self, no cast, single-target
    UnknownAbility3 = 22680, // Helper->self, 6.0s cast, range 20 circle
    WeightOfTheWorld = 22693, // Helper->self, 0.5s cast, range 10 circle

    //
    Maleficium = 22696, // Boss->self, 5.0s cast, range 40 circle
    OracleAuto = 19231, // OracleOfDarkness->player, no cast, single-target
    IceFloe = 22681, // Helper->players, 5.0s cast, range 6 circle
    IcicleImpact = 20058, // Helper->location, 7.0s cast, range 10 circle
    PlungingIce = 22685, // Helper->location, 9.0s cast, range 20 circle
    PulseOfTheLand = 22691, // Helper->self, 0.5s cast, range 10 circle

    TemporaryCurrent = 20036, // Helper->self, 7.0s cast, range 40 width 80 rect // Ramuh 
    ConflagStrike = 20037, // Helper->self, 7.0s cast, range 40 ?-degree cone // Ifrit
    Ferostorm = 20038, // Helper->self, 7.0s cast, range 40 ?-degree cone // Garuda
    JudgmentJolt = 20039, // Helper->self, 7.0s cast, range 10 circle // Leviathan

}
public enum SID : uint
{
    Eukrasia = 2606, // none->player, extra=0x0
    JunctionShiva = 2468, // Boss->Boss, extra=0x0
    DamageDown = 2092, // Boss/Helper->player, extra=0x0
    IceResistanceDownII = 2465, // Helper->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    PiercingResistanceDown = 1694, // Helper->player, extra=0x0
    SlashingResistanceDown = 1693, // Helper->player, extra=0x0
    Swiftcast = 167, // none->player, extra=0x0
    ThinIce = 911, // none->player, extra=0x78
    PhysicalVulnerabilityUp = 2090, // IcePillar->player, extra=0x0
    JunctionTitan = 2469, // Boss->Boss, extra=0x0
    EarthResistanceDownII = 2097, // Helper->player, extra=0x0
    MagicVulnerabilityUp = 2091, // BombBoulder->player, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0
    JunctionLoghrif = 2470, // none->OracleOfDarkness, extra=0x0
    DownForTheCount = 2408, // Helper->player, extra=0xEC7

}
public enum IconID : uint
{
    Icon218 = 218, // player
    Icon62 = 62, // player
    Icon139 = 139, // player
    Icon186 = 186, // player
    Icon185 = 185, // player
    Icon187 = 187, // player
}

public enum TetherID : uint
{
    Tether140 = 140, // Unknown1->Boss
    Tether144 = 144, // Unknown1->Boss
    Tether145 = 145, // Unknown1->Boss
    Tether57 = 57, // IcePillar->player
    Tether1 = 1, // IcePillar->player
    Tether141 = 141, // Unknown1->Boss
    Tether142 = 142, // Unknown1->Boss
    Tether143 = 143, // Unknown1->Boss
    Tether7 = 7, // player->BombBoulder
    Tether139 = 139, // OracleOfDarkness->Unknown1
}
