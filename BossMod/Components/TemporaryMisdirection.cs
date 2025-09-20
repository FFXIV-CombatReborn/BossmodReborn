namespace BossMod.Components;

// generic temporary misdirection component
[SkipLocalsInit]
public abstract class TemporaryMisdirection(BossModule module, uint aid, string hint = "Applies temporary misdirection") : CastHint(module, aid, hint)
{
    private BitMask mask;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is 1422u or 2936u or 3694u or 3909u)
        {
            mask.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is 1422u or 2936u or 3694u or 3909u)
        {
            mask.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (mask[slot])
        {
            hints.AddSpecialMode(AIHints.SpecialMode.Misdirection, default);
        }
    }
}
