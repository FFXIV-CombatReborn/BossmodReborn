namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

sealed class HeatBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeatBurst);
sealed class ParisCurseRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.ParisCurse);
sealed class SpurningFlames(BossModule module) : Components.RaidwideCast(module, (uint)AID.SpurningFlames);
sealed class SpurningFlamesPreEnrage(BossModule module) : Components.RaidwideCast(module, (uint)AID.SpurningFlamesPreEnrage);
sealed class ScouringScorn(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScouringScorn);
sealed class FireOfVictory(BossModule module) : Components.BaitAwayCast(module, (uint)AID.FireOfVictory, 4f, true, true, true, AIHints.PredictedDamageType.Tankbuster);
sealed class SunCirclet(BossModule module) : Components.CastCounter(module, (uint)AID.SunCirclet);
sealed class KindledFlameStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, (uint)AID.KindledFlame2, 6f, 5.2d);

sealed class FireflightRectStart(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FireflightByEmberlightRight, (uint)AID.FireflightByEmberlightLeft,
    (uint)AID.FireflightByPyrelightRight, (uint)AID.FireflightByPyrelightLeft], RectStart.Shape);
sealed class WheelRectStart(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WheelOfFableflightCW, (uint)AID.WheelOfFableflightCCW], RectStart.Shape);
sealed class LongNightRectStart(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FireflightFourLongNightsE, (uint)AID.FireflightFourLongNightsW], RectStart.Shape);
sealed class FableflightLongRectStart(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FableflightLeftVerySlow, (uint)AID.FableflightRightVerySlow], RectStart.Shape);
sealed class FableflightRectStart(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FableflightLeftFast, (uint)AID.FableflightRightFast,
    (uint)AID.FableflightRightSlow, (uint)AID.FableflightLeftSlow], RectStart.Shape);
sealed class CharmdFlightRect(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CharmdFlightFourNightsW, (uint)AID.CharmdFlightFourNightsE], RectStart.Shape);

static class RectStart
{
    public static readonly AOEShapeRect Shape = new(40f, 2f);
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.PariOfPlenty, Contributors = "Equilius, xan, LTS", Category = BossModuleInfo.Category.VariantCriterion, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1079u, NameID = 14274u, PlanLevel = 100)]
public sealed class C013PariOfPlenty(WorldState ws, Actor primary) : BossModule(ws, primary, new(-760f, -805f), new ArenaBoundsSquare(20f));
