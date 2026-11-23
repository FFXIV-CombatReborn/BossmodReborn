namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to 'single' and 'multi' fireplumes (normal or parts of gloryplume)
sealed class Fireplume(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circleBig = new(15f);
    private readonly AOEShapeCircle circleSmall = new(10f);
    private readonly List<AOEInstance> _aoes = [with(10)];

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 8 ? 8 : count;
        var aoesLen = max;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;

        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i < 2)
                aoes[i] = count > 2 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ExperimentalFireplumeSingleAOE:
            case (uint)AID.ExperimentalGloryplumeSingleAOE:
                _aoes.Add(new(circleBig, spell.LocXZ, default, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.ExperimentalFireplumeMultiAOE:
            case (uint)AID.ExperimentalGloryplumeMultiAOE:
                if (_aoes.Count == 0)
                {
                    var startingDirection = Angle.FromDirection(caster.Position - Arena.Center);
                    for (var i = 0; i < 4; ++i)
                    {
                        var activation = Module.CastFinishAt(spell, i * 1);
                        var dir = 15 * (startingDirection - i * 45.Degrees()).ToDirection();
                        _aoes.Add(new(circleSmall, Arena.Center + dir, default, activation));
                        _aoes.Add(new(circleSmall, Arena.Center - dir, default, activation));
                    }
                    _aoes.Add(new(circleSmall, Arena.Center, default, Module.CastFinishAt(spell, 4)));
                }
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.ExperimentalFireplumeSingleAOE:
                case (uint)AID.ExperimentalGloryplumeSingleAOE:
                case (uint)AID.ExperimentalFireplumeMultiAOE:
                case (uint)AID.ExperimentalGloryplumeMultiAOE:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}
