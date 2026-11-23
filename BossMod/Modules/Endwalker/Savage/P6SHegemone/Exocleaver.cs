namespace BossMod.Endwalker.Savage.P6SHegemone;

sealed class Exocleaver(BossModule module) : Components.GenericAOEs(module, (uint)AID.ExocleaverAOE2)
{
    public bool FirstDone;
    private readonly AOEShapeCone _cone = new(30f, 15f.Degrees());
    private readonly List<Angle> _directions = [];

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts > 0)
            return [];

        // TODO: timing
        var offset = (FirstDone ? 30f : 0f).Degrees();
        var count = _directions.Count;
        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < count; ++i)
            aoes[i] = new(_cone, Module.PrimaryActor.Position, _directions[i] + offset);
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ExocleaverAOE1)
            _directions.Add(caster.Rotation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ExocleaverAOE1)
            FirstDone = true;
    }
}
