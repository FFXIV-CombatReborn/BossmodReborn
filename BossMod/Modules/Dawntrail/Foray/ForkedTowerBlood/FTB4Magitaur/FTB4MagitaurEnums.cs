namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

public enum OID : uint
{
    Magitaur = 0x46E3, // R7.0
    AssassinsDagger = 0x46E6, // R1.0
    SagesStaff = 0x46E5, // R1.0
    UniversalEmpowermentConduit = 0x46F9, // R4.0
    AxeEmpowermentConduit = 0x46EC, // R4.0
    LanceEmpowermentConduit = 0x46EB, // R4.0
    LuminousLance = 0x46E4, // R1.0
    ArenaFeatures = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 41765, // Boss->player, no cast, single-target
    Teleport1 = 41399, // Boss->location, no cast, single-target
    Teleport2 = 41548, // Boss->location, no cast, single-target

    UnsealedAuraVisual = 41572, // Boss->self, 4.2+0,8s cast, range 100 circle, raidwide
    UnsealedAura = 39911, // Helper->self, 5.0s cast, ???

    Unseal1 = 41538, // Boss->self, 5.0s cast, single-target, lance, far
    AttackVisual1 = 41541, // Boss->self, no cast, single-target
    Attack1 = 41542, // Helper->player, no cast, single-target
    Unseal2 = 41537, // Boss->self, 5.0s cast, single-target, axe, close
    AttackVisual2 = 41539, // Boss->self, no cast, single-target
    Attack2 = 41540, // Helper->player, no cast, single-target

    AssassinsDaggerVisual = 41568, // Boss->self, 4.7+0,3s cast, single-target
    AssassinsDaggerFirst = 41569, // AssassinsDagger->location, 5.0s cast, width 6 rect charge
    AssassinsDaggerRepeat = 41570, // AssassinsDagger->location, no cast, width 6 rect charge
    AssassinsDaggerLast = 41571, // AssassinsDagger->location, no cast, width 6 rect charge

    CriticalLanceblowVisual = 41547, // Boss->self, 5.0+1,4s cast, single-target
    CriticalLanceblowDonut = 41550, // Helper->self, no cast, range 10-32 donut
    CriticalLanceblowRect = 41549, // Helper->self, no cast, range 20 width 20 rect
    CriticalAxeblowVisual = 41543, // Boss->self, 5.0+1,1s cast, single-target
    CriticalAxeblow1 = 41545, // Helper->self, no cast, range 20 circle
    CriticalAxeblow2 = 41544, // Helper->self, no cast, range 100 circle

    ForkedFuryVisual = 41573, // Boss->self, 4.5+0,5s cast, single-target
    ForkedFury = 41574, // Helper->player, 0.5s cast, single-target

    ArcaneReaction = 41565, // Helper->self, no cast, range 55 width 6 rect
    ArcaneRecoil = 41564, // Helper->player, no cast, single-target
    HolyVisual = 41563, // Boss->self, 19.0+1,0s cast, single-target
    Holy = 39910, // Helper->self, 1.0s cast, ???
    AuraBurstVisual = 41562, // Boss->self, 19.0+1,0s cast, single-target
    AuraBurst = 39909, // Helper->self, 1.0s cast, ???

    SagesStaff = 41566, // Boss->self, 5.0s cast, single-target
    ManaExpulsion = 41567, // SagesStaff->self, no cast, range 40 width 4 rect, line stack

    RuneAxe = 41551, // Boss->self, 5.0s cast, single-target
    RuinousRuneBig = 41553, // Helper->player, no cast, range 11 circle
    RuinousRuneSmall = 41552, // Helper->players, no cast, range 5 circle
    AxeglowCircle = 41556, // Helper->self, no cast, range 100 circle
    AxeglowRect = 41555, // Helper->self, no cast, range 20 width 20 rect
    CarvingRune = 41554, // Helper->player, no cast, single-target

    HolyLance = 41557, // Boss->self, 5.0s cast, single-target
    LuminousLanceVisual = 41558, // LuminousLance->self, no cast, single-target
    LancelightRect = 41560, // Helper->self, no cast, range 20 width 20 rect
    LancelightCircle = 41561, // Helper->self, no cast, range 100 circle
    HolyIV = 41559, // Helper->players, no cast, range 6 circle

    UnsealedAuraEnrageVisual = 41575, // Boss->self, 9.2+0,8s cast, range 100 circle
    UnsealedAuraEnrage = 39912 // Helper->self, 10.0s cast, ???
}

public enum SID : uint
{
    PreyGreaterAxebit = 4337, // none->player, extra=0x0
    PreyLesserAxebit = 4336, // none->player, extra=0x0
    PreyLancepoint = 4338, // none->player, extra=0x0
    Unsealed = 4339 // Boss->Boss, extra=0x354/0x353 - 353: axe, 354: lance
}
