namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class Flare(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Flare, (uint)AID.FlareAOE, 25f, 8.1d);
sealed class StygianShadow(BossModule module) : Components.Adds(module, (uint)OID.StygianShadow);
sealed class Atomos(BossModule module) : Components.Adds(module, (uint)OID.Atomos);
sealed class GhastlyGloomCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GhastlyGloomCrossAOE, new AOEShapeCross(40f, 15f));
sealed class GhastlyGloomDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GhastlyGloomDonutAOE, new AOEShapeDonut(21f, 40f));
sealed class FloodOfDarknessAdd(BossModule module) : Components.CastInterruptHint(module, (uint)AID.FloodOfDarknessAdd); // TODO: only if add is player's?..
sealed class Excruciate(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Excruciate, 4f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class LoomingChaos(BossModule module) : Components.CastCounter(module, (uint)AID.LoomingChaosAOE);
sealed class Phaser(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.Phaser], new AOEShapeCone(23f, 30f.Degrees()));

// TODO: tankswap hints component for phase1
// TODO: phase 2 teleport zones?
// TODO: grim embrace / curse of darkness prevent turning

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1010, NameID = 13624, PlanLevel = 100)]
public sealed class Ch01CloudOfDarkness(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultCenter, DefaultArena)
{
    public static readonly WPos DefaultCenter = new(100f, 100f);
    public static readonly WPos Phase1BoundsCenter = new(100f, 76.28427f);
    public static readonly PolygonCustom[] Diamond = [new([new(115f, 63f), new(128.28427f, 76.28427f), new(100f, 104.56854f), new(71.71573f, 76.28427f), new(85f, 63f)])];
    private static readonly DonutV[] donut = [new(DefaultCenter, 34f, 40f, 80)];
    public static readonly Square[] IntersectionBlockers = [.. GenerateIntersectionBlockers()];
    public static readonly Shape[] Phase2ShapesND = [new Rectangle(new(100f, 115f), 24f, 3f), new Rectangle(new(100f, 85f), 24f, 3f), new Rectangle(new(115f, 100f), 3f, 24f),
    new Rectangle(new(85f, 100f), 3f, 24f), new Square(new(126.5f, 100f), 7.5f), new Square(new(73.5f, 100f), 7.5f)];
    public static readonly Shape[] Phase2ShapesWD = [.. donut, .. Phase2ShapesND];
    public static readonly ArenaBoundsCircle DefaultArena = new(40f);
    public static readonly ArenaBoundsCustom Phase1Bounds = new(Diamond, ScaleFactor: 1.414f);
    public static readonly ArenaBoundsCustom Phase2BoundsWD = new(Phase2ShapesWD, IntersectionBlockers);
    public static readonly ArenaBoundsCustom Phase2BoundsND = new(Phase2ShapesND, [.. IntersectionBlockers, .. donut]);

    private static Square[] GenerateIntersectionBlockers() // at intersections there are small blockers to prevent players from skipping tiles
    {
        var a45 = 45f.Degrees();
        var a135 = 135f.Degrees();
        WDir[] dirs = [a45.ToDirection(), a135.ToDirection(), (-a45).ToDirection(), (-a135).ToDirection()];
        WPos[] pos = [new(85f, 85f), new(115f, 85f), new(115f, 115f), new(85f, 115f)];
        var distance = 3f * MathF.Sqrt(2);

        var squares = new Square[16];
        var index = 0;
        for (var i = 0; i < 4; ++i)
            for (var j = 0; j < 4; ++j)
                squares[index++] = new(pos[i] + distance * dirs[j], 1f, a45);
        return squares;
    }
}
