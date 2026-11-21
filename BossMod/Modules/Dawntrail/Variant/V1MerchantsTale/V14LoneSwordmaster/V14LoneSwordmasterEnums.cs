namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V14LoneSwordmaster;

public enum OID : uint
{
    LoneSwordmaster = 0x4B12,
    Helper = 0x233C,
    // Path 9
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    _Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
    MagnetFloor = 0x1EBF74, // R0.500, x1, EventObj type, magnetic levitation, 00010002 SW NE negative
    _Gen_Actor1ebf71 = 0x1EBF71, // R0.500, x1, EventObj type
    ForceOfWill = 0x4C3C, // R2.000, x0 (spawn during fight), after Lash Of Light
    ForceOfWillTether = 0x4B1E, // R1.000, x0 (spawn during fight), invisible, used by small for tether in UnyieldingWill
    ForceOfWillSmall = 0x4B14, // R0.500, x0 (spawn during fight)
    // Path 11
    MagneticRock = 0x4B16, // R1.800, x0 (spawn during fight)
    // Path 12
    FallenRock = 0x4B15, // R1.800, x0 (spawn during fight)
    ForceOfWillRock = 0x4B13, // R1.250, x0 (spawn during fight)
}
public enum AID : uint
{
    // Path 9
    _AutoAttack_Attack = 45128, // LoneSwordmaster->player, no cast, single-target
    _Weaponskill_MaleficQuartering = 46608, // LoneSwordmaster->self, 5.0s cast, single-target
    _Weaponskill_MaleficInfluence = 46609, // Helper->player, no cast, single-target, (East)
    _Weaponskill_MaleficInfluence1 = 46610, // Helper->player, no cast, single-target (West)
    _Weaponskill_MaleficInfluence2 = 46611, // Helper->player, no cast, single-target (South)
    _Weaponskill_MaleficInfluence3 = 46612, // Helper->player, no cast, single-target (North)
    _Ability_ = 46607, // LoneSwordmaster->location, no cast, single-target
    LashOfLight = 46614, // Helper->self, 4.0s cast, range 40 90.000-degree cone
    _Weaponskill_VanishingHorizon = 46613, // LoneSwordmaster->self, 4.0s cast, single-target
    WillOfTheUnderworld = 47571, // 4C3C->self, 8.0s cast, range 40 width 20 rect
    _Weaponskill_CrusherOfLions = 47993, // LoneSwordmaster->self, 5.5+0.5s cast, single-target
    CrusherOfLions = 47994, // Helper->self, 6.0s cast, range 40 90-degree cone
    _Weaponskill_EarthRendingEight1 = 46618, // LoneSwordmaster->self, 5.0s cast, single-target
    EarthRendingEight = 46619, // Helper->location, 5.0s cast, range 8 circle
    EarthRendingEightCross = 46620, // Helper->self, 3.0s cast, range 40 width 8 cross
    _Weaponskill_WaitingWounds = 46621, // LoneSwordmaster->self, 8.0s cast, single-target
    WaitingWounds = 46622, // Helper->location, 8.0s cast, range 10 circle
    _Weaponskill_HeavensConfluence1 = 47560, // LoneSwordmaster->self, 5.0s cast, single-target
    HeavensConfluenceTarget1 = 46623, // LoneSwordmaster->player, no cast, range 5 circle
    HeavensConfluenceTarget2 = 46624, // LoneSwordmaster->player, no cast, range 5 circle
    HeavensConfluenceIn1 = 46625, // Helper->self, 5.0s cast, range 8 circle
    HeavensConfluenceOut1 = 46626, // Helper->self, 5.0s cast, range 8-60 donut
    HeavensConfluenceIn2 = 46627, // Helper->self, 7.0s cast, range 8 circle
    HeavensConfluenceOut2 = 46628, // Helper->self, 7.0s cast, range 8-60 donut
    _Weaponskill_ShiftingHorizon = 46629, // LoneSwordmaster->self, 4.0s cast, single-target
    UnyieldingWillVisual = 48651, // 4B14->4B1E, 9.9s cast, width 4 rect charge
    UnyieldingWill1 = 46630, // 4B14->location, no cast, width 4 rect charge
    UnyieldingWill2 = 46631, // 4B14->player, no cast, width 4 rect charge
    SteelsbreathRelease = 46632, // LoneSwordmaster->self, 5.0s cast, range 60 circle
    Concentrativity = 46644, // LoneSwordmaster->self, 5.0s cast, range 60 circle
    _Weaponskill_MawOfTheWolf = 46642, // LoneSwordmaster->self, 3.4+1.6s cast, single-target
    MawOfTheWolf = 46643, // Helper->self, 5.0s cast, range 80 width 80 rect
    StingOfTheScorpion = 46645, // LoneSwordmaster->player, 5.0s cast, single-target
    _Weaponskill_HeavensConfluence5 = 47561, // LoneSwordmaster->self, 5.0s cast, single-target
    // Path 10
    Plummet = 46639, // Helper->location, 3.0s cast, range 5 circle
    PlummetBig = 46640, // Helper->location, 4.0s cast, range 10 circle
    PlummetProximity = 46641, // Helper->location, 10.0s cast, range 60 circle
    // Path 11
    _Weaponskill_Magnetism = 46634, // 4B16->self, 8.0s cast, single-target
    Repel = 46635, // Helper->player, 8.0s cast, single-target, all rocks cast magnetism but helpers cast based on debuff, 20f push/pull
    Attract = 46636, // Helper->player, 8.0s cast, single-target
    ConcentrativityRocks = 47233, // LoneSwordmaster->self, 6.5s cast, range 60 circle, 17f kb origin after repel/attract
    // Path 12
    PlummetSmall = 46633, // Helper->location, 3.0s cast, range 3 circle
    WillOfTheUnderworldRock = 46615, // 4B13->self, 8.0s cast, range 40 width 10 rect
    SteelsbreathRelease1 = 46638, // LoneSwordmaster->self, 5.0s cast, range 60 circle
}

