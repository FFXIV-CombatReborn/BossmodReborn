namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretSwallow;

public enum OID : uint
{
    SecretSwallow = 0x302B, //R=4.0
    SwallowHatchling = 0x302C, //R=2.0
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
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

    Telega = 9630, // KeeperOfKeys/FuathTrickster->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768 // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
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
    private static readonly uint[] bonusAdds = [(uint)OID.FuathTrickster, (uint)OID.KeeperOfKeys];
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
                (uint)OID.FuathTrickster => 3,
                (uint)OID.KeeperOfKeys => 2,
                (uint)OID.SwallowHatchling => 1,
                _ => 0
            };
        }
    }
}
