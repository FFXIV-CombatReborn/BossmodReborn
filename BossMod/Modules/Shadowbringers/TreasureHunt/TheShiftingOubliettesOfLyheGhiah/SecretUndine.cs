namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretUndine;

public enum OID : uint
{
    SecretUndine = 0x3011, //R=3.6
    AqueousAether = 0x3013, //R=1.12
    Bubble = 0x3012, //R=1.3, untargetable
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
    AutoAttack1 = 23186, // SecretUndine/AqueousAether->player, no cast, single-target
    AutoAttack2 = 872, // KeeperOfKeys/Mandragoras->player, no cast, single-target

    Hydrowhirl = 21658, // SecretUndine->self, 3.0s cast, range 8 circle
    Hypnowave = 21659, // SecretUndine->self, 3.0s cast, range 30 120-degree cone, causes sleep
    HydrotaphVisual = 21661, // SecretUndine->self, 4.0s cast, single-target
    Hydrotaph = 21662, // Helper->self, 4.0s cast, range 40 circle
    Hydrofan = 21663, // Bubble->self, 5.0s cast, range 44 30-degree cone
    Hydropins = 21660, // SecretUndine->self, 2.5s cast, range 12 width 4 rect
    AquaGlobe = 21664, // AqueousAether->location, 3.0s cast, range 8 circle

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768, // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
    Telega = 9630 // Mandragoras/KeeperOfKeys->self, no cast, single-target, bonus adds disappear
}

sealed class Hydrofan(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydrofan, new AOEShapeCone(44f, 15f.Degrees()));
sealed class Hypnowave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hypnowave, new AOEShapeCone(30f, 60f.Degrees()));
sealed class Hydropins(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydropins, new AOEShapeRect(12f, 2f));
sealed class AquaGlobeHydrowhirl(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AquaGlobe, (uint)AID.Hydrowhirl], 8f);
sealed class Hydrotaph(BossModule module) : Components.RaidwideCast(module, (uint)AID.Hydrotaph);

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

sealed class SecretUndineStates : StateMachineBuilder
{
    public SecretUndineStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hydrofan>()
            .ActivateOnEnter<Hypnowave>()
            .ActivateOnEnter<Hydropins>()
            .ActivateOnEnter<AquaGlobeHydrowhirl>()
            .ActivateOnEnter<Hydrotaph>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(SecretUndine.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(SecretUndineStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.SecretUndine,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Shadowbringers,
Category = BossModuleInfo.Category.TreasureHunt,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 745u,
NameID = 9790u,
SortOrder = 12,
PlanLevel = 0)]
public sealed class SecretUndine : THTemplate
{
    public SecretUndine(WorldState ws, Actor primary) : base(ws, primary)
    {
        aether = Enemies((uint)OID.AqueousAether);
    }
    private readonly List<Actor> aether;
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.SecretUndine, (uint)OID.AqueousAether, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(aether);
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
                (uint)OID.AqueousAether => 1,
                _ => 0
            };
        }
    }
}
