namespace BossMod.Dawntrail.Criterion.C01AMT.C012LoneSwordmaster;

[Flags]
enum Side
{
    None = 0,
    E = 1,
    W = 2,
    S = 4,
    N = 8,
    All = N | E | S | W
}

static class SideHelpers
{
    public static bool Matches(this Side a, Side b) => (a & b) != Side.None;

    public static Side RotateR(this Side a)
    {
        var s = Side.None;
        if ((a & Side.E) != default)
        {
            s |= Side.S;
        }
        if ((a & Side.S) != default)
        {
            s |= Side.W;
        }
        if ((a & Side.W) != default)
        {
            s |= Side.N;
        }
        if ((a & Side.N) != default)
        {
            s |= Side.E;
        }
        return s;
    }

    public static Side RotateL(this Side a)
    {
        var s = Side.None;
        if ((a & Side.E) != default)
        {
            s |= Side.N;
        }
        if ((a & Side.N) != default)
        {
            s |= Side.W;
        }
        if ((a & Side.W) != default)
        {
            s |= Side.S;
        }
        if ((a & Side.S) != default)
        {
            s |= Side.E;
        }
        return s;
    }

    public static Side FromDirection(WDir d)
    {
        if (Math.Abs(d.X) < Math.Abs(d.Z))
        {
            return d.Z > 0f ? Side.S : Side.N;
        }
        return d.X > 0f ? Side.E : Side.W;
    }

    public static Side FromAngle(Angle a) => FromDirection(a.ToDirection());

    public static Side FromTether(uint tetherID) => tetherID switch
    {
        (uint)TetherID.TetherE => Side.E,
        (uint)TetherID.TetherN => Side.N,
        (uint)TetherID.TetherS => Side.S,
        (uint)TetherID.TetherW => Side.W,
        _ => Side.None
    };
}

sealed class Malefic(BossModule module) : BossComponent(module)
{
    private readonly Side[] _playerStates = new Side[PartyState.MaxPartySize];

    public Side this[int index]
    {
        get => (uint)index < PartyState.MaxPartySize ? _playerStates[index] : Side.None;
        set
        {
            if ((uint)index < PartyState.MaxPartySize)
            {
                _playerStates[index] = value;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is >= (uint)SID.MaleficE and <= (uint)SID.MaleficNSEW)
        {
            this[Raid.FindSlot(actor.InstanceID)] = (Side)(status.ID - ((uint)SID.MaleficE - 1u));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is >= (uint)SID.MaleficE and <= (uint)SID.MaleficNSEW)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (this[slot] == (Side)(status.ID - ((uint)SID.MaleficE - 1u)))
            {
                this[slot] = Side.None;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var party = Raid.WithSlot(false, true, true);
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var p = ref party[i];
            DrawSides(Arena, p.Item2, this[p.Item1], Colors.Enemy);
        }
    }

    public static void DrawSides(MiniArena arena, Actor actor, Side state, uint color)
    {
        if (state == Side.None)
        {
            return;
        }
        if ((state & Side.E) != default)
        {
            DrawSide(arena, actor, 90f, color);
        }
        if ((state & Side.W) != default)
        {
            DrawSide(arena, actor, -90f, color);
        }
        if ((state & Side.N) != default)
        {
            DrawSide(arena, actor, 180f, color);
        }
        if ((state & Side.S) != default)
        {
            DrawSide(arena, actor, default, color);
        }
    }

    private static void DrawSide(MiniArena arena, Actor actor, float deg, uint color)
    {
        arena.PathArcTo(actor.Position, 1.5f, (deg - 35f).Degrees().Rad, (deg + 35f).Degrees().Rad);
        arena.PathStroke(false, color);
    }
}

sealed class MaleficPortentCounter(BossModule module) : Components.CastCounter(module, (uint)AID.MaleficPortent);

sealed class MaleficPortent(BossModule module) : Components.CastCounter(module, (uint)AID.MaleficPortent)
{
    private readonly Side[] _sides = new Side[PartyState.MaxPartySize];
    private BitMask _targets;
    private BitMask _forceOfWillTargets;
    private readonly Malefic _malefic = module.FindComponent<Malefic>()!;

    public bool Active => _targets.Any();

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.UnyieldingWill)
        {
            var targetSlot = Raid.FindSlot(tether.Target);
            if ((uint)targetSlot < PartyState.MaxPartySize)
            {
                _forceOfWillTargets.Set(targetSlot);
            }
            return;
        }

