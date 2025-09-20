using static BossMod.Components.GenericKnockback;

namespace BossMod.Components;

// generic knockback/attract component; it's a cast counter for convenience
[SkipLocalsInit]
public abstract class GenericKnockback(BossModule module, uint aid = default, int maxCasts = int.MaxValue, bool stopAtWall = false, bool stopAfterWall = false) : CastCounter(module, aid)
{
    public enum Kind
    {
        None,
        AwayFromOrigin, // standard knockback - specific distance along ray from origin to target
        TowardsOrigin, // standard pull - "knockback" to source -  specific distance along ray from origin to target + 180 degrees
        DirBackward, // standard pull - "knockback" to source - forward along source's direction + 180 degrees
        DirForward, // directional knockback - forward along source's direction
        DirLeft, // directional knockback - forward along source's direction + 90 degrees
        DirRight // directional knockback - forward along source's direction - 90 degrees
    }

    public readonly struct Knockback(
        WPos origin,
        float distance,
        DateTime activation = default,
        AOEShape? shape = null, // if null, assume it is unavoidable raidwide knockback/attract
        Angle direction = default, // irrelevant for non-directional knockback/attract
        Kind kind = Kind.AwayFromOrigin,
        float minDistance = default, // irrelevant for knockbacks
        IReadOnlyList<SafeWall>? safeWalls = null,
        ulong actorID = default,
        bool ignoreImmunes = false
    )
    {
        public readonly WPos Origin = origin;
        public readonly float Distance = distance;
        public readonly DateTime Activation = activation;
        public readonly AOEShape? Shape = shape;
        public readonly Angle Direction = direction;
        public readonly Kind Kind = kind;
        public readonly float MinDistance = minDistance;
        public readonly SafeWall[] SafeWalls = safeWalls?.ToArray() ?? [];
        public readonly ulong ActorID = actorID;
        public readonly bool IgnoreImmunes = ignoreImmunes;
    }

    public readonly struct SafeWall(WPos vertex1, WPos vertex2)
    {
        public readonly WPos Vertex1 = vertex1;
        public readonly WPos Vertex2 = vertex2;
    }

    protected struct PlayerImmuneState
    {
        public DateTime RoleBuffExpire; // 0 if not active
        public DateTime JobBuffExpire; // 0 if not active
        public DateTime DutyBuffExpire; // 0 if not active

        public readonly bool ImmuneAt(DateTime time) => RoleBuffExpire > time || JobBuffExpire > time || DutyBuffExpire > time;
    }

    public bool StopAtWall = stopAtWall; // use if wall is solid rather than deadly
    public bool StopAfterWall = stopAfterWall; // use if the wall is a polygon where you need to check for intersections
    public readonly int MaxCasts = maxCasts; // use to limit number of drawn knockbacks
    private const float approxHitBoxRadius = 0.499f; // calculated because due to floating point errors this does not result in 0.001
    private const float maxIntersectionError = 0.5f - approxHitBoxRadius; // calculated because due to floating point errors this does not result in 0.001

    protected readonly PlayerImmuneState[] PlayerImmunes = new PlayerImmuneState[PartyState.MaxAllies];

    public bool IsImmune(int slot, DateTime time) => PlayerImmunes[slot].ImmuneAt(time);

    public static WPos AwayFromSource(WPos pos, WPos origin, float distance) => pos != origin ? pos + distance * (pos - origin).Normalized() : pos;
    public static WPos AwayFromSource(WPos pos, Actor? source, float distance) => source != null ? AwayFromSource(pos, source.Position, distance) : pos;

    public static void DrawKnockback(WPos from, WPos to, Angle rot, MiniArena arena)
    {
        if (from != to)
        {
            arena.ActorProjected(from, to, rot, Colors.Danger);
            arena.AddLine(from, to);
        }
    }
    public static void DrawKnockback(Actor actor, WPos adjPos, MiniArena arena) => DrawKnockback(actor.Position, adjPos, actor.Rotation, arena);

