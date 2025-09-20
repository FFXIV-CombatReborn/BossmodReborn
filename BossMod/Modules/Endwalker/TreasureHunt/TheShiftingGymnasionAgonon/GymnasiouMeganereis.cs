namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouMeganereis;

public enum OID : uint
{
    Boss = 0x3D39, //R=6.0
    GymnasiouNereis = 0x3D3A, //R=2.0
    GymnasticGarlic = 0x3D51, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasiouLampas = 0x3D4D, //R=2.001
    GymnasiouLyssa = 0x3D4E, //R=3.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // GymnasiouLyssa->player, no cast, single-target
    AutoAttack2 = 871, // GymnasiouNereis->player, no cast, single-target
    AutoAttack3 = 872, // Boss->player, no cast, single-target

    WaveOfTurmoilVisual = 32257, // Boss->self, 5.0s cast, single-target
    WaveOfTurmoil = 32258, // Helper->self, 5.0s cast, range 40 circle, knockback 20 away from source
    Hydrobomb = 32259, // Helper->location, 6.5s cast, range 10 circle
    Ceras = 32255, // Boss->player, 5.0s cast, single-target
    WaterspoutVisual = 32261, // Boss->self, 3.0s cast, single-target
    Waterspout = 32262, // Helper->location, 3.0s cast, range 8 circle
    FallingWaterVisual = 32199, // Boss->self, no cast, single-target
    FallingWater = 32260, // Helper->player, 5.0s cast, range 8 circle
    Hydrocannon = 32264, // GymnasiouNereis->self, 3.6s cast, range 17 width 3 rect
    Hydrocannon2 = 32256, // Boss->self, 3.0s cast, range 27 width 6 rect
    Immersion = 32263, // Boss->self, 5.0s cast, range 50 circle

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // Mandragoras/Lyssa->self, no cast, single-target, bonus add disappear
}

class Ceras(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Ceras);

class WaveOfTurmoil(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WaveOfTurmoil, 20f, stopAtWall: true)
{
    private readonly Hydrobomb _aoe = module.FindComponent<Hydrobomb>()!;
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
            var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
            var center = Arena.Center;
            var forbidden = new ShapeDistance[count];
            for (var i = 0; i < count; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                forbidden[i] = new SDCone(center, 20f, Angle.FromDirection(aoe.Origin - center), cone);
            }
            if (forbidden.Length != 0)
                hints.AddForbiddenZone(new SDUnion(forbidden), Casters.Ref(0).Activation);
        }
    }
}

class Hydrobomb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydrobomb, 10f);
class Waterspout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Waterspout, 8f);
class Hydrocannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydrocannon, new AOEShapeRect(17f, 1.5f));
class Hydrocannon2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydrocannon2, new AOEShapeRect(27f, 3f));
class FallingWater(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FallingWater, 8f);
class Immersion(BossModule module) : Components.RaidwideCast(module, (uint)AID.Immersion);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

class HeavySmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySmash, 6f);

class GymnasiouMeganereisStates : StateMachineBuilder
{
    public GymnasiouMeganereisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Ceras>()
            .ActivateOnEnter<Hydrobomb>()
            .ActivateOnEnter<WaveOfTurmoil>()
            .ActivateOnEnter<Waterspout>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<Hydrocannon2>()
            .ActivateOnEnter<FallingWater>()
            .ActivateOnEnter<Immersion>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => AllDeadOrDestroyed(GymnasiouMeganereis.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12014)]
public class GymnasiouMeganereis(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa, (uint)OID.GymnasiouLampas];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouNereis, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GymnasiouNereis));
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
                (uint)OID.GymnasticOnion => 6,
                (uint)OID.GymnasticEggplant => 5,
                (uint)OID.GymnasticGarlic => 4,
                (uint)OID.GymnasticTomato => 3,
                (uint)OID.GymnasticQueen or (uint)OID.GymnasiouLampas or (uint)OID.GymnasiouLyssa => 2,
                (uint)OID.GymnasiouNereis => 1,
                _ => 0
            };
        }
    }
}
