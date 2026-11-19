namespace BossMod.Dawntrail.Unreal.Un1Byakko;

sealed class StateOfShock(BossModule module) : Components.CastCounter(module, (uint)AID.StateOfShockSecond)
{
    public int NumStuns;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Stun)
            ++NumStuns;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Stun)
            --NumStuns;
    }
}

sealed class HighestStakes(BossModule module) : Components.GenericTowers(module, (uint)AID.HighestStakesAOE)
{
    private BitMask _forbidden;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.HighestStakes)
        {
            Towers.Add(new(actor.Position.Quantized(), 6f, 3, 3, _forbidden, WorldState.FutureTime(6.1d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            Towers.Clear();
            var targets = CollectionsMarshal.AsSpan(spell.Targets);
            var len = targets.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var targ = ref targets[i];
                _forbidden.Set(Raid.FindSlot(targ.ID));
            }
        }
    }
}
