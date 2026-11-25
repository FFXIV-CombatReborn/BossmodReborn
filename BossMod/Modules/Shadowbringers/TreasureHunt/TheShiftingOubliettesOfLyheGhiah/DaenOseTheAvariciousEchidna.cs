namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.DaenOseTheAvariciousEchidna;

public enum OID : uint
{
    DaenOseTheAvariciousEchidna = 0x3032, // R0.75-5.0
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    FuathTrickster = 0x3033, // R0.75
    KeeperOfKeys = 0x3034, // R3.23
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // DaenOseTheAvariciousEchidna->player, no cast, single-target
    AutoAttack2 = 872, // BonusAdds->player, no cast, single-target
    ChangeVisual1 = 21742, // Helper->self, no cast, single-target
    ChangeVisual2 = 21756, // DaenOseTheAvariciousEchidna->self, 6.0s cast, single-target, boss morphs into Echidna

    PetrifactionBoss = 21761, // DaenOseTheAvariciousEchidna->self, 4.0s cast, range 40 circle, gaze
    PetrifactionPlayer = 21762, // Helper->players, 5.0s cast, range 40 circle, gaze
    AbyssalReaper = 21758, // DaenOseTheAvariciousEchidna->self, 5.0s cast, range 18 circle
    AbyssalPillarVisual = 21763, // DaenOseTheAvariciousEchidna->self, 3.0s cast, single-target
    AbyssalPillar = 21764, // Helper->location, 4.0s cast, range 3 circle
    VoidStreamVisual1 = 21765, // DaenOseTheAvariciousEchidna->self, 3.0s cast, single-target
    VoidStreamVisual2 = 21698, // DaenOseTheAvariciousEchidna->self, no cast, single-target
    VoidStream1 = 22798, // Helper->self, 4.5s cast, range 40 45-degree cone
    VoidStream2 = 22797, // Helper->self, 5.0s cast, range 40 45-degree cone
    VoidStream3 = 22799, // Helper->self, 6.0s cast, range 40 45-degree cone
    VoidStream4 = 22800, // Helper->self, 5.8s cast, range 40 45-degree cone
    PetriburstVisual = 21759, // DaenOseTheAvariciousEchidna->self, 5.0s cast, single-target
    Petriburst = 21760, // Helper->players, 5.0s cast, range 6 circle, stack + gaze
    SickleStrike = 21757, // DaenOseTheAvariciousEchidna->player, 4.0s cast, single-target, tankbuster

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

sealed class AbyssalReaper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalReaper, 18f);
sealed class AbyssalPillar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalPillar, 3f);

sealed class Petriburst(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Petriburst, 6f, PartyState.MaxPartySize, PartyState.MaxPartySize);
sealed class PetrifactionBoss(BossModule module) : Components.CastGaze(module, (uint)AID.PetrifactionBoss);

sealed class PetrifactionPlayer(BossModule module) : Components.GenericGaze(module)
{
    private DateTime activation;
    private Actor? target;

    private readonly Eye[] _oneEye = new Eye[1];
    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (target == null)
        {
            return [];
        }
        var loc = target.Position.Quantized();
        {
            _oneEye[0] = new(loc, activation, eyeCenter: loc);
            return _oneEye;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PetrifactionPlayer or (uint)AID.Petriburst)
        {
            activation = Module.CastFinishAt(spell);
            target = WorldState.Actors.Find(spell.TargetID);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PetrifactionPlayer or (uint)AID.Petriburst)
        {
            target = null;
        }
    }
}

sealed class SickleStrike(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SickleStrike);
sealed class VoidStream(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VoidStream1, (uint)AID.VoidStream2,
(uint)AID.VoidStream3, (uint)AID.VoidStream4], new AOEShapeCone(40f, 22.5f.Degrees()), 4);

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);
sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

sealed class DaenOseTheAvariciousEchidnaStates : StateMachineBuilder
{
    public DaenOseTheAvariciousEchidnaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbyssalReaper>()
            .ActivateOnEnter<AbyssalPillar>()
            .ActivateOnEnter<Petriburst>()
            .ActivateOnEnter<PetrifactionBoss>()
            .ActivateOnEnter<PetrifactionPlayer>()
            .ActivateOnEnter<SickleStrike>()
            .ActivateOnEnter<VoidStream>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => AllDeadOrDestroyed(DaenOseTheAvariciousEchidna.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(DaenOseTheAvariciousEchidnaStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.DaenOseTheAvariciousEchidna,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Shadowbringers,
Category = BossModuleInfo.Category.TreasureHunt,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 745u,
NameID = 9808u,
SortOrder = 16,
PlanLevel = 0)]

public sealed class DaenOseTheAvariciousEchidna(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen, (uint)OID.FuathTrickster, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.DaenOseTheAvariciousEchidna, .. bonusAdds];

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
                (uint)OID.SecretOnion => 6,
                (uint)OID.SecretEgg => 5,
                (uint)OID.SecretGarlic => 4,
                (uint)OID.SecretTomato => 3,
                (uint)OID.SecretQueen or (uint)OID.FuathTrickster => 2,
                (uint)OID.KeeperOfKeys => 1,
                _ => 0
            };
        }
    }
}
