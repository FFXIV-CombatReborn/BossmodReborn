namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretDjinn;

public enum OID : uint
{
    SecretDjinn = 0x300F, //R=3.48
    SecretRabbitsTail = 0x3010, //R=1.32
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 23185, // SecretDjinn->player, no cast, single-target
    AutoAttack2 = 872, // SecretRabbitsTail/KeeperOfKeys->player, no cast, single-target

    Gust = 21655, // SecretDjinn->location, 3.0s cast, range 6 circle
    ChangelessWinds = 21657, // SecretDjinn->self, 3.0s cast, range 40 width 8 rect, knockback 10, source forward
    WhirlingGaol = 21654, // SecretDjinn->self, 4.0s cast, range 40 circle, knockback 25 away from source
    Whipwind = 21656, // SecretDjinn->self, 5.0s cast, range 55 width 40 rect, knockback 25, source forward
    GentleBreeze = 21653, // SecretRabbitsTail->self, 3.0s cast, range 15 width 4 rect

    Telega = 9630, // KeeperOfKeys/FuathTrickster->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768 // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
}

sealed class Gust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gust, 6f);
sealed class ChangelessWinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChangelessWinds, new AOEShapeRect(40f, 4f));
sealed class Whipwind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Whipwind, new AOEShapeRect(55f, 20f));
sealed class GentleBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GentleBreeze, new AOEShapeRect(15f, 2f));
sealed class WhirlingGaol(BossModule module) : Components.RaidwideCast(module, (uint)AID.WhirlingGaol, "Raidwide + Knockback");
sealed class WhirlingGaolKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WhirlingGaol, 25f, stopAtWall: true);

sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

sealed class SecretDjinnStates : StateMachineBuilder
{
    public SecretDjinnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<ChangelessWinds>()
            .ActivateOnEnter<Whipwind>()
            .ActivateOnEnter<GentleBreeze>()
            .ActivateOnEnter<WhirlingGaol>()
            .ActivateOnEnter<WhirlingGaolKB>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => AllDeadOrDestroyed(SecretDjinn.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(SecretDjinnStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.SecretDjinn,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Shadowbringers,
Category = BossModuleInfo.Category.TreasureHunt,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 745u,
NameID = 9788u,
SortOrder = 5,
PlanLevel = 0)]
public sealed class SecretDjinn : THTemplate
{
    public SecretDjinn(WorldState ws, Actor primary) : base(ws, primary)
    {
        rabbittails = Enemies((uint)OID.SecretRabbitsTail);
    }
    private readonly List<Actor> rabbittails;

    private static readonly uint[] bonusAdds = [(uint)OID.FuathTrickster, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.SecretDjinn, (uint)OID.SecretRabbitsTail, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(rabbittails);
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
                (uint)OID.SecretRabbitsTail => 1,
                _ => 0
            };
        }
    }
}
