using Dalamud.Bindings.ImGui;

namespace BossMod.Components;

// generic gaze/weakpoint component, allows customized 'eye' position
[SkipLocalsInit]
public abstract class GenericGaze(BossModule module, uint aid = default) : CastCounter(module, aid)
{
    public readonly struct Eye(
        WPos position,
        DateTime activation = default,
        Angle forward = default, // if non-zero, treat specified side as 'forward' for hit calculations
        float range = 10000f,
        bool inverted = false,
        ulong actorID = default)
    {
        public readonly WPos Position = position;
        public readonly DateTime Activation = activation;
        public readonly Angle Forward = forward;
        public readonly float Range = range;
        public readonly bool Inverted = inverted;
        public readonly ulong ActorID = actorID;
    }

    private const float _eyeOuterH = 10f, _eyeOuterV = 6f, _eyeInnerR = 4f;
    private const float _eyeOuterR = (_eyeOuterH * _eyeOuterH + _eyeOuterV * _eyeOuterV) / (2f * _eyeOuterV);
    private const float _eyeOffsetV = _eyeOuterR - _eyeOuterV;

    private const float _eyeHalfAngle = 1.080839f; // (float)Math.Asin(_eyeOuterH / _eyeOuterR);
    private static readonly Vector2 offset = new(default, _eyeOffsetV);
    private const float halfPIHalfAngleP = Angle.HalfPi + _eyeHalfAngle;
    private const float halfPIHalfAngleM = Angle.HalfPi - _eyeHalfAngle;

    public abstract ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var eyes = ActiveEyes(slot, actor);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            if (actor.Position.InCircle(eye.Position, eye.Range) && HitByEye(ref actor, eye) != eye.Inverted)
            {
                hints.Add(eye.Inverted ? "Face the eye!" : "Turn away from gaze!");
                break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var eyes = ActiveEyes(slot, actor);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            if (actor.Position.InCircle(eye.Position, eye.Range))
            {
                var direction = eye.Inverted ? Angle.FromDirection(actor.Position - eye.Position) - eye.Forward
                    : Angle.FromDirection(eye.Position - actor.Position) - eye.Forward;

                var angle = eye.Inverted ? 135f.Degrees() : 45f.Degrees();
                hints.ForbiddenDirections.Add((direction, angle, eye.Activation));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var eyes = ActiveEyes(pcSlot, pc);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            var danger = HitByEye(ref pc, eye) != eye.Inverted;
            var eyeCenter = IndicatorScreenPos(eye.Position);
            DrawEye(eyeCenter, danger);

            if (pc.Position.InCircle(eye.Position, eye.Range))
            {
                var (min, max) = eye.Inverted ? (45f, 315f) : (-45f, 45f);
                Arena.PathArcTo(pc.Position, 1f, (pc.Rotation + eye.Forward + min.Degrees()).Rad, (pc.Rotation + eye.Forward + max.Degrees()).Rad);
                MiniArena.PathStroke(false, Colors.Enemy);
            }
        }
    }

    public static void DrawEye(Vector2 eyeCenter, bool danger)
    {
        var dl = ImGui.GetWindowDrawList();
        dl.PathArcTo(eyeCenter - offset, _eyeOuterR, halfPIHalfAngleP, halfPIHalfAngleM);
        dl.PathArcTo(eyeCenter + offset, _eyeOuterR, -halfPIHalfAngleP, -halfPIHalfAngleM);
        dl.PathFillConvex(danger ? Colors.Enemy : Colors.PC);
        dl.AddCircleFilled(eyeCenter, _eyeInnerR, Colors.Border);
    }

    public static bool HitByEye(ref Actor actor, Eye eye) => (actor.Rotation + eye.Forward).ToDirection().Dot((eye.Position - actor.Position).Normalized()) >= 0.707107f; // 45-degree

    private Vector2 IndicatorScreenPos(WPos eye)
    {
        if (Arena.InBounds(eye) || Arena.Bounds is not ArenaBoundsCircle && Arena.Bounds is ArenaBoundsCustom circle && !circle.IsCircle)
        {
            return Arena.WorldPositionToScreenPosition(eye);
        }
        else
        {
            var dir = (eye - Arena.Center).Normalized();
            return Arena.ScreenCenter + Arena.RotatedCoords(dir.ToVec2()) * (Arena.ScreenHalfSize + Arena.ScreenMarginSize * 0.5f);
        }
    }
}

