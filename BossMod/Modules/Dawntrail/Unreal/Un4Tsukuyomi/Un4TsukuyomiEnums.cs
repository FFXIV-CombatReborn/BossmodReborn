namespace BossMod.Dawntrail.Unreal.Un4Tsukuyomi;

public enum OID : uint
{
    Boss = 0x4A79, // R3.250, x1
    Helper = 0x233C, // R0.500, x1, Helper type
    Specter = 0x18D6, // R0.500, x0 (spawn during fight)
    Yotsuyu = 0x4A83, // R0.600, x0 (spawn during fight)
    SpecterOfZenos = 0x4A81, // R1.152, x0 (spawn during fight)
    SpecterOfGosetsu = 0x4A82, // R0.600, x0 (spawn during fight)
    SpecterOfTheHomeland1 = 0x4A84, // R0.600, x0 (spawn during fight)
    SpecterOfTheHomeland2 = 0x4A85, // R0.600, x0 (spawn during fight)
    SpecterOfThePatriarch = 0x4A7E, // R0.600, x0 (spawn during fight)
    SpecterOfTheMatriarch = 0x4A7F, // R0.600, x0 (spawn during fight)
    SpecterOfTheEmpire1 = 0x4A86, // R0.600, x0 (spawn during fight)
    SpecterOfTheEmpire2 = 0x4A87, // R0.600, x0 (spawn during fight)
    SpecterOfAsahi = 0x4A80, // R0.600, x0 (spawn during fight)
    Moonlight = 0x4A7C, // R1.000, x0 (spawn during fight)
    Moondust = 0x4A7D, // R1.200, x0 (spawn during fight)
    DancingFan = 0x4A7A, // R1.600, x0 (spawn during fight)
    MoonPuddle1 = 0x1EA980, // R0.500, x0 (spawn during fight), EventObj type
    MoonPuddle2 = 0x1EA981 // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack1 = 45409, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // Boss->player, no cast, single-target
    AutoAttack3 = 12209, // Boss->player, no cast, single-target
    AutoAttackAdd1 = 45410, // SpecterOfThePatriarch/SpecterOfTheMatriarch/SpecterOfTheEmpire1->player/Yotsuyu, no cast, single-target
    AutoAttackAdd2 = 45411, // SpecterOfTheHomeland1->player, no cast, single-target
    AutoAttackAdd3 = 45412, // SpecterOfTheHomeland2/SpecterOfTheEmpire2->player, no cast, single-target
    AutoAttackAdd4 = 45415, // SpecterOfAsahi->Yotsuyu, no cast, single-target

    Reprimand = 45358, // Boss->self, 4.0s cast, range 100 circle
    NightfallGun = 45360, // Boss->self, 4.0s cast, single-target
    NightfallSpear = 45361, // Boss->self, 4.0s cast, single-target
    LeadOfTheUnderworld = 45362, // Boss->self, no cast, range 40+R width 8 rect
    SteelOfTheUnderworld = 45363, // Boss->self, no cast, range 40+R 90-degree cone
    WeaponVanishSpear = 45364, // Boss->self, no cast, single-target
    WeaponVanishGun = 45405, // Boss->self, no cast, single-target
    Nightbloom = 45371, // Boss->self, 6.0s cast, range 100 circle
    NightbloomYotsuyu = 11438, // Yotsuyu->self, no cast, single-target
    NightbloomHelper = 45404, // Helper->self, 4.0s cast, range 60 circle

    Concentrativity = 45372, // SpecterOfZenos->self, no cast, range 100 circle, knockback 13 away from source
    UnmovingTroika1 = 45400, // SpecterOfZenos->self, no cast, range 9+R ?-degree cone
    UnmovingTroika2 = 45401, // Specter->self, 1.7s cast, range 9+R ?-degree cone
    UnmovingTroika3 = 45402, // Specter->self, 2.1s cast, range 9+R ?-degree cone
    SpecterOfZenosVisual = 11210, // SpecterOfZenos->self, 3.0s cast, single-target
    GosetsuCharge = 11211, // SpecterOfGosetsu->location, no cast, width 8 rect charge
    GosetsuJump = 11478, // SpecterOfGosetsu->location, no cast, ???
    Dispersivity = 45376, // Specter->self, no cast, range 100 circle