        var side = SideHelpers.FromTether(tether.ID);
        if (side != Side.None)
        {
            var slot = Raid.FindSlot(source.InstanceID);
            if ((uint)slot < PartyState.MaxPartySize)
            {
                _sides[slot] = side;
                _targets.Set(slot);
            }
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (SideHelpers.FromTether(tether.ID) != Side.None)
        {
            var slot = Raid.FindSlot(source.InstanceID);
            if ((uint)slot < PartyState.MaxPartySize)
            {
                _sides[slot] = Side.None;
                _targets.Clear(slot);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_targets.Any())
        {
            return;
        }
        var party = Raid.WithSlot(false, true, true);
        var len = party.Length;
        var origin = Module.PrimaryActor.Position;
        for (var i = 0; i < len; ++i)
        {
            ref var p = ref party[i];
            var slot = p.Item1;
            if (_targets[slot])
            {
                var shouldTake = ShouldTake(pcSlot, slot);
                Arena.AddLine(origin, p.Item2.Position, shouldTake ? Colors.Safe : Colors.Danger, shouldTake && !_targets[pcSlot] ? 2f : 1f);
                Malefic.DrawSides(Arena, p.Item2, _sides[slot], Colors.Danger);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets.Any() && _forceOfWillTargets.Any() && !HasCorrectTether(slot))
        {
            hints.Add(_targets[slot] ? "Pass tether!" : "Take tether!");
        }
    }

    private bool HasCorrectTether(int slot) => (uint)slot < PartyState.MaxPartySize && _targets[slot] != _forceOfWillTargets[slot] && !_malefic[slot].Matches(_sides[slot]);

    private bool ShouldTake(int slot, int targetSlot) => (uint)slot < PartyState.MaxPartySize && !_targets[slot] && !_forceOfWillTargets[slot] && !HasCorrectTether(targetSlot) && !_malefic[slot].Matches(_sides[targetSlot]);
}

sealed class ForceOfWill(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _line = new(40f, 2f);
    private static readonly AOEShapeRect _side = new(40f, 40f, -2f);
    private readonly Malefic _malefic = module.FindComponent<Malefic>()!;
    private readonly List<(Actor From, Actor To, int Order)> _tethers = [];
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _appearedAt;
    private bool _bound;

    private Actor? RealSource(Actor target)
    {
        Actor? parent = null;
        var count = _tethers.Count;
        for (var depth = 0; depth < count; ++depth)
        {
            Actor? from = null;
            for (var i = 0; i < count; ++i)
            {
                var t = _tethers[i];
                if (t.To == target)
                {
                    from = t.From;
                    break;
                }
            }
            if (from == null)
            {
                break;
            }
            parent = target = from;
        }
        return parent;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _tethers.Count;
        if (count == 0)
        {
            return [];
        }
        _aoes.Clear();
        ulong parentID = 0;

        if (RealSource(actor) is Actor force)
        {
            parentID = force.InstanceID;
            if (!_bound)
            {
                var bindAt = _appearedAt.AddSeconds(6.8d);
                var sides = _malefic[slot];
                var rot = force.Rotation;
                var center = SideHelpers.FromAngle(rot + 180f.Degrees());
                if (sides.Matches(center))
                {
                    _aoes.Add(new(_line, force.Position, rot, bindAt));
                }
                if (sides.Matches(center.RotateL()))
                {
                    _aoes.Add(new(_side, force.Position, rot + 90f.Degrees(), bindAt));
                }
                if (sides.Matches(center.RotateR()))
                {
                    _aoes.Add(new(_side, force.Position, rot - 90f.Degrees(), bindAt));
                }
            }
        }

        for (var i = 0; i < count; ++i)
        {
            var (from, to, order) = _tethers[i];
            if (to == actor || from.InstanceID == parentID)
            {
                continue;
            }
            var dir = to.Position - from.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), 2f), from.Position, dir.ToAngle(), _appearedAt.AddSeconds(8d + 0.5d * order)));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = _tethers.Count;
        for (var i = 0; i < count; ++i)
        {
            var t = _tethers[i];
            if (t.To == pc)
            {
                if (t.From.OID != (uint)OID.ForceOfWillWallTether)
                {
                    var dir = pc.Position - t.From.Position;
                    var len = dir.Length();
                    if (len > 0.01f)
                    {
                        Arena.AddRect(t.From.Position, dir / len, len, default, 2f, Colors.Danger);
                    }
                }
                return;
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.UnyieldingWill || WorldState.Actors.Find(tether.Target) is not Actor target)
        {
            return;
        }
        if (_appearedAt == default)
        {
            _appearedAt = WorldState.CurrentTime;
        }

        for (var i = _tethers.Count - 1; i >= 0; --i)
        {
            if (_tethers[i].From == source)
            {
                _tethers.RemoveAt(i);
            }
        }
        _tethers.Add((source, target, source.OID == (uint)OID.ForceOfWillWallTether ? 0 : 1));

        if (target.OID == (uint)OID.ForceOfWillGroundTether)
        {
            var dir = source.Rotation.ToDirection();
            var targetDist = (target.Position - source.Position).Length();
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                var offset = p.Position - source.Position;
                if (offset.InRect(dir, 60f, default, 2f) && Math.Abs(offset.Dot(dir) - targetDist) < 0.1f)
                {
                    _tethers.Add((target, p, 1));
                    break;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.UnyieldingWillWall:
                ++NumCasts;
                for (var i = _tethers.Count - 1; i >= 0; --i)
                {
                    if (_tethers[i].From.OID == (uint)OID.ForceOfWillWallTether)
                    {
                        _tethers.RemoveAt(i);
                    }
                }
                break;
            case (uint)AID.UnyieldingWillGround:
                ++NumCasts;
                for (var i = _tethers.Count - 1; i >= 0; --i)
                {
                    if (_tethers[i].To.OID == default)
                    {
                        _tethers.RemoveAt(i);
                    }
                }
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Bind)
        {
            _bound = true;
        }
    }
}

sealed class WillOfTheUnderworldSmallSlow(BossModule module) : Components.GenericAOEs(module, (uint)AID.WillOfTheUnderworldSmallSlow)
{
    private static readonly AOEShapeRect _rect = new(40f, 5f);
    private readonly Malefic _malefic = module.FindComponent<Malefic>()!;
    private readonly List<(AOEInstance AOE, Side Hit)> _casters = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
        {
            return [];
        }
        _aoes.Clear();
        var sides = _malefic[slot];
        for (var i = 0; i < count; ++i)
        {
            var c = _casters[i];
            if (sides.Matches(c.Hit))
            {
                _aoes.Add(c.AOE);
            }
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _casters.Add((new(_rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID), SideHelpers.FromAngle(spell.Rotation + 180f.Degrees())));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            var id = caster.InstanceID;
            for (var i = _casters.Count - 1; i >= 0; --i)
            {
                if (_casters[i].AOE.ActorID == id)
                {
                    _casters.RemoveAt(i);
                }
            }
        }
    }
}

