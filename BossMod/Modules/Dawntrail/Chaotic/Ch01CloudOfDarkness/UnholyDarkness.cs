namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class UnholyDarkness(BossModule module) : Components.StackWithIcon(module, (uint)IconID.UnholyDarkness, (uint)AID.UnholyDarknessAOE, 6f, 8.1d, 4)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == StackAction)
        {
            Stacks.Clear(); // if one of the target dies, it won't get hit
            ++NumFinishedStacks;
        }
    }
}
