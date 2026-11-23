namespace BossMod.Endwalker.Unreal.Un2Sephirot;

sealed class EinSof(BossModule module) : Components.GenericAOEs(module, (uint)AID.EinSofAOE)
{
    private readonly List<Actor> _active = [];

    private readonly AOEShape _shape = new AOEShapeCircle(10f); // TODO: verify radius

    public bool Active => _active.Count > 0;

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _active.Count;
        if (count == 0)
            return [];
        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < count; ++i)
        {
            aoes[i] = new(_shape, _active[i].Position);
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        switch (state)
        {
            // 0x00100020 - become harmful, happens ~2.5s after appear and ~1.5s before first aoe
            // 0x00400080 - ??? (after 5th aoe)
            case 0x00040008u: // appear as harmless
                _active.Add(actor);
                break;
            case 0x00010200u: // disappear (happens ~1.2s after last aoe)
                _active.Remove(actor);
                break;
        }
    }
}
