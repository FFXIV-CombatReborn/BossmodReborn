namespace BossMod.RealmReborn.Trial.T09WhorleaterH;

public enum OID : uint
{
    Boss = 0xA67, // R4.5
    Tail = 0xA86, // R4.5
    Spume = 0xA85, // R1.0
    WavespineSahagin = 0xA84, // R1.5
    WavetoothSahagin = 0xA83, // R1.5
    Converter = 0x1E922A, // R2.0
    HydroshotZone = 0x1E9230, // R0.5
    DreadstormZone = 0x1E9231, // R0.5
    LeviathanUnk2 = 0xACD, // R0.500, x4
    ElementalConverter = 0xB84, // R0.5
    SpinningDiveHelper = 0xA87, // R4.5
    Helper = 0xA88
}

public enum AID : uint
{
    AutoAttack1 = 1853, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // WavespineSahagin->player, no cast, single-target

    BodySlamRectAOE = 1860, // Boss->self, no cast, range 30+R width 10 rect
    BodySlamNorth = 1938, // Helper->self, no cast, ???
    BodySlamSouth = 1937, // Helper->self, no cast, ???
    VeilOfTheWhorl = 2165, // Boss->self, no cast, single-target
    MantleOfTheWhorl = 2164, // Tail->self, no cast, single-target
    ScaleDarts = 1857, // Tail->player, no cast, single-target
    AetherDraw = 1870, // Spume->ElementalConverter, no cast, single-target
    StunShot = 1862, // WavespineSahagin->player, no cast, single-target
    DreadTide = 1877, // Helper->location, no cast, range 2 circle
    DreadTideBoss = 1876, // Boss->self, no cast, single-target
    AquaBreath = 1855, // Boss->self, no cast, range 10+R circle
    TailWhip = 1856, // Tail->self, no cast, range 10+R circle
    Waterspout = 1859, // Helper->location, no cast, range 4 circle
    WaterspoutBoss = 1858, // Boss->self, no cast, single-target
    Hydroshot = 1864, // WavespineSahagin->location, 2.5s cast, range 5 circle
    TidalRoar = 1868, // Helper->self, no cast, range 60+R circle
    TidalRoarBoss = 1867, // Boss->self, no cast, single-target
    SpinningDiveSnapshot = 1869, // SpinningDiveHelper->self, no cast, range 46+R width 16 rect
    SpinningDiveEffect = 1861, // Helper->self, no cast, range 46+R width 16 rect
    Splash = 1871, // Spume->self, no cast, range 50+R circle
    GrandFall = 1873, // Helper->location, 3.0s cast, range 8 circle
    TidalWave = 1872, // Boss->self, no cast, range 60+R circle
    Dreadstorm = 1865, // WavetoothSahagin-> location, 3.0s cast, range 50, 6 circle
    Ruin = 2214, // WavespineSahagin->player, 1.0s cast
    Bilgestorm = 1863, // WavespineSahagin->self, no cast, range 8+R ?-degree cone
    Darkness = 1875 // WavetoothSahagin->self, 1.0s cast, range 6+R ?-degree cone
}

public enum SID : uint
{
    VeilOfTheWhorl = 478, // Boss->A88/Boss/A87, extra=0x64
    MantleOfTheWhorl = 477, // Tail->Tail, extra=0x64
    Invincibility = 775 // none->A88/Boss/Tail/A87, extra=0x0
}
