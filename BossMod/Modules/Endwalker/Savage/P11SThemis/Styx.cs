namespace BossMod.Endwalker.Savage.P11SThemis;

class Styx(BossModule module) : Components.UniformStackSpread(module, 6f, default, 8, 8)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Styx && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            AddStack(target, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Styx or (uint)AID.StyxAOE)
            ++NumCasts;
    }
}
