namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

sealed class CthonicVent(BossModule module) : Components.GenericAOEs(module)
{
    public int NumTotalCasts;
    private readonly List<WPos> _centers = [];
    private static readonly AOEShapeCircle _shape = new(23);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _centers.Count;
        if (count == 0)
            return [];
        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < count; ++i)
            aoes[i] = new(_shape, _centers[i]);
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // note: we can determine position ~0.1s earlier by using eobjanim
        if (spell.Action.ID == (uint)AID.CthonicVentAOE1)
            _centers.Add(caster.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CthonicVentMoveNear:
                _centers.Add(caster.Position + caster.Rotation.ToDirection() * 30f);
                break;
            case (uint)AID.CthonicVentMoveDiag:
                _centers.Add(caster.Position + caster.Rotation.ToDirection() * 42.426407f);
                break;
            case (uint)AID.CthonicVentAOE1:
            case (uint)AID.CthonicVentAOE2:
            case (uint)AID.CthonicVentAOE3:
                ++NumTotalCasts;
                _centers.RemoveAll(c => c.AlmostEqual(caster.Position, 2f));
                break;
        }
    }
}
