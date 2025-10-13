namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

sealed class Duplicate(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    public static readonly AOEShapeRect Rect = new(16f, 8f);
    private bool phase2;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Duplicate1 or (uint)AID.Duplicate2)
        {
            _aoes.Clear();
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        switch (index)
        {
            case >= 0x2C and <= 0x34 when !phase2 && state == 0x00020001u || phase2 && state == 0x00080010u:
                var tile = index - 0x2C;
                AddAOEs(tile / 3, tile % 3);
                break;
            case >= 0x35 and <= 0x3D:
                switch (state)
                {
                    case 0x00080010u:
                        var tile2 = index - 0x35;
                        AddAOEs(tile2 / 3, tile2 % 3);
                        break;
                    case 0x00020001u:
                        var tile3 = index - 0x35;
                        _aoes.Add(new(Rect, new WPos(784f + tile3 % 3 * 16f, -816f + tile3 / 3 * 16f) + new WDir(default, -8f), default, WorldState.FutureTime(7.8d)));
                        break;
                }
                break;
            case 0x24 when state == 0x00200040u:
                phase2 = true;
                break;
        }
    }

    private void AddAOEs(int row, int col)
    {
        (int dr, int dc)[] offsets =
        [
            (0, 0),
            (-1, 0),
            (1, 0),
            (0, -1),
            (0, 1),
        ];

        for (var i = 0; i < 5; ++i)
        {
            var nRow = row + offsets[i].dr;
            var nCol = col + offsets[i].dc;

            if (nRow is >= 0 and < 3 && nCol is >= 0 and < 3)
            {
                _aoes.Add(new(Rect, (new WPos(784f + nCol * 16f, -816f + nRow * 16f) + new WDir(default, -8f)).Quantized(), Angle.AnglesCardinals[1], WorldState.FutureTime(phase2 ? 11.1d : 10.4d)));
            }
        }
    }
}
