namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

class BoldBoulderTrample(BossModule module) : Components.UniformStackSpread(module, 6f, 20f, 6, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BoldBoulder && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            AddSpread(target);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BoldBoulder)
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.TargetID);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Trample)
            AddStack(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Trample)
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}
