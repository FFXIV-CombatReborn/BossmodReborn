namespace BossMod.Endwalker.VariantCriterion.C03AAI.C033Statice;

abstract class SurpriseBalloon(BossModule module, uint aid) : Components.SimpleKnockbacks(module, aid, 13f);
class NSurpriseBalloon(BossModule module) : SurpriseBalloon(module, (uint)AID.NPop);
class SSurpriseBalloon(BossModule module) : SurpriseBalloon(module, (uint)AID.SPop);

class BeguilingGlitter(BossModule module) : Components.StatusDrivenForcedMarch(module, 2f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 8);

abstract class FaerieRing(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeDonut(6f, 12f)); // TODO: verify inner radius
class NFaerieRing(BossModule module) : FaerieRing(module, (uint)AID.NFaerieRing);
class SFaerieRing(BossModule module) : FaerieRing(module, (uint)AID.SFaerieRing);

public abstract class C033Statice(WorldState ws, Actor primary) : BossModule(ws, primary, new(-200f, default), new ArenaBoundsCircle(20f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12506, SortOrder = 9, PlanLevel = 90, ConfigType = typeof(C033SStaticeConfig))]
public class C033NStatice(WorldState ws, Actor primary) : C033Statice(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12506, SortOrder = 9, PlanLevel = 90)]
public class C033SStatice(WorldState ws, Actor primary) : C033Statice(ws, primary);
