namespace BossMod.Dawntrail.Savage.M1SBlackCat;

class ArenaChanges(BossModule module) : BossComponent(module)
{
    private ArenaBounds? arena;
    public static readonly WPos ArenaCenter = new(100, 100);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
    private static readonly Square defaultSquare = new(ArenaCenter, 20);
    private static readonly WPos[] tilePositions =
    [
        new(85, 85), new(95, 85), new(105, 85), new(115, 85), new(85, 95),
        new(95, 95), new(105, 95), new(115, 95), new(85, 105), new(95, 105),
        new(105, 105), new(115, 105), new(85, 115), new(95, 115), new(105, 115),
        new(115, 115)
    ];
    public readonly List<Shape> DamagedTiles = [];
    private readonly List<Shape> brokenTiles = [];

    private static readonly Square[] tiles = tilePositions.Select(pos => new Square(pos, 5)).ToArray();

    private readonly List<byte> delayedRepair = [];
    private DateTime delayedRepairTime;

    public override void Update()
    {
        if (delayedRepairTime <= Module.WorldState.CurrentTime)
        {
            foreach (byte index in delayedRepair)
            {
                RemoveTileFromLists(index);
            }
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        // index per tile, starting north
        // 0x00, 0x01, 0x02, 0x03
        // 0x04, 0x05, 0x06, 0x07
        // 0x08, 0x09, 0x0A, 0x0B
        // 0x0C, 0x0D, 0x0E, 0x0F
        if (state == 0x00020001) // tile gets damaged
            AddTileToList(DamagedTiles, index);
        else if (state == 0x00200010) // tile gets broken
            MoveTileBetweenLists(DamagedTiles, brokenTiles, index);
        else if (state is 0x01000004 or 0x00800004 or 0x00080004) // tile gets repaired
            RemoveTileFromLists(index);
        else if (state is 0x00400004)
        {
            delayedRepairTime = Module.WorldState.FutureTime(10.0f);
            delayedRepair.Add(index);
            //RemoveTileFromLists(index);
        }
    }

    private void AddTileToList(List<Shape> list, byte index)
    {
        if (index < tiles.Length)
            list.Add(tiles[index]);
    }

    private void MoveTileBetweenLists(List<Shape> fromList, List<Shape> toList, byte index)
    {
        if (index < tiles.Length)
        {
            var tile = tiles[index];
            fromList.Remove(tile);
            toList.Add(tile);
            UpdateArenaBounds();
        }
    }

    private void RemoveTileFromLists(byte index)
    {
        if (index < tiles.Length)
        {
            var tile = tiles[index];
            DamagedTiles.Remove(tile);
            brokenTiles.Remove(tile);
            UpdateArenaBounds();
        }
    }

    private void UpdateArenaBounds()
    {
        arena = new ArenaBoundsComplex([defaultSquare], brokenTiles, Offset: -0.5f);
        Module.Arena.Bounds = arena;
    }
}

class Mouser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            var aoeCount = Math.Clamp(_aoes.Count, 0, NumCasts > 2 ? 2 : 3);
            for (var i = aoeCount; i < _aoes.Count; i++)
                yield return _aoes[i];
            for (var i = 0; i < aoeCount; i++)
                yield return _aoes[i] with { Color = Colors.Danger };
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MouserTelegraphFirst or AID.MouserTelegraphSecond)
            _aoes.Add(new(rect, caster.Position, spell.Rotation, Module.WorldState.FutureTime(9.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && spell.Action.ID == 38054)
        {
            _aoes.RemoveAt(0);
            if (++NumCasts == 19)
                NumCasts = 0;
        }
    }
}
