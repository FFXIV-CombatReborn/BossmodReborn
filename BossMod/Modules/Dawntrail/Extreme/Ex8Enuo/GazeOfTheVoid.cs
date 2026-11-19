namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

sealed class GazeOfTheVoidAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GazeOfTheVoid2, new AOEShapeCone(40f, 22.5f.Degrees()), 7); // This is the easy part!

sealed class GazeOfTheVoidSoaks(BossModule module) : BossComponent(module)
{
    private struct Orb(Actor actor, bool imminent, bool big)
    {
        public readonly Actor Actor = actor;
        public bool Imminent = imminent;
        public readonly bool Big = big;
        public int ClockOrder = -1;
        public readonly float Radius => Big ? 6f : 5f;
    }

    private readonly List<Orb> _orbs = [with(8)];
    private readonly DateTime[] _vuln = new DateTime[PartyState.MaxPartySize];
    private bool _ordered;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.ClockSlow or (uint)TetherID.ClockFast)
        {
            var count = _orbs.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_orbs[i].Actor == source)
                {
                    return;
                }
            }
            _orbs.Add(new(source, tether.ID == (uint)TetherID.ClockFast, source.OID == (uint)OID.VoidSoakLarge));
            Organize(stackalloc float[8]);
        }
    }

    private void Organize(Span<float> keys)
    {
        if (_ordered || _orbs.Count != 8)
        {
            return;
        }
        var center = Arena.Center;
        var orbs = CollectionsMarshal.AsSpan(_orbs);
        for (var g = 0; g < 2; ++g)
        {
            var imminent = g == 1;
            var start = -1;
            for (var i = 0; i < 8; ++i)
            {
                if (orbs[i].Imminent == imminent && orbs[i].Big)
                {
                    start = i;
                    break;
                }
            }
            if (start < 0)
            {
                return;
            }
            var startAngle = Angle.FromDirection(orbs[start].Actor.Position - center).Rad;
            for (var i = 0; i < 8; ++i)
            {
                var d = startAngle - Angle.FromDirection(orbs[i].Actor.Position - center).Rad;
                if (d < 0f)
                {
                    d += 2f * MathF.PI;
                }
                keys[i] = i == start ? 0f : d;
            }
            for (var i = 0; i < 8; ++i)
            {
                if (orbs[i].Imminent != imminent)
                {
                    continue;
                }
                var order = 0;
                for (var j = 0; j < 8; ++j)
                {
                    if (j != i && orbs[j].Imminent == imminent && keys[j] < keys[i])
                    {
                        ++order;
                    }
                }
                orbs[i].ClockOrder = order;
            }
        }
        _ordered = true;
    }

    private bool CanSoak(int slot, Actor actor, in Orb orb)
        => orb.Imminent && orb.Big == (actor.Role == Role.Tank) && (slot >= _vuln.Length || _vuln[slot] < WorldState.CurrentTime);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID.Burst or (uint)AID.ViolentBurst))
        {
            return;
        }
        var count = _orbs.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_orbs[i].Actor == caster)
            {
                var order = _orbs[i].ClockOrder;
                _orbs.RemoveAt(i);
                if (order >= 0)
                {
                    var orbs = CollectionsMarshal.AsSpan(_orbs);
                    var len = orbs.Length;
                    for (var j = 0; j < len; ++j)
                    {
                        if (orbs[j].ClockOrder == order)
                        {
                            orbs[j].Imminent = true;
                        }
                    }
                }
                if (_orbs.Count == 0)
                {
                    _ordered = false;
                }
                return;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.MagicVulnerabilityUp && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _vuln.Length)
        {
            _vuln[slot] = status.ExpireAt;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        var count = _orbs.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_orbs[i].Actor == actor)
            {
                _orbs.RemoveAt(i);
                if (_orbs.Count == 0)
                {
                    _ordered = false;
                }
                return;
            }
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_orbs.Count != 0)
        {
            hints.Add("Soak the orbs in pairs!");
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = _orbs.Count;
        if (count == 0)
        {
            return;
        }
        var allowed = false;
        var soaking = false;
        var orbs = CollectionsMarshal.AsSpan(_orbs);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var o = ref orbs[i];
            var canSoak = CanSoak(slot, actor, o);
            allowed |= canSoak;
            if (actor.Position.InCircle(o.Actor.Position, o.Radius))
            {
                if (canSoak)
                {
                    soaking = true;
                }
                else
                {
                    hints.Add("GTFO from orb!");
                }
            }
        }
        if (allowed && !soaking)
        {
            hints.Add("Soak an orb!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _orbs.Count;
        if (count == 0)
        {
            return;
        }
        var orbs = CollectionsMarshal.AsSpan(_orbs);
        var soakable = 0;
        for (var i = 0; i < count; ++i)
        {
            if (CanSoak(slot, actor, orbs[i]))
            {
                ++soakable;
            }
        }
        var goals = soakable != 0 ? new ShapeDistance[soakable] : null;
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            ref readonly var o = ref orbs[i];
            var pos = o.Actor.Position;
            if (goals != null && CanSoak(slot, actor, o))
            {
                goals[index++] = new SDInvertedCircle(pos, o.Radius - 0.5f);
            }
            else
            {
                hints.AddForbiddenZone(new SDCircle(pos, o.Radius), DateTime.MaxValue); // not ours yet, or vulnerable
            }
        }
        if (goals != null)
        {
            hints.AddForbiddenZone(new SDIntersection(goals), DateTime.MaxValue); // stand in any orb we're allowed to soak
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var orbs = CollectionsMarshal.AsSpan(_orbs);
        var len = orbs.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var o = ref orbs[i];
            Arena.ZoneCircleOutline(o.Actor.Position, o.Radius, CanSoak(pcSlot, pc, o) ? Colors.Safe : Colors.Danger);
        }
    }
}

sealed class Gauntlet(BossModule module) : Components.GenericInvincible(module, "Attack your gauntlet add!")
{
    private const uint TakenFirst = (uint)SID.GauntletTaken5, TakenLast = (uint)SID.GauntletTaken; // 5357-5364
    private const uint ThrownFirst = (uint)SID.GauntletThrown6, ThrownLast = (uint)SID.GauntletThrown2; // 5365-5372
    private readonly int[] _playerOrder = [-1, -1, -1, -1, -1, -1, -1, -1];
    private readonly Actor?[] _adds = new Actor?[8];
    private readonly Actor[] _forbiddenBuffer = new Actor[8];

    protected override ReadOnlySpan<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var order = slot >= 0 && slot < _playerOrder.Length ? _playerOrder[slot] : -1;
        if (order < 0)
        {
            return [];
        }
        var index = 0;
        for (var i = 0; i < 8; ++i)
        {
            if (i != order && _adds[i] is Actor add)
            {
                _forbiddenBuffer[index++] = add;
            }
        }
        return _forbiddenBuffer.AsSpan(0, index);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var id = status.ID;
        if (id is >= TakenFirst and <= TakenLast && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _playerOrder.Length)
        {
            _playerOrder[slot] = (int)(id - TakenFirst);
        }
        else if (id is >= ThrownFirst and <= ThrownLast)
        {
            _adds[id - ThrownFirst] = actor;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        var id = status.ID;
        if (id is >= TakenFirst and <= TakenLast && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _playerOrder.Length)
        {
            _playerOrder[slot] = -1;
        }
        else if (id is >= ThrownFirst and <= ThrownLast && _adds[id - ThrownFirst] == actor)
        {
            _adds[id - ThrownFirst] = null;
        }
    }
}
