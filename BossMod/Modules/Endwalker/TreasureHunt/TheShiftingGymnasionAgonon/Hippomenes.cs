namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.Hippomenes;

public enum OID : uint
{
    Boss = 0x3D49, // R6.3
    BallOfLevin = 0x3D4A, // R1.7
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
    AutoAttack1 = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // GymnasiouLyssa->player, no cast, single-target

    Hypnotize = 32298, // Boss->self, no cast, single-target
    ElectricWhiskerVisual = 32352, // Boss->self, 5.0s cast, single-target
    ElectricWhisker = 32353, // Helper->self, 5.0s cast, range 40 60-degree cone
    Thunderbolt = 32354, // Helper->player, 5.0s cast, range 6 circle, spread

    ChargeBlaster1 = 33164, // Boss->self, 3.0s cast, single-target, front -> back
    ChargeBlaster2 = 32342, // Boss->self, 3.0s cast, single-target, front -> rear -> left -> right
    ChargeBlaster3 = 32343, // Boss->self, 3.0s cast, single-target, front -> right -> rear -> left
    ChargeBlaster4 = 32344, // Boss->self, 3.0s cast, single-target, front -> left -> right -> rear
    DibridBlaster1 = 32345, // Boss->self, 4.0s cast, range 30 270-degree cone, front
    DibridBlaster2 = 33165, // Boss->self, no cast, range 30 270-degree cone, rear
    TetrabridBlaster1 = 32346, // Boss->self, 4.0s cast, range 30 270-degree cone, front
    TetrabridBlaster2 = 32348, // Boss->self, no cast, range 30 270-degree cone, left
    TetrabridBlaster3 = 32349, // Boss->self, no cast, range 30 270-degree cone, right
    TetrabridBlaster4 = 32347, // Boss->self, no cast, range 30 270-degree cone, rear

    ElectricBurstVisual = 32355, // Boss->self, 5.0s cast, single-target
    ElectricBurst = 32356, // Helper->self, 5.0s cast, range 35 circle
    Shock = 32357, // BallOfLevin->self, 4.5s cast, range 10 circle
    Gouge = 32341, // Boss->player, 5.0s cast, single-target, tankbuster
    RumblingThunderVisual = 32350, // Boss->self, 3.0s cast, single-target
    RumblingThunder = 32351, // Helper->location, 3.0s cast, range 6 circle

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // Mandragoras/Lampas/Lyssa->self, no cast, single-target, bonus add disappear
}

class DibridTetrabridBlaster(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Angle a180 = 180f.Degrees(), a90 = 90f.Degrees();
    private static readonly AOEShapeCone cone = new(30, 135f.Degrees());
    private readonly List<AOEInstance> _aoes = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChargeBlaster1:
                AddAOEs([default, a180]);
                break;
            case (uint)AID.ChargeBlaster2:
                AddAOEs([default, a180, a90, -a90]);
                break;
            case (uint)AID.ChargeBlaster3:
                AddAOEs([default, -a90, a180, a90]);
                break;
            case (uint)AID.ChargeBlaster4:
                AddAOEs([default, a90, -a90, a180]);
                break;
        }
        void AddAOEs(Angle[] angles)
        {
            var len = angles.Length;
            for (var i = 0; i < len; ++i)
            {
                _aoes.Add(new(cone, spell.LocXZ, spell.Rotation + angles[i], Module.CastFinishAt(spell, 6f + 3f * i)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.DibridBlaster1:
                case (uint)AID.DibridBlaster2:
                case (uint)AID.TetrabridBlaster1:
                case (uint)AID.TetrabridBlaster2:
                case (uint)AID.TetrabridBlaster3:
                case (uint)AID.TetrabridBlaster4:
                    _aoes.RemoveAt(0);
                    break;
            }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoes.Count;
        if (count == 0)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
        if (count > 1)
        {
            var aoe = _aoes[0];
            // stay close to the middle if there is more than one aoe left
            hints.AddForbiddenZone(new SDInvertedCircle(aoe.Origin, 5f), aoe.Activation);
        }
    }
}

class Thunderbolt(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Thunderbolt, 6f);
class Gouge(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Gouge);
class ElectricBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.ElectricBurst);
class Shock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shock, 10f);
class RumblingThunder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RumblingThunder, 6f);
class ElectricWhisker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectricWhisker, new AOEShapeCone(40f, 30f.Degrees()));

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

class HeavySmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySmash, 6f);

class HippomenesStates : StateMachineBuilder
{
    public HippomenesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DibridTetrabridBlaster>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<Gouge>()
            .ActivateOnEnter<ElectricBurst>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<RumblingThunder>()
            .ActivateOnEnter<ElectricWhisker>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => AllDeadOrDestroyed(Hippomenes.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12030)]
public class Hippomenes(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa, (uint)OID.GymnasiouLampas];
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
                (uint)OID.GymnasticOnion => 5,
                (uint)OID.GymnasticEggplant => 4,
                (uint)OID.GymnasticGarlic => 3,
                (uint)OID.GymnasticTomato => 2,
                (uint)OID.GymnasticQueen or (uint)OID.GymnasiouLampas or (uint)OID.GymnasiouLyssa => 1,
                _ => 0
            };
        }
    }
}
