namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

sealed class ElectromagneticPulse(BossModule module) : Components.GenericAOEs(module, (uint)AID.ElectromagneticPulse)
{
    private static readonly AOEShapeRect rect = new(5f, 20f);
    private static readonly Angle rotation = -60f.Degrees();
    private static readonly WPos[] odd = MakeStrips(20f);
    private static readonly WPos[] even = MakeStrips(15f);
    private readonly List<AOEInstance> _aoes = [with(5)];

    private static WPos[] MakeStrips(float startOffset)
    {
        var pos = A12Hobbes.PlatformCenters[0] + 120f.Degrees().ToDirection() * startOffset;
        var step = 300f.Degrees().ToDirection() * 10f;
        var result = new WPos[5];
        for (var i = 0; i < 5; ++i)
        {
            result[i] = pos;
            pos += step;
        }
        return result;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => A12Hobbes.PlatformIndex(actor.Position) == 0 ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x03)
        {
            if (state == 0x00400020u)
            {
                AddStrips(even);
            }
            else if (state == 0x02000100u)
            {
                AddStrips(odd);
            }
        }
    }

    private void AddStrips(WPos[] strips)
    {
        _aoes.Clear();
        var act = WorldState.FutureTime(4.2d);
        for (var i = 0; i < 5; ++i)
        {
            _aoes.Add(new(rect, strips[i], rotation, act));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
