namespace BossMod.Components;

// generic 'chasing AOE' component - these are AOEs that follow the target for a set amount of casts

[SkipLocalsInit]
public class GenericChasingAOEs(BossModule module, float moveDistance, uint aid = default, string warningText = "GTFO from chasing aoe!") : GenericAOEs(module, aid, warningText)
{
    private readonly float MoveDistance = moveDistance;

    public sealed class Chaser(AOEShape shape, Actor target, WPos prevPos, float moveDist, int numRemaining, DateTime nextActivation, double secondsBetweenActivations)
    {
        public AOEShape Shape = shape;
        public Actor Target = target;
        public WPos PrevPos = prevPos;
        public float MoveDist = moveDist;
        public int NumRemaining = numRemaining;
        public DateTime NextActivation = nextActivation;
        public double SecondsBetweenActivations = secondsBetweenActivations;

        public WPos PredictedPosition()
        {
            var offset = Target.Position - PrevPos;
            var distance = offset.Length();
            var pos = distance > MoveDist ? PrevPos + MoveDist * offset / distance : Target.Position;
            return pos.Quantized();
        }
    }

    public List<Chaser> Chasers = [];

    public bool IsChaserTarget(Actor? actor)
    {
        var count = Chasers.Count;
        for (var i = 0; i < count; ++i)
        {
            if (Chasers[i].Target == actor)
                return true;
        }
        return false;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Chasers.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var c = Chasers[i];
            var pos = c.PredictedPosition();
            var off = pos - c.PrevPos;
            aoes[i] = new(c.Shape, pos, off.LengthSq() > 0 ? Angle.FromDirection(off) : default, c.NextActivation);
        }
        return aoes;
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Chasers.Any(c => c.Target == player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    // return false if chaser was not found
    public bool Advance(WPos pos, float moveDistance, DateTime currentTime, bool removeWhenFinished = true)
    {
        ++NumCasts;
        var c = Chasers.MinBy(c => (c.PredictedPosition() - pos).LengthSq());
        if (c == null)
            return false;

        if (--c.NumRemaining <= 0 && removeWhenFinished)
        {
            Chasers.Remove(c);
        }
        else
        {
            c.PrevPos = pos;
            c.MoveDist = moveDistance;
            c.NextActivation = currentTime.AddSeconds(c.SecondsBetweenActivations);
        }
        return true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Chasers.Count > 0)
        {
            AddForbiddenZones(actor, hints, true);
            AddForbiddenZones(actor, hints, false);
        }
    }

    private void AddForbiddenZones(Actor actor, AIHints hints, bool isTarget)
    {
        // sort of a hack to prevent the AI from getting "stuck" inside the AOE because all paths to safety have equal distance
        foreach (var c in Chasers.Where(c => c.Target == actor == isTarget))
        {
            var circle = (AOEShapeCircle)c.Shape;
            var radius = isTarget ? MoveDistance + circle.Radius : circle.Radius + 1;
            var position = isTarget ? c.PredictedPosition() - circle.Radius * actor.Rotation.ToDirection() : c.PredictedPosition();
            hints.AddForbiddenZone(new SDCircle(position, radius), c.NextActivation);
        }
    }
}

// standard chasing aoe; first cast is long - assume it is baited on the nearest allowed target; successive casts are instant
[SkipLocalsInit]
public class StandardChasingAOEs(BossModule module, AOEShape shape, uint actionFirst, uint actionRest, float moveDistance, double secondsBetweenActivations, int maxCasts, bool resetExcludedTargets = false, uint icon = default, double activationDelay = 5.1d) : GenericChasingAOEs(module, moveDistance)
{
    public StandardChasingAOEs(BossModule module, float radius, uint actionFirst, uint actionRest, float moveDistance, double secondsBetweenActivations, int maxCasts, bool resetExcludedTargets = false, uint icon = default, double activationDelay = 5.1d) : this(module, new AOEShapeCircle(radius), actionFirst, actionRest, moveDistance, secondsBetweenActivations, maxCasts, resetExcludedTargets, icon, activationDelay) { }