    SupremeSelenomancy = 45417, // Boss->self, 4.0s cast, single-target
    LunarHalo = 45386, // Moonlight->self, 6.0s cast, range 5-15 donut
    TsukiNoKakera = 45380, // Boss->self, no cast, single-target
    Moonfall = 45381, // Moondust->self, 6.0s cast, range 20 circle
    Moonburst = 45382, // Moondust->self, no cast, range 100 circle, meteors too close together
    Moonbeam = 45384, // Moondust->self, 2.0s cast, range 20 circle
    Crater = 45408, // Moondust->self, 2.0s cast, range 20 circle
    MoondustVisual = 11999, // Specter->self, no cast, single-target
    MidnightRain = 45378, // Boss->self, 2.0s cast, single-target
    MidnightRainVisual = 45379, // Helper->self, no cast, single-target
    MoonPhaseVisual = 45416, // Boss->self, no cast, single-target
    LunarRays = 45383, // Boss->self, no cast, single-target
    Antitwilight = 45388, // Boss->self, 4.0s cast, range 100 circle
    ZashikiAsobi = 45369, // Boss->self, 4.0s cast, single-target
    TsukiNoMaiogi = 45370, // DancingFan->self, 5.0s cast, range 10 circle
    TormentUntoDeath1 = 45359, // Boss->self/player, 5.0s cast, range 15+R ?-degree cone, magic vulnerability up
    TormentUntoDeath2 = 45418, // Boss->self/player, 5.0s cast, range 15+R ?-degree cone, physical vulnerability up

    DanceOfTheDeadVisual1 = 11261, // Boss->self, no cast, single-target
    DanceOfTheDeadVisual2 = 45399, // Helper->self, no cast, single-target
    DanceOfTheDead = 45406, // Boss->self, no cast, range 100 circle
    BrightBlade = 45391, // Boss->self, 5.0s cast, range 40+R 210-degree cone
    DarkBlade = 45390, // Boss->self, 5.0s cast, range 40+R 210-degree cone
    WaxingGrudge = 45394, // Helper->self, no cast, range 10 circle
    WaningGrudge = 45395, // Helper->self, no cast, range 5-40 donut
    LunacyFirst = 45392, // Boss->players, 5.0s cast, range 6 circle
    LunacyRest = 45393, // Boss->players, no cast, range 6 circle
    HagetsuFirst = 45413, // Boss->players, 5.0s cast, range 6 circle
    HagetsuRest = 45414 // Boss->players, no cast, range 6 circle
}

public enum SID : uint
{
    DownForTheCount = 783, // Boss->player, extra=0xEC7
    Haunt1 = 1542, // none->SpecterOfZenos/SpecterOfTheHomeland1/SpecterOfTheHomeland2/SpecterOfThePatriarch/SpecterOfTheMatriarch/SpecterOfTheEmpire1/SpecterOfTheEmpire2/SpecterOfAsahi, extra=0x0
    Haunt2 = 1543, // none->SpecterOfGosetsu, extra=0x0
    Grudge = 1573, // none->Yotsuyu, extra=0x0
    Stun = 149, // SpecterOfZenos->player, extra=0x0
    Moonlit = 1538, // none->player, extra=0x1/0x2/0x3/0x4/0x5
    Moonshadowed = 1539, // none->player, extra=0x1/0x2/0x3/0x4/0x5
    Doom = 210, // none->player, extra=0x0
    Bleeding = 642, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Moondust->player, extra=0x1/0x2
    FullMoon = 1535, // Boss->Boss, extra=0x56/0x59
    NewMoon = 1536, // Boss->Boss, extra=0x5A
    BloodMoon = 1537, // Boss->Boss, extra=0x58
    Transfiguration = 705, // none->Moondust, extra=0x5B/0x5C
    VeilOfShadow = 1540, // none->player, extra=0x0
    MagicVulnerabilityUp = 1138, // Boss->player, extra=0x0
    PhysicalVulnerabilityUp = 695, // Boss->player, extra=0x0
    DamageDown = 1016 // Helper->player, extra=0x1
}

public enum IconID : uint
{
    Moonfall = 131, // player->self
    Lunacy = 305, // player->self
    Hagetsu = 23 // player->self
}

public enum TetherID : uint
{
    SpecterTether = 17 // SpecterOfThePatriarch/SpecterOfTheMatriarch/SpecterOfAsahi->Yotsuyu
}
