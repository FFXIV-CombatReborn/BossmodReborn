namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

sealed class ArenaChanges(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DestroyTile, Duplicate.Rect)
{
    public readonly List<int> RemovedTiles = new(4);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x1A and <= 0x22)
        {
            var tile = index - 0x1A;
            switch (state)
            {
                case 0x02000004u: // teleporting tile disappears
                case 0x00040004u: // initial removed tile
                    RemovedTiles.Add(tile);
                    UpdateArena();
                    break;
                case 0x01000001u: // tile unfreezes
                case 0x04000001u: // teleporting tile appears
                case 0x00200001u: // quicksand ends
                    RemovedTiles.Remove(tile);
                    UpdateArena();
                    break;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID is (uint)AID.Quake or (uint)AID.Freeze)
        {
            var pos = spell.LocXZ;
            var dir = spell.Rotation.ToDirection();
            for (var i = 0; i < 9; ++i)
            {
                var center = new WPos(784f + i % 3 * 16f, -816f + i / 3 * 16f);
                if (center.InRect(pos, dir, 16f, default, 24f))
                {
                    RemovedTiles.Add(i);
                }
            }
            UpdateArena();
        }
    }

    private void UpdateArena()
    {
        var count = RemovedTiles.Count;
        if (count is 2 or 3)
        {
            return;
        }
        var squares = new Square[count];
        for (var i = 0; i < count; ++i)
        {
            var tile = RemovedTiles[i];
            squares[i] = new Square(new WPos(784f + tile % 3 * 16f, -816f + tile / 3 * 16f), 8f);
        }
        var arena = new ArenaBoundsCustom([new Square(A24Ealdnarche.ArenaCenter, 24f)], squares, Offset: -1f);
        Arena.Bounds = arena;
        Arena.Center = arena.Center;
    }
}
