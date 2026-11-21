namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

public enum OID : uint
{
    Boss = 0x479F, // R4.0
    Painting = 0x47B4, // R1.0
    HeavenBomb = 0x47A1, // R0.8
    PaintBomb = 0x47A0, // R0.8
    CandiedSuccubus = 0x47A5, // R2.5
    MouthwateringMorbol = 0x47A4, // R4.55
    Yan = 0x47A8, // R1.0
    Mu = 0x47A7, // R1.8
    StickyPudding = 0x47A6, // R1.2
    GimmeCat = 0x47AB, // R1.65
    Jabberwock = 0x47A9, // R3.0
    FeatherRay = 0x47AA, // R1.6
    WaterVoidzone = 0x1EBD91, // R0.5
    SweetShot = 0x47A2, // R1.5
    TempestPiece = 0x47A3, // R0.8
    MousseDripVoidzone = 0x1EBD92, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 42932, // Boss->player, no cast, single-target
    AutoAttackMu = 37919, // Mu->player, no cast, single-target
    AutoAttackYan = 37920, // Yan->player, no cast, single-target
    Teleport = 42611, // Boss->location, no cast, single-target
    TeleportGimmeCat = 42672, // GimmeCat->location, no cast, single-target
    TeleportHeavenBomb = 42620, // HeavenBomb->location, 1.0s cast, single-target

    MousseMural = 42684, // Boss->self, 5.0s cast, range 100 circle

    ColorRiot1 = 42641, // Boss->self, 5.0+2,0s cast, single-target, tankbusters, blue close, red far
    ColorRiot2 = 42642, // Boss->self, 5.0+2,0s cast, single-target, red close, blue far
    WarmBomb = 42644, // Helper->player, no cast, range 4 circle
    CoolBomb = 42643, // Helper->player, no cast, range 4 circle

    Wingmark = 42614, // Boss->self, 4.0+0,9s cast, single-target
    ColorClashVisual1 = 42637, // Boss->self, 3.0s cast, single-target, partner stacks
    ColorClashVisual2 = 42635, // Boss->self, 3.0s cast, single-target, light party stacks
    ColorClash1 = 42639, // Helper->players, no cast, range 6 circle, partner stacks
    ColorClash2 = 42640, // Helper->players, no cast, range 6 circle, light party stacks
    ColorClashVisual3 = 42638, // Boss->self, no cast, single-target, shots for partner stacks
    ColorClashVisual4 = 42636, // Boss->self, no cast, single-target, shots for light party stacks

    BadBreath = 42628, // MouthwateringMorbol->self, 1.0s cast, range 50 100-degree cone
    Burst1 = 42619, // HeavenBomb->location, 1.0s cast, range 15 circle
    Burst2 = 42617, // PaintBomb->self, 1.0s cast, range 15 circle
    DarkMist = 42629, // CandiedSuccubus->self, 1.0s cast, range 30 circle

    DoubleStyle1 = 42624, // Boss->self, 12.0+0,9s cast, single-target
    DoubleStyle2 = 42627, // Boss->self, 8.0+0,9s cast, single-target
    DoubleStyle3 = 42631, // Boss->self, 6.0+0,9s cast, single-target
    DoubleStyle4 = 42626, // Boss->self, 12.0+0,9s cast, single-target
    DoubleStyle5 = 42633, // Boss->self, 6.0+0,9s cast, single-target
    DoubleStyle6 = 37834, // Boss->self, 12.0+0,9s cast, single-target
    DoubleStyle7 = 42621, // Boss->self, 12.0+0,9s cast, single-target
    DoubleStyle8 = 42623, // Boss->self, 12.0+0,9s cast, single-target
    DoubleStyle9 = 42622, // Boss->self, 12.0+0,9s cast, single-target
    DoubleStyle10 = 42625, // Boss->self, 12.0+0,9s cast, single-target
    DoubleStyle11 = 37896, // Boss->self, 12.0+0,9s cast, single-target

    SingleStyle = 39485, // Boss->self, 6.0+0,9s cast, single-target
    Rush = 42630, // SweetShot->location, 1.0s cast, width 7 rect charge

    StickyMousseVisual = 42645, // Boss->self, 5.0+0,6s cast, single-target
    StickyMousse = 42646, // Helper->player, no cast, range 4 circle
    BurstStickyMousse = 42647, // Helper->location, no cast, range 4 circle

    Sugarscape1 = 42600, // Boss->self, 1.0+7,0s cast, single-target, desert
    Sugarscape2 = 42595, // Boss->self, 1.0+7,0s cast, single-target
    Layer1 = 42602, // Boss->self, 1.0+6,0s cast, single-target
    Layer2 = 42604, // Boss->self, 1.0+6,0s cast, single-target
    Layer3 = 42648, // Boss->self, 1.0+6,0s cast, single-target
    Layer4 = 42649, // Boss->self, 1.0+6,0s cast, single-target

