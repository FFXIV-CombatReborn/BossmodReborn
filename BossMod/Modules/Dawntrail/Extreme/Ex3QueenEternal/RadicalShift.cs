namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class RadicalShift(BossModule module) : Components.GenericAOEs(module)
{
    public enum Rotation { None, Left, Right }

    private ArenaBoundsCustom? _left;
    private ArenaBoundsCustom? _right;
    private Rotation _nextRotation;
    private AOEInstance[] _aoe = [];
    private static readonly Square[] defaultSquare = [new(Ex3QueenEternal.ArenaCenter, 20f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x0C)
        {
            var rot = state switch
            {
                0x01000080u => Rotation.Left,
                0x08000400u => Rotation.Right,
                _ => Rotation.None
            };
            if (rot != Rotation.None)
            {
                _nextRotation = rot;
                UpdateAOE(NextPlatform);
            }
        }
        else if (state is 0x00020001u or 0x00200010u)
        {
            var platform = index switch
            {
                0x09 => Ex3QueenEternal.WindBounds,
                0x0A => Ex3QueenEternal.EarthBounds,
                0x0B => Ex3QueenEternal.IceBounds,
                _ => null
            };
            if (platform != null)
            {
                (state == 0x00020001u ? ref _right : ref _left) = platform;
                UpdateAOE(NextPlatform);
            }
        }
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (_aoe.Length != 0 && updateID == 0x8000000D && param1 is 0x02u or 0x04u or 0x08u)
        {
            _aoe = [];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RadicalShift)
        {
            _left = _right = null;
            _nextRotation = Rotation.None;
        }
    }

    private ArenaBoundsCustom? NextPlatform => _nextRotation switch
    {
        Rotation.Left => _left,
        Rotation.Right => _right,
        _ => null
    };

    private void UpdateAOE(ArenaBoundsCustom? platform)
    {
        AOEShapeCustom? aoe = null;
        var center = Arena.Center;
        if (platform == Ex3QueenEternal.WindBounds)
        {
            aoe = new(defaultSquare, Trial.T03QueenEternal.T03QueenEternal.XArenaRects, origin: center);
        }
        else if (platform == Ex3QueenEternal.EarthBounds)
        {
            aoe = new(defaultSquare, Trial.T03QueenEternal.T03QueenEternal.SplitArenaRects, origin: center);
        }
        else if (platform == Ex3QueenEternal.IceBounds)
        {
            aoe = new(defaultSquare, Ex3QueenEternal.IceRectsAll, origin: center);
        }
        if (aoe != null)
        {
            _aoe = [new(aoe, center, default, WorldState.FutureTime(6d))];
        }
    }
}

sealed class RadicalShiftAOE(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.RadicalShiftAOE, 5f);
