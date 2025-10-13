namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretBasket;

public enum OID : uint
{
    SecretBasket = 0x302D, //R=2.34
    SecretEchivore = 0x302E, //R=1.05
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // SecretBasket/SecretEchivore/Mandragoras/KeeperOfKeys->player, no cast, single-target

    HeavyStrikeVisual1 = 21698, // SecretBasket->self, no cast, single-target
    HeavyStrikeVisual2 = 21723, // SecretBasket->self, no cast, single-target
    HeavyStrike1 = 21724, // Helper->self, 4.0s cast, range 6+R 270-degree cone
    HeavyStrike2 = 21725, // Helper->self, 4.0s cast, range 12+R 270-degree donut segment
    HeavyStrike3 = 21726, // Helper->self, 4.9s cast, range 18+R 270-degree donut segment

    PollenCorona = 21722, // SecretBasket->self, 3.0s cast, range 8 circle
    StraightPunch = 21721, // SecretBasket->player, 4.0s cast, single-target
    Leafcutter = 21732, // SecretEchivore->self, 3.0s cast, range 15 width 4 rect
    EarthCrusher = 21727, // SecretBasket->self, 3.0s cast, single-target
    EarthCrusher2 = 21728, // Helper->self, 4.0s cast, range 10-20 donut
    SomersaultSlash = 21731, // SecretEchivore->player, no cast, single-target
    EarthquakeVisual = 21729, // SecretBasket->self, 4.0s cast, single-target
    Earthquake = 21730, // Helper->self, no cast, range 20 circle

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768, // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus adds disappear
}

sealed class Earthquake(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.EarthquakeVisual, (uint)AID.Earthquake, 1.2d);

sealed class HeavyStrike(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCone(6.5f, a135), new AOEShapeDonutSector(6.5f, 12.5f, a135), new AOEShapeDonutSector(12.5f, 18.5f, a135)])
{
    private static readonly Angle a135 = 135f.Degrees();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavyStrike1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell), spell.Rotation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.HeavyStrike1 => 0,
                (uint)AID.HeavyStrike2 => 1,
                (uint)AID.HeavyStrike3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(1.1d), spell.Rotation);
        }
    }
}

sealed class PollenCorona(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PollenCorona, 8f);
sealed class StraightPunch(BossModule module) : Components.SingleTargetCast(module, (uint)AID.StraightPunch);
sealed class Leafcutter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Leafcutter, new AOEShapeRect(15f, 2f));
sealed class EarthCrusher(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EarthCrusher2, new AOEShapeDonut(10f, 20f));

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

class SecretBasketStates : StateMachineBuilder
{
    public SecretBasketStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Earthquake>()
            .ActivateOnEnter<HeavyStrike>()
            .ActivateOnEnter<PollenCorona>()
            .ActivateOnEnter<StraightPunch>()
            .ActivateOnEnter<Leafcutter>()
            .ActivateOnEnter<EarthCrusher>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => AllDeadOrDestroyed(SecretBasket.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(SecretBasketStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.SecretBasket,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Shadowbringers,
Category = BossModuleInfo.Category.TreasureHunt,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 745u,
NameID = 9784u,
SortOrder = 3,
PlanLevel = 0)]
public sealed class SecretBasket : THTemplate
{
    public SecretBasket(WorldState ws, Actor primary) : base(ws, primary)
    {
        echivore = Enemies((uint)OID.SecretEchivore);
    }
    private readonly List<Actor> echivore;

    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen, (uint)OID.FuathTrickster, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.SecretBasket, (uint)OID.SecretEchivore, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(echivore);
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
                (uint)OID.SecretOnion => 6,
                (uint)OID.SecretEgg => 5,
                (uint)OID.SecretGarlic => 4,
                (uint)OID.SecretTomato or (uint)OID.FuathTrickster => 3,
                (uint)OID.SecretQueen or (uint)OID.KeeperOfKeys => 2,
                (uint)OID.SecretEchivore => 1,
                _ => 0
            };
        }
    }
}
