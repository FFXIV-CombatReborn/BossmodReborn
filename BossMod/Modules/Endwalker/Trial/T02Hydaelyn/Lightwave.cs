namespace BossMod.Endwalker.Trial.T02Hydaelyn;

sealed class Lightwave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> waves = [with(4)];
    private readonly AOEShapeRect rect = new(16f, 8f, 12f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = waves.Count;
        if (waves.Count == 0)
            return [];
        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < count; ++i)
        {
            var w = waves[i];
            aoes[i] = new(rect, w.Position, w.Rotation, WorldState.FutureTime(1.1d));
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RayOfLight && !waves.Contains(caster))
        {
            waves.Add(caster);
        }
    }
}
