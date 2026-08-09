namespace BossMod.Dawntrail.Foray.FATE.CrescentGremlin;

public enum OID : uint {
    CrescentGremlin = 0x4D8A, // 新月格雷姆林 (main target of FATE 2072)
    CrimsonGremlin = 0x4D8B, // 绯红格雷姆林 (spawn during fight, casts the AOE skills)
    Helper = 0x233C,
    Pot = 0x47CB, // 撒娇罐 (protected NPC)
}

public enum AID : uint {
    AutoAttack = 40542, // CrescentGremlin->player, no cast, single-target
    ViciousBite = 50224, // 臭嘴, CrescentGremlin->player, no cast, single-target (no telegraph)
    CrudeTaunt = 50225, // 粗话拱火, CrimsonGremlin->self, 2.7s cast, range 25 width 6 rect
    NonsenseTaunt = 50226, // 胡话拱火, CrimsonGremlin->location, 2.7s cast, range 5 circle
}

sealed class CrudeTaunt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrudeTaunt, new AOEShapeRect(25.0f, 3.0f));
sealed class NonsenseTaunt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NonsenseTaunt, 5f);

[SkipLocalsInit]
sealed class CrescentGremlinStates : StateMachineBuilder {
    public CrescentGremlinStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<CrudeTaunt>()
            .ActivateOnEnter<NonsenseTaunt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(CrescentGremlinStates),
    ConfigType = null, // replace null with typeof(CrescentGremlinConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.CrescentGremlin,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2072u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CrescentGremlin(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
