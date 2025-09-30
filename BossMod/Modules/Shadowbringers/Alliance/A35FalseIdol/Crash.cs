namespace BossMod.Shadowbringers.Alliance.A35FalseIdol;

sealed class Crash(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeRect rect = new(50f, 5f);
    private bool first = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x08 and <= 0x0C && state == 0x00200010u)
        {
            _aoes.Add(new(rect, new WPos(-675f, -720f + (index - 0x08) * 10f).Quantized(), Angle.AnglesCardinals[0], WorldState.FutureTime(first ? 8.8d : 6.8d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Crash)
        {
            _aoes.Clear();
            first = false;
        }
    }
}
