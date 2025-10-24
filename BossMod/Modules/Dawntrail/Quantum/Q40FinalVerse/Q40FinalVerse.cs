namespace BossMod.Dawntrail.Quantum.FinalVerse.Q40EminentGrief;

[SkipLocalsInit]
sealed class TerrorEyeVoidTrapBallOfFire(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TerrorEye, (uint)AID.BallOfFire, (uint)AID.VoidTrap], 6f);

[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(Q40EminentGriefStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = typeof(TetherID),
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.EminentGrief,
Contributors = "",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.DeepDungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1063u,
NameID = 14037u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Q40EminentGrief : BossModule
{
    public Q40EminentGrief(WorldState ws, Actor primary) : base(ws, primary, ArenaCenter, new ArenaBoundsCustom([new Rectangle(ArenaCenter, 20f, 15f)], AdjustForHitboxOutwards: true))
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

    protected override void UpdateModule()
    {
        BossEater ??= GetActor((uint)OID.DevouredEater);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(vodorigas);
        Arena.Actors(bloodguards);
        Arena.Actors(fonts);
    }
}
