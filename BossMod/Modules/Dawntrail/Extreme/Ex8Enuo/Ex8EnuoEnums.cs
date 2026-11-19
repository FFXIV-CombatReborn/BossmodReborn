namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

public enum OID : uint
{
    Enuo = 0x4DC1,
    Helper = 0x233C,
    Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    YawningVoid = 0x4DC3, // R1.000, x2
    YawningVoid1 = 0x4DC2, // R1.000, x2
    Unknown = 0x4DB8, // R5.000, x2
    Void = 0x4BFA, // R2.000, x0 (spawn during fight), Helper type
    Void1 = 0x4BF7, // R1.000, x0 (spawn during fight), Helper type
    VoidSoakSmall = 0x4DC5, // R0.850, x0 (spawn during fight)
    VoidSoakLarge = 0x4DC6, // R1.750, x0 (spawn during fight)
    VoidVacuum = 0x4DC4, // R0.850, x0 (spawn during fight), Helper type
    AggressiveShadow = 0x4DC9, // R5.000, x0 (spawn during fight)
    SoothingShadow = 0x4DCA, // R5.000, x0 (spawn during fight)
    ProtectiveShadow = 0x4DC8, // R5.000, x0 (spawn during fight)
    LoomingShadow = 0x4DC7, // R12.500, x0 (spawn during fight)
    BeaconInTheDark = 0x4DCB, // R5.000, x0 (spawn during fight)
    NaughtHuntChaser = 0x4EB5, // R0.850, x0 (spawn during fight), Helper type

}

