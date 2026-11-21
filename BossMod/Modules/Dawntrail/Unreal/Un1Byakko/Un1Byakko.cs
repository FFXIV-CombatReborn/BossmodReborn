namespace BossMod.Dawntrail.Unreal.Un1Byakko;

sealed class StormPulseRepeat(BossModule module) : Components.CastCounter(module, (uint)AID.StormPulseRepeat);
sealed class HeavenlyStrike(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HeavenlyStrike, 3f);
sealed class FireAndLightningBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireAndLightningBoss, new AOEShapeRect(54.3f, 10f));
sealed class FireAndLightningAdd(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireAndLightningAdd, new AOEShapeRect(54.75f, 10f));
sealed class SteelClaw(BossModule module) : Components.Cleave(module, (uint)AID.SteelClaw, new AOEShapeCone(17.75f, 60f.Degrees()), [(uint)OID.Hakutei]);
sealed class WhiteHerald(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.WhiteHerald, (uint)AID.WhiteHerald, 15f, 5.1f); // TODO: verify falloff
sealed class DistantClap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DistantClap, new AOEShapeDonut(4f, 25f));
sealed class SweepTheLegBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SweepTheLegBoss, new AOEShapeCone(28.3f, 135f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "Malediktus, ported from upstream", GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 1007u, NameID = 7092u, PlanLevel = 100)]
public sealed class Un1Byakko : BossModule
{
    public Un1Byakko(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private Un1Byakko(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(default, 19.5f, 48)]);
        return (arena.Center, arena);
    }

    public static ArenaBoundsCustom GetIntermissionBounds() => new([new Polygon(default, 15f, 48)]);

    private Actor? _hakutei;
    public Actor? Boss() => PrimaryActor;
    public Actor? Hakutei() => _hakutei;

    protected override void UpdateModule()
    {
        _hakutei ??= GetActor((uint)OID.Hakutei);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_hakutei);
    }
}
