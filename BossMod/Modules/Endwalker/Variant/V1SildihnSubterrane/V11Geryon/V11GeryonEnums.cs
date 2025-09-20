namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

public enum OID : uint
{
    Geryon = 0x398B, // R9.6
    PowderKegBlue = 0x39C9, // R1.0
    PowderKegRed = 0x398C, // R1.0
    SludgeVoidzone = 0x1EB7F0, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Geryon->player, no cast, single-target
    Teleport = 29894, // Geryon->location, no cast, single-target

    ColossalCharge1 = 29900, // Geryon->location, 8.0s cast, width 14 rect charge, knockback 9, dir right
    ColossalCharge2 = 29901, // Geryon->location, 8.0s cast, width 14 rect charge, knockback 9, dir left
    ColossalLaunch = 29896, // Geryon->self, 5.0s cast, range 40 width 40 rect
    ColossalSlam = 29904, // Geryon->self, 6.0s cast, range 60 60-degree cone
    ColossalStrike = 29903, // Geryon->player, 5.0s cast, single-target
    ColossalSwing = 29905, // Geryon->self, 5.0s cast, range 60 180-degree cone

    ExplodingCatapult = 29895, // Geryon->self, 5.0s cast, range 60 circle
    ExplosionCircle = 29908, // PowderKegBlue/PowderKegRed->self, 2.5s cast, range 15 circle
    ExplosionDonut = 29909, // PowderKegRed/PowderKegBlue->self, 2.5s cast, range 3-17 donut
    SubterraneanShudder = 29906, // Geryon->self, 5.0s cast, range 60 circle raidwide
    RedKegTurnBlue = 31260, // PowderKegRed->self, no cast, single-target
    BlueKegTurnRed = 29907, // PowderKegBlue->self, no cast, single-target

    Intake = 29913, // Helper->self, no cast, range 40 width 10 rect, pull 25 towards origin, between centers

    RunawayRunoff = 29911, // Helper->self, 9.0s cast, range 60 circle, knockback 18, away from source
    GigantomillFirstCW = 29898, // Geryon->self, 8.0s cast, range 72 width 10 cross
    GigantomillFirstCCW = 29897, // Geryon->self, 8.0s cast, range 72 width 10 cross
    GigantomillRest = 29899, // Geryon->self, no cast, range 72 width 10 cross

    RollingBoulder = 29914, // Helper->self, no cast, range 10 width 10 rect
    SuddenlySewage = 29912, // Helper->self, no cast, range 20 width 20 rect

    RunawaySludge = 29910, // Helper->self, 5.0s cast, range 9 circle
    Shockwave = 29902 // Geryon->self, 5.0s cast, range 40 width 40 rect, knockback 15 dir left/right
}
