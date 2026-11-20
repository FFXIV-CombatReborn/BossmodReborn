namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

sealed class TerrorEyeVoidTrapBallOfFire(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TerrorEye, (uint)AID.BallOfFire, (uint)AID.VoidTrap], 6f);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.EminentGrief, Contributors = "LTS, with ported work from upstream", Category = BossModuleInfo.Category.Quantum, GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1063u, NameID = 14037u, SortOrder = 1, PlanLevel = 0)]
public sealed class Q1FinalVerse : BossModule
{
    public Q1FinalVerse(WorldState ws, Actor primary) : base(ws, primary, ArenaCenter, new ArenaBoundsCustom([new Rectangle(ArenaCenter, 20f, 15f)], AdjustForHitboxOutwards: true))
    {
        ActivateComponent<LightAndDark>();
        FindComponent<LightAndDark>()!.AddAOE();
        vodorigas = Enemies((uint)OID.VodorigaMinion);
        bloodguards = Enemies((uint)OID.BloodguardMinion);
        fonts = Enemies((uint)OID.ArcaneFont);
    }

    public static readonly WPos ArenaCenter = new(-600f, -300f);
    public Actor? BossEater;
    private readonly List<Actor> vodorigas;
    private readonly List<Actor> bloodguards;
    private readonly List<Actor> fonts;

    protected override bool CheckPull()
    {
        BossEater ??= GetActor((uint)OID.DevouredEater);
        return base.CheckPull();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(vodorigas);
        Arena.Actors(bloodguards);
        Arena.Actors(fonts);
    }
}
