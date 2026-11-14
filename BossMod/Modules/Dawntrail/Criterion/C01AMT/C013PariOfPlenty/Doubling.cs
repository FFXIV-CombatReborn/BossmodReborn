namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

sealed class Doubling(BossModule module) : Components.CastTowers(module, (uint)AID.Explosion, 4f)
{
    private enum RoleDirection { North, East, South, West }

    private static readonly WPos _southPos = new(-759.988f, -785.093f);
    private static readonly WPos _westPos = new(-779.885f, -804.986f);
    private static readonly WPos _eastPos = new(-740.095f, -805f);
    private static readonly WPos _northPos = new(-760f, -824.9f);

    private readonly List<(ulong Player, RoleDirection Direction)> _tetheredRoles = [];
    private readonly List<ulong> _eastPlayers = [];
    private readonly List<ulong> _priorityPlayers = [];
    private readonly List<Tower> _eastTowers = [];
    private readonly List<Tower> _westTowers = [];
    private int _side;
    private bool _towerWave1;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        switch (spell.Action.ID)
        {
            case (uint)AID.FableflightLeftFast:
            case (uint)AID.FableflightLeftSlow:
                if (caster.Position.AlmostEqual(_northPos, 1f))
                {
                    _side = 1;
                }
                break;
            case (uint)AID.FableflightRightFast:
            case (uint)AID.FableflightRightSlow:
                if (caster.Position.AlmostEqual(_northPos, 1f))
                {
                    _side = 2;
                }
                break;
            case (uint)AID.Explosion:
                SortTowerOrder();
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.Explosion)
        {
            if (!_towerWave1)
            {
                _towerWave1 = true;
                _priorityPlayers.RemoveRange(0, Math.Min(2, _priorityPlayers.Count));
            }
            SortTowerOrder();
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.Generic || _tetheredRoles.Count >= 4)
        {
            return;
        }
        var position = source.Position;
        RoleDirection direction;
        if (position.AlmostEqual(_northPos, 1f))
        {
            direction = RoleDirection.North;
        }
        else if (position.AlmostEqual(_eastPos, 1f))
        {
            direction = RoleDirection.East;
        }
        else if (position.AlmostEqual(_westPos, 1f))
        {
            direction = RoleDirection.West;
        }
        else if (position.AlmostEqual(_southPos, 1f))
        {
            direction = RoleDirection.South;
        }
        else
        {
            return;
        }

        var target = tether.Target;
        var count = _tetheredRoles.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_tetheredRoles[i].Player == target)
            {
                return;
            }
        }
        _tetheredRoles.Add((target, direction));

        if (_tetheredRoles.Count == 4)
        {
            for (var i = 0; i < 4; ++i)
            {
                var (player, role) = _tetheredRoles[i];
                var westSide = _side == 1 ? role is RoleDirection.West or RoleDirection.South : role is RoleDirection.North or RoleDirection.West;
                if (!westSide)
                {
                    _eastPlayers.Add(player);
                }
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Rug)
        {
            _priorityPlayers.Add(targetID);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Math.Min(Towers.Count, 4);
        if (count == 0)
        {
            return;
        }
        var id = pc.InstanceID;
        var numPriority = Math.Min(_priorityPlayers.Count, 2);
        var priority = false;
        for (var i = 0; i < numPriority; ++i)
        {
            if (_priorityPlayers[i] == id)
            {
                priority = true;
                break;
            }
        }
        var list = _eastPlayers.Contains(id) ? _eastTowers : _westTowers;
        var targetId = list.Count >= 2 ? list[priority ? 0 : 1].ActorID : 0ul;

        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            ref var tower = ref towers[i];
            tower.Shape.Outline(Arena, tower.Position, tower.Rotation, tower.ActorID == targetId ? Colors.Safe : Colors.Danger, 2f);
        }
    }

    private void SortTowerOrder()
    {
        _eastTowers.Clear();
        _westTowers.Clear();
        var centerX = Arena.Center.X;
        var count = Towers.Count;
        for (var i = 0; i < count; ++i)
        {
            var tower = Towers[i];
            (tower.Position.X > centerX ? _eastTowers : _westTowers).Add(tower);
        }
        if (_eastTowers.Count > 1)
        {
            _eastTowers.Sort((a, b) => TowerOrder(a, true).CompareTo(TowerOrder(b, true)));
        }
        if (_westTowers.Count > 1)
        {
            _westTowers.Sort((a, b) => TowerOrder(a, false).CompareTo(TowerOrder(b, false)));
        }
    }

    private float TowerOrder(in Tower tower, bool east)
    {
        var angle = Angle.FromDirection(tower.Position - Arena.Center);
        var startAngle = east == (_side == 1) ? 180f.Degrees() : default;
        var difference = startAngle.DistanceToAngle(angle).Rad;
        if (_side == 1)
        {
            difference = -difference;
        }
        if (difference < 0f)
        {
            difference += MathF.Tau;
        }
        return difference;
    }
}

