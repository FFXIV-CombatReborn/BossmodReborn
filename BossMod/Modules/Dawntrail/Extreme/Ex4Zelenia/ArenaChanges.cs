namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(2f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.QueensCrusade)
        {
            _aoe = [new(circle, Arena.Center, default, Module.CastFinishAt(spell, 0.1d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x01)
        {
            return;
        }
        switch (state)
        {
            case 0x00020001u:
                _aoe = [];
                Arena.Bounds = Ex4Zelenia.DonutArena;
                break;
            case 0x00080004u:
                Arena.Bounds = Ex4Zelenia.DefaultArena;
                break;
        }
    }
}

class FloorTiles(BossModule module) : BossComponent(module)
{
    public readonly bool[] InnerActiveTiles = new bool[8];
    public readonly bool[] OuterActiveTiles = new bool[8];
    public static readonly AOEShapeCone ConeInner = new(8f, 22.5f.Degrees());
    public static readonly AOEShapeDonutSector DonutS = new(8f, 16f, 22.5f.Degrees()), DonutSIn = new(2f, 8f, 22.5f.Degrees());
    public static readonly Angle[] TileAngles = GetTileAngles();
    public static readonly WDir[] TileDirections = GetTileDirections();

    private static Angle[] GetTileAngles()
    {
        var tileAngles = new Angle[8];
        for (var i = 0; i < 8; ++i)
        {
            tileAngles[i] = (157.5f - 45f * i).Degrees();
        }
        return tileAngles;
    }

    private static WDir[] GetTileDirections()
    {
        var tileDirs = new WDir[8];
        for (var i = 0; i < 8; ++i)
            tileDirs[i] = TileAngles[i].ToDirection();
        return tileDirs;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // arena slices (tile start angle, -22.5° to get center)
        // inner Ring:
        // 0x04: 180°  → index 0
        // 0x05: 135°  → index 1
        // 0x06: 90°   → index 2
        // 0x07: 45°   → index 3
        // 0x08: 0°    → index 4
        // 0x09: -45°  → index 6
        // 0x0A: -90°  → index 7
        // 0x0B: -135° → index 8
        // outer Ring:
        // 0x0C: 180°  → index 0
        // 0x0D: 135°  → index 1
        // 0x0E: 90°   → index 2
        // 0x0F: 45°   → index 3
        // 0x10: 0°    → index 4
        // 0x11: -45°  → index 6
        // 0x12: -90°  → index 7
        // 0x13: -135° → index 8
        if (index is >= 0x04 and <= 0x0B)
        {
            switch (state)
            {
                case 0x00400100u or 0x00800040u:
                    InnerActiveTiles[index - 0x04] = true;
                    break;
                case 0x00040020u:
                    InnerActiveTiles[index - 0x04] = false;
                    break;
            }
        }
        else if (index is >= 0x0C and <= 0x13)
        {
            switch (state)
            {
                case 0x00400100u or 0x00800040u:
                    OuterActiveTiles[index - 0x0C] = true;
                    break;
                case 0x00040020u:
                    OuterActiveTiles[index - 0x0C] = false;
                    break;
            }
        }
    }

    public static void AnalyzeTilesForOuterTowers(bool[] ring, out int midTile, out int opp1, out int opp2, out int oppMid)
    {
        var active = new List<int>();
        for (var i = 0; i < 8; ++i)
        {
            if (ring[i])
            {
                active.Add(i);
            }
        }
        if (active.Count < 2) // only relevant for old replays before new ENVC were added
        {
            midTile = opp1 = opp2 = oppMid = 0;
            return;
        }

        var i1 = active[0];
        var i2 = active[1];

        // Normalize for direction (ensure gap is in the clockwise direction)
        var diff = (i2 - i1 + 8) & 7;

        if (diff == 6)
            (i1, i2) = (i2, i1);  // Swap to ensure clockwise

        midTile = (i1 + 1) & 7;
        opp1 = (i1 + 4) & 7;
        opp2 = (i2 + 4) & 7;
        oppMid = (midTile + 4) & 7;
    }

    public static bool Find4ConnectedInactiveTiles(bool[] inner, bool[] outer, out int[] innerTiles, out int[] outerTiles)
    {
        for (var i = 0; i < 8; ++i)
        {
            var next = (i + 1) & 7;
            if (!inner[i] && !outer[i] && !inner[next] && !outer[next])
            {
                innerTiles = [i, next];
                outerTiles = [i, next];
                return true;
            }
        }

        innerTiles = [];
        outerTiles = [];
        return false; // No such connected 4-tile wedge found
    }

    public static WDir GetWedgeCenterAngle(int i1, int i2)
    {
        var dir1 = TileAngles[i1].ToDirection();
        var dir2 = TileAngles[i2].ToDirection();
        var avg = new WDir((dir1.X + dir2.X) * 0.5f, (dir1.Z + dir2.Z) * 0.5f);
        return avg.Normalized();
    }

    public static void GetLongestActiveChain(bool[] inner, bool[] outer, out int[] innerResult, out int[] outerResult)
    {
        var visited = new bool[8, 2];
        var bestInner = new List<int>();
        var bestOuter = new List<int>();

        Span<(int idx, int ring)> stack = stackalloc (int, int)[16];
        var sp = 0;

        for (var i = 0; i < 8; ++i)
        {
            for (var ring = 0; ring < 2; ++ring)
            {
                if (visited[i, ring])
                    continue;

                if (!(ring == 0 ? inner[i] : outer[i]))
                    continue;

                var currentInner = new List<int>();
                var currentOuter = new List<int>();
                sp = 0;
                stack[sp++] = (i, ring);

                while (sp > 0)
                {
                    var (idx, r) = stack[--sp];
                    if (visited[idx, r])
                        continue;
                    visited[idx, r] = true;

                    if (r == 0)
                        currentInner.Add(idx);
                    else
                        currentOuter.Add(idx);

                    // Cross-ring connection
                    if (!visited[idx, 1 - r] && (1 - r == 0 ? inner[idx] : outer[idx]))
                        stack[sp++] = (idx, 1 - r);

                    // Neighbors
                    var prev = (idx + 7) & 7;
                    if (!visited[prev, r] && (r == 0 ? inner[prev] : outer[prev]))
                        stack[sp++] = (prev, r);

                    var next = (idx + 1) & 7;
                    if (!visited[next, r] && (r == 0 ? inner[next] : outer[next]))
                        stack[sp++] = (next, r);
                }

                var total = currentInner.Count + currentOuter.Count;
                if (total > bestInner.Count + bestOuter.Count)
                {
                    bestInner = currentInner;
                    bestOuter = currentOuter;
                }
            }
        }

        innerResult = [.. bestInner];
        outerResult = [.. bestOuter];
    }
}

class ActiveTiles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly Ex4ZeleniaConfig _config = Service.Config.Get<Ex4ZeleniaConfig>();
    private readonly FloorTiles _tiles = module.FindComponent<FloorTiles>()!;
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count == 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.AlexandrianThunderIIFirst:
                case (uint)AID.AlexandrianThunderIIISpread:
                case (uint)AID.AlexandrianThunderIIIAOE:
                case (uint)AID.AlexandrianThunderIVCircle:
                case (uint)AID.AlexandrianThunderIVDonut:
                    activation = Module.CastFinishAt(spell);
                    AddAOEs();
                    break;
            }
        }
    }

    private void AddAOEs()
    {
        var pos = Arena.Center;
        void AddAOE(AOEShape shape, Angle rot) => _aoes.Add(new(shape, pos, rot, activation, _config.RoseTileColor.ABGR));
        for (var i = 0; i < 8; ++i)
        {
            if (_tiles.InnerActiveTiles[i])
                AddAOE(FloorTiles.ConeInner, FloorTiles.TileAngles[i]);
            if (_tiles.OuterActiveTiles[i])
                AddAOE(FloorTiles.DonutS, FloorTiles.TileAngles[i]);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00800040u && index is >= 0x04 and <= 0x13)
        {
            _aoes.Clear();
            AddAOEs();
        }
    }
}