public enum AID : uint
{
    AutoAttack = 49937, // 4DC1->player, no cast, single-target
    Meteorain = 50049, // 4DC1->self, 5.0s cast, range 40 circle
    UnknownAbility = 49927, // 4DC1->location, no cast, single-target
    NaughtGrows = 49975, // 4DC1->self, 7.0+1.0s cast, single-target
    NaughtGrowsDonut = 49978, // 4DC2->self, 8.0s cast, range 20-60 donut
    GreatReturnToNothing = 49984, // 4BFA->location, no cast, width 6 rect charge
    NaughtWakes = 49973, // 4DC1->self, 2.0+1.0s cast, single-target
    UnknownWeaponskill = 49974, // 4DC2->location, no cast, single-target
    Meltdown = 50040, // 4DC1->self, 4.0+1.0s cast, range 40 circle
    Meltdown1 = 50041, // 233C->location, 4.5s cast, range 5 circle
    Meltdown2 = 50042, // 233C->player, 5.5s cast, range 5 circle
    AiryEmptiness = 50032, // 4DC1->self, 4.0+1.0s cast, single-target
    AiryEmptiness1 = 50034, // 233C->self, no cast, range 60 ?-degree cone
    NaughtGrowsCircle = 49977, // 4DC2->self, 8.0s cast, range 40 circle
    ReturnToNothing = 49983, // 4BF7->location, no cast, width 6 rect charge
    GazeOfTheVoid = 50002, // 4DC1->self, 6.0+1.0s cast, single-target
    GazeOfTheVoid1 = 50004, // 233C->self, 7.0s cast, single-target
    GazeOfTheVoid2 = 50005, // 233C->self, 7.0s cast, range 40 ?-degree cone
    Burst = 50006, // 4DC5->self, no cast, range 5 circle
    ViolentBurst = 50007, // 4DC6->self, no cast, range 6 circle
    Vacuum = 49994, // 4DC1->self, 2.0+1.0s cast, single-target
    SilentTorrentDash = 49996, // 4DC4->location, 3.5s cast, single-target
    SilentTorrentDash2 = 49997, // 4DC4->location, 3.5s cast, single-target
    SilentTorrentDash3 = 49995, // 4DC4->location, 3.5s cast, single-target
    SilentTorrentArc1 = 49999, // 233C->self, 4.0s cast, range ?-19 donut
    SilentTorrentArc2 = 50000, // 233C->self, 4.0s cast, range ?-19 donut
    SilentTorrentArc3 = 49998, // 233C->self, 4.0s cast, range ?-19 donut
    VacuumExplode = 50001, // 4DC4->self, 1.5s cast, range 7 circle
    DenseEmptiness = 50033, // 4DC1->self, 4.0+1.0s cast, single-target
    DenseEmptiness1 = 50035, // 233C->self, no cast, range 60 ?-degree cone
    DeepFreezeCastBar = 50043, // 4DC1->self, 5.0+1.0s cast, range 40 circle
    DeepFreeze = 50044, // 233C->players, 6.0s cast, range 40 circle
    AllForNaught = 50010, // 4DC1->self, 5.0s cast, single-target
    LoomingEmptinessCastbar = 50011, // 4DC7->self, 5.0s cast, single-target
    LoomingEmptinessKnockback = 49982, // 233C->self, 6.0s cast, range 100 circle
    LoomingEmptinessKillzone = 49369, // 233C->self, 6.0s cast, range 8 circle
    VoidalTurbulenceCastBar = 50036, // 4DC7->self, 6.0+1.0s cast, single-target
    EmptyShadow = 50013, // 233C->self, 7.0s cast, range 6 circle
    VoidalTurbulenceCone = 50038, // 233C->self, no cast, range 60 ?-degree cone
    UnknownWeaponSkill1 = 50012, // 4DC9/4DCA/4DC8->self, no cast, single-target
    AutoAttack1 = 50752, // 4DC9->player, no cast, single-target
    AutoAttack2 = 50016, // 4DC7->player, no cast, single-target
    DemonEyeCastbar = 50022, // 4DC9->self, 4.0+1.0s cast, single-target
    UnnkownWeaponskill2 = 49938, // 4DC9/4DC8/4DCA/4DC7->self, no cast, single-target
    DemonEyeGaze = 50023, // 233C->self, 5.0s cast, range 20 circle
    AutoAttack3 = 50753, // 4DCA->player, no cast, single-target
    AutoAttack4 = 50751, // 4DC8->player, no cast, single-target
    CurseOfTheFlesh = 50024, // 4DCA->self, 2.0+1.0s cast, single-target
    CurseOfTheFlesh1 = 50025, // 233C->player, 3.0s cast, single-target
    Nothingness = 50017, // 4DC9/4DC8/4DCA->self, 3.0s cast, range 100 width 4 rect
    LightlessWorldCastbar = 50029, // 4DC1->self, 10.0s cast, single-target
    LightlessWorld1 = 50030, // 233C->self, no cast, range 40 circle
    LightlessWorld2 = 50031, // 233C->self, no cast, range 40 circle
    Almagest = 49972, // 4DC1->self, 5.0s cast, range 40 circle
    NaughtGrowsDoubleCast = 49976, // 4DC1->self, 7.0+1.0s cast, single-target
    NaughtGrowsBossDonut = 49980, // 233C->self, 8.0s cast, range 6-40 donut
    PassageOfNaught = 49987, // 233C->self, 6.0s cast, range 80 width 16 rect
    PassageOfNaught1 = 49986, // 4DC3->self, 6.0s cast, range 80 width 16 rect
    PassageOfNaught2 = 49985, // 4DC2->self, 7.0s cast, range 80 width 16 rect
    ShroudedHolyCastbar = 50045, // 4DC1->self, 5.0+1.0s cast, single-target
    ShroudedHolyTargets = 50046, // 233C->players, 6.0s cast, range 6 circle
    NaughtGrowsBossCircle = 49979, // 233C->self, 8.0s cast, range 12 circle
    DimensionZeroCastbar = 50047, // 4DC1->self, 5.0s cast, single-target
    DimensionZeroHits = 50048, // 4DC1->self, no cast, range 60 width 8 rect
    NaughtHunts = 49992, // 4DC1->self, 6.0+1.0s cast, single-target
    EndlessChaseCast = 48475, // 4EB5->self, 6.0s cast, range 6 circle
    EndlessChaseInstant = 49993, // 4EB5->location, no cast, range 6 circle
    GazeOfTheVoid3 = 50003, // 233C->self, 7.0s cast, single-target
    WeightOfNothing = 50021, // 233C->player, 5.0s cast, range 100 width 8 rect
    DrainTouch = 50018, // 4DC8->self, 5.0s cast, single-target
    WeightOfNothing1 = 50020, // 4DC8->self, 4.0+1.0s cast, single-target
}

