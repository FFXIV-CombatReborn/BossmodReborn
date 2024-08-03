namespace BossMod.Dawntrail.Savage.M1SBlackCat;

public class DoubleSwipe(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DoubleSwipeAOE), 5.0f, 4, 4);
public class QuadrupleSwipe(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.QuadrupleSwipeAOE), 4.0f, 2, 2);
public class BloodyScratch(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BloodyScratch));
public class Shockwave(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Shockwave), 21, stopAfterWall: true);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "Elec", GroupType = BossModuleInfo.GroupType.CFC, NameID = 12686)]
public class M1SBlackCat(WorldState ws, Actor primary) : BossModule(ws, primary,
    ArenaChanges.ArenaCenter,
    ArenaChanges.DefaultBounds);
