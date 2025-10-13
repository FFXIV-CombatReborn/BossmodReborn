namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.GreedyPixie;

public enum OID : uint
{
    GreedyPixie = 0x3018, //R=1.6
    SecretMorpho = 0x3019, //R=1.8
    PixieDouble1 = 0x304C, //R=1.6
    PixieDouble2 = 0x304D, //R=1.6
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
    AutoAttack1 = 23185, // GreedyPixie/Mandragoras->player, no cast, single-target
    AutoAttack2 = 872, // KeeperOfKeys/SecretMorpho->player, no cast, single-target

    WindRune = 21686, // GreedyPixie->self, 3.0s cast, range 40 width 8 rect
    SongRune = 21684, // GreedyPixie->location, 3.0s cast, range 6 circle
    StormRune = 21682, // GreedyPixie->self, 4.0s cast, range 40 circle
    BushBash1 = 22779, // PixieDouble2->self, 7.0s cast, range 12 circle
    BushBash2 = 21683, // GreedyPixie->self, 5.0s cast, range 12 circle
    NatureCall1 = 22780, // PixieDouble1->self, 7.0s cast, range 30 120-degree cone, turns player into a plant
    NatureCall2 = 21685, // GreedyPixie->self, 5.0s cast, range 30 120-degree cone, turns player into a plant

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768, // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

sealed class Windrune(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindRune, new AOEShapeRect(40f, 4f));
sealed class SongRune(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SongRune, 6f);
sealed class StormRune(BossModule module) : Components.RaidwideCast(module, (uint)AID.StormRune);

sealed class BushBash(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BushBash1, (uint)AID.BushBash2], 12f);
sealed class NatureCall(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.NatureCall1, (uint)AID.NatureCall2], new AOEShapeCone(30f, 60f.Degrees()));

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

sealed class GreedyPixieStates : StateMachineBuilder
{
    public GreedyPixieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Windrune>()
            .ActivateOnEnter<StormRune>()
            .ActivateOnEnter<SongRune>()
            .ActivateOnEnter<BushBash>()
            .ActivateOnEnter<NatureCall>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => AllDeadOrDestroyed(GreedyPixie.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(GreedyPixieStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.GreedyPixie,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Shadowbringers,
Category = BossModuleInfo.Category.TreasureHunt,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 745u,
NameID = 9797u,
SortOrder = 2,
PlanLevel = 0)]
public sealed class GreedyPixie : THTemplate
{
    public GreedyPixie(WorldState ws, Actor primary) : base(ws, primary)
    {
        morphos = Enemies((uint)OID.SecretMorpho);
    }
    private readonly List<Actor> morphos;
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen, (uint)OID.FuathTrickster, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.GreedyPixie, (uint)OID.SecretMorpho, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(morphos);
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
                (uint)OID.SecretMorpho => 1,
                _ => 0
            };
        }
    }
}
