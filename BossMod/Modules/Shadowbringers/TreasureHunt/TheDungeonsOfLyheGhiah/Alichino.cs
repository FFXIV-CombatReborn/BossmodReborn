namespace BossMod.Shadowbringers.TreasureHunt.DungeonsOfLyheGhiah.Alichino;

public enum OID : uint
{
    Alichino = 0x2BA7, //R=3.3
    SweetSong = 0x2BAA, // R1.0
    Alich = 0x2BA8, // R0.9
    Ino = 0x2BA9, // R0.9
    DungeonQueen = 0x2A0A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    DungeonGarlic = 0x2A08, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    DungeonTomato = 0x2A09, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    DungeonOnion = 0x2A06, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    DungeonEgg = 0x2A07, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    KeeperOfKeys = 0x2A05, // R3.23
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Alichino/Ino/Alich->player, no cast, single-target

    Knockout = 17880, // Alichino->player, 4.0s cast, single-target, tankbuster
    ManiacalLaughter = 17884, // Alichino->self, 3.0s cast, single-target
    HeatGazeDonut1 = 17883, // Alichino->self, 3.0s cast, range 5-10 donut
    HeatGazeDonut2 = 17887, // Ino->self, 3.0s cast, range 5-10 donut
    HeatGazeCone1 = 17881, // Alichino->self, 3.0s cast, range 40+R 120-degree cone
    HeatGazeCone2 = 17886, // Alich->self, 3.0s cast, range 19+R 60-degree cone
    DiscordantEcho = 17885, // SweetSong->self, 3.0s cast, range 6 circle
    Slapstick = 17882, // Alichino->self, 3.0s cast, range 50 circle

    Pollen = 6452, // DungeonQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // DungeonOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // DungeonTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // DungeonEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // DungeonGarlic->self, 3.5s cast, range 6+R circle
    Mash = 17852, // KeeperOfKeys->self, 2.5s cast, range 12+R width 4 rect
    Scoop = 17853, // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
    Inhale = 17855, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 17854, // KeeperOfKeys->self, 2.5s cast, range 11 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

sealed class DiscordantEcho(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DiscordantEcho, 6f);
sealed class HeatGazeDonut(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HeatGazeDonut1, (uint)AID.HeatGazeDonut2], new AOEShapeDonut(5f, 10f));
sealed class HeatGazeConeBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeatGazeCone1, new AOEShapeCone(43.3f, 60f.Degrees()));
sealed class HeatGazeConeSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeatGazeCone2, new AOEShapeCone(19.9f, 30f.Degrees()));
sealed class Slapstick(BossModule module) : Components.RaidwideCast(module, (uint)AID.Slapstick);
sealed class Knockout(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Knockout);

sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(15.23f, 2f));
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

sealed class AlichinoStates : StateMachineBuilder
{
    public AlichinoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DiscordantEcho>()
            .ActivateOnEnter<HeatGazeDonut>()
            .ActivateOnEnter<HeatGazeConeBig>()
            .ActivateOnEnter<HeatGazeConeSmall>()
            .ActivateOnEnter<Slapstick>()
            .ActivateOnEnter<Knockout>()
            .ActivateOnEnter<HeatGazeConeSmall>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(Alichino.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(AlichinoStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.Alichino,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Shadowbringers,
Category = BossModuleInfo.Category.TreasureHunt,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 688u,
NameID = 8955u,
SortOrder = 1,
PlanLevel = 0)]
public sealed class Alichino : DungeonsOfLyheGhiahRoom5
{
    public Alichino(WorldState ws, Actor primary) : base(ws, primary)
    {
        inos = Enemies((uint)OID.Ino);
        alichs = Enemies((uint)OID.Alich);
    }

    private readonly List<Actor> inos;
    private readonly List<Actor> alichs;

    private static readonly uint[] bonusAdds = [(uint)OID.DungeonEgg, (uint)OID.DungeonGarlic, (uint)OID.DungeonTomato, (uint)OID.DungeonOnion,
    (uint)OID.DungeonQueen, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.Alichino, (uint)OID.Alich, (uint)OID.Ino, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(inos);
        Arena.Actors(alichs);
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
                (uint)OID.DungeonOnion => 6,
                (uint)OID.DungeonEgg => 5,
                (uint)OID.DungeonGarlic => 4,
                (uint)OID.DungeonTomato => 3,
                (uint)OID.DungeonQueen or (uint)OID.KeeperOfKeys => 2,
                (uint)OID.Alich or (uint)OID.Ino => 1,
                _ => 0
            };
        }
    }
}
