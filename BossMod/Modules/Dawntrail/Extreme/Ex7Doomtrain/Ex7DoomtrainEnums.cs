namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

public enum OID : uint
{
    Doomtrain = 0x4A37, // R19.04
    Doomtrain1 = 0x4B7F, // R19.04
    ArenaFeatures = 0x1EA1A1, // R0.5-2.0
    LevinSignal = 0x4A38, // R1.0
    KinematicTurret = 0x4A39, // R1.2
    Aether = 0x4A3A, // R1.5
    GhostTrain = 0x4B81, // R2.72
    _Gen_ = 0x4A36, // R1.0
    DoomtrainHelper = 0x4A3B, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 45716, // Helper->player, no cast, single-target
    Teleport1 = 45641, // Doomtrain->location, no cast, single-target
    Teleport2 = 45655, // Doomtrain->location, no cast, single-target

    DeadMansOverdraughtSpread = 45663, // Doomtrain->self, 4.0+1,0s cast, single-target
    DeadMansOverdraughtStack = 45664, // Doomtrain->self, 4.0+1,0s cast, single-target
    DeadMansWindpipeVisual = 45677, // Doomtrain->self, 6.0+1,0s cast, single-target
    DeadMansExpress = 45670, // Doomtrain->self, 6.0s cast, range 70 width 70 rect, knockback 30, dir forward
    DeadMansWindpipe = 45696, // Helper->self, 7.0s cast, range 30 width 20 rect, pull 28 to -10 of center
    DeadMansBlastpipeVisual = 45678, // Doomtrain->self, no cast, single-target
    DeadMansBlastpipe = 45679, // Helper->self, 3.0s cast, range 10 width 20 rect
    PlasmaBeam1 = 45671, // LevinSignal->self, 1.0s cast, range 30 width 5 rect, bottom
    PlasmaBeam2 = 45672, // LevinSignal->self, 1.0s cast, range 30 width 5 rect, top
    PlasmaBeam3 = 45674, // LevinSignal->self, 1.0s cast, range 10 width 5 rect
    PlasmaBeam4 = 45673, // LevinSignal->self, 1.0s cast, range 20 width 5 rect

    Plasma = 45675, // Helper->players, no cast, range 5 circle, spread
    HyperexplosivePlasma = 45676, // Helper->players, no cast, range 5 circle, partner stack

    UnlimitedExpressVisual = 45623, // Doomtrain->self, 5.0s cast, single-target
    UnlimitedExpress = 45680, // Helper->self, 5.9s cast, range 70 width 70 rect

    TurretCrossing = 45628, // Doomtrain->self, 3.0s cast, single-target
    Electray1 = 45681, // KinematicTurret->self, 7.0s cast, range 25 width 5 rect
    Electray2 = 45683, // KinematicTurret->self, 7.0s cast, range 20 width 5 rect
    Electray3 = 45686, // KinematicTurret->self, 7.0s cast, range 5 width 5 rect
    Electray4 = 45682, // KinematicTurret->self, 7.0s cast, range 25 width 5 rect

    LightningBurstVisual = 45660, // Doomtrain->self, 5.0s cast, single-target, tankbuster
    LightningBurst = 45715, // Helper->player, 0.5s cast, range 5 circle

    RunawayTrainVisual1 = 45638, // Doomtrain->self, 5.0s cast, single-target, intermission start
    RunawayTrainVisual2 = 45644, // Doomtrain1->self, no cast, single-target, intermission end
    Overdraught = 45639, // Aether->self, no cast, single-target

    AetherialRay = 45693, // Helper->self, no cast, range 50 45-degree cone, tankbuster
    Aetherochar = 45697, // Helper->player, no cast, range 6 circle, spread
    Aetherosote = 45698, // Helper->players, no cast, range 6 circle, stack

    RunawayTrain = 45645, // Helper->self, no cast, range 20 circle, intermission end

    ShockwaveVisual = 45646, // Doomtrain->self, no cast, single-target
    Shockwave = 45699, // Helper->self, no cast, range 50 circle, raidwide