    public readonly AOEShape Shape = shape;
    public readonly uint ActionFirst = actionFirst;
    public readonly uint ActionRest = actionRest;
    public readonly float MoveDistance = moveDistance;
    public readonly double SecondsBetweenActivations = secondsBetweenActivations;
    public int MaxCasts = maxCasts;
    public BitMask ExcludedTargets; // any targets in this mask aren't considered to be possible targets
    public readonly uint Icon = icon;
    public readonly double ActivationDelay = activationDelay;
    public readonly bool ResetExcludedTargets = resetExcludedTargets;
    public BitMask Targets; // to keep track of the icon before mechanic starts for handling custom forbidden zones
    public DateTime Activation;

    public override void Update()
    {
        var count = Chasers.Count;
        if (count == 0)
            return;

        for (var i = count - 1; i >= 0; --i)
        {
            var c = Chasers[i];
            if ((c.Target.IsDestroyed || c.Target.IsDead) && c.NumRemaining < MaxCasts)
                Chasers.RemoveAt(i);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Chasers.Count;
        if (Chasers.Count == 0)
            return;

        for (var i = 0; i < count; ++i)
        {
            var c = Chasers[i];
            Arena.AddLine(c.PrevPos, c.Target.Position);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == ActionFirst)
        {
            var pos = spell.LocXZ;
            var (slot, target) = Raid.WithSlot().ExcludedFromMask(ExcludedTargets).MinBy(ip => (ip.Item2.Position - pos).LengthSq());
            if (target != null)
            {
                Targets[Module.Raid.FindSlot(target.InstanceID)] = false;
                Chasers.Add(new(Shape, target, pos, 0, MaxCasts, Module.CastFinishAt(spell), SecondsBetweenActivations)); // initial cast does not move anywhere
                ExcludedTargets[slot] = true;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is var id && id == ActionFirst || id == ActionRest)
        {
            var pos = spell.MainTargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ;
            Advance(pos.Quantized(), MoveDistance, WorldState.CurrentTime);
            if (Chasers.Count == 0 && ResetExcludedTargets)
            {
                ExcludedTargets = default;
                NumCasts = 0;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == Icon)
        {
            Activation = WorldState.FutureTime(ActivationDelay);
            Targets[Module.Raid.FindSlot(targetID)] = false;
        }
    }
}

// since open world players don't count towards party, we need to make a new component
[SkipLocalsInit]
public abstract class OpenWorldChasingAOEs(BossModule module, AOEShape shape, uint actionFirst, uint actionRest, float moveDistance, float secondsBetweenActivations, int maxCasts, bool resetExcludedTargets = false, uint icon = default, float activationDelay = 5.1f) : StandardChasingAOEs(module, shape, actionFirst, actionRest, moveDistance, secondsBetweenActivations, maxCasts, resetExcludedTargets, icon, activationDelay)
{
    public new HashSet<Actor> ExcludedTargets = []; // any targets in this hashset aren't considered to be possible targets

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == ActionFirst)
        {
            var pos = spell.TargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;
            Actor? target = null;
            var minDistanceSq = float.MaxValue;

            foreach (var actor in WorldState.Actors.Actors.Values)
            {
                if (actor.OID == 0 && !ExcludedTargets.Contains(actor))
                {
                    var distanceSq = (actor.Position - pos).LengthSq();
                    if (distanceSq < minDistanceSq)
                    {
                        minDistanceSq = distanceSq;
                        target = actor;
                    }
                }
            }
            if (target != null)
            {
                Targets[Module.Raid.FindSlot(target.InstanceID)] = false;
                Chasers.Add(new(Shape, target, pos, 0, MaxCasts, Module.CastFinishAt(spell), SecondsBetweenActivations)); // initial cast does not move anywhere
                ExcludedTargets.Add(target);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == ActionFirst || spell.Action.ID == ActionRest)
        {
            var pos = spell.MainTargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ;
            Advance(pos.Quantized(), MoveDistance, WorldState.CurrentTime);
            if (Chasers.Count == 0 && ResetExcludedTargets)
            {
                ExcludedTargets.Clear();
                NumCasts = 0;
            }
        }
    }
}
