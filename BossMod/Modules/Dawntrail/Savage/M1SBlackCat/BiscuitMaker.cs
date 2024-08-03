using static BossMod.ActorCastEvent;
using static BossMod.Components.Knockback;

namespace BossMod.Dawntrail.Savage.M1SBlackCat;

public class BiscuitMaker(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BiscuitMakerSecond))
{
    public Actor? Source;
    public Actor? FirstTarget;
    public DateTime? AggroSwapStart;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Source == null)
            return;
        if (actor.Role == Role.Tank
            && AggroSwapStart <= Module.WorldState.CurrentTime)
        {
            if (FirstTarget == actor && WorldState.Actors.Find(Module.PrimaryActor.TargetID) == actor)
            {
                hints.Add("Pass aggro or invul!");
            }
            else if (WorldState.Actors.Find(Module.PrimaryActor.TargetID) == FirstTarget)
            {
                hints.Add("Taunt!");
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BiscuitMakerFirst)
        {
            Source = caster;
            FirstTarget = WorldState.Actors.Find(spell.TargetID);
            AggroSwapStart = Module.WorldState.FutureTime(2.5f);
        }
    }
}
