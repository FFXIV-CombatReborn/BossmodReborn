namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class SuddenlySewage(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeRect square = new(20f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
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
                _aoes.Add(new(square, (pos + new WDir(default, -10f)).Quantized(), Angle.AnglesCardinals[1], WorldState.FutureTime(8d)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SuddenlySewage && ((++NumCasts) & 1) == 0)
        {
            _aoes.RemoveRange(0, 2);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count != 4)
        {
            return;
        }
        // make ai stay close to middle of the aoes to ensure successfully dodging the combo
        ref var aoe = ref _aoes.Ref(0);
        hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 6f), aoe.Activation.AddSeconds(2d));
    }
}
