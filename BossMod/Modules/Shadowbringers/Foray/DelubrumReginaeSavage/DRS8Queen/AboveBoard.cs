namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

// note: this is exactly the same as queen's guard component
sealed class AboveBoard(BossModule module) : Components.GenericAOEs(module)
{
    public enum State { Initial, ThrowUpDone, ShortExplosionsDone, LongExplosionsDone }

    public State CurState;
    private readonly List<Actor> _smallBombs = module.Enemies((uint)OID.AetherialBolt);
    private readonly List<Actor> _bigBombs = module.Enemies((uint)OID.AetherialBurst);
    private bool _invertedBombs; // bombs are always either all normal (big=short) or all inverted
    private BitMask _invertedPlayers; // default for player is 'long', short is considered inverted (has visible status)
    private DateTime _activation = module.WorldState.FutureTime(14.4d);

    private readonly AOEShapeCircle _shape = new(10f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var imminentBombs = AreBigBombsDangerous(slot) ? _bigBombs : _smallBombs;
        var count = imminentBombs.Count;
        if (count == 0)
            return [];
        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < count; ++i)
        {
            aoes[i] = new(_shape, imminentBombs[i].Position, default, _activation);
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.ReversalOfForces:
                if (actor.OID is (uint)OID.AetherialBolt or (uint)OID.AetherialBurst)
                    _invertedBombs = true;
                else
                    _invertedPlayers[Raid.FindSlot(actor.InstanceID)] = true;
                break;
            case (uint)SID.AboveBoardPlayerLong:
            case (uint)SID.AboveBoardPlayerShort:
            case (uint)SID.AboveBoardBombLong:
            case (uint)SID.AboveBoardBombShort:
                AdvanceState(State.ThrowUpDone);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LotsCastBigShort:
            case (uint)AID.LotsCastSmallShort:
                AdvanceState(State.ShortExplosionsDone);
                break;
            case (uint)AID.LotsCastLong:
                AdvanceState(State.LongExplosionsDone);
                _activation = WorldState.FutureTime(4.2d);
                break;
        }
    }

    private bool AreBigBombsDangerous(int slot)
    {
        if (_invertedPlayers[slot])
        {
            // inverted players fall right before first bomb explosion, so they have to avoid first bombs, then move to avoid second bombs
            var firstSetImminent = CurState < State.ShortExplosionsDone;
            return firstSetImminent != _invertedBombs; // first set is big if inverted
        }
        else
        {
            // normally players fall right before second bomb explosion, so they only avoid second bombs
            // second bombs are normally small, big if inverted
            return _invertedBombs;
        }
    }

    private void AdvanceState(State dest)
    {
        if (CurState < dest)
            CurState = dest;
    }
}
