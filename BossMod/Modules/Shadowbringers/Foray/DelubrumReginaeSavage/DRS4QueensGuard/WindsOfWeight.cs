namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

sealed class WindsOfWeight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _green = [];
    private readonly List<Actor> _purple = [];
    private BitMask _invertedPlayers;

    private readonly AOEShapeCircle _shape = new(20f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var actors = _invertedPlayers[slot] ? _purple : _green;
        var count = actors.Count;
        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < count; ++i)
        {
            var c = actors[i];
            aoes[i] = new(_shape, c.CastInfo!.LocXZ, c.CastInfo.Rotation, Module.CastFinishAt(c.CastInfo));
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ReversalOfForces)
            _invertedPlayers[Raid.FindSlot(actor.InstanceID)] = true;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CasterList(spell)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CasterList(spell)?.Remove(caster);
    }

    private List<Actor>? CasterList(ActorCastInfo spell) => spell.Action.ID switch
    {
        (uint)AID.WindsOfFate => _green,
        (uint)AID.WeightOfFortune => _purple,
        _ => null
    };
}
