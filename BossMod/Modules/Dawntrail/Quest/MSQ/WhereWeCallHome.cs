namespace BossMod.Dawntrail.Quest.MSQ.WhereWeCallHome;

public enum OID : uint
{
    Boss = 0x4B4D, // R8.7
    Thunderhead = 0x4B4F, // R1.3
    BallLightning = 0x4B4E, // R1.5
    Spriggan = 0x4B52,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 45128, // Boss->ally, no cast, single-target
    StormingRoar = 46834, // Boss->self, 5.0s cast, range 40 circle
    StormChase1 = 46825, // Boss->self, 6.0s cast, single-target
    StormChase2 = 46826, // Boss->self, 8.0s cast, single-target
    HyperconductivePlasmaVisual = 47528, // Boss->self, 5.0s cast, single-target
    HyperconductivePlasma = 46833, // Helper->players, 6.0s cast, range 6 circle, stack
    ElectrostaticTrouncing = 46829, // Boss->self, 11.0s cast, range 40 135-degree cone
    TrounceFast = 46830, // Helper->self, 5.0s cast, range 20 45-degree cone
    TrounceSlow = 46831, // Helper->self, 11.0s cast, range 20 45-degree cone
    Thundercloud1 = 46814, // Boss->self, 3.0s cast, single-target
    Thundercloud2 = 46815, // Boss->self, no cast, single-target
    Thundercloud3 = 46816, // Boss->self, 23.0s cast, single-target
    LightningBoltCloudInstant = 46817, // Thunderhead->self, 1.0s cast, range 8 circle
    LightningBoltCloud = 46818, // Thunderhead->self, 4.0s cast, range 8 circle
    LightningBoltSlow = 46827, // Helper->location, 8.5s cast, range 6 circle
    LightningBoltFast = 46828, // Helper->location, 6.5s cast, range 6 circle
    LightningBoltSpread = 46832, // Helper->player/allies, 5.0s cast, range 6 circle
    StormingRoarStun = 46821, // Boss->self, 3.0s cast, range 40 circle
    Shock = 46824, // BallLightning->self, no cast, range 8 circle
    EclipticMeteor = 46822 // Boss->self, 57.0s cast, single-target, hide behind the spriggan
}

sealed class StormingRoar(BossModule module) : Components.RaidwideCast(module, (uint)AID.StormingRoar);
sealed class LightningBoltFast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBoltFast, 6f, 11);
sealed class LightningBoltSlow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBoltSlow, 6f, 18);
sealed class LightningBoltCloud(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBoltCloud, 8f);
sealed class HyperconductivePlasma(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.HyperconductivePlasma, 6f, 5);
sealed class ElectrostaticTrouncing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectrostaticTrouncing, new AOEShapeCone(40f, 67.5f.Degrees()));
sealed class TrounceCone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TrounceFast, (uint)AID.TrounceSlow], new AOEShapeCone(20f, 22.5f.Degrees()), 4);
sealed class LightningBoltSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.LightningBoltSpread, 6f);
sealed class BallLightning(BossModule module) : Components.Adds(module, (uint)OID.BallLightning, 1, true);

sealed class EclipticMeteor(BossModule module) : Components.GenericLineOfSightAOE(module, (uint)AID.EclipticMeteor, 500f)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Spriggan)
        {
            var boss = Module.PrimaryActor;
            var activation = boss.CastInfo is ActorCastInfo cast ? Module.CastFinishAt(cast) : default;
            Modify(boss.Position, [(actor.Position, actor.HitboxRadius)], activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.EclipticMeteor)
        {
            Modify(null, []);
        }
    }
}

sealed class GreaterBehemothStates : StateMachineBuilder
{
    public GreaterBehemothStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StormingRoar>()
            .ActivateOnEnter<LightningBoltFast>()
            .ActivateOnEnter<LightningBoltSlow>()
            .ActivateOnEnter<LightningBoltCloud>()
            .ActivateOnEnter<HyperconductivePlasma>()
            .ActivateOnEnter<ElectrostaticTrouncing>()
            .ActivateOnEnter<TrounceCone>()
            .ActivateOnEnter<LightningBoltSpread>()
            .ActivateOnEnter<BallLightning>()
            .ActivateOnEnter<EclipticMeteor>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Ported from upstream and improved by LTS", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70969u, NameID = 14360u)]
public sealed class GreaterBehemoth(WorldState ws, Actor primary) : BossModule(ws, primary, new(121f, -428f), new ArenaBoundsCircle(20f));
