namespace BossMod.Dawntrail.Raid.M10NDaringDevils;

public enum OID : uint
{
    RedHot = 0x4B53,
    DeepBlue = 0x4b54,
    Helper = 0x233C,
    TheXtremes = 0x4BDE,
    XtremeAether = 0x4B55,
    SickSwell = 0x4B56,
    _Gen_ = 0x4AE7,
    Actor1ea1a1 = 0x1EA1A1,
    Exit = 0x1E850B,
    InfernoVoidzone = 0x1EBF31, // Alley-oop Inferno puddle
    CutbackBlazeVoidzone = 0x1EBF34,
    HotAerialVoidzone = 0x1EBF32,
    BlastingSnapVoidzone = 0x1EBF35,
}

public enum AID : uint
{
    AutoAttack = 48637, // RedHot->player, no cast, single-target
    HotImpact = 46464, // RedHot->players, 5.0s cast, range 6 circle
    AlleyOopInferno = 46470, // RedHot->self, 4.3+0.7s cast, single-target
    AlleyOopInferno1 = 46471, // Helper->player, 5.0s cast, range 5 circle
    RedHotSurfboardMount = 46456, // RedHot->location, no cast, single-target
    CutbackBlaze = 46478, // RedHot->self, 4.3+0.7s cast, single-target
    CutbackBlaze1 = 46479, // Helper->self, no cast, range 60 ?-degree cone
    DiversDare = 46466, // RedHot->self, 5.0s cast, range 60 circle
    AutoAttack1 = 48638, // DeepBlue->player, no cast, single-target
    SickSwell = 46480, // DeepBlue->self, 3.0s cast, single-target
    SickestTakeOff = 46482, // DeepBlue->self, 4.0s cast, single-target
    SickestTakeOff1 = 46483, // Helper->self, 7.0s cast, range 50 width 15 rect
    SickSwell1 = 46481, // Helper->self, 7.0s cast, Knockback range 50 width 50 rect
    DeepBlueSurfboardMount = 46457, // DeepBlue->location, no cast, single-target
    DeepVarial = 47247, // DeepBlue->location, 5.3+1.0s cast, ???
    DeepVarial1 = 46488, // Helper->self, 6.8s cast, range 60 120.000-degree cone
    DeepImpact = 46465, // DeepBlue->player, 5.0s cast, range 6 circle
    XtremeSpectacular1 = 46497, // RedHot->self, 4.0s cast, single-target
    XtremeSpectacular = 46498, // DeepBlue->self, 4.0s cast, single-target
    XtremeSpectacular2 = 46499, // TheXtremes->self, 7.4s cast, range 50 width 40 rect
    XtremeSpectacular3 = 46555, // TheXtremes->self, no cast, range 60 circle
    XtremeSpectacular4 = 47049, // TheXtremes->self, no cast, range 60 circle
    EpicBrotherhood1 = 46458, // RedHot->DeepBlue, no cast, single-target
    EpicBrotherhood = 46459, // DeepBlue->RedHot, no cast, single-target
    HotAerial = 46474, // RedHot->self, 4.9s cast, single-target
    HotAerial1 = 46475, // RedHot->location, no cast, single-target
    HotAerial2 = 46476, // Helper->self, 6.0s cast, range 6 circle
    Ability_2 = 46460, // RedHot->self, no cast, single-target
    SteamBurst = 46507, // XtremeAether->self, 3.0s cast, range 9 circle
    Pyrotation = 46472, // RedHot->self, 4.3+0.7s cast, single-target
    Pyrotation1 = 46473, // Helper->players, no cast, range 6 circle
    AlleyOopMaelstrom = 46495, // Helper->self, 3.0s cast, range 60 30.000-degree cone
    AlleyOopMaelstrom1 = 46494, // DeepBlue->self, 3.0s cast, single-target
    AlleyOopMaelstrom2 = 46496, // Helper->self, 3.0s cast, range 60 15.000-degree cone
    DiversDare1 = 46467, // DeepBlue->self, 5.0s cast, range 60 circle
    Firesnaking = 46462, // RedHot->self, 5.0s cast, range 60 circle
    Watersnaking = 46463, // DeepBlue->self, 5.0s cast, range 60 circle
    InsaneAir1 = 47251, // RedHot->self, 5.9+1.5s cast, single-target
    InsaneAir = 47252, // DeepBlue->self, 5.9+1.5s cast, single-target
    BlastingSnap = 46503, // RedHot->self, no cast, single-target
    PlungingSnap = 46504, // DeepBlue->self, no cast, single-target
    BlastingSnap1 = 46505, // Helper->self, no cast, range 60 ?-degree cone
    PlungingSnap1 = 46506, // Helper->self, no cast, range 60 ?-degree cone
    InsaneAir3 = 47253, // RedHot->self, 3.9+1.5s cast, single-target
    InsaneAir2 = 47254, // DeepBlue->self, 3.9+1.5s cast, single-target
    Ability_3 = 46461, // DeepBlue->self, no cast, single-target
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0982_2c = 372,
    WatersnakingTarget = 380,
    FiresnakingTarget = 381,
}

public enum SID : uint
{
    Burns1 = 3065, // none->player, extra=0x0
    Burns2 = 3066, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2
    _Gen_ = 2056, // none->DeepBlue, extra=0x435
    Watersnaking = 4975, // none->player, extra=0x0
    Firesnaking = 4974, // none->player, extra=0x0
    DirectionalDisregard = 3808, // none->RedHot/DeepBlue, extra=0x0
    Covered = 2413, // DeepBlue->RedHot, extra=0x1
    Cover = 2412, // none->DeepBlue, extra=0x64

}

public enum IconID : uint
{
    _Gen_Icon_m0676trg_tw_d0t1p = 259, // player->self
    _Gen_Icon_target_ae_5m_s5_fire0c = 660, // player->self
    CutbackBlazeBait = 664, // RedHot->player
    _Gen_Icon_tank_lockonae_6m_5s_01t = 344, // player->self
    FireStack = 659, // player->self
    _Gen_Icon_m0982trg_c2c = 661, // player->self
    _Gen_Icon_m0982trg_d0c = 651, // _Gen_->player
    _Gen_Icon_m0982trg_f0c = 665, // _Gen_->player
    _Gen_Icon_m0982trg_c0c = 635, // player->self
    _Gen_Icon_m0982trg_c1c = 636, // player->self
    _Gen_Icon_m0982trg_c3c = 662, // player->self

}

