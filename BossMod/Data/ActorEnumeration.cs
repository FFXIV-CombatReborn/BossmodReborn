namespace BossMod;

// for raid members, we support both indexed and non-indexed enumeration
public static class ActorEnumeration
{
    // build a mask with set bits corresponding to slots in range
    public static BitMask Mask(this IEnumerable<(int, Actor)> range)
    {
        BitMask mask = default;
        foreach ((var i, _) in range)
        {
            mask.Set(i);
        }

        return mask;
    }

    // convert slot+actor range into actor range
    public static IEnumerable<Actor> Actors(this IEnumerable<(int, Actor)> range)
    {
        foreach (var (_, actor) in range)
        {
            yield return actor;
        }
    }

    // filter range with slot+actor by slot or by actor
    public static IEnumerable<(int, Actor)> WhereSlot(this IEnumerable<(int, Actor)> range, Func<int, bool> predicate)
    {
        foreach (var item in range)
        {
            if (predicate(item.Item1))
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<(int, Actor)> WhereActor(this IEnumerable<(int, Actor)> range, Func<Actor, bool> predicate)
    {
        foreach (var item in range)
        {
            if (predicate(item.Item2))
            {
                yield return item;
            }
        }
    }

    // exclude specified actor from enumeration
    public static IEnumerable<Actor> Exclude(this IEnumerable<Actor> range, Actor? actor)
    {
        foreach (var x in range)
        {
            if (x != actor)
            {
                yield return x;
            }
        }
    }

    public static IEnumerable<(int, Actor)> Exclude(this IEnumerable<(int, Actor)> range, Actor? actor) => range.WhereActor(x => x != actor);

    public static IEnumerable<(int, Actor)> Exclude(this IEnumerable<(int, Actor)> range, int slot) => range.WhereSlot(i => i != slot);

    public static IEnumerable<Actor> Exclude(this IEnumerable<Actor> range, IEnumerable<Actor> actors)
    {
        var actorSet = new HashSet<Actor>(actors);
        foreach (var x in range)
        {
            if (!actorSet.Contains(x))
            {
                yield return x;
            }
        }
    }

    // select actors that have their corresponding bit in mask set
    public static IEnumerable<(int, Actor)> IncludedInMask(this IEnumerable<(int, Actor)> range, BitMask mask) => range.WhereSlot(i => mask[i]);

    // select actors that have their corresponding bit in mask cleared
    public static IEnumerable<(int, Actor)> ExcludedFromMask(this IEnumerable<(int, Actor)> range, BitMask mask) => range.WhereSlot(i => !mask[i]);

    // select actors in specified radius from specified point
    public static IEnumerable<Actor> InRadius(this IEnumerable<Actor> range, WPos origin, float radius)
    {
        foreach (var actor in range)
        {
            if (actor.Position.InCircle(origin, radius))
            {
                yield return actor;
            }
        }
    }

    public static IEnumerable<(int, Actor)> InRadius(this IEnumerable<(int, Actor)> range, WPos origin, float radius) => range.WhereActor(actor => actor.Position.InCircle(origin, radius));

    // select actors outside specified radius from specified point
    public static IEnumerable<Actor> OutOfRadius(this IEnumerable<Actor> range, WPos origin, float radius)
    {
        foreach (var actor in range)
        {
            if (!actor.Position.InCircle(origin, radius))
            {
                yield return actor;
            }
        }
    }

    public static IEnumerable<(int, Actor)> OutOfRadius(this IEnumerable<(int, Actor)> range, WPos origin, float radius) => range.WhereActor(actor => !actor.Position.InCircle(origin, radius));

    // select actors in specified radius from specified actor, excluding actor itself
    public static IEnumerable<Actor> InRadiusExcluding(this IEnumerable<Actor> range, Actor origin, float radius) => range.Exclude(origin).InRadius(origin.Position, radius);

    public static IEnumerable<(int, Actor)> InRadiusExcluding(this IEnumerable<(int, Actor)> range, Actor origin, float radius) => range.Exclude(origin).InRadius(origin.Position, radius);

    // select actors in specified shape
    public static List<Actor> InShape(this IEnumerable<Actor> range, AOEShape shape, Actor origin)
    {
        List<Actor> result = [];

        foreach (var actor in range)
        {
            if (shape.Check(actor.Position, origin))
            {
                result.Add(actor);
            }
        }

        return result;
    }

    public static List<Actor> InShape(this IEnumerable<Actor> range, AOEShape shape, WPos origin, Angle rotation)
    {
        List<Actor> result = [];

        foreach (var actor in range)
        {
            if (shape.Check(actor.Position, origin, rotation))
            {
                result.Add(actor);
            }
        }

        return result;
    }

    public static List<(int, Actor)> InShape(this IEnumerable<(int, Actor)> range, AOEShape shape, WPos origin, Angle rotation)
    {
        List<(int, Actor)> result = [];

        foreach (var tuple in range)
        {
            if (shape.Check(tuple.Item2.Position, origin, rotation))
            {
                result.Add(tuple);
            }
        }

        return result;
    }

    // select actors that have tether with specific ID
    public static IEnumerable<Actor> Tethered<ID>(this IEnumerable<Actor> range, ID id) where ID : Enum
    {
        var castID = (uint)(object)id;
        foreach (var actor in range)
        {
            if (actor.Tether.ID == castID)
            {
                yield return actor;
            }
        }
    }

    public static IEnumerable<(int, Actor)> Tethered<ID>(this IEnumerable<(int, Actor)> range, ID id) where ID : Enum
    {
        var castID = (uint)(object)id;
        return range.WhereActor(actor => actor.Tether.ID == castID);
    }

    // sort range by distance from point
    public static IEnumerable<Actor> SortedByRange(this IEnumerable<Actor> range, WPos origin)
    {
        var actors = new List<(Actor actor, float distanceSq)>();

        foreach (var a in range)
        {
            var distanceSq = (a.Position - origin).LengthSq();
            actors.Add((a, distanceSq));
        }

        actors.Sort(static (a, b) => a.distanceSq.CompareTo(b.distanceSq));

        for (var i = 0; i < actors.Count; ++i)
        {
            yield return actors[i].actor;
        }
    }

    public static IEnumerable<(int, Actor)> SortedByRange(this IEnumerable<(int, Actor)> range, WPos origin)
    {
        var actors = new List<(int index, Actor actor, float distanceSq)>();

        foreach (var a in range)
        {
            var distanceSq = (a.Item2.Position - origin).LengthSq();
            actors.Add((a.Item1, a.Item2, distanceSq));
        }

        actors.Sort(static (a, b) => a.distanceSq.CompareTo(b.distanceSq));

        for (var i = 0; i < actors.Count; ++i)
        {
            var actor = actors[i];
            yield return (actor.index, actor.actor);
        }
    }

    // Find closest actor to a given point
    public static Actor? Closest(this IEnumerable<Actor> range, WPos origin)
    {
        Actor? closest = null;
        var minDistSq = float.MaxValue;

        foreach (var actor in range)
        {
            var distSq = (actor.Position - origin).LengthSq();
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                closest = actor;
            }
        }
        return closest;
    }

    // Find closest actor (with index) to a given point
    public static (int, Actor) Closest(this IEnumerable<(int, Actor)> range, WPos origin)
    {
        (int, Actor)? closest = null;
        var minDistSq = float.MaxValue;

        foreach (var (index, actor) in range)
        {
            var distSq = (actor.Position - origin).LengthSq();
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                closest = (index, actor);
            }
        }
        return closest!.Value;
    }

