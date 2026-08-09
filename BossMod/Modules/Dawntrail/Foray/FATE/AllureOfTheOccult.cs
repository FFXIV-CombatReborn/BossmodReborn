namespace BossMod.Dawntrail.Foray.FATE.AllureOfTheOccult;

public enum OID : uint {
    SensualSandy = 0x4D56,
    Helper = 0x233C,
    PoisonCloud = 0x4D57, // R1.700, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 50535, // SensualSandy->player, no cast, single-target
    PutridBreath = 48944, // SensualSandy->self, 5.0s cast, range 25 130.000-degree cone
    PutridBreath1 = 48952, // SensualSandy->self, 3.0s cast, range 25 130.000-degree cone
    WildWildBreath = 48945, // SensualSandy->self, 5.0s cast, range 30 width 6 cross
    WildWildWildWildWildBreath = 48946, // SensualSandy->self, 5.0s cast, range 30 width 6 cross
    ExtensibleTendrils = 48947, // SensualSandy->self, 3.0s cast, range 30 width 6 cross
    PoisonPassel = 48951, // SensualSandy->self, 3.0s cast, single-target
    Burst = 48950, // 4D57->self, 5.0s cast, range 10 circle
}

sealed class PutridBreath(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PutridBreath, (uint)AID.PutridBreath1],
    new AOEShapeCone(25.0f, 65.0f.Degrees()));
sealed class WildWildBreath(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.WildWildBreath, (uint)AID.WildWildWildWildWildBreath, (uint)AID.ExtensibleTendrils], new AOEShapeCross(30.0f, 3.0f));
// Crosses resolve before the poison clouds. Keep the later circles visible, but do not make them
// steer AI until the cross has gone off; avoiding both future snapshots at once makes navigation
// take a huge detour outside the encounter instead of performing the intended two short steps.
sealed class Burst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burst, new AOEShapeCircle(10.0f))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = base.ActiveAOEs(slot, actor);
        var crossComponent = Module.FindComponent<WildWildBreath>();
        if (crossComponent == null)
            return aoes;
        var crosses = crossComponent.ActiveCasters;
        if (crosses.Length == 0)
            return aoes;

        var crossActivation = crosses[0].Activation;
        for (var i = 0; i < aoes.Length; ++i)
        {
            var aoe = Casters[i];
            if (crossActivation < aoe.Activation)
            {
                aoe.Risky = false;
                aoe.Color = Colors.AOE;
                Casters[i] = aoe;
            }
        }
        return ActiveCasters;
    }
}

[SkipLocalsInit]
sealed class AllureOfTheOccultStates : StateMachineBuilder {
    public AllureOfTheOccultStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<PutridBreath>()
            .ActivateOnEnter<WildWildBreath>()
            .ActivateOnEnter<Burst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(AllureOfTheOccultStates),
    ConfigType = null, // replace null with typeof(SensualSandyConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.SensualSandy,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2078u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class AllureOfTheOccult(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
