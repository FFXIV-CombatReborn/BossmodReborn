namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class Cloudsplitter(BossModule module) : Components.BaitAwayCast(module, (uint)AID.CloudsplitterAOE, 6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class CriticalReaverRaidwide(BossModule module) : Components.CastCounter(module, (uint)AID.CriticalReaverRaidwide);
sealed class CriticalReaverEnrage(BossModule module) : Components.CastInterruptHint(module, (uint)AID.CriticalReaverEnrage);
sealed class Meteor(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Meteor);
sealed class TachiGekko(BossModule module) : Components.CastGaze(module, (uint)AID.TachiGekko);
sealed class TachiKasha(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TachiKasha, 20f);
sealed class TachiYukikaze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TachiYukikaze, new AOEShapeRect(50f, 2.5f));
sealed class Raiton(BossModule module) : Components.RaidwideCast(module, (uint)AID.Raiton);
sealed class Utsusemi(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.Utsusemi, 10f, needToKite: true);

[ModuleInfo(BossModuleInfo.Maturity.AISupport, PrimaryActorOID = (uint)OID.BossGK, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13640, SortOrder = 7, PlanLevel = 100)]
public sealed class A13ArkAngels(WorldState ws, Actor primary) : BossModule(ws, primary, new(865f, -820f), new ArenaBoundsCircle(34.5f))
{
    public static readonly ArenaBoundsCircle DefaultBounds = new(25f);
    public static readonly uint[] Bosses = [(uint)OID.BossHM, (uint)OID.BossEV, (uint)OID.BossTT, (uint)OID.BossMR, (uint)OID.BossGK];

    private Actor? _bossHM;
    private Actor? _bossEV;
    private Actor? _bossMR;
    private Actor? _bossTT;
    private Actor? _shield;
    public Actor? BossHM() => _bossHM;
    public Actor? BossEV() => _bossEV;
    public Actor? BossMR() => _bossMR;
    public Actor? BossTT() => _bossTT;
    public Actor? BossGK() => PrimaryActor;

    protected override void UpdateModule()
    {
        _bossHM ??= GetActor((uint)OID.BossHM);
        _bossEV ??= GetActor((uint)OID.BossEV);
        _bossMR ??= GetActor((uint)OID.BossMR);
        _bossTT ??= GetActor((uint)OID.BossTT);
        _shield ??= GetActor((uint)OID.ArkShield);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (FindComponent<DecisiveBattle>() is DecisiveBattle comp && comp.AssignedBoss[pcSlot] is var slot && slot != null)
        {
            Arena.Actor(slot);
        }
        else if (!_shield?.IsDead ?? false)
        {
            Arena.Actor(_shield);
        }
        else
        {
            Arena.Actors(this, Bosses);
        }
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID == (uint)OID.BossHM)
            {
                if (!_shield?.IsDead ?? false)
                {
                    e.Priority = AIHints.Enemy.PriorityInvincible;
                }
                break;
            }
        }
    }
}
