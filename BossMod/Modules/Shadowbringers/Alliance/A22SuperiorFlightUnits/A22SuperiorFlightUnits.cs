namespace BossMod.Shadowbringers.Alliance.A22SuperiorFlightUnits;

sealed class StandardSurfaceMissile : Components.SimpleAOEGroups
{
    public StandardSurfaceMissile(BossModule module) : base(module, [(uint)AID.StandardSurfaceMissile1, (uint)AID.StandardSurfaceMissile2], 10f)
    {
        MaxDangerColor = 3;
    }
}

sealed class LethalRevolution(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LethalRevolution, 15f);
sealed class GuidedMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GuidedMissile, 4f);
sealed class SurfaceMissileHighOrderExplosiveBlastCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SurfaceMissile, (uint)AID.HighOrderExplosiveBlastCircle], 6f);
sealed class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AntiPersonnelMissile, 6f);
sealed class PrecisionGuidedMissile(BossModule module) : Components.BaitAwayCast(module, (uint)AID.PrecisionGuidedMissile, 6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class ManeuverHighPoweredLaser(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.ManeuverHighPoweredLaserMarker, (uint)AID.ManeuverHighPoweredLaser, 5.3d, 80f, 7f, 8, 8);
sealed class ManeuverMissileCommand(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.ManeuverMissileCommand, (uint)AID.BarrageImpact, 2.1d, "Raidwide x3");

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.FlightUnitALpha, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9364, SortOrder = 3)]
public sealed class A22SuperiorFlightUnits(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaShape.Center, ArenaShape)
{
    public static readonly ArenaBoundsCustom ArenaShape = new([new PolygonCustom([new(-210.188f, -141.012f), new(-209.278f, -141.081f), new(-207.235f, -141.234f),
    new(-204.348f, -141.893f), new(-201.592f, -142.975f), new(-199.028f, -144.455f), new(-196.712f, -146.302f), new(-194.698f, -148.472f),
    new(-193.438f, -150.321f), new(-192.889f, -151.212f), new(-191.745f, -153.586f), new(-190.873f, -156.416f), new(-190.431f, -159.344f),
    new(-190.431f, -162.305f), new(-190.873f, -165.233f), new(-191.745f, -168.063f), new(-192.48f, -169.609f), new(-193.03f, -170.731f),
    new(-212.609f, -204.642f), new(-213.172f, -205.53f), new(-214.51f, -207.493f), new(-216.524f, -209.663f), new(-218.839f, -211.51f),
    new(-221.404f, -212.99f), new(-224.16f, -214.072f), new(-226.866f, -214.685f), new(-229.408f, -214.9f), new(-230.589f, -214.9f),
    new(-233.134f, -214.685f), new(-235.84f, -214.072f), new(-238.596f, -212.99f), new(-241.161f, -211.51f), new(-243.476f, -209.663f),
    new(-245.49f, -207.493f), new(-246.779f, -205.607f), new(-247.293f, -204.811f), new(-266.97f, -170.731f), new(-267.502f, -169.638f),
    new(-268.255f, -168.063f), new(-269.127f, -165.233f), new(-269.569f, -162.305f), new(-269.569f, -159.344f), new(-269.127f, -156.416f),
    new(-268.255f, -153.586f), new(-267.149f, -151.245f), new(-266.611f, -150.343f), new(-265.093f, -148.25f), new(-263.288f, -146.301f),
    new(-260.973f, -144.455f), new(-258.408f, -142.974f), new(-255.652f, -141.893f), new(-252.765f, -141.234f), new(-250.75f, -141.083f),
    new(-249.812f, -141.013f)])], AdjustForHitbox: true);

    public Actor? BossBeta;
    public Actor? BossChi;

    protected override void UpdateModule()
    {
        BossBeta ??= GetActor((uint)OID.FlightUnitBEta);
        BossChi ??= GetActor((uint)OID.FlightUnitCHi);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(BossBeta);
        Arena.Actor(BossChi);
    }
}
