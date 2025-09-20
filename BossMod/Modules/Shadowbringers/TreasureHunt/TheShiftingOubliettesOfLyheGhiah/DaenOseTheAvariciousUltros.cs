namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.DaenOseTheAvariciousUltros;

public enum OID : uint
{
    Boss = 0x3030, // R0.75-5.1
    StylishTentacle = 0x3031, // R7.2
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/SecretTomato/SecretQueen->player, no cast, single-target
    Change = 21741, // Boss->self, 6.0s cast, single-target, boss morphs into Ultros

    TentacleVisual = 21753, // Boss->self, no cast, single-target
    Tentacle = 21754, // StylishTentacle->self, 3.0s cast, range 8 circle
    Megavolt = 21752, // Boss->self, 3.5s cast, range 11 circle
    Wallop = 21755, // StylishTentacle->self, 5.0s cast, range 20 width 10 rect
    ThunderIII = 21743, // Boss->player, 4.0s cast, single-target, tankbuster

    WaveOfTurmoilVisual = 21748, // Boss->self, 5.0s cast, single-target
    WaveOfTurmoil = 21749, // Helper->self, 5.0s cast, range 40 circle, knockback 20, away from source

    SoakingSplatter = 21750, // Helper->location, 6.5s cast, range 10 circle
    AquaBreath = 21751, // Boss->self, 3.0s cast, range 13 90-degree cone

    FallingWaterVisual = 21746, // Boss->self, 5.0s cast, single-target
    FallingWater = 21747, // Helper->player, 5.0s cast, range 8 circle, spread

    WaterspoutVisual = 21744, // Boss->self, 3.0s cast, single-target
    Waterspout = 21745, // Helper->location, 3.0s cast, range 4 circle

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus adds disappear
}

class AquaBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AquaBreath, new AOEShapeCone(13f, 45f.Degrees()));
class Tentacle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tentacle, 8f);
class Wallop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wallop, new AOEShapeRect(20f, 5f));
class Megavolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Megavolt, 11f);
class Waterspout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Waterspout, 4f);
class SoakingSplatter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SoakingSplatter, 10f);
class FallingWater(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FallingWater, 8f);
class ThunderIII(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ThunderIII);

class WaveOfTurmoil(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WaveOfTurmoil, 20f, stopAtWall: true)
{
    private readonly SoakingSplatter _aoe = module.FindComponent<SoakingSplatter>()!;
    private static readonly Angle cone = 30f.Degrees();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var count = _aoe.Casters.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var count = _aoe.Casters.Count;
            var forbidden = new ShapeDistance[count];
            var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
            var center = Arena.Center;
            for (var i = 0; i < count; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                forbidden[i] = new SDCone(center, 20f, Angle.FromDirection(aoe.Origin - center), cone);
            }
            if (count != 0)
            {
                hints.AddForbiddenZone(new SDUnion(forbidden), Casters.Ref(0).Activation);
            }
        }
    }
}

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class DaenOseTheAvariciousUltrosStates : StateMachineBuilder
{
    public DaenOseTheAvariciousUltrosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AquaBreath>()
            .ActivateOnEnter<Tentacle>()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<Megavolt>()
            .ActivateOnEnter<Waterspout>()
            .ActivateOnEnter<SoakingSplatter>()
            .ActivateOnEnter<FallingWater>()
            .ActivateOnEnter<ThunderIII>()
            .ActivateOnEnter<WaveOfTurmoil>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(DaenOseTheAvariciousUltros.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9808, SortOrder = 2)]
public class DaenOseTheAvariciousUltros(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, bonusAdds, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.SecretOnion => 5,
                (uint)OID.SecretEgg => 4,
                (uint)OID.SecretGarlic => 3,
                (uint)OID.SecretTomato => 2,
                (uint)OID.SecretQueen => 1,
                _ => 0
            };
        }
    }
}