    DerailmentSiegeVisual1 = 45648, // Doomtrain->self, 6.0+1,0s cast, single-target
    DerailmentSiegeVisual2 = 45700, // Doomtrain->self, 6.0+1,0s cast, single-target
    DerailmentSiegeVisual3 = 45702, // Doomtrain->self, 6.0+1,0s cast, single-target
    DerailmentSiegeVisual4 = 45701, // Doomtrain->self, 6.0+1,0s cast, single-target
    DerailmentSiegeVisual5 = 45649, // Helper->self, 10.0s cast, range 5 circle
    DerailmentSiegeVisual6 = 45703, // Helper->self, 11.0s cast, range 5 circle
    DerailmentSiegeVisual7 = 45704, // Helper->self, 12.0s cast, range 5 circle
    DerailmentSiegeVisual8 = 45705, // Helper->self, 13.0s cast, range 5 circle
    DerailmentSiege1 = 45706, // Helper->self, no cast, range 5 circle
    DerailmentSiege2 = 45707, // Helper->self, 0.5s cast, range 5 circle
    UnmitigatedExplosion = 45708, // Helper->self, no cast, range 100 circle, tower fail

    DerailVisual1 = 45709, // Doomtrain->self, 5.1s cast, single-target
    DerailVisual2 = 46489, // Doomtrain->self, 5.1s cast, single-target
    DerailVisual3 = 46490, // Doomtrain->self, 5.1s cast, single-target
    DerailEnrageVisual = 45710, // Doomtrain->self, 15.1s cast, single-target
    Derail1 = 45711, // Helper->self, 5.0s cast, range 30 width 20 rect
    Derail2 = 45712, // Helper->self, 15.0s cast, range 30 width 20 rect

    ThirdRailVisual = 45665, // Doomtrain->self, no cast, single-target
    ThirdRail = 45666, // Helper->location, 3.0s cast, range 4 circle
    HeadlightFirstVisual = 45690, // Doomtrain->self, 6.0s cast, single-target
    HeadlightSecond = 45691, // Doomtrain->self, no cast, single-target
    Headlight = 45692, // DoomtrainHelper->self, no cast, range 70 width 70 rect
    ThunderousBreathSecondVisual = 45688, // Doomtrain->self, no cast, single-target
    ThunderousBreathFirstVisual = 45687, // Doomtrain->self, 6.0s cast, single-target
    ThunderousBreath = 45689, // Helper->self, no cast, range 70 width 70 rect

    ArcaneRevelation = 47527, // Doomtrain->self, 2.0+1,0s cast, single-target
    HailOfThunderVisual1 = 45658, // Doomtrain->self, no cast, single-target
    HailOfThunderVisual2 = 45656, // Doomtrain->self, no cast, single-target
    HailOfThunderVisual3 = 45657, // Doomtrain->self, no cast, single-target
    HailOfThunder1 = 45713, // Helper->location, 2.7s cast, range 16 circle

    HyperconductivePlasma = 45714, // Helper->players, no cast, range 13 circle

    Psychokinesis = 45668, // Doomtrain->self, 7.0s cast, single-target
    Plummet = 45669 // Helper->players, 5.0s cast, range 8 circle, spread
}

public enum SID : uint
{
    _Gen_DeadMansOverdraught = 4720, // none->Doomtrain, extra=0x0
    _Gen_MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    _Gen_VulnerabilityUp = 1789, // Helper/LevinSignal/KinematicTurret/DoomtrainHelper->player, extra=0x1/0x2/0x3
    _Gen_ = 4541, // none->GhostTrain, extra=0x578/0x960
    _Gen_1 = 2056, // none->Doomtrain1, extra=0x400
    _Gen_2 = 2552, // none->GhostTrain, extra=0x42B
    _Gen_3 = 4176, // none->GhostTrain, extra=0x0
    _Gen_SystemLock = 2578, // none->player, extra=0x0
    _Gen_4 = 4721, // none->player, extra=0x0
    _Gen_5 = 3913, // none->Doomtrain, extra=0x3D7/0x3D8
    _Gen_DesignatedConductor = 4719, // none->player, extra=0x0
    _Gen_PhysicalVulnerabilityUp = 2940, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    LightningBurst = 343, // player->self
    _Gen_Icon_m0969_mrt_sht_se_c0k2 = 639, // Doomtrain1->self
    AetherialRay = 412, // player->self
    Aetherochar = 638, // Doomtrain1->self
    Aetherosote = 637, // Doomtrain1->self
    _Gen_Icon_loc08sp_05a_se_c2 = 499, // player->self
}

public enum LogMessageID : uint
{
    RunawayTrain = 11285,
}
