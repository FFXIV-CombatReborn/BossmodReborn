namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

sealed class ParisCurseStackSpread(BossModule module) : Components.UniformStackSpread(module, 15f, 15f, 2, 2)
{
    public int NumCasts;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.CurseOfCompanionship:
                AddStack(actor, status.ExpireAt);
                break;
            case (uint)SID.CurseOfSolitude:
                AddSpread(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FirePowder:
                ++NumCasts;
                Spreads.Clear();
                break;
            case (uint)AID.HighFirePowder:
                ++NumCasts;
                Stacks.Clear();
                break;
        }
    }
}

sealed class FableflightLong(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _rect = new(40f, 80f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FableflightLeftVerySlow or (uint)AID.FableflightRightVerySlow)
        {
            _aoe = [new(_rect, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 2d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CarpetRide1Left or (uint)AID.CarpetRide1Right)
        {
            ++NumCasts;
            _aoe = [];
        }
    }
}

class BurningGleam(BossModule module, double delay = 9d, int maxShown = int.MaxValue) : Components.GenericAOEs(module, (uint)AID.BurningGleam)
{
    public const ushort GemActivated = 0x11D3;
    public static readonly AOEShapeCross Cross = new(40f, 5f);
    public readonly List<AOEInstance> Casters = [];
    private readonly double _delay = delay;
    private readonly int _maxShown = maxShown;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        return count == 0 ? [] : CollectionsMarshal.AsSpan(Casters)[..Math.Min(count, _maxShown)];
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == GemActivated && actor.OID == (uint)OID.FieryBauble)
        {
            Casters.Add(new(Cross, actor.Position, default, WorldState.FutureTime(_delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            var pos = caster.Position;
            for (var i = Casters.Count - 1; i >= 0; --i)
            {
                if (Casters[i].Origin.AlmostEqual(pos, 1f))
                {
                    Casters.RemoveAt(i);
                }
            }
        }
    }
}

sealed class BurningGleamSlow(BossModule module) : BurningGleam(module, 18d, 2);

sealed class ChillingGleam(BossModule module) : Components.GenericAOEs(module, (uint)AID.ChillingGleam)
{
    private static readonly AOEShapeCross _inverted = new(40f, 5f, invertForbiddenZone: true);
    private AOEInstance[] _aoe = [];
    private AOEInstance[] _aoeInverted = [];
    private Actor? _gem;
    private Actor? _carpet;
    private int _numMoves;
    private DateTime _activation;
    private BitMask _immolation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _immolation[slot] ? _aoeInverted : _aoe;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Length == 0)
        {
            return;
        }
        var inside = _aoe[0].Check(actor.Position);
        if (_immolation[slot])
        {
            hints.Add("Get hit by ice cross!", !inside);
        }
        else if (inside)
        {
            hints.Add(WarningText);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.CurseOfImmolation)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                _immolation.Set(slot);
            }
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == BurningGleam.GemActivated && actor.OID == (uint)OID.IcyBauble)
        {
            _gem = actor;
            _activation = WorldState.FutureTime(23.4d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CarpetAppear:
                if (_gem != null && caster.Position.AlmostEqual(_gem.Position, 1f))
                {
                    _carpet = caster;
                }
                break;
            case (uint)AID.CarpetDash:
                if (caster == _carpet && ++_numMoves == 3)
                {
                    var pos = spell.TargetXZ;
                    _aoe = [new(BurningGleam.Cross, pos, default, _activation)];
                    _aoeInverted = [new(_inverted, pos, default, _activation, Colors.SafeFromAOE)];
                }
                break;
            case (uint)AID.ChillingGleam:
                ++NumCasts;
                _aoe = [];
                _aoeInverted = [];
                break;
        }
    }
}

sealed class GridMap(int gridDimension, float cellSize, WPos center)
{
    public enum TileType : byte { E, B, R }

