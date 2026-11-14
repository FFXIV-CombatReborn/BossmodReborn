namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V10Rukhkh;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x22, Helper type
    Rukhkh = 0x4AD5, // R7.500, x1
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x2 (spawn during fight), EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1 (spawn during fight), EventObj type
    SandPearl = 0x4AD6, // R1.500-4.500, x3
    Growball = 0x4ADA, // R3.400, x0 (spawn during fight)
    _Gen_BanditMissive = 0x1EBF2E, // R0.500, x1, EventObj type
    _Gen_FallingRock = 0x4B76, // R1.000, x2
    LargeExplosive = 0x4AD9, // R1.000, x3
    _Gen_Thief = 0x4C4A, // R0.500, x1
    _Gen_Thief1 = 0x4C49, // R0.500, x1
    _Gen_Thief2 = 0x4AD7, // R0.500, x1
}

public enum AID : uint
{
    _AutoAttack_Attack = 6499, // Rukhkh->player, no cast, single-target
    _Weaponskill_ = 46840, // Rukhkh->location, no cast, single-target
    SphereOfSand = 45748, // Rukhkh->self, 3.0s cast, range 40 circle
    SandplumeVisual = 45751, // Rukhkh->self, 5.0s cast, single-target
    Sandplume1 = 45752, // Helper->self, 5.3s cast, range 20 45.000-degree cone
    SandPearlGrow = 45959, // Helper->4AD6, no cast, single-target
    Sandplume2 = 46836, // Helper->self, 2.5s cast, range 20 45.000-degree cone
    _Weaponskill_SandBurst = 46835, // Rukhkh->self, 3.0s cast, single-target
    SphereShatterSmall = 45749, // 4AD6->self, 1.0s cast, range 5 circle
    SphereShatterLarge = 45750, // 4AD6->self, 1.0s cast, range 17 circle
    BanishingMist1 = 45753, // Rukhkh->self, 4.0s cast, single-target
    BanishingMist2 = 45754, // Rukhkh->self, 3.0s cast, single-target
    BanishingFootstep = 45755, // Helper->self, no cast, range 6 circle
    Reappear = 45757, // Rukhkh->self, no cast, single-target
    SonicHowlVisual = 45756, // Rukhkh->self, no cast, single-target
    SonicHowl = 45761, // Helper->self, 3.0s cast, range 24 circle
    _Weaponskill_WindborneSeeds = 45760, // Rukhkh->self, 3.0s cast, single-target
    DryTyphoonFirst = 46957, // Helper->self, 5.0s cast, range 20 30.000-degree cone
    _Ability_DryTyphoon1 = 46956, // Rukhkh->self, 5.0s cast, single-target
    DryTyphoonRest = 46958, // Helper->self, 4.5s cast, range 20 30.000-degree cone
    Seedsprout = 47234, // Rukhkh->self, 7.0s cast, single-target
    SyrupSpout = 45762, // 4ADA->self, 1.0s cast, range 9 circle
    StreamingSands = 45764, // Rukhkh->self, 5.0s cast, range 40 circle
    BigBurst = 45758, // 4AD9->self, 4.0s cast, range 15 circle
    FallingRock = 45759, // Helper->self, no cast, range 9 width 6 rect
    BitingScratch = 45763, // Rukhkh->self, 5.0s cast, range 40 90.000-degree cone
    Beaksbane = 45765,
}

public enum TetherID : uint
{
    BanishingMist = 375, // 4AD6/4ADA->Rukhkh
}