sealed class WillOfTheUnderworldSmallFast(BossModule module) : Components.GenericAOEs(module, (uint)AID.WillOfTheUnderworldSmallFast)
{
    private static readonly AOEShapeRect _rect = new(40f, 5f);
    private readonly Malefic _malefic = module.FindComponent<Malefic>()!;
    private readonly Side[] _predictedSide = new Side[PartyState.MaxPartySize];
    private readonly List<(Actor Caster, DateTime Predicted)> _casters = [];
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _tethersAt = DateTime.MaxValue;
    private int _numSpawned;
    public bool Risky = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Math.Min(_casters.Count, 4);
        if (count == 0)
        {
            return [];
        }
        _aoes.Clear();
        var sides = _malefic[slot];
        var predicted = (uint)slot < PartyState.MaxPartySize ? _predictedSide[slot] : Side.None;
        for (var i = 0; i < count; ++i)
        {
            var (caster, activation) = _casters[i];
            if (_tethersAt < activation)
            {
                sides |= predicted;
            }
            if (sides.Matches(SideHelpers.FromAngle(caster.Rotation + 180f.Degrees())))
            {
                _aoes.Add(new(_rect, caster.Position, caster.Rotation, activation, risky: Risky));
            }
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ForceOfWillSmall)
        {
            _casters.Add((actor, WorldState.FutureTime(_numSpawned < 2 ? 8.7d : _numSpawned < 4 ? 6.8d : 7d)));
            ++_numSpawned;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var side = SideHelpers.FromTether(tether.ID);
        if (side != Side.None)
        {
            var slot = Raid.FindSlot(source.InstanceID);
            if ((uint)slot < PartyState.MaxPartySize)
            {
                _tethersAt = WorldState.FutureTime(7d);
                _predictedSide[slot] = side;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = _casters.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_casters[i].Caster == caster)
                {
                    _casters.Ref(i).Predicted = Module.CastFinishAt(spell);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            for (var i = _casters.Count - 1; i >= 0; --i)
            {
                if (_casters[i].Caster == caster)
                {
                    _casters.RemoveAt(i);
                }
            }
        }
    }
}

sealed class MaleficAlignment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MaleficAlignment, new AOEShapeCone(40f, 45f.Degrees()))
{
    private readonly Malefic _malefic = module.FindComponent<Malefic>()!;
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }
        _aoes.Clear();
        var sides = _malefic[slot];
        var casters = CollectionsMarshal.AsSpan(Casters);
        for (var i = 0; i < count; ++i)
        {
            ref var c = ref casters[i];
            if (sides.Matches(SideHelpers.FromAngle(c.Rotation + 180f.Degrees())))
            {
                _aoes.Add(c);
            }
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }
}

