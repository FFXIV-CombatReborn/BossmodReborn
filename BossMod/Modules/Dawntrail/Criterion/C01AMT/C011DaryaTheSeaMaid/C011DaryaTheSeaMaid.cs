namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

sealed class PiercingPlunge(BossModule module) : Components.RaidwideCast(module, (uint)AID.PiercingPlunge);
sealed class Hydrobullet(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Hydrobullet1, 15f);

sealed class SurgingCurrent(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurgingCurrent1, new AOEShapeCone(60f, 45f.Degrees()), 2)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == WatchedAction && Casters.Count > 1)
        {
            Casters.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
    }
}

sealed class AquaBall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AquaBall1, 5f)
{
    public int NumBaits;
    private DateTime _lastStarted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            var now = WorldState.CurrentTime;
            if (_lastStarted.AddSeconds(1d) < now)
            {
                ++NumBaits;
            }
            _lastStarted = now;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.DaryaTheSeaMaid, Contributors = "Equilius, xan, LTS", Category = BossModuleInfo.Category.VariantCriterion, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1079u, NameID = 14291u, PlanLevel = 100)]
public sealed class C011DaryaTheSeaMaid(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(20f))
{
    public static readonly WPos ArenaCenter = new(375f, 530f);

    public static WPos TileCenter(WPos position) => ArenaCenter + ((position - ArenaCenter) / 8f).Rounded() * 8f;
}
