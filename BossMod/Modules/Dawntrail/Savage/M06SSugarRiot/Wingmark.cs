namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class Wingmark(BossModule module) : Components.GenericKnockback(module)
{
    private DateTime activation;
    private BitMask wingmark;
    public BitMask StunStatus;
    private SingleDoubleStyle1? _aoe = module.FindComponent<SingleDoubleStyle1>();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (wingmark[slot] && Math.Max(0d, (activation - WorldState.CurrentTime).TotalSeconds) < 8.7d)
            return new Knockback[1] { new(actor.Position, 34f, activation, default, actor.Rotation, Kind.DirForward) };
        return [];
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        _aoe ??= Module.FindComponent<SingleDoubleStyle1>();
        if (_aoe != null)
        {
            var aoes = _aoe.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (aoe.Check(pos))
                {
                    return true;
                }
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Wingmark)
        {
            activation = status.ExpireAt;
            wingmark[Raid.FindSlot(actor.InstanceID)] = true;
        }
        else if (status.ID == (uint)SID.Stun)
        {
            StunStatus[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Wingmark)
        {
            wingmark[Raid.FindSlot(actor.InstanceID)] = false;
        }
        else if (status.ID == (uint)SID.Stun)
        {
            StunStatus[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }
}
