namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

sealed class Teleport(BossModule module) : Components.CastCounter(module, (uint)AID.Teleport);
sealed class TeraSlash(BossModule module) : Components.CastCounter(module, (uint)AID.TeraSlash);
sealed class DoomArc(BossModule module) : Components.RaidwideCast(module, (uint)AID.DoomArc);
sealed class UnbridledRage(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(100f, 4f), (uint)IconID.UnbridledRage, (uint)AID.UnbridledRageAOE, 5.9d);
sealed class DarkNova(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DarkNova, 6f);

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13653, SortOrder = 8, PlanLevel = 100)]
public sealed class A14ShadowLord(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    private const int RadiusSmall = 8;
    private const int HalfWidth = 2;
    private const int Edges = 64;
    public static readonly WPos ArenaCenter = new(150f, 800f);
    public static readonly ArenaBoundsCircle DefaultBounds = new(30f);
    public static readonly Polygon[] Circles = [new(new(166.251f, 800f), RadiusSmall, Edges), new(new(133.788f, 800f), RadiusSmall, Edges),
    new(new(150f, 816.227f), RadiusSmall, Edges), new(new(150f, 783.812f), RadiusSmall, Edges)]; // the circle coordinates are not perfectly placed for some reason, got these from analyzing the collision data
    private static readonly RectangleSE[] rects = [new(Circles[1].Center, Circles[2].Center, HalfWidth), new(Circles[1].Center, Circles[3].Center, HalfWidth),
    new(Circles[3].Center, Circles[0].Center, HalfWidth), new(Circles[0].Center, Circles[2].Center, HalfWidth)];
    public static readonly Shape[] Combined = [.. Circles, .. rects];
    public static readonly ArenaBoundsCustom ComplexBounds = new(Combined);
}
