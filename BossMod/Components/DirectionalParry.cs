namespace BossMod.Components;

// generic 'directional parry' component that shows actors and sides it's forbidden to attack them from
// uses common status + custom prediction

[SkipLocalsInit]
public class DirectionalParry(BossModule module, uint[] actorOID) : AddsMulti(module, actorOID)
{
    public enum Side
    {
        None = 0x0,
        Front = 0x1,
        Back = 0x2,
        Left = 0x4,
        Right = 0x8,
        All = 0xF
    }

    public const uint ParrySID = 680u; // common 'directional parry' status

    public readonly Dictionary<ulong, int> ActorStates = []; // value == active-side | (imminent-side << 4)
    public bool Active
    {
        get
        {
            foreach (var state in ActorStates.Values)
            {
                if ((Side)(state & 0xF) != Side.None)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActorStates.Count == 0)
            return;
        var actors = ActiveActors;
        Actor? target = null;
        var count = ActiveActors.Count;
        for (var i = 0; i < count; ++i)
        {
            var a = actors[i];
            if (a.InstanceID == actor.TargetID)
            {
                target = a;
                break;
            }
        }

        if (target == null || !ActorStates.TryGetValue(actor.TargetID, out var targetState))
            return;

        var active = (Side)(targetState & 0xF);
        var imminent = (Side)((targetState >> 4) & 0xF);
        var forbiddenSides = active | imminent;

        var attackDir = (actor.Position - target.Position).Normalized();
        var facing = target.Rotation.ToDirection();
        var forwardDot = attackDir.Dot(facing);

        var attackSide =
            forwardDot > 0.7071067f ? Side.Front :
            forwardDot < -0.7071067f ? Side.Back :
            (attackDir.Dot(facing.OrthoL()) > 0f ? Side.Left : Side.Right);

        if ((forbiddenSides & attackSide) != default)
            hints.Add("Attack target from unshielded side!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (id, targetState) in ActorStates)
        {
            var target = WorldState.Actors.Find(id);
            if (target == null)
                continue;

            void forbidDirection(Angle offset)
                => hints.AddForbiddenZone(new SDCone(target.Position, 100f, target.Rotation + offset, 45f.Degrees()), DateTime.MaxValue);

            var active = (Side)(targetState & 0xF);
            var imminent = (Side)((targetState >> 4) & 0xF);
            var forbiddenSides = active | imminent;
            var attackDir = (actor.Position - target.Position).Normalized();
            var facing = target.Rotation.ToDirection();

            var forwardDot = attackDir.Dot(facing);
            var orthoDot = attackDir.Dot(facing.OrthoL());

            var attackSide =
                forwardDot > 0.7071067f ? Side.Front :
                forwardDot < -0.7071067f ? Side.Back :
                (orthoDot > 0f ? Side.Left : Side.Right);

            if ((forbiddenSides & attackSide) != default)
            {
                if (active != default)
                    hints.SetPriority(target, AIHints.Enemy.PriorityForbidden);

                if (actor.TargetID == id)
                {
                    if ((forbiddenSides & Side.Front) != default)
                        forbidDirection(default);
                    if ((forbiddenSides & Side.Left) != default)
                        forbidDirection(90f.Degrees());
                    if ((forbiddenSides & Side.Back) != default)
                        forbidDirection(180f.Degrees());
                    if ((forbiddenSides & Side.Right) != default)
                        forbidDirection(270f.Degrees());
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var a in ActiveActors)
        {
            if (ActorStates.TryGetValue(a.InstanceID, out var aState))
            {
                var active = ActiveSides(aState);
                var imminent = ImminentSides(aState);
                DrawParry(a, default, active, imminent, Side.Front);
                DrawParry(a, 180f.Degrees(), active, imminent, Side.Back);
                DrawParry(a, 90f.Degrees(), active, imminent, Side.Left);
                DrawParry(a, 270f.Degrees(), active, imminent, Side.Right);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == ParrySID)
        {
            // TODO: front+back is 3, left+right is C, but I don't really know which is which, didn't see examples yet...
            // remove any predictions
            ActorStates[actor.InstanceID] = status.Extra & 0xF;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == ParrySID)
        {
            UpdateState(actor.InstanceID, ActorState(actor.InstanceID) & ~0xF);
        }
    }

    public void PredictParrySide(ulong instanceID, Side sides)
    {
        UpdateState(instanceID, ((int)sides << 4) | ActorState(instanceID) & 0xF);
    }

    private void DrawParry(Actor actor, Angle offset, Side active, Side imminent, Side check)
    {
        if ((active & check) != default)
            DrawParry(actor, offset, Colors.Enemy);
        else if ((imminent & check) != default)
            DrawParry(actor, offset);
    }

    private void DrawParry(Actor actor, Angle offset, uint color = default)
    {
        var dir = actor.Rotation + offset;
        Arena.PathArcTo(actor.Position, 1.5f, (dir - 45f.Degrees()).Rad, (dir + 45f.Degrees()).Rad);
        MiniArena.PathStroke(false, color);
    }

    public int ActorState(ulong instanceID) => ActorStates.GetValueOrDefault(instanceID, 0);

    public void UpdateState(ulong instanceID, int state)
    {
        if (state == 0)
            ActorStates.Remove(instanceID);
        else
            ActorStates[instanceID] = state;
    }

    private static Side ActiveSides(int state) => (Side)(state & (int)Side.All);
    private static Side ImminentSides(int state) => ActiveSides(state >> 4);
}
