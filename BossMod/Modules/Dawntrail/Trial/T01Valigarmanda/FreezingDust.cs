namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

sealed class FreezingDust(BossModule module) : Components.StayMove(module, 3d)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FreezingDust)
        {
            Array.Fill(PlayerStates, new(Requirement.Move, Module.CastFinishAt(spell, 1d)));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.FreezingUp && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = default;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status) // it sometimes seems to skip the freezing up debuff?
    {
        if (status.ID == (uint)SID.DeepFreeze && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        ref readonly var ps = ref PlayerStates[slot];
        if (ps.Requirement != Requirement.Move)
        {
            return;
        }
        base.AddAIHints(slot, actor, assignment, hints);
        hints.AddForbiddenZone(new SDInvertedRect(Arena.Center, new WDir(default, 1f), 10f, 10, 10f), ps.Activation);
    }
}
