namespace BossMod.QuestBattle;

public abstract class UnmanagedRotation(WorldState ws, float effectiveRange)
{
    protected AIHints Hints = null!;
    protected Actor Player = null!;
    protected WorldState World => ws;
    protected uint MP;
    private static readonly ZoneModuleConfig config = Service.Config.Get<ZoneModuleConfig>();

    protected Roleplay.AID ComboAction => (Roleplay.AID)World.Client.ComboState.Action;

    protected abstract void Exec(Actor? primaryTarget);

    public void Execute(Actor player, AIHints hints)
    {
        if (AI.AIManager.Instance?.Beh != null || Autorotation.MiscAI.NormalMovement.Instance != null || config.EnableQuestBattles)
        {
            Hints = hints;
            Player = player;

            MP = (uint)Player.PendingMPRaw;
            var count = Hints.PotentialTargets.Count;

            Actor? closestPriorityTarget = null;
            var minDistanceSq = float.MaxValue;
            var maxPriority = int.MinValue;

            for (var i = 0; i < count; ++i)
            {
                var target = Hints.PotentialTargets[i];
                var priority = target.Priority;
                if (priority < 0)
                    continue;
                var distanceSq = (target.Actor.Position - player.Position).LengthSq();
                if (priority > maxPriority || priority == maxPriority && distanceSq < minDistanceSq)
                {
                    maxPriority = priority;
                    minDistanceSq = distanceSq;
                    closestPriorityTarget = target.Actor;
                }
            }

            if (closestPriorityTarget != null)
            {
                Hints.ForcedTarget = closestPriorityTarget;
                Hints.GoalZones.Add(AIHints.GoalSingleTarget(closestPriorityTarget, effectiveRange));
            }
            Exec(closestPriorityTarget);
        }
    }

    protected void UseAction(Roleplay.AID action, Actor? target, float additionalPriority = default, Vector3 targetPos = default, Angle? facingAngle = null) => UseAction(ActionID.MakeSpell(action), target, additionalPriority, targetPos, facingAngle);
    protected void UseAction(ActionID action, Actor? target, float additionalPriority = default, Vector3 targetPos = default, Angle? facingAngle = null)
    {
        var def = ActionDefinitions.Instance[action];
        if (def == null)
            return;
        Hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.High + additionalPriority, castTime: def.CastTime - 0.5f, targetPos: targetPos, facingAngle: facingAngle);
    }

    protected float StatusDuration(DateTime expireAt) => Math.Max((float)(expireAt - World.CurrentTime).TotalSeconds, 0.0f);

    protected (float Left, int Stacks) StatusDetails(Actor? actor, uint sid, ulong sourceID, float pendingDuration = 1000f)
    {
        var status = actor?.FindStatus(sid, sourceID, World.FutureTime(pendingDuration));
        return status != null ? (StatusDuration(status.Value.ExpireAt), status.Value.Extra & 0xFF) : (0, 0);
    }
    protected (float Left, int Stacks) StatusDetails<SID>(Actor? actor, SID sid, ulong sourceID, float pendingDuration = 1000f) where SID : Enum => StatusDetails(actor, (uint)(object)sid, sourceID, pendingDuration);
}

public abstract class RotationModule<R>(BossModule module) : BossComponent(module) where R : UnmanagedRotation
{
    private readonly R _rotation = New<R>.Constructor<WorldState>()(module.WorldState);
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) => _rotation.Execute(actor, hints);
}
