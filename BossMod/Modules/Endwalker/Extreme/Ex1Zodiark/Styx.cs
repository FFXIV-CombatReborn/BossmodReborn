namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

class Styx(BossModule module) : Components.UniformStackSpread(module, 5f, default, 8, 8)
{
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.StyxAOE)
            ++NumCasts;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Styx)
            AddStack(actor);
    }
}
