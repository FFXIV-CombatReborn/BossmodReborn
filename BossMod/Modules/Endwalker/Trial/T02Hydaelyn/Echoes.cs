namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class Echoes(BossModule module) : Components.UniformStackSpread(module, 6, 0, 8, 8)
{
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Echoes)
        {
            ++NumCasts;
            if (NumCasts == 5)
            {
                Stacks.Clear();
                NumCasts = 0;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Echoes)
            AddStack(actor, WorldState.FutureTime(5));
    }
}
