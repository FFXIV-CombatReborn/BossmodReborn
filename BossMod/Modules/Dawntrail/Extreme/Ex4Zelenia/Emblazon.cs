namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class Emblazon(BossModule module) : Components.GenericTowers(module, (uint)AID.Emblazon)
{
    private readonly FloorTiles _tiles = module.FindComponent<FloorTiles>()!;
    public BitMask Allowed;
    private BitMask forbiddenAlexandrianBanish;
    public bool? Mechanic; // null = first, false = second, true = third
    public WDir WedgeCenterDirection;
    private int emblazoncounter;
    private bool pattern; // false pattern 1 (tower with index 0x2E), true pattern 2 (tower with index 0x2F)

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x2F && state == 0x00020001u)
        {
            pattern = true;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HolyHazardVisual1)
        {
            if (!pattern)
            {
                const int index1 = (0x2E - 0x2D - 1 + 8) & 7;
                const int index2 = (0x2E - 0x2D - 2 + 8) & 7;
                const int index3 = (0x32 - 0x2D - 1 + 8) & 7;
                const int index4 = (0x32 - 0x2D - 2 + 8) & 7;
                AddTower(FloorTiles.DonutSIn, FloorTiles.TileAngles[index1]);
                AddTower(FloorTiles.DonutSIn, FloorTiles.TileAngles[index2]);
                AddTower(FloorTiles.DonutSIn, FloorTiles.TileAngles[index3]);
                AddTower(FloorTiles.DonutSIn, FloorTiles.TileAngles[index4]);
            }
            else
            {
                const int index1 = (0x15 - 0x14 - 1 + 8) & 7;
                const int index2 = (0x15 - 0x14 - 2 + 8) & 7;
                const int index3 = (0x19 - 0x14 - 1 + 8) & 7;
                const int index4 = (0x19 - 0x14 - 2 + 8) & 7;
                AddTower(FloorTiles.DonutS, FloorTiles.TileAngles[index1]);
                AddTower(FloorTiles.DonutS, FloorTiles.TileAngles[index2]);
                AddTower(FloorTiles.DonutS, FloorTiles.TileAngles[index3]);
                AddTower(FloorTiles.DonutS, FloorTiles.TileAngles[index4]);
            }
        }
        else if (spell.Action.ID == (uint)AID.HolyHazardVisual2)
        {
            if (pattern)
            {
                const int index1 = (0x2F - 0x2D + 1) & 7;
                const int index2 = (0x2F - 0x2D + 2) & 7;
                const int index3 = (0x33 - 0x2D + 1) & 7;
                const int index4 = (0x33 - 0x2D + 2) & 7;
                AddTower(FloorTiles.DonutSIn, FloorTiles.TileAngles[index1]);
                AddTower(FloorTiles.DonutSIn, FloorTiles.TileAngles[index2]);
                AddTower(FloorTiles.DonutSIn, FloorTiles.TileAngles[index3]);
                AddTower(FloorTiles.DonutSIn, FloorTiles.TileAngles[index4]);
            }
            else
            {
                const int index1 = (0x16 - 0x14 + 1) & 7;
                const int index2 = (0x16 - 0x14 + 2) & 7;
                const int index3 = (0x1A - 0x14 + 1) & 7;
                const int index4 = (0x1A - 0x14 + 2) & 7;
                AddTower(FloorTiles.DonutS, FloorTiles.TileAngles[index1]);
                AddTower(FloorTiles.DonutS, FloorTiles.TileAngles[index2]);
                AddTower(FloorTiles.DonutS, FloorTiles.TileAngles[index3]);
                AddTower(FloorTiles.DonutS, FloorTiles.TileAngles[index4]);
            }
        }
        void AddTower(AOEShapeDonutSector shape, Angle rot) => Towers.Add(new(Arena.Center, shape, forbiddenSoakers: ~Allowed, activation: Module.CastFinishAt(spell, 4.5d), rotation: rot));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Emblazon)
        {
            Allowed.Set(Raid.FindSlot(targetID));
            if (Mechanic == true)
                return;
            if (++emblazoncounter == 4)
            {
                var towers = CollectionsMarshal.AsSpan(Towers);
                var len = towers.Length;
                var forbidden = ~Allowed;
                for (var i = 0; i < len; ++i)
                {
                    ref var t = ref towers[i];
                    t.ForbiddenSoakers = forbidden;
                }
            }
            if (Towers.Count == 0)
            {
                var act = WorldState.FutureTime(6.8d);
                var pos = Arena.Center;
                if (Mechanic == null)
                {
                    FloorTiles.AnalyzeTilesForOuterTowers(_tiles.InnerActiveTiles, out var mid, out var o1, out var o2, out var oMid);
                    int[] tiles = [mid, o1, o2, oMid];
                    for (var i = 0; i < 4; ++i)
                    {
                        AddTower(FloorTiles.DonutSIn, FloorTiles.TileAngles[tiles[i]]);
                    }
                }
                else if (Mechanic == false)
                {
                    FloorTiles.Find4ConnectedInactiveTiles(_tiles.InnerActiveTiles, _tiles.OuterActiveTiles, out var innerResult, out var outerResult);
                    for (var i = 0; i < 2; ++i)
                    {
                        AddTower(FloorTiles.DonutSIn, FloorTiles.TileAngles[innerResult[i]]);
                        AddTower(FloorTiles.DonutS, FloorTiles.TileAngles[outerResult[i]]);
                    }
                    WedgeCenterDirection = FloorTiles.GetWedgeCenterAngle(innerResult[0], innerResult[1]);
                }
                void AddTower(AOEShapeDonutSector shape, Angle rot) => Towers.Add(new(pos, shape, activation: act, rotation: rot));
            }
        }
        else if (iconID == (uint)IconID.AlexandrianThunderIII)
        {
            forbiddenAlexandrianBanish.Set(Raid.FindSlot(targetID));
        }
    }

    // "tower" can be completly ignored by players without emblazon marker
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Allowed[slot] || forbiddenAlexandrianBanish[slot])
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Allowed[pcSlot])
        {
            base.DrawArenaForeground(pcSlot, pc);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Allowed[pcSlot] || forbiddenAlexandrianBanish[pcSlot])
        {
            base.DrawArenaBackground(pcSlot, pc);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Allowed[slot] || forbiddenAlexandrianBanish[slot])
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