    // note: if implementation returns multiple sources, it is assumed they are applied sequentially (so they should be pre-sorted in activation order)
    public abstract ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor);

    // called to determine whether we need to show hint
    public virtual bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !StopAtWall && !Arena.InBounds(pos);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var movements = CalculateMovements(slot, actor);
        var count = movements.Count;
        for (var i = 0; i < count; ++i)
        {
            var movement = movements[i];
            if (DestinationUnsafe(slot, actor, movement.to))
            {
                hints.Add("About to be knocked into danger!");
                break;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var movements = CalculateMovements(pcSlot, pc);
        var count = movements.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = movements[i];
            DrawKnockback(e.from, e.to, pc.Rotation, Arena);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
        {
            switch (status.ID)
            {
                case 3054u: //Guard in PVP
                case (uint)WHM.SID.Surecast:
                case (uint)WAR.SID.ArmsLength:
                    PlayerImmunes[slot].RoleBuffExpire = status.ExpireAt;
                    break;
                case 1722u: //Bluemage Diamondback
                case (uint)WAR.SID.InnerStrength:
                    PlayerImmunes[slot].JobBuffExpire = status.ExpireAt;
                    break;
                case 2345u: //Lost Manawall in Bozja
                    PlayerImmunes[slot].DutyBuffExpire = status.ExpireAt;
                    break;
            }
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
        {
            switch (status.ID)
            {
                case 3054u: //Guard in PVP
                case (uint)WHM.SID.Surecast:
                case (uint)WAR.SID.ArmsLength:
                    PlayerImmunes[slot].RoleBuffExpire = default;
                    break;
                case 1722u: //Bluemage Diamondback
                case (uint)WAR.SID.InnerStrength:
                    PlayerImmunes[slot].JobBuffExpire = default;
                    break;
                case 2345u: //Lost Manawall in Bozja
                    PlayerImmunes[slot].DutyBuffExpire = default;
                    break;
            }
        }
    }

    public List<(WPos from, WPos to)> CalculateMovements(int slot, Actor actor)
    {
        if (MaxCasts <= 0)
            return [];
        var movements = new List<(WPos, WPos)>();
        var from = actor.Position;
        var count = 0;
        var activeKnockbacks = ActiveKnockbacks(slot, actor);
        var len = activeKnockbacks.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var s = ref activeKnockbacks[i];
            if (!s.IgnoreImmunes && PlayerImmunes[slot].ImmuneAt(s.Activation))
            {
                continue; // this source won't affect player due to immunity
            }
            if (s.Shape != null && !s.Shape.Check(from, s.Origin, s.Direction))
            {
                continue; // this source won't affect player due to being out of aoe
            }

            var dir = s.Kind switch
            {
                Kind.AwayFromOrigin => from != s.Origin ? (from - s.Origin).Normalized() : default,
                Kind.TowardsOrigin => from != s.Origin ? (s.Origin - from).Normalized() : default,
                Kind.DirBackward => (s.Direction + 180f.Degrees()).ToDirection(),
                Kind.DirForward => s.Direction.ToDirection(),
                Kind.DirLeft => s.Direction.ToDirection().OrthoL(),
                Kind.DirRight => s.Direction.ToDirection().OrthoR(),
                _ => default
            };
            if (dir == default)
                continue; // couldn't determine direction for some reason

            var distance = s.Distance;
            if (s.Kind == Kind.TowardsOrigin)
                distance = Math.Min(distance, (s.Origin - from).Length() - s.MinDistance);
            if (s.Kind == Kind.DirBackward)
            {
                var perpendicularDir = s.Direction.ToDirection().OrthoL();
                var perpendicularDistance = Math.Abs((from - s.Origin).Cross(perpendicularDir) / perpendicularDir.Length());
                distance = Math.Min(distance, perpendicularDistance - s.MinDistance);
            }

            if (distance <= 0f)
                continue; // this could happen if attract starts from < min distance

            if (StopAtWall)
                distance = Math.Min(distance, Arena.IntersectRayBounds(from, dir) - Math.Clamp(actor.HitboxRadius - approxHitBoxRadius, maxIntersectionError, actor.HitboxRadius - approxHitBoxRadius)); // hitbox radius can be != 0.5 if player is transformed/mounted, but normal arenas with walls should account for walkable arena in their shape already
            if (StopAfterWall)
            {
                distance = Math.Min(distance, Arena.IntersectRayBounds(from, dir) + maxIntersectionError);
            }

            var walls = s.SafeWalls;
            var lenW = walls.Length;
            if (lenW != 0)
            {
                var distanceToWall = float.MaxValue;
                for (var j = 0; j < lenW; ++j)
                {
                    var wall = walls[j];
                    var t = Intersect.RaySegment(from, dir, wall.Vertex1, wall.Vertex2);
                    if (t < distanceToWall && t <= s.Distance)
                    {
                        distanceToWall = t;
                    }
                }
                var hitboxradius = actor.HitboxRadius < approxHitBoxRadius ? 0.5f : actor.HitboxRadius; // some NPCs have less than 0.5 radius and cause error while clamping
                distance = distanceToWall < float.MaxValue
                    ? Math.Min(distance, distanceToWall - Math.Clamp(hitboxradius - approxHitBoxRadius, maxIntersectionError, hitboxradius - approxHitBoxRadius))
                    : Math.Min(distance, Arena.IntersectRayBounds(from, dir) + maxIntersectionError);
            }

            var to = from + distance * dir;
            movements.Add((from, to));
            from = to;

            if (++count == MaxCasts)
            {
                break;
            }
        }
        return movements;
    }
}

