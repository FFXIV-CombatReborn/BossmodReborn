namespace BossMod.Endwalker.Savage.P8S2;

sealed class EndOfDays(BossModule module) : Components.GenericAOEs(module, (uint)AID.EndOfDays)
{
    public List<(Actor caster, DateTime finish)> Casters = [];

    private readonly AOEShapeRect _shape = new(60f, 5f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];

        var max = count > 3 ? 3 : count;
        var aoesLen = max;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;

        for (var i = 0; i < max; ++i)
        {
            var c = Casters[i];
            var rotation = c.caster.CastInfo?.Rotation ?? c.caster.Rotation;
            var finish = c.caster.CastInfo != null ? Module.CastFinishAt(c.caster.CastInfo) : c.finish;

            aoes[i] = new(_shape, c.caster.Position, rotation, finish);
        }

        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Casters.RemoveAll(c => c.caster == caster);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.IllusoryHephaistosLanes && id == 0x11D3)
            Casters.Add((actor, WorldState.FutureTime(8))); // ~2s before cast start
    }
}