    public readonly WPos Center = center;
    public readonly int GridDimension = gridDimension;
    public readonly float CellSize = cellSize;
    public readonly float HalfSize = gridDimension * cellSize * 0.5f;
    public readonly TileType[] Tiles = new TileType[gridDimension * gridDimension];

    public WPos GridToWorld(int gridCell)
    {
        var start = HalfSize - CellSize * 0.5f;
        return new(Center.X - start + gridCell % GridDimension * CellSize, Center.Z - start + gridCell / GridDimension * CellSize);
    }

    private bool CellIntersectsShape(AOEShape shape, WPos origin, Angle angle, int gridCell)
    {
        var center = GridToWorld(gridCell);
        var half = CellSize * 0.5f;
        return shape.Check(center, origin, angle)
            || shape.Check(new(center.X - half, center.Z - half), origin, angle)
            || shape.Check(new(center.X - half, center.Z + half), origin, angle)
            || shape.Check(new(center.X + half, center.Z - half), origin, angle)
            || shape.Check(new(center.X + half, center.Z + half), origin, angle);
    }

    public void MarkTiles(AOEShape shape, WPos origin, Angle angle, TileType t)
    {
        var len = Tiles.Length;
        for (var i = 0; i < len; ++i)
        {
            if ((t == TileType.R || Tiles[i] != TileType.R) && CellIntersectsShape(shape, origin, angle, i))
            {
                Tiles[i] = t;
            }
        }
    }

    public List<int[]> FindSafeSpotsFromRedIndices()
    {
        var results = new List<int[]>();
        var dim = GridDimension;
        for (var y = 0; y < dim; ++y)
        {
            for (var x = 0; x < dim; ++x)
            {
                for (var o = 0; o < 4; ++o)
                {
                    var dx = (o & 1) == 0 ? 1 : -1;
                    var dy = (o & 2) == 0 ? 1 : -1;
                    var x2 = x + 2 * dx;
                    var y2 = y + 2 * dy;
                    if (x2 < 0 || x2 >= dim || y2 < 0 || y2 >= dim)
                    {
                        continue;
                    }
                    var p0 = y * dim + x;
                    var p2 = y * dim + x2;
                    var p4 = y2 * dim + x;
                    if (Tiles[y * dim + x + dx] != TileType.R || Tiles[(y + dy) * dim + x] != TileType.R)
                    {
                        continue;
                    }

                    int empty = -1, blue1 = -1, blue2 = -1, numBlue = 0, numEmpty = 0;
                    Classify(p0, ref empty, ref blue1, ref blue2, ref numBlue, ref numEmpty);
                    Classify(p2, ref empty, ref blue1, ref blue2, ref numBlue, ref numEmpty);
                    Classify(p4, ref empty, ref blue1, ref blue2, ref numBlue, ref numEmpty);
                    if (numBlue != 2 || numEmpty != 1)
                    {
                        continue;
                    }

                    var minB = Math.Min(blue1, blue2);
                    var maxB = Math.Max(blue1, blue2);
                    var seen = false;
                    var count = results.Count;
                    for (var i = 0; i < count; ++i)
                    {
                        var r = results[i];
                        if (r[2] == empty && Math.Min(r[0], r[1]) == minB && Math.Max(r[0], r[1]) == maxB)
                        {
                            seen = true;
                            break;
                        }
                    }
                    if (!seen)
                    {
                        results.Add([blue1, blue2, empty]);
                    }
                }
            }
        }
        return results;
    }

    private void Classify(int tile, ref int empty, ref int blue1, ref int blue2, ref int numBlue, ref int numEmpty)
    {
        switch (Tiles[tile])
        {
            case TileType.E:
                empty = tile;
                ++numEmpty;
                break;
            case TileType.B:
                if (numBlue++ == 0)
                {
                    blue1 = tile;
                }
                else
                {
                    blue2 = tile;
                }
                break;
        }
    }
}

