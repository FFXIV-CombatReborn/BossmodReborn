namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

// ids were derived from game data (BNpcBase block mirrors the EX block, action ids = EX + 0x9E5C) and then checked
// against a full-kill replay from 2026-10-18. everything with a comment below was seen in that replay.
public enum OID : uint
{
    Boss = 0x4DDE, // R22.000, x1, sits on the east edge during phase 1
    Tail = 0x4DDF, // R17.940, Helper type, spawns 12s after the tail marker
    Cocoon = 0x4DE0, // R3.000, phase 2
    Ginryu = 0x4DE1, // R1.800, phase 2 add
    Icicle = 0x4DE2, // R2.500, x3 per summon
    EyeOfTheStorm = 0x4DE3, // R1.000, center, during super cyclone
    WormsHeart = 0x4DE4, // R29.000, Part type, "The Worm's Heart"
    LeftWing = 0x4DE5, // R15.000, Part type, icicles and ice storm
    RightWing = 0x4DE6, // R15.000, Part type, levinbolt and hypernova
    Hakkinryu = 0x4DE7, // R3.600, phase 2 add
    MassiveCocoon = 0x4DE8, // R6.000, phase 2
    Reiryu = 0x4DE9, // R2.000, x4, phase 1 adds casting dragonflight
    Helper = 0x4DEA, // R0.500, phase 1 helpers at the arena center
    Helper2 = 0x4DEB, // R0.500, phase 2/3 helpers
    LeftWingP3 = 0x4DEC, // R11.200, Part type, phase 3 and enrage
    RightWingP3 = 0x4DED, // R11.200, Part type, phase 3 and enrage
    CocoonMeteor = 0x1FA7, // R0.500, x10, casts the phase 2 meteor impacts
    ReiryuHelper = 0x1FA8, // R0.500, x12
    Platform = 0x1EA1A1, // EventObj, 9 tiles at (0/+-20, 0/+-20)
    WaterSpout = 0x1E8536, // EventObj, tidal wave source
    WaterPuddle = 0x1E950D // EventObj, tidal wave puddles on tile centers
}

public enum AID : uint
{
    AutoAttack = 50260, // LeftWing/RightWing/LeftWingP3/RightWingP3->player, no cast, single-target

    // phase 1, 3x3 tile arena
    EarthenFuryVisual = 50265, // Boss->self, 10.0s cast, single-target
    EarthenFury = 50234, // Helper->self, 10.0s cast, range 60 circle
    EarthenFurySmash = 50282, // Helper->self, 10.0s cast, range 20 width 20 rect, cracks the north and south middle tiles
    TidalWaveVisual = 50261, // Boss->self, 10.0s cast, single-target
    TidalWave = 50230, // Helper->self, 10.0s cast, range 80 width 60 rect, knockback 35 from the water spout
    HellfireVisual = 50262, // Boss->self, 10.0s cast, single-target
    Hellfire = 50231, // Helper->self, 10.0s cast, range 60 circle
    JudgmentBoltVisual = 50263, // Boss->self, 10.0s cast, single-target
    JudgmentBolt = 50232, // Helper->self, 10.0s cast, range 60 circle
    DiamondDustVisual = 50264, // Boss->self, 10.0s cast, single-target
    DiamondDust = 50233, // Helper->self, 10.0s cast, range 60 circle
    AerialBlastVisual = 50266, // Boss->self, 10.0s cast, single-target
    AerialBlast = 50235, // Helper->self, 10.0s cast, range 60 circle, knockback 15 from center
    SuperCyclone = 50280, // Helper->self, 0.5s cast, range 90 circle, knockback 5 from center, x3
    BurningChains = 50270, // Helper2->self, no cast, damage on players still chained
    TailSlap1 = 50238, // Tail->self, 3.0s cast, range 40 width 20 rect
    TailSlap2 = 50281, // Tail->self, 3.0s cast, range 40 width 20 rect
    TailSlapTile = 50239, // Tail->self, no cast, follows each tail slap
    LevinboltVisual = 50246, // RightWing->self, 8.0s cast, single-target
    Levinbolt = 50247, // Helper->player, no cast, range 5 circle, everyone spreads
    HypernovaVisual = 50244, // RightWing->self, 8.0s cast, single-target
    Hypernova = 50245, // Helper->players, no cast, range 8 circle, full party stack
    SummonIcicle = 50250, // LeftWing->self, 4.0s cast, single-target
    IcicleImpact = 50251, // Icicle->self, no cast, range 6 circle
    Spikesicle = 50252, // Icicle->self, 2.5s cast, range 62 width 10 rect, from the west edge across the arena
    IceStormVisual = 50253, // LeftWing->self, 6.0s cast, single-target
    IceStorm = 50254, // Helper->self, no cast, range 60 circle
    AkhMorn = 50255, // Boss->players, 4.0s cast, range 4 circle, shared tankbuster, 3 hits
    AkhMornRepeat = 50256, // Boss->players, no cast, range 4 circle
    AkhRhai = 50257, // Helper->location, no cast, range 4 circle, puddles chasing players
    AkhRhai2 = 50258, // Helper->location, no cast, range 4 circle
    EarthBreath = 50248, // Boss->self, 6.0s cast, range 6 cone
    EarthBreathAOE = 50249, // Helper->self, no cast, range 80 cone aimed at each earthshaker target
    Dragonfist = 50284, // Boss->self, no cast, single-target
    DragonfistAOE = 50285, // Helper->self, 4.0s cast, range 16 circle on the center tile
    GyreCharge = 50271, // Helper->self, 6.3s cast, range 100 width 60 rect, diagonal from a corner
    DragonSpirit = 50286, // Boss->self, no cast, range 90 circle
    Dragonflight = 50228, // Reiryu->self, 30.0s cast, kill the adds before it ends

