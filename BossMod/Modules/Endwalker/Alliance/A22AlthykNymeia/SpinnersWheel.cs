namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

sealed class SpinnersWheelSelect(BossModule module) : BossComponent(module)
{
    public enum Branch { None, Gaze, StayMove }

    public Branch SelectedBranch;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var branch = status.ID switch
        {
            (uint)SID.ArcaneAttraction or (uint)SID.AttractionReversed => Branch.Gaze,
            (uint)SID.ArcaneFever or (uint)SID.FeverReversed => Branch.StayMove,
            _ => Branch.None
        };
        if (branch != Branch.None)
            SelectedBranch = branch;
    }
}

abstract class SpinnersWheelGaze(BossModule module, bool inverted, uint aid, uint sid) : Components.GenericGaze(module, aid)
{
    private readonly Actor? _source = module.GetActor((uint)OID.Nymeia);
    private DateTime _activation;
    private BitMask _affected;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (_source != null && _affected[slot])
        {
            var loc = _source.Position.Quantized();
            Eye[] eye = [new(loc, _activation, inverted: inverted, eyeCenter: loc)];
            return eye;
        }
        return [];
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == sid)
        {
            _activation = status.Extra == 0x28 ? WorldState.FutureTime(1.7d) : status.ExpireAt;
            _affected.Set(Raid.FindSlot(actor.InstanceID));
        }
    }
}
sealed class SpinnersWheelArcaneAttraction(BossModule module) : SpinnersWheelGaze(module, false, (uint)AID.SpinnersWheelArcaneAttraction, (uint)SID.ArcaneAttraction);
sealed class SpinnersWheelAttractionReversed(BossModule module) : SpinnersWheelGaze(module, true, (uint)AID.SpinnersWheelAttractionReversed, (uint)SID.AttractionReversed);

sealed class SpinnersWheelStayMove(BossModule module) : Components.StayMove(module)
{
    public int ActiveDebuffs;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.ArcaneFever:
                if (Raid.FindSlot(actor.InstanceID) is var feverSlot && feverSlot >= 0)
                    PlayerStates[feverSlot] = new(Requirement.Stay, status.Extra == 0x28 ? WorldState.FutureTime(1.7d) : status.ExpireAt);
                break;
            case (uint)SID.FeverReversed:
                if (Raid.FindSlot(actor.InstanceID) is var revSlot && revSlot >= 0)
                    PlayerStates[revSlot] = new(Requirement.Move, status.Extra == 0x28 ? WorldState.FutureTime(1.7d) : status.ExpireAt);
                break;
            case (uint)SID.Pyretic:
            case (uint)SID.FreezingUp:
                ++ActiveDebuffs;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.Pyretic or (uint)SID.FreezingUp)
        {
            --ActiveDebuffs;
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            {
                PlayerStates[slot] = default;
            }
        }
    }
}
