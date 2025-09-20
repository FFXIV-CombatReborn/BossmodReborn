namespace BossMod.Components;

// component for mechanics that either require players to move or stay still
// priorities can be used to simplify implementation when e.g. status changes at different stages of the mechanic (eg if prep status is replaced with pyretic, we want to allow them to happen in any sequence)
[SkipLocalsInit]
public class StayMove(BossModule module, double maxTimeToShowHint = 1e3d) : BossComponent(module)
{
    public enum Requirement { None, Stay, Stay2, Move }
    public readonly struct PlayerState(Requirement requirement, DateTime activation, int priority = 0)
    {
        public readonly Requirement Requirement = requirement;
        public readonly DateTime Activation = activation;
        public readonly int Priority = priority;
    }

    public readonly PlayerState[] PlayerStates = new PlayerState[PartyState.MaxAllies];
    public readonly double MaxTimeToShowHint = maxTimeToShowHint;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        ref readonly var state = ref PlayerStates[slot];
        switch (state.Requirement)
        {
            case Requirement.Stay:
                if (state.Activation <= WorldState.FutureTime(MaxTimeToShowHint))
                {
                    hints.Add("Stop everything!", actor.PrevPosition != actor.PrevPosition || actor.CastInfo != null || actor.TargetID != default); // note: assume if target is selected, we might autoattack...
                }
                break;
            case Requirement.Stay2:
                if (state.Activation <= WorldState.FutureTime(MaxTimeToShowHint))
                {
                    hints.Add("Don't move!", actor.PrevPosition != actor.PrevPosition); // you are allowed to attack here, only moving is forbidden
                }
                break;
            case Requirement.Move:
                if (state.Activation <= WorldState.FutureTime(MaxTimeToShowHint))
                {
                    hints.Add("Move!", actor.PrevPosition == actor.PrevPosition);
                }
                break;

        }
        if (actor.IsDead && state.Requirement != Requirement.None)
        {
            PlayerStates[slot] = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        ref readonly var state = ref PlayerStates[slot];
        if (state.Activation == default)
        {
            return;
        }
        switch (state.Requirement)
        {
            case Requirement.Stay:
                hints.AddSpecialMode(AIHints.SpecialMode.Pyretic, state.Activation);
                break;
            case Requirement.Stay2:
                hints.AddSpecialMode(AIHints.SpecialMode.NoMovement, state.Activation);
                break;
            case Requirement.Move:
                hints.AddSpecialMode(AIHints.SpecialMode.Freezing, state.Activation);
                break;
        }
    }

    // update player state, if current priority is same or lower
    protected void SetState(int slot, ref readonly PlayerState state)
    {
        if (slot >= 0 && PlayerStates[slot].Priority <= state.Priority)
        {
            PlayerStates[slot] = state;
        }
    }

    protected void ClearState(int slot, int priority = int.MaxValue)
    {
        if (slot >= 0 && PlayerStates[slot].Priority <= priority)
        {
            PlayerStates[slot] = default;
        }
    }
}
