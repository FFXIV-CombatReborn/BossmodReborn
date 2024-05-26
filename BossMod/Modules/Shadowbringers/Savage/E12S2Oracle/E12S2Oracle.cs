namespace BossMod.Shadowbringers.Savage.E12S2Oracle;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 759, NameID = 9832)]
public class E12S2Oracle(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -75), new ArenaBoundsCircle(20));
