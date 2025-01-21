namespace BossMod.Dawntrail.TreasureHunt.CenoteJaJaGural.Room3;

public enum OID : uint
{
    Boss = 0x1EBAA6, // R0.5
    BirdOfTheCenote1 = 0x431D, // R1.61
    BirdOfTheCenote2 = 0x431E, // R1.61
    CenoteTocoToco = 0x4324, // R0.72
    CenoteToucalibri = 0x4323, // R0.72
    CenoteSarracenia1 = 0x4319, // R2.0
    CenoteSarracenia2 = 0x431A, // R2.0
    CenoteFlytrap = 0x4320, // R0.8
    CenoteRoselet = 0x431F, // R0.8
    CenoteBloodglider = 0x431B, // R1.6
    CenoteWamoura = 0x431C, // R1.6
    CenoteMorpho = 0x4321, // R1.0
    CenoteWamouracampa = 0x4322, // R1.6

    UolonOfFortune = 0x42FF, // R3.5
    AlpacaOfFortune = 0x42FE, // R1.8
    TuraliOnion = 0x4300, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliEggplant = 0x4301, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliGarlic = 0x4302, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliTomato = 0x4303, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    TuligoraQueen = 0x4304, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
}

public enum AID : uint
{
    AutoAttack1 = 871, // BirdOfTheCenote2->player, no cast, single-target
    AutoAttack2 = 872, // UolonOfFortune/CenoteBloodglider/CenoteWamoura/CenoteMorpho/CenoteSarracenia1/CenoteSarracenia2/CenoteRoselet/CenoteFlytrap/BirdOfTheCenote1->player, no cast, single-target
    AutoAttack3 = 870, // CenoteWamouracampa/CenoteToucalibri/CenoteTocoToco->player, no cast, single-target

    Wingbeat = 38307, // BirdOfTheCenote1/BirdOfTheCenote2->self, 3.0s cast, range 18 60-degree cone
    Feathercut = 38308, // BirdOfTheCenote2->self, 3.0s cast, range 40 width 8 rect
    BloodyCaress = 38304, // CenoteSarracenia1/CenoteSarracenia2->self, 3.0s cast, range 12 120-degree cone
    GoldDust = 38303, // CenoteSarracenia1->location, 4.0s cast, range 8 circle
    SwiftSough = 38310, // CenoteFlytrap->self, 3.0s cast, range 13 60-degree cone
    BitterNectar = 38309, // CenoteRoselet->location, 3.0s cast, range 5 circle
    Proboscis = 38305, // CenoteBloodglider/CenoteWamoura->player, no cast, single-target
    FireBreak = 38306, // CenoteWamoura->self, 2.5s cast, range 8 90-degree cone
    Tornado = 38313, // CenoteTocoToco/CenoteToucalibri->location, 3.0s cast, range 6 circle
    Incubus = 38311, // CenoteMorpho->location, 3.0s cast, range 6 circle
    FireII = 38312, // CenoteWamouracampa->location, 3.0s cast, range 5 circle

    Inhale = 38280, // UolonOfFortune->self, 0.5s cast, range 27 120-degree cone
    Spin = 38279, // UolonOfFortune->self, 3.0s cast, range 11 circle
    RottenSpores = 38277, // UolonOfFortune->location, 3.0s cast, range 6 circle
    TearyTwirl = 32301, // TuraliOnion->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // TuraliTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // TuraliGarlic->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // TuraliEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // TuligoraQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

class Wingbeat(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Wingbeat), new AOEShapeCone(18, 30.Degrees()));
class Feathercut(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Feathercut), new AOEShapeRect(40, 4));
class GoldDust(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GoldDust), 8);
class BloodyCaress(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BloodyCaress), new AOEShapeCone(12, 60.Degrees()));
class SwiftSough(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SwiftSough), new AOEShapeCone(13, 30.Degrees()));
class FireBreak(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FireBreak), new AOEShapeCone(8, 45.Degrees()));

abstract class CircleLoc6(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6);
class Tornado(BossModule module) : CircleLoc6(module, AID.Tornado);
class Incubus(BossModule module) : CircleLoc6(module, AID.Incubus);
class RottenSpores(BossModule module) : CircleLoc6(module, AID.RottenSpores);

abstract class CircleLoc5(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 5);
class FireII(BossModule module) : CircleLoc5(module, AID.FireII);
class BitterNectar(BossModule module) : CircleLoc5(module, AID.BitterNectar);

class Spin(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Spin), 11);

abstract class Mandragoras(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 7);
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class Room3States : StateMachineBuilder
{
    public Room3States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Feathercut>()
            .ActivateOnEnter<Wingbeat>()
            .ActivateOnEnter<GoldDust>()
            .ActivateOnEnter<BloodyCaress>()
            .ActivateOnEnter<SwiftSough>()
            .ActivateOnEnter<BitterNectar>()
            .ActivateOnEnter<FireBreak>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<Incubus>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<RottenSpores>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => module.Enemies(Room3.All).All(x => x.IsDeadOrDestroyed) || module.PrimaryActor.EventState == 7;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 993, NameID = 5057)]
public class Room3(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaBounds.Center, ArenaBounds)
{
    private static readonly WPos arenaCenter = new(160, 19);
    private static readonly Angle a135 = 135.Degrees();
    private static readonly WDir dir135 = a135.ToDirection(), dirM135 = (-a135).ToDirection();
    public static readonly ArenaBoundsComplex ArenaBounds = new([new Polygon(arenaCenter, 19.5f * CosPI.Pi36th, 36), new Rectangle(arenaCenter + 8.65f * dir135, 20, 6.15f, a135),
    new Rectangle(arenaCenter + 8.65f * dirM135, 20, 6.15f, -a135), new Rectangle(arenaCenter + 12 * dir135, 20, 4.3f, a135), new Rectangle(arenaCenter + 12 * dirM135, 20, 4.3f, -a135),
    new Rectangle(arenaCenter + 14.3f * dir135, 20, 3.5f, a135), new Rectangle(arenaCenter + 14.3f * dirM135, 20, 3.5f, -a135)], [new Rectangle(new(160, 39), 20, 1.7f)]);
    private static readonly uint[] bonusAdds = [(uint)OID.TuligoraQueen, (uint)OID.TuraliTomato, (uint)OID.TuraliOnion, (uint)OID.TuraliEggplant,
    (uint)OID.TuraliGarlic, (uint)OID.UolonOfFortune, (uint)OID.AlpacaOfFortune];
    private static readonly uint[] trash = [(uint)OID.BirdOfTheCenote1, (uint)OID.BirdOfTheCenote2, (uint)OID.CenoteTocoToco, (uint)OID.CenoteToucalibri,
    (uint)OID.CenoteSarracenia1, (uint)OID.CenoteSarracenia2, (uint)OID.CenoteFlytrap, (uint)OID.CenoteRoselet, (uint)OID.CenoteBloodglider,
    (uint)OID.CenoteWamoura, (uint)OID.CenoteMorpho, (uint)OID.CenoteWamouracampa];
    public static readonly uint[] All = [(uint)OID.Boss, .. trash, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, Colors.Object);
        Arena.Actors(Enemies(trash));
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override bool CheckPull() => !PrimaryActor.IsTargetable;

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.TuraliOnion => 6,
                OID.TuraliEggplant => 5,
                OID.TuraliGarlic => 4,
                OID.TuraliTomato => 3,
                OID.TuligoraQueen or OID.AlpacaOfFortune => 2,
                OID.UolonOfFortune => 1,
                _ => 0
            };
        }
    }
}