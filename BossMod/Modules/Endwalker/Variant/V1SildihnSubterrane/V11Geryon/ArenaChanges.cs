namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly List<Square> squares = new(2);
    private static readonly AOEShapeRect square = new(10f, 10f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ColossalStrike && Arena.Bounds.Radius == 24.5f)
        {
            _aoes.Add(new(V11Geryon.Square, Arena.Center, default, WorldState.FutureTime(4d)));
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x13 && state == 0x00080004u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoes.Clear();
        }
        else if (state == 0x00800040u)
        {
            WPos pos = index switch
            {
                0x05 => new(173f, 167f),
                0x06 => new(193f, 167f),
                0x07 => new(173f, 187f),
                0x08 => new(193f, 187f),
                _ => default
            };
            if (pos != default)
            {
                squares.Add(new Square(pos, 10f));
                _aoes.Add(new(square, pos, default, WorldState.FutureTime(3d)));
            }
        }
        else if (state == 0x08000400u && squares.Count != 0 && index is >= 0x05 and <= 0x08)
        {
            _aoes.Clear();
            var arena = new ArenaBoundsCustom([new Square(Arena.Center, 19.5f)], [.. squares]);
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
            squares.Clear();
        }
        else if (state == 0x00080004u && index is >= 0x05 and <= 0x08)
        {
            Arena.Bounds = new ArenaBoundsSquare(19.5f);
            Arena.Center = V11Geryon.ArenaCenter3;
        }
    }
}