// generic 'knockback from/attract to cast target' component
// TODO: knockback is really applied when effectresult arrives rather than when actioneffect arrives, this is important for ai hints (they can reposition too early otherwise)
[SkipLocalsInit]
public class SimpleKnockbacks(BossModule module, uint aid, float distance, bool ignoreImmunes = false, int maxCasts = int.MaxValue, AOEShape? shape = null, Kind kind = Kind.AwayFromOrigin, float minDistance = default, bool minDistanceBetweenHitboxes = false, bool stopAtWall = false, bool stopAfterWall = false)
    : GenericKnockback(module, aid, maxCasts, stopAtWall, stopAfterWall)
{
    public readonly float Distance = distance;
    public readonly AOEShape? Shape = shape;
    public readonly Kind KnockbackKind = kind;
    public readonly float MinDistance = minDistance;
    public readonly bool IgnoreImmunes = ignoreImmunes;
    public readonly bool MinDistanceBetweenHitboxes = minDistanceBetweenHitboxes;
    public readonly List<Knockback> Casters = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(Casters);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var minDist = KnockbackKind == Kind.TowardsOrigin ? (MinDistance + (MinDistanceBetweenHitboxes ? Raid.Player()!.HitboxRadius + caster.HitboxRadius : default)) : MinDistance;
            Casters.Add(new(spell.LocXZ, Distance, Module.CastFinishAt(spell), Shape, spell.Rotation, KnockbackKind, minDist, [], caster.InstanceID, IgnoreImmunes));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = Casters.Count;
            var id = caster.InstanceID;
            var kbs = CollectionsMarshal.AsSpan(Casters);
            for (var i = 0; i < count; ++i)
            {
                if (kbs[i].ActorID == id)
                {
                    Casters.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

[SkipLocalsInit]
public class SimpleKnockbackGroups(BossModule module, uint[] aids, float distance, bool ignoreImmunes = false, int maxCasts = int.MaxValue, AOEShape? shape = null, Kind kind = Kind.AwayFromOrigin, float minDistance = default, bool minDistanceBetweenHitboxes = false, bool stopAtWall = false, bool stopAfterWall = false) : SimpleKnockbacks(module, default, distance, ignoreImmunes, maxCasts, shape, kind, minDistance, minDistanceBetweenHitboxes, stopAtWall, stopAfterWall)
{
    protected readonly uint[] AIDs = aids;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        for (var i = 0; i < len; ++i)
        {
            if (spell.Action.ID == AIDs[i])
            {
                var minDist = KnockbackKind == Kind.TowardsOrigin ? (MinDistance + (MinDistanceBetweenHitboxes ? Raid.Player()!.HitboxRadius + caster.HitboxRadius : default)) : default;
                Casters.Add(new(spell.LocXZ, Distance, Module.CastFinishAt(spell), Shape, spell.Rotation, KnockbackKind, minDist, [], caster.InstanceID, IgnoreImmunes));
                return;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        // we probably dont need to check for AIDs here since actorID should already be unique to any active spell
        var count = Casters.Count;
        var id = caster.InstanceID;
        var kbs = CollectionsMarshal.AsSpan(Casters);
        for (var i = 0; i < count; ++i)
        {
            if (kbs[i].ActorID == id)
            {
                Casters.RemoveAt(i);
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
