namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretSerpent;

public enum OID : uint
{
    SecretSerpent = 0x3025, //R=5.29
    SerpentHatchling = 0x3026, //R=3.45
    WaterVoidzone = 0x1EA7D5,
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    KeeperOfKeys = 0x3034, // R3.23
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // SecretSerpent/SerpentHatchling->player, no cast, single-target
    AutoAttack2 = 872, // Mandragoras->player, no cast, single-target

    Douse = 21701, // SecretSerpent->location, 3.0s cast, range 8 circle
    Drench1 = 21700, // SecretSerpent->self, 3.0s cast, range 10+R 90-degree cone
    Drench2 = 22771, // SerpentHatchling->self, 3.0s cast, range 10+R 90-degree cone
    FangsEnd = 21699, // SecretSerpent->player, 4.0s cast, single-target
    ScaleRipple = 21702, // SecretSerpent->self, 2.5s cast, range 8 circle

    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768, // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus adds disappear
}

sealed class DouseVoidzone(BossModule module) : Components.Voidzone(module, 7.5f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.WaterVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
sealed class DouseScaleRipple(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Douse, (uint)AID.ScaleRipple], 8f);
sealed class FangsEnd(BossModule module) : Components.SingleTargetCast(module, (uint)AID.FangsEnd);
sealed class Drench1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Drench1, new AOEShapeCone(15.29f, 45f.Degrees()));
sealed class Drench2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Drench2, new AOEShapeCone(13.45f, 45f.Degrees()));

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

sealed class SecretSerpentStates : StateMachineBuilder
{
    public SecretSerpentStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DouseScaleRipple>()
            .ActivateOnEnter<DouseVoidzone>()
            .ActivateOnEnter<FangsEnd>()
            .ActivateOnEnter<Drench1>()
            .ActivateOnEnter<Drench2>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => AllDeadOrDestroyed(SecretSerpent.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(SecretSerpentStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.SecretSerpent,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Shadowbringers,
Category = BossModuleInfo.Category.TreasureHunt,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 745u,
NameID = 9776u,
SortOrder = 10,
PlanLevel = 0)]
public sealed class SecretSerpent : THTemplate
{
    public SecretSerpent(WorldState ws, Actor primary) : base(ws, primary)
    {
        hatchlings = Enemies((uint)OID.SerpentHatchling);
    }
    private readonly List<Actor> hatchlings;

    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.SecretSerpent, (uint)OID.SerpentHatchling, .. bonusAdds];

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
                (uint)OID.SecretOnion => 6,
                (uint)OID.SecretEgg => 5,
                (uint)OID.SecretGarlic => 4,
                (uint)OID.SecretTomato => 3,
                (uint)OID.SecretQueen or (uint)OID.KeeperOfKeys => 2,
                (uint)OID.SerpentHatchling => 1,
                _ => 0
            };
        }
    }
}
