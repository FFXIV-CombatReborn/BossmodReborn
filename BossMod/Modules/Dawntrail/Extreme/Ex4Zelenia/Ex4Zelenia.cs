namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class ThornedCatharsisRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThornedCatharsis)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class PerfumedQuietusRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.PerfumedQuietus1)
{
    public override bool KeepOnPhaseChange => true;
}

sealed class SpecterOfTheLost(BossModule module) : Components.TankbusterTether(module, (uint)AID.SpecterOfTheLost, (uint)TetherID.SpecterOfTheLost, new AOEShapeCone(48f, 22.5f.Degrees()), 7.8d);
sealed class AlexandrianThunderIIISpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AlexandrianThunderIIISpread, 4f);
sealed class AlexandrianBanishII(BossModule module) : Components.StackWithIcon(module, (uint)IconID.AlexandrianBanishII, (uint)AID.AlexandrianBanishII, 4f, 5.8f, 4, 4);
sealed class AlexandrianThunderIIIAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AlexandrianThunderIIIAOE, 4f);
sealed class HolyHazard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyHazard, new AOEShapeCone(24f, 60f.Degrees()), 2);
sealed class PowerBreak(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PowerBreak1, (uint)AID.PowerBreak2], new AOEShapeRect(24f, 32f));
sealed class ThunderSlash : Components.SimpleAOEs
{
    public ThunderSlash(BossModule module) : base(module, (uint)AID.ThunderSlash, new AOEShapeCone(24f, 30f.Degrees()), 2)
    {
        MaxDangerColor = 2;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1031, NameID = 13861, PlanLevel = 100)]
public sealed class Ex4Zelenia(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, DefaultArena)
{
    private static readonly WPos arenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsCustom DefaultArena = new([new Polygon(arenaCenter, 16f, 64)]);
    public static readonly ArenaBoundsCustom DonutArena = new([new DonutV(arenaCenter, 2f, 16f, 64)]);
}
