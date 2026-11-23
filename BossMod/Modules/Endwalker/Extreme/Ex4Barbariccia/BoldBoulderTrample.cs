namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

sealed class BoldBoulderTrample(BossModule module) : Components.UniformStackSpread(module, 6f, 20f, 6, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BoldBoulder && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            AddSpread(target);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BoldBoulder)
        {
            for (var i = Spreads.Count - 1; i >= 0; --i)
            {
                if (Spreads[i].Target.InstanceID == spell.TargetID)
                    Spreads.RemoveAt(i);
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Trample)
            AddStack(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Trample)
        {
            for (var i = Stacks.Count - 1; i >= 0; --i)
            {
                if (Stacks[i].Target.InstanceID == spell.MainTargetID)
                    Stacks.RemoveAt(i);
            }
        }
    }
}
