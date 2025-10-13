namespace BossMod.Components;

[SkipLocalsInit]
abstract class GenericInvincible(BossModule module, string hint = "Attacking invincible target!", int priority = AIHints.Enemy.PriorityInvincible) : BossComponent(module)
{
    public bool EnableHints = true;

    protected abstract ReadOnlySpan<Actor> ForbiddenTargets(int slot, Actor actor);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnableHints)
        {
            var targets = ForbiddenTargets(slot, actor);
            var len = targets.Length;
            for (var i = 0; i < len; ++i)
            {
                if (targets[i].InstanceID == actor.TargetID)
                {
                    hints.Add(hint);
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var targets = ForbiddenTargets(slot, actor);
        var len = targets.Length;
        for (var i = 0; i < len; ++i)
        {
            hints.SetPriority(targets[i], priority);
        }
    }
}

[SkipLocalsInit]
class InvincibleStatus(BossModule module, uint statusId, string hint = "Attacking invincible target!") : GenericInvincible(module, hint)
{
    protected readonly List<Actor> _actors = [];

    protected override ReadOnlySpan<Actor> ForbiddenTargets(int slot, Actor actor) => CollectionsMarshal.AsSpan(_actors);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == statusId && !_actors.Contains(actor))
        {
            _actors.Add(actor);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == statusId)
        {
            _actors.Remove(actor);
        }
    }
}

[SkipLocalsInit]
class HPThreshold(BossModule module, uint oid, float ratio) : BossComponent(module)
{
    public readonly List<Actor> actors = module.Enemies(oid);
    public float Ratio = ratio;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = actors.Count;
        for (var i = 0; i < count; ++i)
        {
            var enemy = actors[i];
            if (enemy.PendingHPRatio < Ratio)
            {
                hints.SetPriority(enemy, AIHints.Enemy.PriorityPointless);
            }
        }
    }
}
