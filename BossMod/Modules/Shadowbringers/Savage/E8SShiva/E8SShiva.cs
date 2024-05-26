namespace BossMod.Shadowbringers.Savage.E8SShiva;

class AbsoluteZero(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AbsoluteZero));


[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 729, NameID = 9353)]
public class E8SShiva(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
