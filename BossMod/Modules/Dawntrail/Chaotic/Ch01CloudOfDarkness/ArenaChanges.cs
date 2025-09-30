namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly Square[] DefaultPolygon = [new(Ch01CloudOfDarkness.DefaultCenter, 40f)];
    private static readonly AOEShapeCustom P1Transition = new(DefaultPolygon, Ch01CloudOfDarkness.Diamond);
    private static readonly AOEShapeCustom P2Transition = new(DefaultPolygon, Ch01CloudOfDarkness.Phase2ShapesWD);
    private static readonly AOEShapeDonut donut = new(34f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            switch (state)
            {
                case 0x00200010u:
                    SetAOE(P1Transition);
                    break;
                case 0x00020001u:
                    SetAOE(P2Transition);
                    break;
            }
        }
        else if (index == 0x02)
        {
            switch (state)
            {
                case 0x00020001u:
                    SetArena(Ch01CloudOfDarkness.Phase2BoundsND);
                    break;
                case 0x00080004u:
                    SetArena(Ch01CloudOfDarkness.Phase2BoundsWD);
                    break;
            }
        }
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID != 0x8000000D)
        {
            return;
        }
        switch (param1)
        {
            case 0x10000000u: // default arena
                Arena.Bounds = Ch01CloudOfDarkness.DefaultArena;
                Arena.Center = Ch01CloudOfDarkness.DefaultCenter;
                break;
            case 0x20000000u: // (phase 2)
                SetArena(Ch01CloudOfDarkness.Phase2BoundsWD);
                break;
            case 0x40000000u: // diamond arena (phase 1)
                SetArena(Ch01CloudOfDarkness.Phase1Bounds);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DarkDominion)
        {
            SetAOE(donut);
        }
    }

    private void SetArena(ArenaBoundsCustom bounds)
    {
        Arena.Bounds = bounds;
        Arena.Center = bounds.Center;
        _aoe = [];
    }

    private void SetAOE(AOEShape shape)
    {
        _aoe = [new(shape, Arena.Center, default, WorldState.FutureTime(9d))];
    }
}