sealed class ParisCurseSolver(BossModule module) : Components.GenericAOEs(module)
{
    private enum SafeHalf { Unknown, North, South }

    private const int Dimension = 4;
    private const float CellSize = 10f;
    private static readonly AOEShapeRect _cell = new(CellSize * 0.5f, CellSize * 0.5f, CellSize * 0.5f);
    private static readonly AOEShapeCross _cross = new(40f, 4f);
    private static readonly int[] _allowedNorth = [0, 1, 2, 3, 4, 5, 6, 8, 9, 12];
    private static readonly int[] _allowedSouth = [3, 6, 7, 9, 10, 11, 12, 13, 14, 15];
    private static readonly uint _colorBlue = Color.FromRGBA(0x268BD280).ABGR;
    private static readonly uint _colorEmpty = Color.FromRGBA(0xC0C0C080).ABGR;

    private readonly GridMap _grid = new(Dimension, CellSize, module.Center);
    private readonly List<Actor> _redCrystals = [];
    private readonly AOEInstance[][] _assigned = new AOEInstance[PartyState.MaxPartySize][];
    private AOEInstance[] _candidates = [];
    private Actor? _blueCrystalTracker;
    private float _blueCrystalTrackerDistance = float.MaxValue;
    private SafeHalf _safeHalf;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => (uint)slot < PartyState.MaxPartySize && _assigned[slot] is { } assigned ? assigned : _candidates;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CarpetAppear)
        {
            var crystals = Module.Enemies((uint)OID.IcyBauble);
            if (crystals.Count != 0)
            {
                var distance = (caster.Position - crystals[0].Position).LengthSq();
                if (distance < _blueCrystalTrackerDistance)
                {
                    _blueCrystalTrackerDistance = distance;
                    _blueCrystalTracker = caster;
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Unravel:
                if (_candidates.Length == 0)
                {
                    Solve();
                }
                break;
            case (uint)AID.BurningGleam:
                _redCrystals.Add(caster);
                break;
            case (uint)AID.FableflightLeftVerySlow:
                _safeHalf = SafeHalf.North;
                break;
            case (uint)AID.FableflightRightVerySlow:
                _safeHalf = SafeHalf.South;
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var dir = new WDir(1f, default);
        const float half = CellSize * 0.5f;
        for (var i = 0; i < Dimension * Dimension; ++i)
        {
            Arena.AddRect(_grid.GridToWorld(i), dir, half, half, half, 0xFFFFFFFFu, 2f);
        }
    }

    private void Solve()
    {
        if (_blueCrystalTracker == null)
        {
            return;
        }
        _grid.MarkTiles(_cross, _blueCrystalTracker.Position, default, GridMap.TileType.B);
        var numRed = _redCrystals.Count;
        for (var i = 0; i < numRed; ++i)
        {
            _grid.MarkTiles(_cross, _redCrystals[i].Position, default, GridMap.TileType.R);
        }

        var safeSpots = _grid.FindSafeSpotsFromRedIndices();
        var allowed = _safeHalf == SafeHalf.North ? _allowedNorth : _allowedSouth;
        int[]? spot = null;
        var numSpots = safeSpots.Count;
        for (var i = 0; i < numSpots; ++i)
        {
            var s = safeSpots[i];
            if (Array.IndexOf(allowed, s[0]) >= 0 && Array.IndexOf(allowed, s[1]) >= 0 && Array.IndexOf(allowed, s[2]) >= 0)
            {
                spot = s;
                break;
            }
        }
        if (spot == null)
        {
            return;
        }

        var now = WorldState.CurrentTime;
        _candidates = new AOEInstance[3];
        for (var i = 0; i < 3; ++i)
        {
            var tile = spot[i];
            _candidates[i] = new(_cell, _grid.GridToWorld(tile), default, now, _grid.Tiles[tile] == GridMap.TileType.B ? _colorBlue : _colorEmpty, false);
        }
        Assign(spot, now);
    }

    private void Assign(int[] spot, DateTime now)
    {
        var north = _safeHalf == SafeHalf.South ? Math.Min(spot[0], Math.Min(spot[1], spot[2])) : Math.Max(spot[0], Math.Max(spot[1], spot[2]));
        int otherA = -1, otherB = -1;
        for (var i = 0; i < 3; ++i)
        {
            if (spot[i] != north)
            {
                if (otherA < 0)
                {
                    otherA = spot[i];
                }
                else
                {
                    otherB = spot[i];
                }
            }
        }
        if (otherB < 0)
        {
            return;
        }
        var second = _safeHalf == SafeHalf.North ? Math.Min(otherA, otherB) : Math.Max(otherA, otherB);
        var third = otherA == second ? otherB : otherA;

        Span<int> ordered = [north, second, third];
        Span<int> blueTiles = stackalloc int[3];
        var numBlue = 0;
        var emptyTile = -1;
        for (var i = 0; i < 3; ++i)
        {
            if (_grid.Tiles[ordered[i]] == GridMap.TileType.B)
            {
                blueTiles[numBlue++] = ordered[i];
            }
            else if (_grid.Tiles[ordered[i]] == GridMap.TileType.E)
            {
                emptyTile = ordered[i];
            }
        }
        if (numBlue != 2 || emptyTile < 0)
        {
            return;
        }

        var roles = Service.Config.Get<PartyRolesConfig>().EffectiveRolePerSlot(Raid);
        var party = Raid.WithSlot(false, true, true);
        var len = party.Length;
        var seenRoles = 0;
        for (var i = 0; i < len; ++i)
        {
            var priority = RolePriority(roles[party[i].Item1]);
            if (priority >= 0)
            {
                seenRoles |= 1 << priority;
            }
        }
        if (seenRoles != 0b1111)
        {
            return;
        }

        Span<int> fireSlots = stackalloc int[PartyState.MaxPartySize];
        Span<int> firePriorities = stackalloc int[PartyState.MaxPartySize];
        var numFire = 0;
        var spreadSlot = -1;
        for (var i = 0; i < len; ++i)
        {
            var (slot, player) = party[i];
            if (slot >= PartyState.MaxPartySize)
            {
                continue;
            }
            if (player.FindStatus((uint)SID.CurseOfImmolation) == null)
            {
                _assigned[slot] = [new(_cell, _grid.GridToWorld(emptyTile), default, now, Colors.SafeFromAOE, false)];
                continue;
            }
            var priority = RolePriority(roles[slot]);
            var index = numFire++;
            while (index > 0 && firePriorities[index - 1] > priority)
            {
                fireSlots[index] = fireSlots[index - 1];
                firePriorities[index] = firePriorities[index - 1];
                --index;
            }
            fireSlots[index] = slot;
            firePriorities[index] = priority;
        }
        for (var i = 0; i < numFire; ++i)
        {
            if (Raid[fireSlots[i]]?.FindStatus((uint)SID.CurseOfSolitude) != null)
            {
                spreadSlot = fireSlots[i];
                break;
            }
        }

        for (var i = 0; i < numFire; ++i)
        {
            var slot = fireSlots[i];
            int tile;
            if (numFire == 2)
            {
                tile = blueTiles[i];
            }
            else if (numFire == 3 && spreadSlot >= 0)
            {
                tile = slot == spreadSlot ? blueTiles[1] : blueTiles[0];
            }
            else
            {
                continue;
            }
            _assigned[slot] = [new(_cell, _grid.GridToWorld(tile), default, now, Colors.SafeFromAOE, false)];
        }
    }

    private static int RolePriority(Role r) => r switch
    {
        Role.Tank => 0,
        Role.Melee => 1,
        Role.Ranged => 2,
        Role.Healer => 3,
        _ => -1
    };
}
