namespace BossMod.Dawntrail.Ultimate.FRU;

// TODO: can target change if boss is provoked mid cast?
sealed class P4AkhMorn(BossModule module) : Components.UniformStackSpread(module, 4f, default, 4)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AkhMornOracle or (uint)AID.AkhMornUsurper && WorldState.Actors.Find(caster.TargetID) is var target && target != null)
            AddStack(target, Module.CastFinishAt(spell, 0.9d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AkhMornAOEOracle)
            ++NumCasts;
    }
}
