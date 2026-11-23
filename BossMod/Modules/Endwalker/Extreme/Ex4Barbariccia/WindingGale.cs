namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

// TODO: not sure how 'spiral arms' are really implemented
sealed class WindingGale(BossModule module) : Components.GenericAOEs(module, (uint)AID.WindingGale)
{
    private readonly List<Actor> _casters = [];

    private readonly AOEShapeDonutSector _shape = new(9f, 11f, 90f.Degrees());

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];
        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < count; ++i)
        {
            var c = _casters[i];
            aoes[i] = new(_shape, c.Position + _shape.OuterRadius * c.Rotation.ToDirection(), c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _casters.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _casters.Remove(caster);
        }
    }
}
