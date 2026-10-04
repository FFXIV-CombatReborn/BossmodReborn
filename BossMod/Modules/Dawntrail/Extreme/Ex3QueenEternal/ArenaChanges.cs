namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override bool KeepOnPhaseChange => true;
    private bool firstEarthArena = true;
    public bool EnrageCastStarted;
    public bool EnrageCastEnded;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AuthorityEternal)
        {
            EnrageCastStarted = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AuthorityEternal)
        {
            EnrageCastEnded = true;
        }
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID != 0x8000000D || param1 > 0x08u)
        {
            return;
        }
        switch (param1)
        {
            case 0x01u: // default arena
                SetDefaultArena();
                break;
            case 0x02u: // x arena (wind)
                var arenaWind = Trial.T03QueenEternal.T03QueenEternal.GetXArena();
                SetArena(arenaWind, arenaWind.Center);
                break;
            case 0x04u: // disjointed rect (Earth) arena
                if (firstEarthArena)
                {
                    var center = new WPos(100f, 100f);
                    var sq = new Square(new(100f, 100f), 21f);
                    var polyFull = new RelSimplifiedComplexPolygon(sq.Contour(center));
                    var polySplit = PolygonClipper.GetCombinedPolygon(center, Trial.T03QueenEternal.T03QueenEternal.GetSplitArenaRects());
                    var arena = new ArenaBoundsCustom([sq], WorldProjectionLayers: [new(polyFull, 0f, borderY: 0f), new(polySplit, 0f, borderY: 0f)]);
                    SetArena(arena, center);
                    firstEarthArena = false;
                }
                else
                {
                    var arenaEarth = Trial.T03QueenEternal.T03QueenEternal.GetSplitArena();
                    SetArena(arenaEarth, arenaEarth.Center);
                }
                break;
            case 0x08u: // ice arena
                var arenaIce = new ArenaBoundsCustom(Ex3QueenEternal.GetIceRects());
                SetArena(arenaIce, arenaIce.Center);
                break;
        }
    }

    private void SetDefaultArena() => SetArena(new ArenaBoundsSquare(20f), new(100f, 100f));

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x08)
        {
            if (state == 0x01000080u)
            {
                SetArena(new ArenaBoundsRect(20f, 10f), new(100f, 110f));
            }
            else if (state == 0x02000001u)
            {
                SetDefaultArena();
            }
        }
    }

    private void SetArena(ArenaBounds bounds, WPos center)
    {
        Arena.Bounds = bounds;
        Arena.Center = center;
    }
}
