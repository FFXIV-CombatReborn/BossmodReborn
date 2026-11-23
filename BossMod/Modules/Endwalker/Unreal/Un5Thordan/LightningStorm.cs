namespace BossMod.Endwalker.Unreal.Un5Thordan;

// note: we don't use simple 'spread from cast targets', because casts are staggered a bit, which is ugly
sealed class LightningStorm(BossModule module) : Components.UniformStackSpread(module, 0, 5)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LightningStorm)
            AddSpread(actor, WorldState.FutureTime(4));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LightningStormAOE)
            Spreads.Clear();
    }
}
