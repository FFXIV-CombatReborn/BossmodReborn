namespace BossMod.Dawntrail.Alliance.A12Fafnir;

sealed class DarkMatterBlast(BossModule module) : Components.RaidwideCast(module, (uint)AID.DarkMatterBlast)
{
    public override bool KeepOnPhaseChange => true;
}

sealed class HurricaneWingRW(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.HurricaneWingRaidwide, (uint)AID.HurricaneWingRaidwideAOE1, 2.7d, "Raidwide x9")
{
    public override bool KeepOnPhaseChange => true;
}

sealed class PestilentSphere(BossModule module) : Components.SingleTargetCast(module, (uint)AID.PestilentSphere)
{
    public override bool KeepOnPhaseChange => true;
}

sealed class ShudderingEarth(BossModule module) : Components.CastCounter(module, (uint)AID.ShudderingEarth);

sealed class Darter(BossModule module) : Components.Adds(module, (uint)OID.Darter, 1)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class Venom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Venom, new AOEShapeCone(30f, 60f.Degrees()))
{
    public override bool KeepOnPhaseChange => true;
}

sealed class AbsoluteTerror(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbsoluteTerrorAOE, new AOEShapeRect(70f, 10f))
{
    public override bool KeepOnPhaseChange => true;
}

sealed class WingedTerror(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WingedTerrorAOE, new AOEShapeRect(70f, 12.5f))
{
    public override bool KeepOnPhaseChange => true;
}

sealed class BalefulBreath(BossModule module) : Components.LineStack(module, (uint)IconID.BalefulBreath, (uint)AID.BalefulBreathAOERest, 8.2d, 70f, 3f, PartyState.MaxAllianceSize, PartyState.MaxAllianceSize, 3, false)
{
    public override bool KeepOnPhaseChange => true;
}

sealed class SharpSpike(BossModule module) : Components.BaitAwayIcon(module, 4f, (uint)IconID.SharpSpike, (uint)AID.SharpSpikeAOE, 6.2d)
{
    public override bool KeepOnPhaseChange => true;
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13662, SortOrder = 4, PlanLevel = 100)]
public sealed class A12Fafnir(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsCircle(34.5f))
{
    public static readonly WPos ArenaCenter = new(-500f, 600f);
    public static readonly ArenaBoundsCircle DefaultBounds = new(30f);
    public static readonly ArenaBoundsCircle FireArena = new(16f);
}
