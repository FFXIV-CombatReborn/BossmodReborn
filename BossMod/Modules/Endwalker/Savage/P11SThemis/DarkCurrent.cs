namespace BossMod.Endwalker.Savage.P11SThemis;

class DarkCurrent(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 9 ? 9 : count;
        var aoes = new AOEInstance[max];

        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i < 3)
                aoes[i] = count > 3 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DarkCurrentAOEFirst or (uint)AID.DarkCurrentAOERest)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is 1 or 2 && state is 0x00020001 or 0x00200010)
        {
            // index 01 => start from N/S, 02 => start from E/W
            // state 00020001 => CW => 00080004 end, 00200010 => CCW => 00800004 end
            var startingAngle = index == 2 ? 90f.Degrees() : default;
            var rotation = state == 0x00020001 ? -22.5f.Degrees() : 22.5f.Degrees();
            void AddAOE(WDir offset, DateTime act) => _aoes.Add(new(_shape, (Arena.Center + offset).Quantized(), default, act));
            for (var i = 0; i < 8; ++i)
            {
                var act = WorldState.FutureTime(7.1d + i * 1.1d);
                var offset = 13 * (startingAngle + i * rotation).ToDirection();
                AddAOE(default, act);
                AddAOE(offset, act);
                AddAOE(-offset, act);
            }
        }
    }
}

class BlindingLight(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BlindingLightAOE, 6f);
