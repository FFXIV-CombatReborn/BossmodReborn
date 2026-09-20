namespace BossMod.Endwalker.Raid.P1NErichthonios;

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.Boss, Contributors = "Athena & Zettai", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 808u, NameID = 10576u, PlanLevel = 90)]
public sealed class P1N(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f))
{
    public const float InnerCircleRadius = 12f;

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Bounds is ArenaBoundsCircle)
        {
            var diagonal = Bounds.Radius / 1.414214f;
            Arena.ZoneCircleOutline(Center, InnerCircleRadius, Colors.Border);
            Arena.AddLine(Center + new WDir(Bounds.Radius, 0f), Center - new WDir(Bounds.Radius, 0f), Colors.Border);
            Arena.AddLine(Center + new WDir(0f, Bounds.Radius), Center - new WDir(0f, Bounds.Radius), Colors.Border);
            Arena.AddLine(Center + new WDir(diagonal, diagonal), Center - new WDir(diagonal, diagonal), Colors.Border);
            Arena.AddLine(Center + new WDir(diagonal, -diagonal), Center - new WDir(diagonal, -diagonal), Colors.Border);
        }
    }
}