sealed class CarpetRideBaited(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        float side;
        switch (spell.Action.ID)
        {
            case (uint)AID.CarpetRideTetherRight:
                side = -90f;
                break;
            case (uint)AID.CarpetRideTetherLeft:
                side = 90f;
                break;
            default:
                return;
        }
        var from = caster.Position;
        var dir = spell.LocXZ - from;
        _aoes.Add(new(new AOEShapeRect(40f, dir.Length() * 0.5f), from + dir * 0.5f, dir.ToAngle() + side.Degrees(), Module.CastFinishAt(spell), actorID: caster.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CarpetRideTetherRight or (uint)AID.CarpetRideTetherLeft)
        {
            ++NumCasts;
            var id = caster.InstanceID;
            for (var i = _aoes.Count - 1; i >= 0; --i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                }
            }
        }
    }
}

sealed class FableflightTether(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCone _coneRight = new(50f, 90f.Degrees(), -90f.Degrees());
    private static readonly AOEShapeCone _coneLeft = new(50f, 90f.Degrees(), 90f.Degrees());
    private readonly Actor?[] _sources = new Actor?[PartyState.MaxPartySize];
    private readonly DateTime[] _resolve = new DateTime[PartyState.MaxPartySize];
    private readonly List<Bait> _futureBaits = [];
    private bool _initialized;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Generic)
        {
            var slot = Raid.FindSlot(tether.Target);
            if ((uint)slot < PartyState.MaxPartySize)
            {
                _sources[slot] = source;
                Init();
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.CarpetRideTetherRight or (uint)AID.CarpetRideTetherLeft && CurrentBaits.Count != 0)
        {
            CurrentBaits.RemoveAt(0);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Explosion or (uint)AID.UnmitigatedExplosion && CurrentBaits.Count == 0 && _futureBaits.Count != 0)
        {
            CurrentBaits.AddRange(_futureBaits);
            _futureBaits.Clear();
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Prey)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if ((uint)slot < PartyState.MaxPartySize)
            {
                _resolve[slot] = status.ExpireAt;
                Init();
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = CurrentBaits.Count;
        if (count == 0)
        {
            return;
        }
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        Actor[]? party = null;
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            if (b.Target != actor)
            {
                hints.AddForbiddenZone(b.Shape, BaitOrigin(ref b), b.Rotation, b.Activation);
                continue;
            }
            party ??= Raid.WithoutSlot(false, true, true);
            var inverse = b.Shape == _coneRight ? _coneLeft : _coneRight;
            var origin = b.Source.Position;
            var len = party.Length;
            for (var j = 0; j < len; ++j)
            {
                var p = party[j];
                if (p != actor)
                {
                    hints.AddForbiddenZone(inverse, origin, b.Source.AngleTo(p), b.Activation);
                }
            }
        }
    }

    private void Init()
    {
        if (_initialized)
        {
            return;
        }
        var party = Raid.WithSlot(false, true, true);
        var len = party.Length;
        if (len == 0)
        {
            return;
        }
        for (var i = 0; i < len; ++i)
        {
            var slot = party[i].Item1;
            if (slot >= PartyState.MaxPartySize || _sources[slot] == null || _resolve[slot] == default)
            {
                return;
            }
        }
        _initialized = true;
        var threshold = WorldState.FutureTime(8d);
        for (var i = 0; i < len; ++i)
        {
            var (slot, player) = party[i];
            var src = _sources[slot]!;
            var res = _resolve[slot];
            var bait = new Bait(src, player, src.CastInfo?.Action.ID is (uint)AID.FableflightRightSlow or (uint)AID.FableflightRightFast ? _coneRight : _coneLeft, res);
            if (res > threshold)
            {
                _futureBaits.Add(bait);
            }
            else
            {
                CurrentBaits.Add(bait);
            }
        }
    }
}
