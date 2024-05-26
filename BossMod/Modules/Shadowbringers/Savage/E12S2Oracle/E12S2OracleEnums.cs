namespace BossMod.Shadowbringers.Savage.E12S2Oracle;

public enum OID : uint
{
    Boss = 0x30A2, // R7.040, x1
    Helper = 0x233C, // R0.500, x20

    Unknown = 0x18D6, // R0.500, x2
    Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
    EdensPromise = 0x3099, // R7.500, x1
    Mitron = 0x30A1, // R0.500, x1
    VisionOfGaia = 0x30A0, // R1.000, x1
    AngersHourglass = 0x2DDF, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 19231, // Boss->player, no cast, single-target

    BasicRelativity = 22752, // Boss->self, 10.0s cast, range 40 circle

    DarkBlizzardIII = 22739, // Helper->players, no cast, range ?-12 donut

    DarkCurrent1 = 22743, // AngersHourglass->self, 4.0s cast, single-target
    DarkCurrent2 = 22744, // AngersHourglass->self, no cast, range 40 width 6 rect

    DarkEruption1 = 22732, // Boss->self, 5.0s cast, single-target
    DarkEruption2 = 22733, // Helper->players, no cast, range 6 circle
    DarkEruption3 = 22734, // Helper->player, no cast, single-target

    DarkestDance1 = 20019, // Boss->players, no cast, range 8 circle
    DarkestDance2 = 22718, // Boss->self, 5.0s cast, single-target
    DarkestDance3 = 22721, // Boss->self, no cast, range 40 circle

    DarkFireIII = 22735, // Helper->players, no cast, range 8 circle

    DarkWaterIII1 = 22729, // Boss->self, 5.0s cast, single-target
    DarkWaterIII2 = 22730, // Helper->players, no cast, range 6 circle
    DarkWaterIII3 = 22731, // Helper->player, no cast, single-target

    EmptyHate = 22748, // AngersHourglass->self, 3.0s cast, range 40 width 40 rect
    EmptyRage = 22747, // AngersHourglass->self, 4.0s cast, range 20 circle

    HellsJudgment = 22767, // Boss->self, 4.0s cast, range 40 circle
    Quicken = 22750, // Helper->AngersHourglass, no cast, single-target
    Return = 22742, // Helper->player, no cast, single-target
    Shadoweye = 22738, // Helper->self, no cast, ???
    ShockwavePulsar = 22768, // Boss->self, 5.0s cast, range 40 circle
    Slow = 22751, // Helper->AngersHourglass, no cast, single-target
    Speed = 22749, // Boss->self, 4.0s cast, single-target
    SpellInWaiting = 22728, // Boss->self, 2.0s cast, single-target

    SingularApocalypse = 22757, // Boss->self, 4.0s cast, single-target
    Cataclysm = 22722, // Boss->location, 6.0s cast, range 25 circle

}

public enum SID : uint
{
    Eukrasia = 2606, // none->player, extra=0x0
    SpellInWaiting = 1808, // Boss->Boss, extra=0x0
    SpellInWaitingDarkWaterIII = 2461, // none->player, extra=0x0
    UnknownStatus1 = 2458, // none->player, extra=0x0
    MagicVulnerabilityUp = 2091, // Helper->player, extra=0x0
    SpellInWaitingDarkEruption = 2460, // none->player, extra=0x0
    UnknownStatus2 = 2457, // none->player, extra=0x0
    PhysicalVulnerabilityUp = 2090, // Boss->player, extra=0x0
    Swiftcast = 167, // none->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    SpellInWaitingReturn = 2464, // none->player, extra=0x0
    UnknownStatus3 = 2459, // none->player, extra=0x0
    SpellInWaitingDarkBlizzardIII = 2462, // none->player, extra=0x0
    SpellInWaitingDarkFireIII = 2455, // none->player, extra=0x0
    SpellInWaitingShadoweye = 2456, // none->player, extra=0x0
    TwiceComeRuin = 2534, // AngersHourglass->player, extra=0x1
    Doom1 = 2519, // AngersHourglass/Helper->player, extra=0x0
    Return = 2452, // none->player, extra=0x0
    Doom2 = 2516, // Helper->player, extra=0x0
    Stun = 149, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon62 = 62, // player
    Icon184 = 184, // player
    Icon139 = 139, // player
}

public enum TetherID : uint
{
    Tether134 = 134, // AngersHourglass->Boss
    Tether133 = 133, // AngersHourglass->Boss
}