// gaze that happens on cast end
[SkipLocalsInit]
public class CastGaze(BossModule module, uint aid, bool inverted = false, float range = 10000f, int maxCasts = int.MaxValue) : GenericGaze(module, aid)
{
    public readonly List<Eye> Eyes = [];
    public int MaxCasts = maxCasts; // used for staggered gazes, when showing all active would be pointless

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = Eyes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > MaxCasts ? MaxCasts : count;
        return CollectionsMarshal.AsSpan(Eyes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Eyes.Add(new(spell.LocXZ, Module.CastFinishAt(spell), default, range, inverted, caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = Eyes.Count;
            var id = caster.InstanceID;
            var eyes = CollectionsMarshal.AsSpan(Eyes);
            for (var i = 0; i < count; ++i)
            {
                if (eyes[i].ActorID == id)
                {
                    Eyes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

[SkipLocalsInit]
public class CastGazes(BossModule module, uint[] aids, bool inverted = false, float range = 10000f, int maxCasts = int.MaxValue, int expectedNumCasters = 99) : CastGaze(module, default, maxCasts: maxCasts)
{
    protected readonly uint[] AIDs = aids;
    protected readonly int ExpectedNumCasters = expectedNumCasters;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        for (var i = 0; i < len; ++i)
        {
            if (spell.Action.ID == AIDs[i])
            {
                Eyes.Add(new(spell.LocXZ, Module.CastFinishAt(spell), default, range, inverted, caster.InstanceID));
                if (Eyes.Count == ExpectedNumCasters)
                {
                    Eyes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
                }
                return;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        // we probably dont need to check for AIDs here since actorID should already be unique to any active spell
        var count = Eyes.Count;
        var id = caster.InstanceID;
        var eyes = CollectionsMarshal.AsSpan(Eyes);
        for (var i = 0; i < count; ++i)
        {
            if (eyes[i].ActorID == id)
            {
                Eyes.RemoveAt(i);
                return;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDs.Length;
        for (var i = 0; i < len; ++i)
        {
            if (spell.Action.ID == AIDs[i])
            {
                ++NumCasts;
                return;
            }
        }
    }
}

// cast weakpoint component: a number of casts (with supposedly non-intersecting shapes), player should face specific side determined by active status to the caster for aoe he's in
[SkipLocalsInit]
public class CastWeakpoint(BossModule module, uint aid, AOEShape shape, uint statusForward, uint statusBackward, uint statusLeft, uint statusRight) : GenericGaze(module, aid)
{
    public CastWeakpoint(BossModule module, uint aid, float radius, uint statusForward, uint statusBackward, uint statusLeft, uint statusRight) : this(module, aid, new AOEShapeCircle(radius), statusForward, statusBackward, statusLeft, statusRight) { }
    public AOEShape Shape = shape;
    public readonly uint[] Statuses = [statusForward, statusLeft, statusBackward, statusRight]; // 4 elements: fwd, left, back, right
    protected readonly List<Actor> _casters = [];
    private readonly Dictionary<ulong, Angle> _playerWeakpoints = [];
    protected float fallbackTime;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];
        Actor? caster = null;
        var minRemainingTime = float.MaxValue;
        // if there are multiple casters, take one that finishes first
        for (var i = 0; i < count; ++i)
        {
            var a = _casters[i];
            if (Shape.Check(actor.Position, a.Position, a.CastInfo?.Rotation ?? a.Rotation))
            {
                if ((a.CastInfo?.RemainingTime ?? fallbackTime) < minRemainingTime)
                {
                    caster = a;
                    minRemainingTime = a.CastInfo?.RemainingTime ?? fallbackTime;
                }
            }
        }

        if (caster != null && _playerWeakpoints.TryGetValue(actor.InstanceID, out var angle))
            return new Eye[1] { new(caster.Position, Module.CastFinishAt(caster.CastInfo), angle, inverted: true) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _casters.Remove(caster);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind >= 0)
            _playerWeakpoints[actor.InstanceID] = statusKind * 90f.Degrees();
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind >= 0)
            _playerWeakpoints.Remove(actor.InstanceID);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var eyes = ActiveEyes(slot, actor);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (!HitByEye(ref actor, eyes[i]))
            {
                hints.Add("Face open weakpoint to eye!");
                return;
            }
        }
    }
}