    // Find farthest actor from a given point
    public static Actor? Farthest(this IEnumerable<Actor> range, WPos origin)
    {
        Actor? farthest = null;
        var maxDistSq = float.MinValue;

        foreach (var actor in range)
        {
            var distSq = (actor.Position - origin).LengthSq();
            if (distSq > maxDistSq)
            {
                maxDistSq = distSq;
                farthest = actor;
            }
        }
        return farthest;
    }

    // count num actors matching and not matching a condition
    public static (int match, int mismatch) CountByCondition(this IEnumerable<Actor> range, Func<Actor, bool> condition)
    {
        int match = 0, mismatch = 0;
        foreach (var a in range)
        {
            if (condition(a))
            {
                ++match;
            }
            else
            {
                ++mismatch;
            }
        }
        return (match, mismatch);
    }

    public static (int, Actor)[] ClockOrder(this (int, Actor)[] range, Actor starting, WPos center, bool counterclockwise = false)
    {
        var len = range.Length;
        var startingAngle = (starting.Position - center).ToAngle();
        var rad = startingAngle.Rad;
        var list = new List<((int, Actor) item, float angle)>(len);
        for (var i = 0; i < len; ++i)
        {
            var r = range[i];
            var thisAngle = (r.Item2.Position - center).ToAngle().Rad;
            if (counterclockwise)
            {
                if (r.Item2 != starting && thisAngle < rad)
                {
                    thisAngle += Angle.DoublePI;
                }
            }
            else
            {
                if (r.Item2 != starting && thisAngle > rad)
                {
                    thisAngle -= Angle.DoublePI;
                }
            }
            list.Add((r, thisAngle));
        }
        if (counterclockwise)
        {
            list.Sort(static (a, b) => a.angle.CompareTo(b.angle));
        }
        else
        {
            list.Sort(static (a, b) => b.angle.CompareTo(a.angle));
        }

        var array = new (int, Actor)[len];
        for (var i = 0; i < len; ++i)
        {
            array[i] = list[i].item;
        }
        return array;
    }

