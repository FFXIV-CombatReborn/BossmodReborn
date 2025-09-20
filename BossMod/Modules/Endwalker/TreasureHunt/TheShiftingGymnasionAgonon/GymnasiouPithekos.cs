namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouPithekos;

public enum OID : uint
{
    Boss = 0x3D2B, //R=6
    BallOfLevin = 0x3E90, //R=1.7
    GymnasiouPithekosMikros = 0x3D2C, //R=4.2
    GymnasiouLyssa = 0x3D4E, //R=3.75
    GymnasticGarlic = 0x3D51, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // GymnasiouPithekosMikros->player, no cast, single-target

    Thundercall = 32212, // Boss->location, 2.5s cast, range 3 circle
    LightningBoltVisual = 32214, // Boss->self, 3.0s cast, single-target
    LightningBolt = 32215, // Helper->location, 3.0s cast, range 6 circle
    ThunderIV = 32213, // BallOfLevin->self, 7.0s cast, range 18 circle
    Spark = 32216, // Boss->self, 4.0s cast, range 14-30 donut

    RockThrow = 32217, // GymnasiouPithekosMikros->location, 3.0s cast, range 6 circle
    SweepingGouge = 32211, // Boss->player, 5.0s cast, single-target

    HeavySmash = 32317, // GymnasiouLyssa -> location 3.0s cast, range 6 circle
    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    Telega = 9630 // Mandragoras/GymnasiouLyssa->self, no cast, single-target, bonus add disappear
}

public enum IconID : uint
{
    Thundercall = 111 // Thundercall marker
}

class Spark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spark, new AOEShapeDonut(14f, 30f));
class SweepingGouge(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SweepingGouge);
class Thundercall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thundercall, 3f);

class Thundercall2(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(18f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Thundercall)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle, WorldState.FutureTime(6d)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Thundercall)
        {
            CurrentBaits.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Count != 0 && CurrentBaits.Ref(0).Target == actor)
        {
            ref var b = ref CurrentBaits.Ref(0);
            if (b.Target == actor)
            {
                hints.AddForbiddenZone(new SDCircle(Arena.Center, 17.5f), b.Activation);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
        {
            return;
        }
        if (CurrentBaits.Ref(0).Target != actor)
        {
            base.AddHints(slot, actor, hints);
        }
        else
        {
            hints.Add("Bait levinorb away!");
        }
    }
}

class RockThrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RockThrow, 6f);
class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBolt, 6f);
class ThunderIV(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderIV, 18f);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

class HeavySmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySmash, 6f);

class GymnasiouPithekosStates : StateMachineBuilder
{
    public GymnasiouPithekosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Spark>()
            .ActivateOnEnter<Thundercall>()
            .ActivateOnEnter<Thundercall2>()
            .ActivateOnEnter<RockThrow>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<SweepingGouge>()
            .ActivateOnEnter<ThunderIV>()
            .ActivateOnEnter<HeavySmash>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(GymnasiouPithekos.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12001)]
public class GymnasiouPithekos(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouPithekosMikros, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GymnasiouPithekosMikros));
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
                (uint)OID.GymnasticQueen or (uint)OID.GymnasiouLyssa => 2,
                (uint)OID.GymnasiouPithekosMikros => 1,
                _ => 0
            };
        }
    }
}