sealed class WillOfTheUnderworldLarge(BossModule module) : Components.GenericAOEs(module, (uint)AID.WillOfTheUnderworldLarge)
{
    private static readonly AOEShapeRect _full = new(40f, 10f);
    private readonly Malefic _malefic = module.FindComponent<Malefic>()!;
    private readonly List<(Actor Caster, DateTime Predicted)> _casters = [];
    private readonly List<AOEInstance> _aoes = [];
    private List<Actor>? _rocks;
    private int _numSpawned;
    public bool Risky;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Math.Min(_casters.Count, 4);
        if (count == 0)
        {
            return [];
        }
        _aoes.Clear();
        var sides = _malefic[slot];
        var numRocks = _rocks?.Count ?? 0;
        const float rockR = SilentEightRock.RockRadius;

        for (var i = 0; i < count; ++i)
        {
            var (c, activation) = _casters[i];
            var rot = c.Rotation;
            if (!sides.Matches(SideHelpers.FromAngle(rot + 180f.Degrees())))
            {
                continue;
            }

            var dir = rot.ToDirection();
            var origin = c.Position;
            Actor? hitRock = null;
            for (var j = 0; j < numRocks; ++j)
            {
                var r = _rocks![j];
                if (!r.IsDead && r.Position.InRect(origin, dir, 40f, default, 10f))
                {
                    hitRock = r;
                    break;
                }
            }

            if (hitRock == null)
            {
                _aoes.Add(new(_full, origin, rot, activation, risky: Risky));
                continue;
            }

            var rockDir = hitRock.Position - origin;
            var lenFront = dir.Dot(rockDir);
            var lateral = dir.Cross(rockDir);
            var lenBehind = 40f - lenFront;
            var widthR = 10f - lateral - rockR;
            if (widthR > 0f)
            {
                _aoes.Add(new(new AOEShapeRect(lenBehind, widthR * 0.5f), hitRock.Position + dir.OrthoR() * (widthR * 0.5f + rockR), rot, activation, risky: Risky));
            }
            var widthL = 10f + lateral - rockR;
            if (widthL > 0f)
            {
                _aoes.Add(new(new AOEShapeRect(lenBehind, widthL * 0.5f), hitRock.Position + dir.OrthoL() * (widthL * 0.5f + rockR), rot, activation, risky: Risky));
            }
            _aoes.Add(new(new AOEShapeRect(lenFront, 10f), origin, rot, activation, risky: Risky));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ForceOfWillLarge)
        {
            _rocks ??= Module.Enemies((uint)OID.Rock);
            _casters.Add((actor, WorldState.FutureTime(_numSpawned < 2 ? 8.7d : _numSpawned < 4 ? 6.8d : 7d)));
            ++_numSpawned;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = _casters.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_casters[i].Caster == caster)
                {
                    _casters.Ref(i).Predicted = Module.CastFinishAt(spell);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            for (var i = _casters.Count - 1; i >= 0; --i)
            {
                if (_casters[i].Caster == caster)
                {
                    _casters.RemoveAt(i);
                }
            }
        }
    }
}