    public static Actor[] ClockOrder(this List<Actor> range, Actor starting, WPos center, bool counterclockwise = false)
    {
        var count = range.Count;
        var list = new List<(Actor actor, float angle)>(count);
        var startingAngle = (starting.Position - center).ToAngle();
        var rad = startingAngle.Rad;
        for (var i = 0; i < count; ++i)
        {
            var r = range[i];
            var thisAngle = (r.Position - center).ToAngle().Rad;
            if (counterclockwise)
            {
                if (r != starting && thisAngle < rad)
                {
                    thisAngle += Angle.DoublePI;
                }
            }
            else
            {
                if (r != starting && thisAngle > rad)
                {
                    thisAngle -= Angle.DoublePI;
                }
            }
            list.Add((r, thisAngle));
        }
        if (counterclockwise)
        {
            list.Sort(static (a, b) => a.angle.CompareTo(b.angle));
        }
        else
        {
            list.Sort(static (a, b) => b.angle.CompareTo(a.angle));
        }

        var array = new Actor[count];
        for (var i = 0; i < count; ++i)
        {
            array[i] = list[i].actor;
        }
        return array;
    }

    // ---- allocation-free helpers for party arrays (Raid.WithoutSlot / Raid.WithSlot) and actor lists, BMR shit ----
    public static BitMask Mask(this (int, Actor)[] range)
    {
        BitMask mask = default;
        for (var i = 0; i < range.Length; ++i)
            mask.Set(range[i].Item1);
        return mask;
    }

    public static BitMask MaskExcluding(this (int, Actor)[] range, Actor? exclude)
    {
        BitMask mask = default;
        for (var i = 0; i < range.Length; ++i)
        {
            if (range[i].Item2 != exclude)
                mask.Set(range[i].Item1);
        }
        return mask;
    }

    public static BitMask MaskInRadius(this (int, Actor)[] range, WPos origin, float radius)
    {
        BitMask mask = default;
        for (var i = 0; i < range.Length; ++i)
        {
            if (range[i].Item2.Position.InCircle(origin, radius))
                mask.Set(range[i].Item1);
        }
        return mask;
    }

    public static BitMask MaskInRadiusExcluding(this (int, Actor)[] range, Actor origin, float radius)
    {
        BitMask mask = default;
        var pos = origin.Position;
        for (var i = 0; i < range.Length; ++i)
        {
            var a = range[i].Item2;
            if (a != origin && a.Position.InCircle(pos, radius))
                mask.Set(range[i].Item1);
        }
        return mask;
    }

    public static BitMask MaskInShape(this (int, Actor)[] range, AOEShape shape, WPos origin, Angle rotation, Actor? exclude = null)
    {
        BitMask mask = default;
        for (var i = 0; i < range.Length; ++i)
        {
            var a = range[i].Item2;
            if (a != exclude && shape.Check(a.Position, origin, rotation))
                mask.Set(range[i].Item1);
        }
        return mask;
    }

    public static BitMask MaskByRole(this (int, Actor)[] range, Role role)
    {
        BitMask mask = default;
        for (var i = 0; i < range.Length; ++i)
        {
            if (range[i].Item2.Role == role)
                mask.Set(range[i].Item1);
        }
        return mask;
    }

    public static bool AnyInRadius(this Actor[] range, WPos origin, float radius)
    {
        for (var i = 0; i < range.Length; ++i)
        {
            if (range[i].Position.InCircle(origin, radius))
                return true;
        }
        return false;
    }

    public static bool AnyInRadius(this List<Actor> range, WPos origin, float radius)
    {
        var count = range.Count;
        for (var i = 0; i < count; ++i)
        {
            if (range[i].Position.InCircle(origin, radius))
                return true;
        }
        return false;
    }

    public static bool AnyInRadiusExcluding(this Actor[] range, Actor origin, float radius)
    {
        var pos = origin.Position;
        for (var i = 0; i < range.Length; ++i)
        {
            var a = range[i];
            if (a != origin && a.Position.InCircle(pos, radius))
                return true;
        }
        return false;
    }

    public static int CountInRadius(this Actor[] range, WPos origin, float radius)
    {
        var n = 0;
        for (var i = 0; i < range.Length; ++i)
        {
            if (range[i].Position.InCircle(origin, radius))
                ++n;
        }
        return n;
    }

