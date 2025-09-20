namespace BossMod.Dawntrail.Raid.M01NBlackCat;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public static readonly WPos ArenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);
    private static readonly Square[] defaultSquare = [new(ArenaCenter, 20f)];
    public BitMask DamagedCells;
    public BitMask DestroyedCells;
    public static readonly Square[] Tiles = GenerateTiles();

    private static Square[] GenerateTiles()
    {
        var squares = new Square[16];
        for (var i = 0; i < 16; ++i)
            squares[i] = new Square(CellCenter(i), 5f);
        return squares;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        // index per tile, starting north
        // 0x00, 0x01, 0x02, 0x03
        // 0x04, 0x05, 0x06, 0x07
        // 0x08, 0x09, 0x0A, 0x0B
        // 0x0C, 0x0D, 0x0E, 0x0F
        if (index > 0x0F)
        {
            return;
        }
        if (state == 0x00020001u) // tile gets damaged
            DamagedCells[index] = true;
        else if (state == 0x00200010u) // tile gets broken
        {
            DamagedCells[index] = false;
            DestroyedCells[index] = true;
        }
        else if (state is 0x01000004u or 0x00800004u) // tile gets repaired
        {
            DamagedCells[index] = false;
            DestroyedCells[index] = false;
        }
        UpdateArenaBounds();
    }

    public static int CellIndex(WPos pos)
    {
        var off = pos - ArenaCenter;
        return (CoordinateIndex(off.Z) << 2) | CoordinateIndex(off.X);
    }

    private static int CoordinateIndex(float coord) => coord switch
    {
        < -10 => 0,
        < 0 => 1,
        < 10 => 2,
        _ => 3
    };

    public static WPos CellCenter(int index)
    {
        var x = -15f + 10f * (index & 3);
        var z = -15f + 10f * (index >> 2);
        return ArenaCenter + new WDir(x, z);
    }

    private void UpdateArenaBounds()
    {
        List<Square> brokenTilesList = [];
        var len = Tiles.Length;
        for (var i = 0; i < len; ++i)
        {
            if (DestroyedCells[i])
                brokenTilesList.Add(Tiles[i]);
        }

        Square[] brokenTiles = [.. brokenTilesList];
        var arena = new ArenaBoundsCustom(defaultSquare, brokenTiles, Offset: -1f);
        Arena.Bounds = arena;
        Arena.Center = arena.Center;
    }
}

sealed class Mouser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(19);
    private static readonly AOEShapeRect rect = new(10f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var countDanger = NumCasts > 2 ? 2 : 3;
        var total = countDanger + 4;
        var max = total > count ? count : total;

        var aoes = CollectionsMarshal.AsSpan(_aoes)[..max];
        if (count > countDanger)
        {
            var color = Colors.Danger;
            for (var i = 0; i < countDanger; ++i)
            {
                aoes[i].Color = color;
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MouserTelegraphFirst or (uint)AID.MouserTelegraphSecond)
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, WorldState.FutureTime(9.7d)));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index <= 0x0F && state is 0x00020001u or 0x00200010u && _aoes.Count != 0)
            _aoes.RemoveAt(0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Mouser)
            if (++NumCasts == 19)
                NumCasts = 0;
    }
}
