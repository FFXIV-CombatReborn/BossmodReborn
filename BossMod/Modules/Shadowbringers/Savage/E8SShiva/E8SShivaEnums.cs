namespace BossMod.Shadowbringers.Savage.E8SShiva;

public enum OID : uint
{
    Boss = 0x2D6B, // R4.130, x?
    Helper = 0x233C, // R0.500, x?, mixed types

    Actor1e91d4 = 0x1E91D4, // R0.500, x?, EventObj type
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x?, EventObj type
    AqueousAether = 0x2D6F, // R0.960, x?
    DrachenWanderer = 0x2D72, // R1.000-2.500, x?
    EarthenAether = 0x2D70, // R0.960, x?
    ElectricAether = 0x2D6E, // R0.960, x?
    FrozenMirror = 0x2D9F, // R1.000, x?
    GreatWyrm = 0x2D79, // R3.500, x?, Part type
    LostFindsCache = 0x1EB070, // R2.000, x?, EventObj type
    LuminousAether = 0x2D71, // R0.960, x?
    Mothercrystal = 0x2D86, // R1.000, x?
}

public enum AID : uint
{
    AutoAttack = 19801, // Boss->player, no cast, single-target
    AutoAttack2 = 20051, // Boss->player, no cast, single-target
    GreatWyrmAuto = 19863, // GreatWyrm->player, no cast, single-target

    AbsoluteZero = 19916, // Boss->self, 5.0s cast, range 40 circle
    MirrorMirror = 19802, // Boss->self, 3.0s cast, single-target

    DoubleSlap = 19813, // Boss->player, 5.0s cast, single-target
    BitingFrost = 19814, // Boss->self, 5.0s cast, range 30 270-degree cone
    DrivingFrost = 19815, // Boss->self, 5.0s cast, range 40 90-degree cone

    ReflectedDrivingFrost1 = 19896, // FrozenMirror->self, 5.0s cast, range 40 270-degree cone
    ReflectedDrivingFrost2 = 19908, // FrozenMirror->self, 10.0s cast, range 40 270-degree cone

    UnknownWeaponskill = 19800, // Boss->location, no cast, single-target
    DiamondFrost = 19820, // Boss->self, 5.0s cast, range 40 circle
    FrigidStone = 19867, // Helper->location, no cast, range 5 circle
    IcicleImpact = 19872, // Helper->location, 8.0s cast, range 10 circle
    HeavenlyStrike = 19809, // Boss->self, 5.0s cast, range 40 circle

    FrigidNeedle1 = 19868, // Helper->self, 5.0s cast, range 5 circle
    FrigidNeedle2 = 19869, // Helper->self, 5.0s cast, range 40 width 5 cross

    FrigidWater = 19870, // Helper->location, no cast, range 40 circle
    FrigidEruption = 19871, // Helper->location, 2.0s cast, range 5 circle
    ShiningArmor = 19921, // Helper->self, no cast, range 40 circle

    ShatteredWorld1 = 19828, // Boss->self, 10.0s cast, single-target
    ShatteredWorld2 = 19917, // Helper->location, no cast, range 10 circle

    ShockSpikes = 19844, // ElectricAether->self, no cast, single-target
    Stoneskin = 19845, // EarthenAether->self, 4.0s cast, single-target

    Rush1 = 19846, // LuminousAether->self, 5.0s cast, single-target
    Rush2 = 19993, // LuminousAether->player, no cast, range 4 circle

    HeartAsunder = 19884, // Mothercrystal->location, no cast, range 20 circle
    Skyfall = 19885, // Helper->location, no cast, range 100 circle
    LongingOfTheLost = 19887, // Helper->location, no cast, range 10 circle

    AkhMorn1 = 19833, // Boss->players, 4.0s cast, range 4 circle
    AkhMorn2 = 19864, // GreatWyrm->player, 4.0s cast, range 4 circle
    AkhMorn3 = 20028, // Helper->players, no cast, range 4 circle
    AkhMorn4 = 19834, // Boss->players, no cast, range 4 circle
    AkhMorn5 = 19919, // GreatWyrm->player, no cast, range 4 circle

    MornAfah = 19835, // Boss->player, 6.0s cast, range 4 circle
    WyrmsLament = 19836, // Boss->self, 5.0s cast, range 40 circle

    HallowedWings1 = 19829, // Boss->self, 5.0s cast, range 80 width 40 rect
    HallowedWings2 = 19830, // Boss->self, 5.0s cast, range 80 width 40 rect

    ReflectedHallowedWings1 = 19856, // FrozenMirror->self, no cast, range 40 width 40 rect
    ReflectedHallowedWings2 = 19857, // FrozenMirror->self, no cast, range 40 width 40 rect
    ReflectedHallowedWings3 = 19899, // FrozenMirror->self, 5.0s cast, range 40 width 40 rect
    ReflectedHallowedWings4 = 19900, // FrozenMirror->self, 5.0s cast, range 40 width 40 rect
    ReflectedHallowedWings5 = 19911, // FrozenMirror->self, 10.0s cast, range 40 width 40 rect
    ReflectedHallowedWings6 = 19912, // FrozenMirror->self, 10.0s cast, range 40 width 40 rect
}

public enum SID : uint
{
    Eukrasia = 2606, // none->player, extra=0x0
    DamageDown = 2092, // Boss/FrozenMirror/Helper->player, extra=0x0
    Freezing = 2251, // Boss->player, extra=0x0
    Heavy = 240, // Helper->player, extra=0x32
    Unknown1 = 2273, // Boss->Boss, extra=0x179/0x17F
    Stun = 149, // Helper->player, extra=0x0
    Unknown2 = 2234, // none->EarthenAether/AqueousAether/ElectricAether, extra=0x0/0x32/0x19
    DownForTheCount = 1963, // Helper->player, extra=0xEC7
    ShockSpikes = 2263, // ElectricAether->ElectricAether, extra=0x64
    LightResistanceDown = 2278, // LuminousAether->player, extra=0x0
    GraceOfLight = 2262, // none->player, extra=0x0
    HatedOfFrost = 2260, // Boss->player, extra=0x0
    HatedOfTheWyrm = 2261, // Helper->player, extra=0x0

}

public enum IconID : uint
{ 
    Icon96 = 96, // player
    Icon87 = 87, // player
}

public enum TetherID : uint
{
    Tether84 = 84, // LuminousAether->Mothercrystal/player
}