    public static int CountInRadius(this List<Actor> range, WPos origin, float radius)
    {
        var n = 0;
        var count = range.Count;
        for (var i = 0; i < count; ++i)
        {
            if (range[i].Position.InCircle(origin, radius))
                ++n;
        }
        return n;
    }

    public static int CountInRadius(this Actor[] range, WPos origin, float radius, Role role)
    {
        var n = 0;
        for (var i = 0; i < range.Length; ++i)
        {
            var a = range[i];
            if (a.Role == role && a.Position.InCircle(origin, radius))
                ++n;
        }
        return n;
    }

    public static bool AnyInShape(this Actor[] range, AOEShape shape, WPos origin, Angle rotation, Actor? exclude = null)
    {
        for (var i = 0; i < range.Length; ++i)
        {
            var a = range[i];
            if (a != exclude && shape.Check(a.Position, origin, rotation))
                return true;
        }
        return false;
    }

    public static int CountInShape(this Actor[] range, AOEShape shape, WPos origin, Angle rotation, Actor? exclude = null)
    {
        var n = 0;
        for (var i = 0; i < range.Length; ++i)
        {
            var a = range[i];
            if (a != exclude && shape.Check(a.Position, origin, rotation))
                ++n;
        }
        return n;
    }

    public static Actor? ClosestTo(this Actor[] range, WPos origin, Actor? exclude = null)
    {
        Actor? best = null;
        var bestDistSq = float.MaxValue;
        for (var i = 0; i < range.Length; ++i)
        {
            var a = range[i];
            if (a == exclude)
                continue;
            var distSq = (a.Position - origin).LengthSq();
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = a;
            }
        }
        return best;
    }

    public static Actor? ClosestTo(this List<Actor> range, WPos origin, Actor? exclude = null)
    {
        Actor? best = null;
        var bestDistSq = float.MaxValue;
        var count = range.Count;
        for (var i = 0; i < count; ++i)
        {
            var a = range[i];
            if (a == exclude)
                continue;
            var distSq = (a.Position - origin).LengthSq();
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = a;
            }
        }
        return best;
    }

    public static Actor? FarthestFrom(this Actor[] range, WPos origin, Actor? exclude = null)
    {
        Actor? best = null;
        var bestDistSq = float.MinValue;
        for (var i = 0; i < range.Length; ++i)
        {
            var a = range[i];
            if (a == exclude)
                continue;
            var distSq = (a.Position - origin).LengthSq();
            if (distSq > bestDistSq)
            {
                bestDistSq = distSq;
                best = a;
            }
        }
        return best;
    }

    public static ReadOnlySpan<Actor> SpanRange(this List<Actor> list, int start, int count = int.MaxValue)
    {
        var total = list.Count;
        if (start >= total)
            return [];
        return CollectionsMarshal.AsSpan(list).Slice(start, Math.Min(count, total - start));
    }

    public static bool AnyInRadius(this ReadOnlySpan<Actor> range, WPos origin, float radius)
    {
        for (var i = 0; i < range.Length; ++i)
        {
            if (range[i].Position.InCircle(origin, radius))
                return true;
        }
        return false;
    }

    public static Actor? FirstInRadius(this ReadOnlySpan<Actor> range, WPos origin, float radius)
    {
        for (var i = 0; i < range.Length; ++i)
        {
            if (range[i].Position.InCircle(origin, radius))
                return range[i];
        }
        return null;
    }

    public static Actor? FirstInRadius(this List<Actor> range, WPos origin, float radius) => FirstInRadius(CollectionsMarshal.AsSpan(range), origin, radius);

    public static void SortedByRange(this Actor[] range, WPos origin, List<Actor> result, Actor? exclude = null)
    {
        result.Clear();
        for (var i = 0; i < range.Length; ++i)
        {
            var a = range[i];
            if (a == exclude)
                continue;
            var distSq = (a.Position - origin).LengthSq();
            var j = result.Count;
            result.Add(a);
            while (j > 0 && (result[j - 1].Position - origin).LengthSq() > distSq)
            {
                result[j] = result[j - 1];
                --j;
            }
            result[j] = a;
        }
    }

    public static void SortedByRange(this (int, Actor)[] range, WPos origin, List<(int, Actor)> result, int excludeSlot = -1)
    {
        result.Clear();
        for (var i = 0; i < range.Length; ++i)
        {
            var e = range[i];
            if (e.Item1 == excludeSlot)
                continue;
            var distSq = (e.Item2.Position - origin).LengthSq();
            var j = result.Count;
            result.Add(e);
            while (j > 0 && (result[j - 1].Item2.Position - origin).LengthSq() > distSq)
            {
                result[j] = result[j - 1];
                --j;
            }
            result[j] = e;
        }
    }
}
