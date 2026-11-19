namespace BossMod.Dawntrail.Alliance.A30Shantoto;

sealed class AeroDynamics(BossModule module) : Components.GenericKnockback(module, stopAtWall: false)
{
    private readonly List<SafeWall> _walls = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        //Build safewalls when we see the phase start, walk 6.5f left and right from the actors, then draw it 24 towards the edge (usually over the edge) to make sure we don't leave any gaps. It's an approximation but seems good enough in practice.
        if (spell.Action.ID is var id && id == (uint)AID.SuperiorStoneII1)
        {
            var left = caster.Position + caster.Rotation.ToDirection() + 90.Degrees().ToDirection() * 6.5f;
            var leftback = left + caster.Rotation.ToDirection() * 24f;
            _walls.Add(new(left, leftback));
            var right = caster.Position + caster.Rotation.ToDirection() - 90.Degrees().ToDirection() * 6.5f;
            var rightback = right + caster.Rotation.ToDirection() * 24f;
            _walls.Add(new(right, rightback));
        }
        else if (id == (uint)AID.GroundbreakingQuake)
        {
            _walls.Clear();
        }
    }

    private enum Direction : uint
    {
        None,
        West = 90,
        East = 270,
    }

    private class StatusKB(int slot, Direction direction, DateTime activation)
    {
        public int Slot => slot;
        public Direction Direction => direction;
        public DateTime Activation => activation;
    }

    private readonly List<StatusKB> _statuskbs = [];

    private StatusKB? FindStatusKB(int slot)
    {
        var count = _statuskbs.Count;
        for (var i = 0; i < count; ++i)
        {
            var k = _statuskbs[i];
            if (k.Slot == slot)
            {
                return k;
            }
        }
        return null;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is var id && id == (uint)SID.WesterlyWinds)
        {
            var p = Raid.FindSlot(actor.InstanceID);
            if (p >= 0)
            {
                _statuskbs.Add(new(p, Direction.West, status.ExpireAt));
                //Service.Log($"Adding West: {p}");
            }
        }
        else if (id == (uint)SID.EasterlyWinds)
        {
            var p = Raid.FindSlot(actor.InstanceID);
            if (p >= 0)
            {
                _statuskbs.Add(new(p, Direction.East, status.ExpireAt));
                //Service.Log($"Adding East: {p}");
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.EasterlyWinds or (uint)SID.WesterlyWinds)
        {
            var p = Raid.FindSlot(actor.InstanceID);
            if (p >= 0)
            {
                _statuskbs.Clear();
            }
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_statuskbs.Count == 0)
        {
            return [];
        }
        var kb = FindStatusKB(slot);
        if (kb != null)
        {
            var _knockbacks = new Knockback[1];
            if (kb.Direction != Direction.None)
            {
                _knockbacks[0] = new Knockback(actor.Position, 40f, kb.Activation, direction: ((float)kb.Direction).Degrees(), kind: Kind.DirForward, safeWalls: _walls, ignoreImmunes: false);
                return _knockbacks;
            }
        }
        return [];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var kb = FindStatusKB(slot);
        if (kb == null || kb.Direction == Direction.None || IsImmune(slot, WorldState.CurrentTime))
        {
            return;
        }
        var len = _walls.Count;
        if (len > 0)
        {
            var dir = ((float)kb.Direction).Degrees().ToDirection();
            hints.AddForbiddenZone(new SDKnockbackFixedDirectionAgainstSafewalls(dir, [.. _walls], 60f, len), WorldState.CurrentTime);
        }
    }
}
