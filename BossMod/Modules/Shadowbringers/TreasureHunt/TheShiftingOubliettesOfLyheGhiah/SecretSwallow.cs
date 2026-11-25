namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretSwallow;

public enum OID : uint
{
    SecretSwallow = 0x302B, //R=4.0
    SwallowHatchling = 0x302C, //R=2.0
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // SecretSwallow/SwallowHatchling->player, no cast, single-target
    AutoAttack2 = 872, // KeeperOfKeys->player, no cast, single-target

    ElectricWhorl = 21720, // SecretSwallow->self, 4.5s cast, range 8-60 donut
    Hydrocannon = 21712, // SecretSwallow->self, no cast, single-target
    Hydrocannon2 = 21766, // Helper->location, 3.0s cast, range 8 circle
    Ceras = 21716, // SecretSwallow->player, 4.0s cast, single-target, applies poison
    SeventhWave = 21719, // SecretSwallow->self, 4.5s cast, range 11 circle
    BodySlam = 21718, // SecretSwallow->location, 4.0s cast, range 10 circle, knockback 20, away from source
    PrevailingCurrent = 21717, // SwallowHatchling->self, 3.0s cast, range 22+R width 6 rect

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

sealed class ElectricWhorl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeventhWave, 11f);
sealed class PrevailingCurrent(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PrevailingCurrent, new AOEShapeRect(24f, 3f));
sealed class SeventhWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectricWhorl, new AOEShapeDonut(8f, 60f));
sealed class Hydrocannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydrocannon2, 8f);
sealed class Ceras(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Ceras);
sealed class BodySlam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BodySlam, 10f);

sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));
sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

sealed class SecretSwallowStates : StateMachineBuilder
{
    public SecretSwallowStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricWhorl>()
            .ActivateOnEnter<PrevailingCurrent>()
            .ActivateOnEnter<SeventhWave>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<Ceras>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(SecretSwallow.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(SecretSwallowStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.SecretSwallow,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Shadowbringers,
Category = BossModuleInfo.Category.TreasureHunt,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 745u,
NameID = 9782u,
SortOrder = 11,
PlanLevel = 0)]
public sealed class SecretSwallow : THTemplate
{
    public SecretSwallow(WorldState ws, Actor primary) : base(ws, primary)
    {
        hatchlings = Enemies((uint)OID.SwallowHatchling);
    }
    private readonly List<Actor> hatchlings;
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen, (uint)OID.KeeperOfKeys, (uint)OID.FuathTrickster];
    public static readonly uint[] All = [(uint)OID.SecretSwallow, (uint)OID.SwallowHatchling, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(hatchlings);
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
                (uint)OID.SecretOnion => 7,
                (uint)OID.SecretEgg => 6,
                (uint)OID.SecretGarlic => 5,
                (uint)OID.SecretTomato => 4,
                (uint)OID.SecretQueen or (uint)OID.FuathTrickster => 3,
                (uint)OID.KeeperOfKeys => 2,
                (uint)OID.SwallowHatchling => 1,
                _ => 0
            };
        }
    }
}