public enum TetherID : uint
{
    NaughtGrowsWildCharge = 430, // player->Enuo
    Tether_chn_z5fd11_0a1 = 395, // 4DB8->Enuo
    Tether_chn_z5fd09_0a1 = 393, // 4DB8->Enuo
    ClockSlow = 406, // VoidSoakSmall/VoidSoakLarge->Enuo, orb resolves later
    ClockFast = 407, // VoidSoakSmall/VoidSoakLarge->Enuo, orb resolves first
    Tether_chn_tergetfix1f = 284, // 4DC9/4DCA/4DC8->player
    Tether_chn_z5fd10_0a1 = 394, // 4DB8->Enuo
    Tether_chn_z5fd12_0a1 = 396, // 4DB8->Enuo
    NaughtHuntFirst = 404, // 4EB5->player
    NaughtHuntJump = 405, // player->player
}

public enum IconID : uint
{
    NaughtGrowsWildChargeSingle = 702, // Enuo->player
    NaughtGrowsWildChargeDouble = 701, // Enuo->player
    Icon_m0742trg_b1t1 = 327, // player->self
    VoidTurbulanceCone = 721, // player->self
    Icon_tank_laser_5sec_lockon_c0a1 = 471, // player->self
    SharedHoly = 318, // player->self
    DimensionZeroTarget = 719, // Enuo->player
    NaughtHuntTarget = 172, // player->self
}

public enum SID : uint
{
    MagicVulnerabilityUp = 2941, // Void/Helper/Void1/Void2/Void3->player, extra=0x0
    ChainsOfCondemnation = 4562, // Enuo->player, extra=0x0
    UnknownVoid2_3 = 2234, // none->Void2/Void3, extra=0x58/0x4B
    FreezingUp = 3523, // Enuo->player, extra=0x0
    DirectionalDisregard = 3808, // none->Enuo, extra=0x0
    Unknown_Shadows = 2056, // none->AggressiveShadow/SoothingShadow/ProtectiveShadow/LoomingShadow, extra=0x46B
    Unbecoming = 4882, // none->player, extra=0x0
    DarkResistanceDownII = 3323, // Helper->player, extra=0x0
    GauntletTaken = 5364, // none->player, extra=0x0
    GauntletTaken1 = 5362, // none->player, extra=0x0
    GauntletTaken2 = 5361, // none->player, extra=0x0
    GauntletTaken3 = 5363, // none->player, extra=0x0
    GauntletThrown = 5369, // none->AggressiveShadow, extra=0x0
    GauntletThrown1 = 5371, // none->AggressiveShadow, extra=0x0
    GauntletThrown2 = 5372, // none->AggressiveShadow, extra=0x0
    GauntletThrown3 = 5370, // none->AggressiveShadow, extra=0x0
    GauntletTaken4 = 5360, // none->player, extra=0x0
    GauntletTaken5 = 5357, // none->player, extra=0x0
    GauntletTaken6 = 5358, // none->player, extra=0x0
    GauntletTaken7 = 5359, // none->player, extra=0x0
    GauntletThrown4 = 5368, // none->SoothingShadow, extra=0x0
    GauntletThrown5 = 5366, // none->ProtectiveShadow, extra=0x0
    GauntletThrown6 = 5365, // none->ProtectiveShadow, extra=0x0
    GauntletThrown7 = 5367, // none->SoothingShadow, extra=0x0
    QuantumEntanglement = 4884, // none->player, extra=0x0
    QuantumNullification = 4883, // none->SoothingShadow, extra=0x0
    Disease = 3943, // Helper->player, extra=0x32
    InEvent = 1268, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Helper/Void5/YawningVoid1/YawningVoid->player, extra=0x1/0x2/0x3/0x4/0x5

}