    SprayPain1 = 42657, // Helper->self, 7.0s cast, range 10 circle
    SprayPain2 = 39468, // Helper->self, 8.5s cast, range 10 circle
    Brulee = 42658, // Helper->location, no cast, range 15 circle
    CrowdBrulee = 39469, // Helper->location, no cast, range 6 circle
    PuddingGrafVisual = 42677, // Boss->self, 3.0s cast, single-target
    PuddingGraf = 42678, // Helper->player, 5.0s cast, range 6 circle

    SoulSugar = 42661, // Boss->self, 3.0s cast, single-target

    LivePainting1 = 42662, // Boss->self, 4.0s cast, single-target
    LivePainting2 = 42663, // Boss->self, 4.0s cast, single-target
    LivePainting3 = 42664, // Boss->self, 4.0s cast, single-target
    LivePainting4 = 42665, // Boss->self, 4.0s cast, single-target
    ICraveViolence = 42673, // GimmeCat->self, 3.0s cast, range 6 circle
    WaterIIIVisual = 37831, // FeatherRay->self, 3.0s cast, single-target
    WaterIII = 42671, // FeatherRay->players, no cast, range 8 circle
    ManxomeWindersnatch = 42669, // Jabberwock->player, no cast, single-target
    ReadyOreNot = 42666, // Boss->self, 7.0s cast, range 100 circle
    SlayousSnickerSnack = 42670, // Jabberwock->player, no cast, single-target
    HangryHiss = 42674, // GimmeCat->self, 5.0s cast, range 100 circle
    RallyingCheer = 42667, // Mu->Yan, no cast, single-target
    OreRigato = 42668, // Mu->self, 5.0s cast, range 100 circle
    UnlimitedCraving = 39479, // GimmeCat->self, no cast, single-target

    TasteOfThunderAOEVisual = 42652, // Helper->self, no cast, single-target
    TasteOfThunderAOE = 42653, // Helper->location, 3.0s cast, range 3 circle
    TasteOfThunderSpread = 42634, // Helper->player, no cast, range 6 circle
    TasteOfFire = 42632, // Helper->players, no cast, range 6 circle, light party stack
    Highlightning = 42651, // TempestPiece->self, 2.0s cast, range 21 circle
    LightningBolt = 42650, // Helper->location, 3.0s cast, range 4 circle
    LightningStorm = 42654, // Helper->players, no cast, range 8 circle
    LevinDrop = 42655, // Helper->self, no cast, range 60 circle
    PuddingPartyVisual = 42605, // Boss->self, 4.0+1,0s cast, single-target, 5x stack
    PuddingParty = 42681, // Helper->players, no cast, range 6 circle

    LevinMerengue = 42656, // Helper->self, no cast, range 60 circle

    MousseDripVisual = 42679, // Boss->self, 5.0s cast, single-target, 4x partner stack with voidzone spawn 
    MousseDrip = 42680, // Helper->players, no cast, range 5 circle
    MoussacreVisual = 42682, // Boss->self, 4.0+1,0s cast, single-target
    Moussacre = 42683, // Helper->self, no cast, range 60 45-degree cone
    Explosion = 42659, // Helper->self, no cast, range 3 circle
    UnmitigatedExplosion = 42660, // Helper->self, no cast, range 100 circle

    ArtisticAnarchy = 42685, // Boss->self, 8.0+0,9s cast, single-target
    BadBreathEnrage = 42688, // MouthwateringMorbol->self, 3.0s cast, range 50 100-degree cone
    RushEnrage = 42690, // SweetShot->location, 3.0s cast, width 7 rect charge
    DarkMistEnrage = 42689, // CandiedSuccubus->self, 3.0s cast, range 30 circle
    BurstEnrage1 = 42687, // HeavenBomb->location, 3.0s cast, range 15 circle
    BurstEnrage2 = 42686 // PaintBomb->self, 3.0s cast, range 15 circle
}

public enum IconID : uint
{
    ManxomeWindersnatch = 23, // player->self
    LightningStorm = 602, // player->self
    PuddingParty = 305, // player->self
    MousseDrip = 316 // player->self
}

public enum TetherID : uint
{
    ActivateMechanicDoubleStyle1 = 319, // HeavenBomb/player/SweetShot/MouthwateringMorbol/PaintBomb/CandiedSuccubus->Boss
    ActivateMechanicDoubleStyle2 = 320, // CandiedSuccubus/player/SweetShot/MouthwateringMorbol->Boss
    WaterIII = 17 // FeatherRay->player
}

public enum SID : uint
{
    CoolTint = 4452, // Helper->player, extra=0x0
    WarmTint = 4451, // Helper->player, extra=0x0
    Wingmark = 4450, // none->player, extra=0x0
    Stun = 4163, // none->player, extra=0x0
    MousseMine = 4453, // Helper->player, extra=0x0
    BurningUp = 4448, // none->player, extra=0x0
    Sweltering = 4449, // none->player, extra=0x0
    HeatingUp = 4454, // none->player, extra=0x0
    HuffyCat = 4457 // none->GimmeCat, extra=0x1/0x2/0x3, 3 stacks = cat enrage (meteor)
}
