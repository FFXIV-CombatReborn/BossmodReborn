namespace BossMod.Shadowbringers.Savage.E12S1EdensPromise;

class Maleficium(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Maleficium));
class IceFloe(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.IceFloe), 6, 4);
class IcicleImpact(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.IcicleImpact), 10);
class JudgmentJolt(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.JudgmentJolt), new AOEShapeCircle(10));
class PulseOfTheLand(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PulseOfTheLand), new AOEShapeCircle(10));
class TemporaryCurrent(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TemporaryCurrent), new AOEShapeRect(40, 40, -7));
class RapturousReach1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RapturousReach1), new AOEShapeCone(40, 112.5f.Degrees()));
class RapturousReach2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RapturousReach2), new AOEShapeCone(40, 112.5f.Degrees()));

class Ferostorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Ferostorm), new AOEShapeCone(40, 30.Degrees()));
class ConflagStrike(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ConflagStrike), new AOEShapeCone(40, 75.Degrees()));
class PlungingIce(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.PlungingIce), 30, stopAtWall: true);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 759, NameID = 9813)]
public class E12S1EdensPromise(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -75), new ArenaBoundsCircle(20));
