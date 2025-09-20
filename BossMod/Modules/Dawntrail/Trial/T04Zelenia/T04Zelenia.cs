namespace BossMod.Dawntrail.Trial.T04Zelenia;

sealed class PowerBreak(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PowerBreak1, (uint)AID.PowerBreak2], new AOEShapeRect(24f, 32f));

sealed class HolyHazard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyHazard, new AOEShapeCone(24f, 60f.Degrees()), 2);

sealed class RosebloodBloom(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RosebloodBloom, 10f, true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDInvertedCircle(c.Origin, 6f), c.Activation);
        }
    }
}

sealed class ThunderSlash : Components.SimpleAOEs
{
    public ThunderSlash(BossModule module) : base(module, (uint)AID.ThunderSlash, new AOEShapeCone(24f, 30f.Degrees()), 4)
    {
        MaxDangerColor = 2;
    }
}

sealed class PerfumedQuietus(BossModule module) : Components.RaidwideCast(module, (uint)AID.RosebloodBloom); // using the knockback here, since after knockback player is stunned for a cutscene and can't heal up
sealed class ThornedCatharsis(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThornedCatharsis);
sealed class SpecterOfTheLost(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SpecterOfTheLost, new AOEShapeCone(50f, 22.5f.Degrees()), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1030, NameID = 13861)]
public sealed class T04Zelenia(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, DefaultArena)
{
    private static readonly WPos arenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsCustom DefaultArena = new([new Polygon(arenaCenter, 16f, 64)]);
    public static readonly ArenaBoundsCustom DonutArena = new([new DonutV(arenaCenter, 4f, 16f, 64)]);
}