    // phase 2, small platform below
    DarkMatter = 50243, // Boss->self, 3.0s cast, range 90 circle, floor breaks after
    Touchdown = 50287, // Boss->self, no cast, single-target
    TouchdownAOE = 50288, // Helper2->self, 5.0s cast, range 50 circle, proximity
    MeteorImpact = 50241, // Cocoon/MassiveCocoon->self, 4.0s cast, range 60 circle, proximity
    MeteorImpact2 = 50283, // CocoonMeteor->self, 5.0s cast, range 60 circle, proximity
    ExplosiveHatch = 50242, // Cocoon->self, no cast, range 90 circle
    CocoonHatch = 50273, // Cocoon/MassiveCocoon->self, no cast, follows each meteor impact
    Fireball = 50279, // Ginryu/Hakkinryu->location, 2.5s cast, range 4 circle
    BlazingTrail = 50277, // Ginryu->self, 3.0s cast, range 15 width 11 rect
    SpikedTail = 50276, // Ginryu->player, 1.5s cast, single-target
    DeathSentence = 50278, // Hakkinryu->player, 4.0s cast, single-target
    Collapse = 50275, // Hakkinryu->self, no cast, range 8 cone
    DoubleDive = 50274, // not seen in the replay
    Protostar = 50240, // Boss->self, 6.0s cast, range 100 width 40 rect, covers the whole platform
    ProtostarAOE = 50267, // Helper2->self, no cast, range 100 width 40 rect
    TailSpitVisual = 50289, // Boss->self, 1.0s cast, single-target
    TailSpit = 50290, // Helper2->self, 3.3s cast, range 30 circle, raidwide
    TailSpitSmall = 50227, // Helper2->self, 3.3s cast, range 5 circle
    Shatter = 50291, // Boss->self, no cast, single-target, platform breaks

    // phase 3, big platform
    AetherialRay = 50292, // Helper2->self, 2.5s cast, range 100 width 3 rect, from the wings
    TeraSlash = 50296, // Boss->player, 4.0s cast, single-target tankbuster
    AtomicRayVisual = 50297, // Boss->self, 2.5s cast, single-target
    AtomicRay = 50298, // Helper2->players, no cast, range 6 circle, 2-player stacks on icon targets
    Wormwail = 50293, // Boss->self, 5.0s cast, range 50 donut
    BenightingBreathVisual = 50294, // Boss->self, 3.3s cast, single-target, alternates with wormwail
    BenightingBreath = 50295, // Helper2->self, 4.0s cast, range 50 90-degree cone
    IceStormVisualP3 = 50308, // LeftWingP3->self, 6.0s cast, single-target
    SummonIcicleP3 = 50307, // LeftWingP3->self, 4.0s cast, single-target
    LevinboltVisualP3 = 50306, // RightWingP3->self, 10.0s cast, single-target
    LevinboltAOE = 50309, // Helper2->location, 3.0s cast, range 5 circle
    HypernovaVisualP3 = 50305, // RightWingP3->self, 10.0s cast, single-target

    // enrage
    TidalWaveEnrage = 50299, // Boss->self, 70.0s cast, single-target
    TidalWaveEnrageAOE = 50300, // Helper2->self, no cast, range 80 width 60 rect
    JudgmentBoltEnrage = 50301, // LeftWingP3->self, 35.0s cast, single-target
    HellfireEnrage = 50302, // RightWingP3->self, 35.0s cast, single-target
    HellfireEnrageAOE = 50303, // Helper2->self, no cast, range 60 circle
    JudgmentBoltEnrageAOE = 50304 // Helper2->self, no cast, range 60 circle
}

public enum SID : uint
{
    BurningChains = 769, // Helper->player, run apart to break
    Fetters = 822, // Boss->player, after dark matter
    ThinIce = 911, // Helper->player, after diamond dust
    Affixed = 1267, // Boss->player
    WormsCurse = 1419, // Boss->player, "The Worm's Curse"
    DeepFreeze = 3480 // Helper->player
}

public enum IconID : uint
{
    Earthshaker = 40, // player, earth breath bait, 2 targets
    CocoonSpread = 57, // player, phase 2, 5 targets
    BurningChains = 97, // player, pairs
    AtomicRay = 93, // player, phase 3, 2-player stack
    TailMarker = 126 // player
}

public enum TetherID : uint
{
    Dragonflight = 1, // Reiryu->player
    BurningChains = 9, // player->player
    AkhMorn = 22 // player->player, right before each akh morn, meaning unknown
}
