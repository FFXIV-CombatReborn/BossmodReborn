namespace BossMod.Endwalker.Savage.P6SHegemone;

// our grid looks like this:
// 0  1  2  3
// 4  5  6  7
// 8  9  A  B
// C  D  E  F
class Polyominoid(BossModule module) : Components.GenericAOEs(module, (uint)AID.PolyominousDark)
{
    public enum State { None, Plus, Cross }

    private readonly State[] _states = new State[16];
    private readonly List<(Actor, Actor)> _tethers = [];
    private BitMask _dangerCells;
    private bool _dangerDirty;

    private static readonly AOEShape _shape = new AOEShapeRect(5f, 5f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var indices = _dangerCells.SetBits();
        var count = indices.Length;

        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
            aoes[i] = new(_shape, IndexToPosition(indices[i]));
        return aoes;
    }

    public override void Update()
    {
        if (!_dangerDirty)
            return;

        State[] effStates = [.. _states];
        foreach (var (from, to) in _tethers)
        {
            var i1 = PositionToIndex(from.Position);
            var i2 = PositionToIndex(to.Position);
            (effStates[i2], effStates[i1]) = (effStates[i1], effStates[i2]);
        }

        _dangerDirty = false;
        _dangerCells = default;
        var len = effStates.Length;
        for (var i = 0; i < len; ++i)
        {
            switch (effStates[i])
            {
                case State.Plus:
                    AddPlus(i);
                    break;
                case State.Cross:
                    AddCross(i);
                    break;
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.PolyExchange)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target != null)
            {
                _tethers.Add((source, target));
                _dangerDirty = true;
            }
        }
    }

    //public override void OnUntethered(Actor source, ActorTetherInfo tether)
    //{
    //    if (tether.ID == (uint)TetherID.PolyExchange)
    //    {
    //        _tethers.RemoveAll(st => st.Item1 == source);
    //        _dangerDirty = true;
    //    }
    //}

    public override void OnMapEffect(byte index, uint state)
    {
        var square = index switch
        {
            1 => 0x0,
            2 => 0x1,
            3 => 0x2,
            4 => 0x3,
            5 => 0x5,
            6 => 0x6,
            7 => 0x9,
            8 => 0xA,
            9 => 0xC,
            10 => 0xD,
            11 => 0xE,
            12 => 0xF,
            13 => 0x8,
            14 => 0xB,
            15 => 0x4,
            16 => 0x7, // not sure
            _ => -1
        };

        if (square < 0)
            return;

        switch (state)
        {
            //case 0x00100001: // x to +
            //case 0x00800020: // + to x
            //case 0x00080004:
            //    _states[square] = State.None;
            //    break;
            case 0x00020001: // +
                _states[square] = State.Plus;
                break;
            case 0x00400020: // x
                _states[square] = State.Cross;
                break;
        }
        _dangerDirty = true;
    }

    private static int CoordinateToIndex(float c) => c switch
    {
        < 90 => 0,
        < 100 => 1,
        < 110 => 2,
        _ => 3
    };
    private static int PositionToIndex(WPos pos) => CoordinateToIndex(pos.Z) * 4 + CoordinateToIndex(pos.X);
    private static float IndexToCoordinate(int i) => 85 + 10 * i;
    private static WPos IndexToPosition(int i) => new(IndexToCoordinate(i & 3), IndexToCoordinate(i >> 2));

    private void AddPlus(int sq)
    {
        var x = sq & 0x3;
        var z = sq & 0xC;
        for (var i = 0; i < 4; ++i)
            _dangerCells[z + i] = true;
        for (var i = 0; i < 4; ++i)
            _dangerCells[x + i * 4] = true;
    }

    private void AddCross(int sq)
    {
        var x = sq & 0x3;
        var z = sq & 0xC;
        _dangerCells[sq] = true;
        for (var i = 1; i < 4; ++i)
        {
            var x1 = x + i;
            var x2 = x - i;
            var z1 = z + i * 4;
            var z2 = z - i * 4;
            if (x1 < 4 && z1 < 16)
                _dangerCells[x1 + z1] = true;
            if (x1 < 4 && z2 >= 0)
                _dangerCells[x1 + z2] = true;
            if (x2 >= 0 && z1 < 16)
                _dangerCells[x2 + z1] = true;
            if (x2 >= 0 && z2 >= 0)
                _dangerCells[x2 + z2] = true;
        }
    }
}
