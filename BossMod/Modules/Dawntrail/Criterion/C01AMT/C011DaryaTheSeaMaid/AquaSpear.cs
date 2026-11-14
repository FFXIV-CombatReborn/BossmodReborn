namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

sealed class AquaSpearCast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaterZone1, new AOEShapeRect(8f, 4f));

sealed class AquaSpearPlayer(BossModule module) : Components.GenericAOEs(module, (uint)AID.WaterZone3)
{
    private static readonly AOEShapeRect _tile = new(4f, 4f, 4f);
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
        {
            return [];
        }
        _aoes.Clear();
        var party = Raid.WithoutSlot(false, true, true);
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            if (p != actor)
            {
                _aoes.Add(new(_tile, C011DaryaTheSeaMaid.TileCenter(p.Position), default, _activation));
            }
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_activation != default)
        {
            _tile.Outline(Arena, C011DaryaTheSeaMaid.TileCenter(pc.Position), default, Colors.Danger);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _activation = default;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.YellowTile)
        {
            _activation = WorldState.FutureTime(6d);
        }
    }
}

sealed class AquaSpearTile(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _tile = new(4f, 4f, 4f);
    private readonly List<(Actor Actor, DateTime Activation)> _tiles = [];
    private readonly List<AOEInstance> _aoes = [];

    public bool Active => _tiles.Count != 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _tiles.Count;
        if (count == 0)
        {
            return [];
        }
        _aoes.Clear();
        for (var i = 0; i < count; ++i)
        {
            var (tile, activation) = _tiles[i];
            if (tile.EventState != 7)
            {
                _aoes.Add(new(_tile, tile.Position, default, activation));
            }
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.WaterTile)
        {
            _tiles.Add((actor, WorldState.FutureTime(2d)));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == (uint)STATE.ThirdState && actor.OID == (uint)OID.WaterTile)
        {
            for (var i = _tiles.Count - 1; i >= 0; --i)
            {
                if (_tiles[i].Actor == actor)
                {
                    _tiles.RemoveAt(i);
                    break;
                }
            }
        }
    }
}

sealed class CrossCurrent(BossModule module) : Components.GenericAOEs(module, (uint)AID.CrossCurrent, "GTFO from bait!")
{
    private static readonly AOEShapeCross _cross = new(36f, 4f);
    private readonly List<AOEInstance> _aoes = [];
    private BitMask _targets;
    private DateTime _activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_targets.Any())
        {
            return [];
        }
        _aoes.Clear();
        var party = Raid.WithSlot(false, true, true);
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var p = ref party[i];
            if (_targets[p.Item1] && p.Item2 != actor)
            {
                _aoes.Add(new(_cross, C011DaryaTheSeaMaid.TileCenter(p.Item2.Position), default, _activation));
            }
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_targets[pcSlot])
        {
            _cross.Outline(Arena, C011DaryaTheSeaMaid.TileCenter(pc.Position), default, Colors.Danger);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.CrossCurrent)
        {
            var slot = Raid.FindSlot(targetID);
            if (slot >= 0)
            {
                _targets.Set(slot);
                _activation = WorldState.FutureTime(6.1d);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _targets.Reset();
        }
    }
}
