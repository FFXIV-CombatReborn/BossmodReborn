namespace BossMod.Endwalker.Raid.P1NErichthonios;

public enum OID : uint
{
    Boss = 0x3521, // R9.975
    Helper = 0x233C, // R0.5
    FlailAnchor = 0x3523, // R3.5, visual
    FlailBall = 0x3524 // R3.5, visual
}

public enum AID : uint
{
    AutoAttack = 872,

    GaolersFlailLeft = 26073, // Boss->self, 8.0s cast, visual
    GaolersFlailRight = 26074, // Boss->self, 8.0s cast, visual
    AetherflailLeft = 26079, // Boss->self, 8.0s cast, visual
    AetherflailRight = 26080, // Boss->self, 8.0s cast, visual
    PitilessFlail = 26085, // Boss->player, 5.0s cast, knockback
    TrueHolyVisual = 26086, // Boss->self, no cast
    TrueHoly = 26087, // Helper->player, no cast, stack
    ShiningCells = 26089, // Boss->self, 7.0s cast, raidwide and arena change
    SlamShut = 26090, // Boss->self, 7.0s cast, raidwide and arena change
    Aetherchain = 26091, // Boss->self, 5.0s cast
    PowerfulFire = 26092, // Helper->self, no cast
    PowerfulLight = 26093, // Helper->self, no cast
    Intemperance = 26094, // Boss->self, 2.0s cast
    IntemperateTorment = 26095, // Boss->self, 6.0s cast
    HotSpellPreview = 26096, // Helper->self, 1.5s cast, 19x19 rect
    ColdSpellPreview = 26097, // Helper->self, 1.5s cast, 19x19 rect
    PainfulFlux = 26098, // Helper->self, no cast
    HeavyHand = 26099, // Boss->player, 5.0s cast, tankbuster
    WardersWrath = 26100, // Boss->self, 5.0s cast, raidwide
    CellsTransition = 26101, // Boss->self, no cast

    HotSpell = 27845, // Helper->self, 4.0s cast, 19x19 rect
    ColdSpell = 27846, // Helper->self, 4.0s cast, 19x19 rect
    PitilessFlailImpact = 27925, // Helper->self, no cast
    GaolersFlailLeftAOE = 28066, // Helper->self, 8.7s cast, 270-degree cone
    GaolersFlailRightAOE = 28067 // Helper->self, 8.7s cast, 270-degree cone
}

public enum SID : uint
{
    AetherExplosion = 2195, // Boss, hidden; extra 0x14C=red and 0x14D=blue
    ColdSpell = 2739,
    HotSpell = 2740
}

public enum IconID : uint
{
    PitilessFlail = 1,
    TrueHoly = 62,
    HeavyHand = 218
}