public enum SID : uint
{
    // Path 9
    MaleficE = 4773, // none->player, extra=0x0
    MaleficW = 4774, // none->player, extra=0x0
    MaleficEW = 4775, // none->player, extra=0x0
    MaleficS = 4776, // none->player, extra=0x0
    MaleficSE = 4777, // none->player, extra=0x0
    MaleficSW = 4778, // none->player, extra=0x0
    MaleficSEW = 4779, // none->player, extra=0x0
    MaleficN = 4780, // none->player, extra=0x0
    MaleficNE = 4781, // none->player, extra=0x0
    MaleficNW = 4782, // none->player, extra=0x0
    MaleficNEW = 4783, // none->player, extra=0x0
    MaleficNS = 4784, // none->player, extra=0x0
    MaleficNSE = 4785, // none->player, extra=0x0
    MaleficNSW = 4786, // none->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0
    PositiveCharge = 4821, // none->player, extra=0x0
    NegativeCharge = 4822, // none->player, extra=0x0
    MagneticLevitation = 4837, // none->player, extra=0x12C
    _Gen_Bleeding1 = 3077, // none->player, extra=0x0
    _Gen_Bleeding2 = 3078, // none->player, extra=0x0
    // Path 11
    _Gen_PositiveCharge = 4823, // none->4B16, extra=0x0
    _Gen_NegativeCharge = 4824, // none->4B16, extra=0x0
    _Gen_Stun = 4163, // Helper->player, extra=0x0
    // Path 12
    _Gen_Incurable = 2398, // none->4B15, extra=0x0
    RockMaleficW = 4967, // none->4B15, extra=0x0
    RockMaleficE = 4966, // none->4B15, extra=0x0
}

public enum IconID : uint
{
    Checkmark = 136, // player->self
    RedX = 137, // player->self
    HeavensConfluence = 376, // player->self
    Tankbuster = 218, // player->self
}

public enum TetherID : uint
{
    Magnet = 38, // 4B16->player
    UnyieldingWill = 371, // 4B14/4B1E->4B1E/player
}
