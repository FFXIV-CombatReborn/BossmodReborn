namespace BossMod.Shadowbringers.Raid.E11NFatebreaker;

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.Boss, Contributors = "Athena & Zettai", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 751u, NameID = 9707u, PlanLevel = 80)]
public sealed class E11N(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));
