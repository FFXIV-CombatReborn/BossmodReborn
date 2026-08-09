namespace BossMod.Dawntrail.Foray.FATE.CrescentFan;

public enum OID : uint {
    CrescentFan = 0x4D8D, // 新月风扇 (main target of FATE 2073)
    BigFan = 0x4D8E, // 大风扇 (spawn during fight)
    Helper = 0x233C,
    Pot = 0x4D8C, // 撒娇罐 (protected NPC, shared pot OID 0x47CB also used)
}

public enum AID : uint {
    AutoAttack = 40542, // CrescentFan/BigFan->player, no cast, single-target
    HighPressureTornado = 50221, // 高压龙卷, CrescentFan->self, 2.7s cast, range 15 width 4 rect
    HighPressureTornadoBig = 50222, // 高压龙卷, BigFan->self, 2.7s cast, range 15 width 4 rect
    Tempest = 50223, // 暴风, BigFan->self, 5.7s cast, raidwide (whole arena, no avoid)
}

sealed class HighPressureTornado(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HighPressureTornado, (uint)AID.HighPressureTornadoBig], new AOEShapeRect(15.0f, 2.0f));
sealed class Tempest(BossModule module) : Components.RaidwideCast(module, (uint)AID.Tempest);

[SkipLocalsInit]
sealed class CrescentFanStates : StateMachineBuilder {
    public CrescentFanStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<HighPressureTornado>()
            .ActivateOnEnter<Tempest>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(CrescentFanStates),
    ConfigType = null, // replace null with typeof(CrescentFanConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.CrescentFan,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2073u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CrescentFan(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
