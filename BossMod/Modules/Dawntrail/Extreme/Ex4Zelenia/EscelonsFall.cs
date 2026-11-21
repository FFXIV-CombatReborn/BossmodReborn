namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class EscelonsFall(BossModule module) : Components.GenericBaitAway(module, (uint)AID.EscelonsFall)
{
    public enum Mechanic { None, Near, Far }
    public Mechanic CurMechanic;
    private DateTime _activation;
    private readonly List<Mechanic> order = [with(4)];

    private static readonly AOEShapeCone cone = new(24f, 22.5f.Degrees());

    public override void Update()
    {
        if (CurMechanic == Mechanic.None)
            return;

        CurrentBaits.Clear();

        var party = Raid.WithoutSlot(false, true, true);
        var len = party.Length;

        (Actor actor, float distSq)[] distances = new (Actor, float)[len];
        var center = Arena.Center;

        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            var distSq = (p.Position - center).LengthSq();
            distances[i] = (p, distSq);
        }

        var isNear = CurMechanic == Mechanic.Near;

        var targets = Math.Min(4, len);
        for (var i = 0; i < targets; ++i)
        {
            var selIdx = i;
            for (var j = i + 1; j < len; ++j)
            {
                var distJSq = distances[j].distSq;
                var distSelIdx = distances[selIdx].distSq;
                var isBetter = isNear ? distJSq < distSelIdx : distJSq > distSelIdx;
                if (isBetter)
                    selIdx = j;
            }

            if (selIdx != i)
            {
                (distances[selIdx], distances[i]) = (distances[i], distances[selIdx]);
            }

            CurrentBaits.Add(new(Module.PrimaryActor, distances[i].actor, cone, _activation));
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        var count = order.Count;
        if (count > 0)
        {
            var sb = new StringBuilder(4 * (count - 1) + count * 4);
            var ord = CollectionsMarshal.AsSpan(order);
            for (var i = 0; i < count; ++i)
            {
                sb.Append(ord[i]);

                if (i < count - 1)
                    sb.Append(" -> ");
            }
            hints.Add(sb.ToString());
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.EscelonsFall)
        {
            _activation = status.ExpireAt.AddSeconds(12.9d);
            var distance = status.Extra switch
            {
                0x2F6 => Mechanic.Near,
                0x2F7 => Mechanic.Far,
                _ => Mechanic.None
            };
            if (distance != Mechanic.None)
            {
                order.Add(distance);
                if (CurMechanic == Mechanic.None)
                    CurMechanic = distance;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            if ((NumCasts & 3) == 0)
            {
                if (order.Count != 0)
                {
                    order.RemoveAt(0);
                }
                ForbiddenPlayers = default;
                CurMechanic = order.Count != 0 ? order[0] : Mechanic.None;
            }
            ForbiddenPlayers.Set(Raid.FindSlot(spell.MainTargetID));
            _activation = WorldState.FutureTime(3d);
        }
    }
}
