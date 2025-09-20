namespace BossMod.Shadowbringers.Foray.Duel.Duel3Sartauvoir;

sealed class Pyrolatry(BossModule module) : Components.RaidwideCast(module, (uint)AID.Pyrolatry);
sealed class Flashover(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flashover, 19f);
sealed class FlamingRain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlamingRain, 6f);
sealed class PillarOfFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarOfFlame, 8f);
sealed class TimeEruption(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TimeEruption1, (uint)AID.TimeEruption2], new AOEShapeRect(20f, 10f), 2, 4);
sealed class ThermalGust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermalGust, new AOEShapeRect(44f, 5f));
sealed class SearingWind(BossModule module) : Components.Voidzone(module, 3f, GetSearingWind)
{
    private static List<Actor> GetSearingWind(BossModule module) => module.Enemies((uint)OID.Peri);
}

sealed class Backdraft(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Backdraft, 16f, true)
{
    private static readonly Angle a45 = 45f.Degrees(), a90 = 90f.Degrees(), a225 = 22.5f.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var forbidden = new ShapeDistance[4];
            for (var i = 0; i < 4; ++i)
            {
                forbidden[i] = new SDInvertedCone(c.Origin, 5f, a45 + a90 * i, a225);
            }
            hints.AddForbiddenZone(new SDIntersection(forbidden), c.Activation);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 735, NameID = 12)]
public sealed class Duel3Sartauvoir(WorldState ws, Actor primary) : BossModule(ws, primary, new(-15f, 145f), new ArenaBoundsSquare(18f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InSquare(Arena.Center, 20f);
}